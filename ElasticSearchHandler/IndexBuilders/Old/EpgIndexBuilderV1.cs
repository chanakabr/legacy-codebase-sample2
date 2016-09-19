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
using ElasticSearch.Searcher;
using ApiObjects.SearchObjects;
using ApiObjects.Response;
using System.Data;

namespace ElasticSearchHandler.IndexBuilders
{
    public class EpgIndexBuilderV1 : AbstractIndexBuilder
    {
        private static readonly string EPG = "epg";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Data Members

        int sizeOfBulk;

        #endregion

        #region Ctor

        public EpgIndexBuilderV1(int groupID)
            : base(groupID)
        {
            serializer = new ESSerializerV1();
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
            {
                log.ErrorFormat("Couldn't load group in cache when building index for group {0}", groupId);
                return success;
            }

            // If request doesn't have start date, use [NOW - 7 days] as default
            if (!this.StartDate.HasValue)
            {
                this.StartDate = DateTime.UtcNow.Date.AddDays(-7);
            }

            // If request doesn't have end date, use [NOW + 7 days] as default
            if (!this.EndDate.HasValue)
            {
                this.EndDate = DateTime.UtcNow.Date.AddDays(7);
            }

            string groupAlias = GetAlias();
            string newIndexName = GetNewIndexName();

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
                    log.Error(string.Format("could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead", language.Code));
                }

                string baseType = GetIndexType();

                string sMapping = serializer.CreateEpgMapping(group.m_oEpgGroupSettings.m_lMetasName, group.m_oEpgGroupSettings.m_lTagsName, indexAnalyzer, searchAnalyzer,
                    baseType, autocompleteIndexAnalyzer, autocompleteSearchAnalyzer);
                string specificType = GetIndexType(language);
                bool bMappingRes = api.InsertMapping(newIndexName, specificType, sMapping.ToString());

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

            PopulateIndex(newIndexName, group);

            #region insert channel queries

            InsertChannelsQueries(groupManager, group, newIndexName);

            #endregion

            #region Switch Index

            log.DebugFormat("Finished populating epg index = {0}", newIndexName);

            string originalIndex = GetAlias();
            bool indexExists = api.IndexExists(originalIndex);

            if (this.SwitchIndexAlias || !indexExists)
            {
                List<string> oldIndices = api.GetAliases(groupAlias);

                success = api.SwitchIndex(newIndexName, groupAlias, oldIndices, null);

                if (!success)
                {
                    log.ErrorFormat("Failed switching index for new index name = {0}, group alias = {1}", newIndexName, groupAlias);
                }

                if (this.DeleteOldIndices && success && oldIndices.Count > 0)
                {
                    api.DeleteIndices(oldIndices);
                }
            }

            #endregion

            return success;
        }

        #endregion

        #region Private and protected Methods

        protected virtual void InsertChannelsQueries(GroupManager groupManager, Group group, string newIndexName)
        {
            if (group.channelIDs != null)
            {
                log.Info(string.Format("Start indexing channels. total channels={0}", group.channelIDs.Count));

                List<KeyValuePair<int, string>> channelRequests = new List<KeyValuePair<int, string>>();
                try
                {
                    List<Channel> allChannels = groupManager.GetChannels(group.channelIDs.ToList(), groupId);

                    var unifiedQueryBuilder = new ESUnifiedQueryBuilder(null, groupId);

                    foreach (Channel currentChannel in allChannels)
                    {
                        if (currentChannel == null || currentChannel.m_nIsActive != 1)
                            continue;

                        string channelQuery = string.Empty;

                        if (currentChannel.m_nChannelTypeID == (int)ChannelType.KSQL)
                        {
                            // Only if it this channel is relevant to EPG, build its query
                            if (currentChannel.m_nMediaType.Count(type => type != Channel.EPG_ASSET_TYPE) > 0)
                            {
                                UnifiedSearchDefinitions definitions = ElasticsearchTasksCommon.Utils.BuildSearchDefinitions(currentChannel, false);

                                unifiedQueryBuilder.SearchDefinitions = definitions;
                                channelQuery = unifiedQueryBuilder.BuildSearchQueryString();
                            }
                        }

                        if (!string.IsNullOrEmpty(channelQuery))
                        {
                            channelRequests.Add(new KeyValuePair<int, string>(currentChannel.m_nChannelID, channelQuery));

                            if (channelRequests.Count > 50)
                            {
                                api.CreateBulkIndexRequest("_percolator", newIndexName, channelRequests);
                                channelRequests.Clear();
                            }
                        }
                    }

                    if (channelRequests.Count > 0)
                    {
                        api.CreateBulkIndexRequest("_percolator", newIndexName, channelRequests);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Caught exception while indexing channels. Ex={0};Stack={1}", ex.Message, ex.StackTrace));
                }
            }
        }

        protected virtual string GetIndexType()
        {
            return EPG;
        }

        protected virtual string GetIndexType(ApiObjects.LanguageObj language)
        {
            return (language.IsDefault) ? EPG : string.Concat(EPG, "_", language.Code);
        }

        protected virtual string GetAlias()
        {
            return ElasticSearchTaskUtils.GetEpgGroupAliasStr(groupId);
        }

        protected virtual void PopulateIndex(string newIndexName, GroupsCacheManager.Group group)
        {
            DateTime tempDate = StartDate.Value;

            while (tempDate <= this.EndDate.Value)
            {
                PopulateEpgIndex(newIndexName, EPG, tempDate, group);
                tempDate = tempDate.AddDays(1);
            }
        }

        protected virtual string SerializeEPGObject(EpgCB epg)
        {
            return serializer.SerializeEpgObject(epg);
        }

        protected virtual string GetNewIndexName()
        {
            return ElasticSearchTaskUtils.GetNewEpgIndexStr(groupId);
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

        protected void PopulateEpgIndex(string index, string type, DateTime date, GroupsCacheManager.Group group)
        {
            try
            {
                // Get EPG objects from CB
                Dictionary<ulong, Dictionary<string, EpgCB>> programs = GetEpgPrograms(groupId, date);

                AddEPGsToIndex(index, type, programs, group);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed when populating epg index. index = {0}, type = {1}, date = {2}, message = {3}, st = {4}",
                    index, type, date, ex.Message, ex.StackTrace);

                throw ex;
            }
        }

        protected virtual void AddEPGsToIndex(string index, string type, Dictionary<ulong, Dictionary<string, EpgCB>> programs, Group group)
        {
            // Basic validation
            if (programs == null)
            {
                log.ErrorFormat("AddEPGsToIndex {0}/{1} for group {2}: programs is null!", index, type, this.groupId);
                return;
            }

            List<KeyValuePair<ulong, string>> epgList = new List<KeyValuePair<ulong, string>>();

            // GetLinear Channel Values 
            var programsList = new List<EpgCB>();

            foreach (var programsValues in programs.Values)
            {
                programsList.AddRange(programsValues.Values);
            }

            ElasticSearchTaskUtils.GetLinearChannelValues(programsList, groupId);
            
            List<ESBulkRequestObj<int>> bulkList = new List<ESBulkRequestObj<int>>();

            // Run on all programs
            foreach (ulong epgID in programs.Keys)
            {
                foreach (var languageCode in programs[epgID].Keys)
                {
                    EpgCB epg = programs[epgID][languageCode];

                    var language = group.GetLanguage(languageCode);
                    
                    if (language == null)
                    {
                        language = group.GetGroupDefaultLanguage();
                    }

                    if (epg != null)
                    {
                        // Serialize EPG object to string
                        string serializedEpg = SerializeEPGObject(epg);
                        string epgType = ElasticSearchTaskUtils.GetTanslationType(type, language);

                        bulkList.Add(new ESBulkRequestObj<int>()
                              {
                                  docID = (int)epgID,
                                  index = index,
                                  type = epgType,
                                  document = serializedEpg,
                                  Operation = eOperation.index
                              });
                    }
                    if (bulkList.Count >= sizeOfBulk)
                    {
                        Task<object> t = Task<object>.Factory.StartNew(() => api.CreateBulkRequest(bulkList));
                        t.Wait();
                        bulkList = new List<ESBulkRequestObj<int>>();
                    }
                }
            }
            // If we have anything left that is less than the size of the bulk
            if (bulkList.Count > 0)
            {
                Task<object> t = Task<object>.Factory.StartNew(() => api.CreateBulkRequest(bulkList));
                t.Wait();
                bulkList = new List<ESBulkRequestObj<int>>();
            }

        }

        protected virtual ulong GetDocumentId(ulong epgId)
        {
            return epgId;
        }

        

        #endregion
    }
}
