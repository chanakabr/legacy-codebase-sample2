using ApiObjects;
using Catalog;
using ElasticSearch.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpgBL;
using Catalog.Cache;
using GroupsCacheManager;
using KLogMonitor;
using System.Reflection;

namespace ElasticSearchFeeder.IndexBuilders
{
    public class EpgIndexBuilder : AbstractIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected ElasticSearchApi m_oESApi;
        protected int m_nGroupID;
        protected ESSerializer m_oESSerializer;

        public EpgIndexBuilder(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oESApi = new ElasticSearchApi();
            m_oESSerializer = new ESSerializer();
        }

        public override async Task<bool> BuildIndex()
        {
            bool bRes = false;
            GroupManager groupManager = new GroupManager();
            bool bres = groupManager.RemoveGroup(m_nGroupID);
            Group oGroup = groupManager.GetGroup(m_nGroupID);

            if (oGroup == null)
                return bRes;

            DateTime tempDate = dStartDate;

            string sGroupAlias = Utils.GetEpgGroupAliasStr(m_nGroupID);
            string sNewIndex = Utils.GetNewEpgIndexStr(m_nGroupID);

            List<string> lAnalyzers;
            List<string> lFilters;
            List<string> tokenizers;

            GetAnalyzers(oGroup.GetLangauges(), out lAnalyzers, out lFilters, out tokenizers);

            bRes = m_oESApi.BuildIndex(sNewIndex, 0, 0, lAnalyzers, lFilters, tokenizers);

            #region create mapping
            foreach (ApiObjects.LanguageObj language in oGroup.GetLangauges())
            {
                string indexAnalyzer, searchAnalyzer;
                string autocompleteIndexAnalyzer = null;
                string autocompleteSearchAnalyzer = null;

                string analyzerDefinitionName = ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code, string.Empty);

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
                    log.ErrorFormat("Error - could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead. ElasticSearch", language.Code);
                }

                string sMapping = m_oESSerializer.CreateEpgMapping(oGroup.m_oEpgGroupSettings.m_lMetasName, oGroup.m_oEpgGroupSettings.m_lTagsName, indexAnalyzer, searchAnalyzer,
                    autocompleteIndexAnalyzer, autocompleteSearchAnalyzer);
                string sType = (language.IsDefault) ? EPG : string.Concat(EPG, "_", language.Code);
                bool bMappingRes = m_oESApi.InsertMapping(sNewIndex, sType, sMapping.ToString());

                if (language.IsDefault && !bMappingRes)
                    bRes = false;

                if (!bMappingRes)
                    log.Error("Error - " + string.Concat("Could not create mapping of type epg for language ", language.Name));

            }
            #endregion

            if (!bRes)
            {
                log.Error("Error - " + string.Format("Failed creating index for index:{0}", sNewIndex));
                return bRes;
            }

            while (tempDate <= dEndDate)
            {
                await PopulateEpgIndex(sNewIndex, EPG, tempDate);
                tempDate = tempDate.AddDays(1);
            }

            bool indexExists = m_oESApi.IndexExists(Utils.GetEpgGroupAliasStr(m_nGroupID));

            if (bSwitchIndex || !indexExists)
            {
                List<string> lOldIndices = m_oESApi.GetAliases(sGroupAlias);

                bRes = await Task<bool>.Factory.StartNew(() => m_oESApi.SwitchIndex(sNewIndex, sGroupAlias, lOldIndices, null));

                if (bRes && lOldIndices.Count > 0)
                {
                    await Task.Factory.StartNew(() => m_oESApi.DeleteIndices(lOldIndices));
                }
            }

            return bRes;
        }


        private void GetAnalyzers(List<ApiObjects.LanguageObj> lLanguages, out List<string> lAnalyzers, out List<string> lFilters, out List<string> tokenizers)
        {
            lAnalyzers = new List<string>();
            lFilters = new List<string>();
            tokenizers = new List<string>();

            if (lLanguages != null)
            {
                foreach (ApiObjects.LanguageObj language in lLanguages)
                {
                    string analyzer = ElasticSearchApi.GetAnalyzerDefinition(ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code, string.Empty));
                    string filter = ElasticSearchApi.GetFilterDefinition(ElasticSearch.Common.Utils.GetLangCodeFilterKey(language.Code, string.Empty));
                    string tokenizer = ElasticSearchApi.GetTokenizerDefinition(ElasticSearch.Common.Utils.GetLangCodeTokenizerKey(language.Code, string.Empty));

                    if (string.IsNullOrEmpty(analyzer))
                    {
                        log.ErrorFormat("Error - " + string.Format("analyzer for language {0} doesn't exist", language.Code) + " ESFeeder");
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

        protected async Task PopulateEpgIndex(string sIndex, string sType, DateTime dDate)
        {
            Dictionary<ulong, EpgCB> programs = await GetEpgPrograms(m_nGroupID, dDate, 0);

            List<KeyValuePair<ulong, string>> lEpgObject = new List<KeyValuePair<ulong, string>>();
            foreach (ulong epgID in programs.Keys)
            {
                EpgCB oEpg = programs[epgID];

                if (oEpg != null)
                {
                    string sEpgObj = m_oESSerializer.SerializeEpgObject(oEpg);
                    lEpgObject.Add(new KeyValuePair<ulong, string>(oEpg.EpgID, sEpgObj));
                }

                if (lEpgObject.Count >= 50)
                {
                    m_oESApi.CreateBulkIndexRequest(sIndex, sType, lEpgObject);

                    lEpgObject = new List<KeyValuePair<ulong, string>>();
                }
            }

            if (lEpgObject.Count > 0)
            {

                m_oESApi.CreateBulkIndexRequest(sIndex, sType, lEpgObject);
            }
        }

        protected async Task<Dictionary<ulong, EpgCB>> GetEpgPrograms(int nGroupID, DateTime? dDateTime, int nEpgID)
        {
            Dictionary<ulong, EpgCB> epgs = new Dictionary<ulong, EpgCB>();

            //Get All programs by group_id + date from CB
            TvinciEpgBL oEpgBL = new TvinciEpgBL(nGroupID);
            List<EpgCB> lEpgCB = await Task.Factory.StartNew(() => oEpgBL.GetGroupEpgs(0, 0, dDateTime, dDateTime.Value.AddDays(1)));

            if (lEpgCB != null && lEpgCB.Count > 0)
            {
                foreach (EpgCB epg in lEpgCB)
                {
                    epgs.Add(epg.EpgID, epg);
                }
            }

            return epgs;
        }
    }
}
