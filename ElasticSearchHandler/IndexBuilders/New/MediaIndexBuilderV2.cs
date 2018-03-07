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
            
            MappingAnalyzers defaultMappingAnalyzers = GetMappingAnalyzers(defaultLanguage, VERSION);

            // Mapping for each language
            foreach (ApiObjects.LanguageObj language in languages)
            {
                string type = MEDIA;

                if (!language.IsDefault)
                {
                    type = string.Concat(MEDIA, "_", language.Code);
                }

                MappingAnalyzers specificMappingAnalyzers = GetMappingAnalyzers(language, VERSION);

                #region Join tags and metas of EPG and media to same mapping

                List<string> tags = new List<string>();
                Dictionary<string, KeyValuePair<eESFieldType, string>> metas = new Dictionary<string, KeyValuePair<eESFieldType, string>>();
                // Check if group supports Templates
                if (doesGroupUsesTemplates)
                {
                    try
                    {
                        HashSet<string> topicsToIgnore = Core.Catalog.CatalogLogic.GetTopicsToIgnoreOnBuildIndex();
                        tags = catalogGroupCache.TopicsMapBySystemName.Where(x => x.Value.Type == ApiObjects.MetaType.Tag && !topicsToIgnore.Contains(x.Value.SystemName)).Select(x => x.Key).ToList();
                        foreach (Topic topic in catalogGroupCache.TopicsMapBySystemName.Where(x => x.Value.Type != ApiObjects.MetaType.Tag && !topicsToIgnore.Contains(x.Value.SystemName)).Select(x => x.Value))
                        {

                            string nullValue;
                            eESFieldType metaType;
                            serializer.GetMetaType(topic.Type, out metaType, out nullValue);
                            metas.Add(topic.SystemName, new KeyValuePair<eESFieldType, string>(metaType, nullValue));
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Failed BuildIndex for groupId: {0} because CatalogGroupCache", groupId), ex);
                        return false;
                    }
                }
                else
                {
                    if (group.m_oGroupTags != null)
                    {
                        tags.AddRange(group.m_oGroupTags.Values);
                    }

                    if (group.m_oEpgGroupSettings != null && group.m_oEpgGroupSettings.m_lTagsName != null)
                    {
                        foreach (var item in group.m_oEpgGroupSettings.m_lTagsName)
                        {
                            if (!tags.Contains(item))
                            {
                                tags.Add(item);
                            }
                        }
                    }

                    if (group.m_oMetasValuesByGroupId != null)
                    {
                        foreach (Dictionary<string, string> metaMap in group.m_oMetasValuesByGroupId.Values)
                        {
                            foreach (KeyValuePair<string, string> meta in metaMap)
                            {
                                string nullValue;
                                eESFieldType metaType;
                                serializer.GetMetaType(meta.Key, out metaType, out nullValue);
                                metas.Add(meta.Value, new KeyValuePair<eESFieldType, string>(metaType, nullValue));
                            }
                        }
                    }

                    if (group.m_oEpgGroupSettings != null && group.m_oEpgGroupSettings.m_lMetasName != null)
                    {
                        foreach (string epgMeta in group.m_oEpgGroupSettings.m_lMetasName)
                        {
                            string nullValue;
                            eESFieldType metaType;
                            serializer.GetMetaType(epgMeta, out metaType, out nullValue);
                            metas.Add(epgMeta, new KeyValuePair<eESFieldType, string>(metaType, nullValue));
                        }
                    }
                }

                #endregion

                // Ask serializer to create the mapping definitions string
                string mapping = serializer.CreateMediaMapping(metas, tags, specificMappingAnalyzers, defaultMappingAnalyzers);

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
                        ApiObjects.LanguageObj language = doesGroupUsesTemplates ? catalogGroupCache.LanguageMapById[languageId] : group.GetLanguage(languageId);
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
                            Task t = Task.Run(() => 
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
                    Task t = Task.Run(() =>
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
