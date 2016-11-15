using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElasticSearch.Common;
using ElasticsearchTasksCommon;
using Catalog;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using System.Data;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using KlogMonitorHelper;

namespace ElasticSearchHandler.IndexBuilders
{
    public class MediaIndexBuilderV2 : AbstractIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts
        
        private static readonly string MEDIA = "media";
        protected const string VERSION = "2";

        #endregion

        #region Ctor

        public MediaIndexBuilderV2(int groupID)
            : base(groupID)
        {
            serializer = new ESSerializerV2();
        }

        #endregion

        #region Interface Methods

        public override bool BuildIndex()
        {
            ContextData cd = new ContextData();
            string newIndexName = ElasticSearchTaskUtils.GetNewMediaIndexStr(groupId);

            #region Build new index and specify number of nodes/shards

            // Basic TCM configurations for indexing - number of shards/replicas, size of bulks 
            int numOfShards = TVinciShared.WS_Utils.GetTcmIntValue("ES_NUM_OF_SHARDS");
            int numOfReplicas = TVinciShared.WS_Utils.GetTcmIntValue("ES_NUM_OF_REPLICAS");
            int sizeOfBulk = TVinciShared.WS_Utils.GetTcmIntValue("ES_BULK_SIZE");
            int maxResults = TVinciShared.WS_Utils.GetTcmIntValue("MAX_RESULTS");

            // Default for size of bulk should be 50, if not stated otherwise in TCM
            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 50;
            }

            // Default size of max results should be 100,000
            if (maxResults == 0)
            {
                maxResults = 100000;
            }

            GroupManager groupManager = new GroupManager();
            groupManager.RemoveGroup(groupId);
            Group group = groupManager.GetGroup(groupId);

            // Without the group we cannot advance at all - there must be an error in CB or something
            if (group == null)
            {
                log.ErrorFormat("Could not load group {0} in media index builder", groupId);
                return false;
            }

            List<string> analyzers;
            List<string> filters;
            List<string> tokenizers;

            // get definitions of analyzers, filters and tokenizers
            GetAnalyzers(group.GetLangauges(), out analyzers, out filters, out tokenizers);

            bool actionResult = api.BuildIndex(newIndexName, numOfShards, numOfReplicas, analyzers, filters, tokenizers, maxResults);

            if (!actionResult)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return actionResult;
            }

            #endregion

            #region create mapping

            var languages = group.GetLangauges();

            // Mapping for each language
            foreach (ApiObjects.LanguageObj language in languages)
            {
                string indexAnalyzer, searchAnalyzer;
                string autocompleteIndexAnalyzer = null;
                string autocompleteSearchAnalyzer = null;

                // create names for analyzers to be used in the mapping later on
                string analyzerDefinitionName = ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code, VERSION);

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

                string type = MEDIA;
                string suffix = null;

                if (!language.IsDefault)
                {
                    type = string.Concat(MEDIA, "_", language.Code);
                    suffix = language.Code;
                }

                // Ask serializer to create the mapping definitions string
                string mapping = serializer.CreateMediaMapping(
                    group.m_oMetasValuesByGroupId, group.m_oGroupTags,
                    indexAnalyzer, searchAnalyzer, autocompleteIndexAnalyzer, autocompleteSearchAnalyzer, suffix);      

                bool mappingResult = api.InsertMapping(newIndexName, type, mapping.ToString());

                // Most important is the mapping for the default language, we can live without the others...
                if (language.IsDefault && !mappingResult)
                {
                    actionResult = false;
                }

                if (!mappingResult)
                {
                    log.Error(string.Concat("Could not create mapping of type media for language ", language.Name));
                }
            }

            // If we didn't succeed up until now - don't continue
            if (!actionResult)
            {
                return actionResult;
            }

            #endregion

            #region insert medias

            // Get ALL media in group
            Dictionary<int, Dictionary<int, Media>> groupMedias = ElasticsearchTasksCommon.Utils.GetGroupMediasTotal(groupId, 0);

            if (groupMedias != null)
            {
                log.Debug("Info - " + string.Format("Start indexing medias. total medias={0}", groupMedias.Count));
                List<ESBulkRequestObj<int>> bulkList = new List<ESBulkRequestObj<int>>();

                // For each media
                foreach (var groupMedia in groupMedias)
                {
                    var mediaId = groupMedia.Key;
                    
                    // For each language
                    foreach (int languageId in groupMedia.Value.Keys)
                    {
                        var language = group.GetLanguage(languageId);
                        string suffix = null;

                        if (!language.IsDefault)
                        {
                            suffix = language.Code;
                        }

                        Media media = groupMedia.Value[languageId];

                        if (media != null)
                        {
                            // Serialize media and create a bulk request for it
                            string serializedMedia = serializer.SerializeMediaObject(media, suffix);

                            string documentType = ElasticSearchTaskUtils.GetTanslationType(MEDIA, language);

                            bulkList.Add(new ESBulkRequestObj<int>()
                            {
                                docID = media.m_nMediaID,
                                index = newIndexName,
                                type = documentType,
                                document = serializedMedia
                            });
                        }

                        // If we exceeded the size of a single bulk reuquest
                        if (bulkList.Count >= sizeOfBulk)
                        {
                            // Send request to elastic search in a different thread
                            Task t = Task.Factory.StartNew(() => 
                                {
                                    cd.Load();

                                    var invalidResults = api.CreateBulkRequest(bulkList);

                                    // Log invalid results
                                    if (invalidResults != null && invalidResults.Count > 0)
                                    {
                                        foreach (var item in invalidResults)
                                        {
                                            log.ErrorFormat("Error - Could not add Media to ES index. GroupID={0};Type={1};ID={2};error={3};",
                                                groupId, MEDIA, item.Key, item.Value);
                                        }
                                    }
                                });

                            t.Wait();
                            bulkList.Clear();
                        }
                    }
                }

                // If we have a final bulk pending
                if (bulkList.Count > 0)
                {
                    // Send request to elastic search in a different thread
                    Task t = Task.Factory.StartNew(() =>
                    {
                        cd.Load();
                        var invalidResults = api.CreateBulkRequest(bulkList);

                        if (invalidResults != null && invalidResults.Count > 0)
                        {
                            foreach (var item in invalidResults)
                            {
                                log.ErrorFormat("Error - Could not add Media to ES index. GroupID={0};Type={1};ID={2};error={3};",
                                    groupId, MEDIA, item.Key, item.Value);
                            }
                        }
                    });
                    t.Wait();
                }
            }

            #endregion

            #region insert channel queries

            HashSet<string> channelsToRemove; 
            ChannelIndexBuilderV2.BuildChannelQueries(groupId, api, group.channelIDs, newIndexName, out channelsToRemove);
            
            #endregion

            // Switch index alias + Delete old indices handling

            string alias = ElasticSearchTaskUtils.GetMediaGroupAliasStr(groupId);
            bool indexExists = api.IndexExists(alias);

            if (this.SwitchIndexAlias || !indexExists)
            {
                List<string> oldIndices = api.GetAliases(alias);

                Task<bool> taskSwitchIndex = Task<bool>.Factory.StartNew(() =>
                {
                    cd.Load();
                    return api.SwitchIndex(newIndexName, alias, oldIndices);
                });

                taskSwitchIndex.Wait();

                if (!taskSwitchIndex.Result)
                {
                    log.ErrorFormat("Failed switching index for new index name = {0}, group alias = {1}", newIndexName, alias);
                    actionResult = false;
                }

                if (this.DeleteOldIndices && taskSwitchIndex.Result && oldIndices.Count > 0)
                {
                    Task t = Task.Factory.StartNew(() =>
                    {
                        cd.Load();
                        api.DeleteIndices(oldIndices);
                    });

                    t.Wait();
                }
            }

            return true;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private void GetAnalyzers(List<ApiObjects.LanguageObj> lLanguages, out List<string> lAnalyzers, out List<string> lFilters, out List<string> tokenizers)
        {
            lAnalyzers = new List<string>();
            lFilters = new List<string>();
            tokenizers = new List<string>();

            if (lLanguages != null)
            {
                foreach (ApiObjects.LanguageObj language in lLanguages)
                {
                    string analyzer = ElasticSearchApi.GetAnalyzerDefinition(ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code, VERSION));
                    string filter = ElasticSearchApi.GetFilterDefinition(ElasticSearch.Common.Utils.GetLangCodeFilterKey(language.Code, VERSION));
                    string tokenizer = ElasticSearchApi.GetTokenizerDefinition(ElasticSearch.Common.Utils.GetLangCodeTokenizerKey(language.Code, VERSION));

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

        private static ApiObjects.SearchObjects.MediaSearchObj BuildBaseChannelSearchObject(Channel channel)
        {
            ApiObjects.SearchObjects.MediaSearchObj searchObject = new ApiObjects.SearchObjects.MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;

            if (channel.m_nMediaType != null)
            {
                searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType.Select(type => type.ToString()));
            }

            searchObject.m_sPermittedWatchRules = ElasticsearchTasksCommon.Utils.GetPermittedWatchRules(channel.m_nGroupID);
            searchObject.m_oOrder = new ApiObjects.SearchObjects.OrderObj();

            searchObject.m_bUseStartDate = false;
            searchObject.m_bUseFinalEndDate = false;

            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);
            return searchObject;
        }

        private static void CopySearchValuesToSearchObjects(ref ApiObjects.SearchObjects.MediaSearchObj searchObject,
            ApiObjects.SearchObjects.CutWith cutWith, List<ApiObjects.SearchObjects.SearchValue> channelSearchValues)
        {
            List<ApiObjects.SearchObjects.SearchValue> m_dAnd = new List<ApiObjects.SearchObjects.SearchValue>();
            List<ApiObjects.SearchObjects.SearchValue> m_dOr = new List<ApiObjects.SearchObjects.SearchValue>();

            ApiObjects.SearchObjects.SearchValue search = new ApiObjects.SearchObjects.SearchValue();
            if (channelSearchValues != null && channelSearchValues.Count > 0)
            {
                foreach (ApiObjects.SearchObjects.SearchValue searchValue in channelSearchValues)
                {
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        search = new ApiObjects.SearchObjects.SearchValue();
                        search.m_sKey = searchValue.m_sKey;
                        search.m_lValue = searchValue.m_lValue;
                        search.m_sKeyPrefix = searchValue.m_sKeyPrefix;
                        search.m_eInnerCutWith = searchValue.m_eInnerCutWith;

                        switch (cutWith)
                        {
                            case ApiObjects.SearchObjects.CutWith.OR:
                                {
                                    m_dOr.Add(search);
                                    break;
                                }
                            case ApiObjects.SearchObjects.CutWith.AND:
                                {
                                    m_dAnd.Add(search);
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
            }

            if (m_dOr.Count > 0)
            {
                searchObject.m_dOr = m_dOr;
            }

            if (m_dAnd.Count > 0)
            {
                searchObject.m_dAnd = m_dAnd;
            }
        }

        #endregion
    }
}
