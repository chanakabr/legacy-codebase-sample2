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

namespace ElasticSearchHandler.IndexBuilders
{
    public class EpgIndexBuilder : AbstractIndexBuilder
    {
        private static readonly string EPG = "epg";

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
                    Logger.Logger.Log("Error", string.Format("could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead", language.Code), "ElasticSearch");
                }

                string sMapping = serializer.CreateEpgMapping(group.m_oEpgGroupSettings.m_lMetasName, group.m_oEpgGroupSettings.m_lTagsName, indexAnalyzer, searchAnalyzer,
                    autocompleteIndexAnalyzer, autocompleteSearchAnalyzer);
                string sType = (language.IsDefault) ? EPG : string.Concat(EPG, "_", language.Code);
                bool bMappingRes = api.InsertMapping(newIndexName, sType, sMapping.ToString());

                if (language.IsDefault && !bMappingRes)
                    success = false;

                if (!bMappingRes)
                    Logger.Logger.Log("Error", string.Concat("Could not create mapping of type epg for language ", language.Name), "ESFeeder");

            }
            #endregion

            if (!success)
            {
                Logger.Logger.Log("Error", string.Format("Failed creating index for index:{0}", newIndexName), "ESFeeder");
                return success;
            }

            while (tempDate <= this.EndDate.Value)
            {
                PopulateEpgIndex(newIndexName, EPG, tempDate);
                tempDate = tempDate.AddDays(1);
            }

            bool indexExists = api.IndexExists(ElasticSearchTaskUtils.GetEpgGroupAliasStr(groupId));

            if (this.SwitchIndexAlias || !indexExists)
            {
                List<string> lOldIndices = api.GetAliases(groupAlias);

                success = api.SwitchIndex(newIndexName, groupAlias, lOldIndices, null);

                if (success && lOldIndices.Count > 0)
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
                        Logger.Logger.Log("Error", string.Format("analyzer for language {0} doesn't exist", language.Code), "ESFeeder");
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
            Dictionary<ulong, EpgCB> programs = GetEpgPrograms(groupId, date, 0);

            List<KeyValuePair<ulong, string>> epgList = new List<KeyValuePair<ulong, string>>();
            foreach (ulong epgID in programs.Keys)
            {
                EpgCB epg = programs[epgID];

                if (epg != null)
                {
                    string serializedEpg = serializer.SerializeEpgObject(epg);
                    epgList.Add(new KeyValuePair<ulong, string>(epg.EpgID, serializedEpg));
                }

                if (epgList.Count >= 50)
                {
                    api.CreateBulkIndexRequest(index, type, epgList);

                    epgList = new List<KeyValuePair<ulong, string>>();
                }
            }

            if (epgList.Count > 0)
            {
                api.CreateBulkIndexRequest(index, type, epgList);
            }
        }

        protected Dictionary<ulong, EpgCB> GetEpgPrograms(int groupId, DateTime? dateTime, int epgID)
        {
            Dictionary<ulong, EpgCB> epgs = new Dictionary<ulong, EpgCB>();

            //Get All programs by group_id + date from CB
            TvinciEpgBL oEpgBL = new TvinciEpgBL(groupId);
            List<EpgCB> lEpgCB = oEpgBL.GetGroupEpgs(0, 0, dateTime, dateTime.Value.AddDays(1));

            if (lEpgCB != null && lEpgCB.Count > 0)
            {
                foreach (EpgCB epg in lEpgCB)
                {
                    epgs.Add(epg.EpgID, epg);
                }
            }

            return epgs;
        }

        #endregion
    }
}
