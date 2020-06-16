using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElasticSearch.Common;
using ElasticsearchTasksCommon;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using System.Data;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using KlogMonitorHelper;
using Core.Catalog.CatalogManagement;
using ConfigurationManager;
using Core.Catalog;

namespace ElasticSearchHandler.IndexBuilders
{
    public class MediaIndexBuilderV2 : AbstractIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts

        // Basic TCM configurations for indexing - number of shards/replicas, size of bulks 
        int numOfShards = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.NumberOfShards.Value;
        int numOfReplicas = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.NumberOfReplicas.Value;
        int sizeOfBulk = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.BulkSize.Value;
        int maxResults = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;
        long mediaPageSize = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.MediaPageSize.Value;

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

            // Default size of ES bulk request size
            sizeOfBulk = sizeOfBulk == 0 ? 1000 : sizeOfBulk;
            // Default size of epg cb bulk size
            mediaPageSize = mediaPageSize == 0 ? 1000 : mediaPageSize;

            // Default size of max results should be 100,000
            if (maxResults == 0)
            {
                maxResults = 100000;
            }

            CatalogGroupCache catalogGroupCache = null;
            Group group = null;
            GroupManager groupManager = new GroupManager();
            List<ApiObjects.LanguageObj> languages = null;
            ApiObjects.LanguageObj defaultLanguage = null;
            bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
            if (doesGroupUsesTemplates)
            {
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling BuildIndex", groupId);
                    return false;
                }

                languages = catalogGroupCache.LanguageMapById.Values.ToList();
                defaultLanguage = catalogGroupCache.DefaultLanguage;
            }
            else
            {
                groupManager.RemoveGroup(groupId);
                group = groupManager.GetGroup(groupId);
                if (group == null)
                {
                    log.ErrorFormat("Could not load group {0} in media index builder", groupId);
                    return false;
                }

                languages = group.GetLangauges();
                defaultLanguage = group.GetGroupDefaultLanguage();
            }

            List<string> analyzers;
            List<string> filters;
            List<string> tokenizers;

            // get definitions of analyzers, filters and tokenizers
            GetAnalyzers(languages, out analyzers, out filters, out tokenizers);

            bool actionResult = api.BuildIndex(newIndexName, numOfShards, numOfReplicas, analyzers, filters, tokenizers, maxResults);

            if (!actionResult)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return actionResult;
            }

            #endregion

            #region create mapping            
            
            MappingAnalyzers defaultMappingAnalyzers = IndexManager.GetMappingAnalyzers(defaultLanguage, VERSION);

            HashSet<string> metasToPad = null;

            if (!IndexManager.GetMetasAndTagsForMapping(groupId, doesGroupUsesTemplates, out Dictionary<string, KeyValuePair<eESFieldType, string>> metas, out List<string> tags,
                out metasToPad, serializer, group, catalogGroupCache))
            {
                log.Error("Failed GetMetasAndTagsForMapping as part of BuildIndex");
                return false;
            }

            MetasToPad = metasToPad;

            // Mapping for each language
            foreach (ApiObjects.LanguageObj language in languages)
            {
                string type = MEDIA;

                if (!language.IsDefault)
                {
                    type = string.Concat(MEDIA, "_", language.Code);
                }

                MappingAnalyzers specificMappingAnalyzers = IndexManager.GetMappingAnalyzers(language, VERSION);
                
                // Ask serializer to create the mapping definitions string
                string mapping = serializer.CreateMediaMapping(metas, tags, metasToPad, specificMappingAnalyzers, defaultMappingAnalyzers);
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

            log.DebugFormat("Start GetGroupMediasTotal for group {0}", groupId);

            if (doesGroupUsesTemplates)
            {
                Dictionary<int, Dictionary<int, Media>> groupMedias;
                long nextId = 0;

                while (true)
                {
                    System.Collections.Concurrent.ConcurrentDictionary<int, Dictionary<int, Media>> opcGroupMedias = ElasticsearchTasksCommon.Utils.GetGroupMediasTotalForOPCAccount(groupId, 0, nextId, mediaPageSize);
                    if (opcGroupMedias == null || opcGroupMedias.Count == 0)
                        break;

                    groupMedias = opcGroupMedias.ToDictionary(x => x.Key, x => x.Value);
                    InsertMedias(cd, groupMedias, catalogGroupCache, doesGroupUsesTemplates, group, newIndexName, sizeOfBulk);
                    var nextNextId = groupMedias.Max(x => x.Key);
                    if (nextId == nextNextId)
                        break;

                    nextId = nextNextId;
                }
            }
            else
            {
                // Get ALL media in group
                Dictionary<int, Dictionary<int, Media>> groupMedias = ElasticsearchTasksCommon.Utils.GetGroupMediasTotal(groupId, 0);
                InsertMedias(cd, groupMedias, catalogGroupCache, doesGroupUsesTemplates, group, newIndexName, sizeOfBulk);
            }

            #endregion

            #region insert channel queries

            HashSet<string> channelsToRemove;
            HashSet<int> channelIds = new HashSet<int>();
            if (!doesGroupUsesTemplates)
            {
                channelIds = group.channelIDs;
            }

            ChannelIndexBuilderV2.BuildChannelQueries(groupId, api, ref channelIds, newIndexName, out channelsToRemove, doesGroupUsesTemplates);
            
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
                    Task t = Task.Run(() =>
                    {
                        cd.Load();
                        api.DeleteIndices(oldIndices);
                    });

                    t.Wait();
                }
            }

            return true;
        }

        private void InsertMedias(ContextData cd, Dictionary<int, Dictionary<int, Media>> groupMedias, CatalogGroupCache catalogGroupCache, bool doesGroupUsesTemplates, Group group, string newIndexName, int sizeOfBulk)
        {
            if (groupMedias != null)
            {
                log.DebugFormat("Start indexing medias. total medias={0}", groupMedias.Count);
                // save current value to restore at the end
                int currentDefaultConnectionLimit = System.Net.ServicePointManager.DefaultConnectionLimit;
                try
                {
                    int numOfBulkRequests = 0;

                    Dictionary<int, List<ESBulkRequestObj<int>>> bulkRequests = new Dictionary<int, List<ESBulkRequestObj<int>>>() { { numOfBulkRequests, new List<ESBulkRequestObj<int>>() } };

                    // For each media
                    foreach (var groupMedia in groupMedias)
                    {
                        var mediaId = groupMedia.Key;

                        // For each language
                        foreach (int languageId in groupMedia.Value.Keys)
                        {
                            ApiObjects.LanguageObj language = doesGroupUsesTemplates ? catalogGroupCache.LanguageMapById[languageId] : group.GetLanguage(languageId);
                            string suffix = null;

                            if (!language.IsDefault)
                            {
                                suffix = language.Code;
                            }

                            Media media = groupMedia.Value[languageId];

                            if (media != null)
                            {
                                media.PadMetas(MetasToPad);

                                // Serialize media and create a bulk request for it
                                string serializedMedia = serializer.SerializeMediaObject(media, suffix);
                                string documentType = ElasticSearchTaskUtils.GetTanslationType(MEDIA, language);

                                // If we exceeded the size of a single bulk reuquest then create another list
                                if (bulkRequests[numOfBulkRequests].Count >= sizeOfBulk)
                                {
                                    numOfBulkRequests++;
                                    bulkRequests.Add(numOfBulkRequests, new List<ESBulkRequestObj<int>>());
                                }

                                ESBulkRequestObj<int> bulkRequest = new ESBulkRequestObj<int>(media.m_nMediaID, newIndexName, documentType, serializedMedia);
                                bulkRequests[numOfBulkRequests].Add(bulkRequest);
                            }
                        }
                    }
                    
                    System.Net.ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount;
                    System.Collections.Concurrent.ConcurrentBag<List<ESBulkRequestObj<int>>> failedBulkRequests = new System.Collections.Concurrent.ConcurrentBag<List<ESBulkRequestObj<int>>>();
                    // Send request to elastic search in a different thread
                    System.Threading.Tasks.Parallel.ForEach(bulkRequests, (bulkRequest, state) =>
                    {
                        List<ESBulkRequestObj<int>> invalidResults;
                        bool bulkResult = api.CreateBulkRequests(bulkRequest.Value, out invalidResults);

                        // Log invalid results
                        if (!bulkResult && invalidResults != null && invalidResults.Count > 0)
                        {
                            log.Warn($"Bulk request when indexing media for partner {groupId} has invalid results. Will retry soon.");
                            // add entire failed retry requests to failedBulkRequests, will try again not in parallel (maybe ES is loaded)
                            failedBulkRequests.Add(invalidResults);                            
                        }
                    });

                    // retry on all failed bulk requests (this time not in parallel)
                    if (failedBulkRequests.Count > 0)
                    {
                        foreach (List<ESBulkRequestObj<int>> bulkRequest in failedBulkRequests)
                        {
                            List<ESBulkRequestObj<int>> invalidResults;
                            bool bulkResult = api.CreateBulkRequests(bulkRequest, out invalidResults);

                            // Log invalid results
                            if (!bulkResult && invalidResults != null && invalidResults.Count > 0)
                            {
                                foreach (var item in invalidResults)
                                {
                                    log.ErrorFormat(
                                        "Error - Could not add Media to ES index, additional retry will not be attempted. GroupID={0};Type={1};ID={2};error={3};", 
                                        groupId, MEDIA, item.docID, item.error);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    log.Error("Failed during InsertMedias", ex);
                }
                finally
                {
                    System.Net.ServicePointManager.DefaultConnectionLimit = currentDefaultConnectionLimit;
                }
            }
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private void GetAnalyzers(List<ApiObjects.LanguageObj> languages, out List<string> analyzers, out List<string> filters, out List<string> tokenizers)
        {
            analyzers = new List<string>();
            filters = new List<string>();
            tokenizers = new List<string>();

            if (languages != null)
            {
                foreach (ApiObjects.LanguageObj language in languages)
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
                        analyzers.Add(analyzer);
                    }

                    if (!string.IsNullOrEmpty(filter))
                    {
                        filters.Add(filter);
                    }

                    if (!string.IsNullOrEmpty(tokenizer))
                    {
                        tokenizers.Add(tokenizer);
                    }
                }

                // we always want a lowercase analyzer
                analyzers.Add(LOWERCASE_ANALYZER);

                // we always want "autocomplete" ability
                filters.Add(PHRASE_STARTS_WITH_FILTER);
                analyzers.Add(PHRASE_STARTS_WITH_ANALYZER);
                analyzers.Add(PHRASE_STARTS_WITH_SEARCH_ANALYZER);
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
