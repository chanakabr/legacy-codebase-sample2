using ApiObjects;
using Catalog;
using ElasticSearch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupsCacheManager;
using System.Threading.Tasks;
using EpgBL;
using KLogMonitor;
using System.Reflection;

namespace ElasticSearchHandler.IndexBuilders
{
    public class EpgIndexBuilder : AbstractIndexBuilder
    {
        private static readonly string EPG = "epg";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Data Members

        int sizeOfBulk;

        #endregion

        #region Ctor

        public EpgIndexBuilder(int groupID)
            : base(groupID)
        {

        }

        #endregion

        #region Override Methods

        public override bool BuildIndex()
        {
            bool success = false;
            GroupManager groupManager = new GroupManager();
            groupManager.RemoveGroup(groupId);
            Group group = groupManager.GetGroup(groupId);

            if (group == null)
                return success;

            DateTime tempDate = StartDate.Value;

            string groupAlias = ElasticSearchTaskUtils.GetEpgGroupAliasStr(groupId);
            string newIndexName = ElasticSearchTaskUtils.GetNewEpgIndexStr(groupId);

            List<string> analyzers;
            List<string> filters;
            List<string> tokenizers;

            GetAnalyzers(group.GetLangauges(), out analyzers, out filters, out tokenizers);

            string sizeOfBulkString = ElasticSearchTaskUtils.GetTcmConfigValue("ES_BULK_SIZE");

            int.TryParse(sizeOfBulkString, out sizeOfBulk);

            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 50;
            }

            success = api.BuildIndex(newIndexName, 0, 0, analyzers, filters, tokenizers);

            #region create mapping
            foreach (ApiObjects.LanguageObj language in group.GetLangauges())
            {
                string indexAnalyzer, searchAnalyzer;
                string autocompleteIndexAnalyzer = null;
                string autocompleteSearchAnalyzer = null;

                string analyzerDefinitionName = ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code);

                if (ElasticSearchApi.AnalyzerExists(analyzerDefinitionName))
                {
                    indexAnalyzer = string.Concat(language.Code, "_index_", "analyzer");
                    searchAnalyzer = string.Concat(language.Code, "_search_", "analyzer");

                    if (ElasticSearchApi.GetAnalyzerDefinition(analyzerDefinitionName).Contains("autocomplete"))
                    {
                        autocompleteIndexAnalyzer = string.Concat(language.Code, "_autocomplete_analyzer");
                        autocompleteSearchAnalyzer = string.Concat(language.Code, "_autocomplete_search_analyzer");
                    }
                }
                else
                {
                    indexAnalyzer = "whitespace";
                    searchAnalyzer = "whitespace";
                    log.Error(string.Format("could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead", language.Code));
                }

                string sMapping = serializer.CreateEpgMapping(group.m_oEpgGroupSettings.m_lMetasName, group.m_oEpgGroupSettings.m_lTagsName, indexAnalyzer, searchAnalyzer,
                    autocompleteIndexAnalyzer, autocompleteSearchAnalyzer);
                string sType = (language.IsDefault) ? EPG : string.Concat(EPG, "_", language.Code);
                bool bMappingRes = api.InsertMapping(newIndexName, sType, sMapping.ToString());

                if (language.IsDefault && !bMappingRes)
                    success = false;

                if (!bMappingRes)
                {
                    log.Error(string.Concat("Could not create mapping of type epg for language ", language.Name));
                }

            }
            #endregion

            if (!success)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return success;
            }

            log.DebugFormat("Start populating epg index = {0}", newIndexName);

            while (tempDate <= this.EndDate.Value)
            {
                PopulateEpgIndex(newIndexName, EPG, tempDate);
                tempDate = tempDate.AddDays(1);
            }

            log.DebugFormat("Finished populating epg index = {0}", newIndexName);

            bool indexExists = api.IndexExists(ElasticSearchTaskUtils.GetEpgGroupAliasStr(groupId));

            if (this.SwitchIndexAlias || !indexExists)
            {
                List<string> lOldIndices = api.GetAliases(groupAlias);

                success = api.SwitchIndex(newIndexName, groupAlias, lOldIndices, null);

                if (this.DeleteOldIndices && success && lOldIndices.Count > 0)
                {
                    api.DeleteIndices(lOldIndices);
                }
            }

            return success;
        }

        #endregion

        #region Private and protected Methods

        private void GetAnalyzers(List<ApiObjects.LanguageObj> lLanguages, out List<string> lAnalyzers, out List<string> lFilters, out List<string> tokenizers)
        {
            lAnalyzers = new List<string>();
            lFilters = new List<string>();
            tokenizers = new List<string>();

            if (lLanguages != null)
            {
                foreach (ApiObjects.LanguageObj language in lLanguages)
                {
                    string analyzer = ElasticSearchApi.GetAnalyzerDefinition(ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code));
                    string filter = ElasticSearchApi.GetFilterDefinition(ElasticSearch.Common.Utils.GetLangCodeFilterKey(language.Code));
                    string tokenizer = ElasticSearchApi.GetTokenizerDefinition(ElasticSearch.Common.Utils.GetLangCodeTokenizerKey(language.Code));

                    if (string.IsNullOrEmpty(analyzer))
                    {
                        log.Error(string.Format("analyzer for language {0} doesn't exist", language.Code));
                    }
                    else
                    {
                        lAnalyzers.Add(analyzer);
                    }

                    if (!string.IsNullOrEmpty(filter))
                    {
                        lFilters.Add(filter);
                    }

                    if (!string.IsNullOrEmpty(tokenizer))
                    {
                        tokenizers.Add(tokenizer);
                    }
                }
            }
        }

        protected void PopulateEpgIndex(string index, string type, DateTime date)
        {
            try
            {
                // Get EPG objects from CB
                Dictionary<ulong, EpgCB> programs = GetEpgPrograms(groupId, date);

                List<KeyValuePair<ulong, string>> epgList = new List<KeyValuePair<ulong, string>>();

                // Run on all programs
                foreach (ulong epgID in programs.Keys)
                {
                    EpgCB epg = programs[epgID];

                    if (epg != null)
                    {
                        // Serialize EPG object to string
                        string serializedEpg = serializer.SerializeEpgObject(epg);
                        epgList.Add(new KeyValuePair<ulong, string>(epg.EpgID, serializedEpg));
                    }

                    // If we exceeded maximum size of bulk 
                    if (epgList.Count >= sizeOfBulk)
                    {
                        // create bulk request now and clear list
                        api.CreateBulkIndexRequest(index, type, epgList);

                        epgList.Clear();
                    }
                }

                // If we have anything left that is less than the size of the bulk
                if (epgList.Count > 0)
                {
                    api.CreateBulkIndexRequest(index, type, epgList);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed when populating epg index. index = {0}, type = {1}, date = {2}, message = {3}, st = {4}",
                    index, type, date, ex.Message, ex.StackTrace);

                throw ex;
            }
        }

        protected Dictionary<ulong, EpgCB> GetEpgPrograms(int groupId, DateTime? dateTime)
        {
            try
            {
                Dictionary<ulong, EpgCB> epgs = new Dictionary<ulong, EpgCB>();

                //Get All programs by group_id + date from CB
                TvinciEpgBL oEpgBL = new TvinciEpgBL(groupId);
                List<EpgCB> lEpgCB = oEpgBL.GetGroupEpgs(0, 0, dateTime, dateTime.Value.AddDays(1));
                
                if (lEpgCB != null && lEpgCB.Count > 0)
                {
                    foreach (EpgCB epg in lEpgCB)
                    {
                        if (epg != null)
                        {
                            epgs.Add(epg.EpgID, epg);
                        }
                        else
                        {
                            log.ErrorFormat("Received null epg from TvinciEpgBL, date is {0}", dateTime);
                        }
                    }
                }
                else
                {
                    log.DebugFormat("Got 0 or null EPG Programs. group = {0}, date = {1}", groupId, dateTime);
                }

                return epgs;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetEpgPrograms. group id = {2}, Date = {3}, Message = {0}, stack trace = {1}", 
                    ex.Message, ex.StackTrace,
                    groupId, dateTime);
                throw ex;
            }
        }

        #endregion
    }
}
