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
using Core.Catalog.CatalogManagement;
using ConfigurationManager;

namespace ElasticSearchHandler.IndexBuilders
{
    public class MediaIndexBuilderV1 : AbstractIndexBuilder
    {
        private static readonly string MEDIA = "media";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public MediaIndexBuilderV1(int groupID)
            : base(groupID)
        {
            serializer = new ESSerializerV1();
        }

        #region Interface Methods

        public override bool BuildIndex()
        {
            string newIndexName = ElasticSearchTaskUtils.GetNewMediaIndexStr(groupId);

            #region Build new index and specify number of nodes/shards
            
            int numOfShards = ApplicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfShards.IntValue;
            int numOfReplicas = ApplicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfReplicas.IntValue; ;
            int sizeOfBulk = ApplicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.IntValue;
            
            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 50;
            }

            CatalogGroupCache catalogGroupCache = null;
            Group group = null;
            GroupManager groupManager = new GroupManager();
            List<ApiObjects.LanguageObj> languages = null;
            bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
            if (doesGroupUsesTemplates)
            {                
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling BuildIndex", groupId);
                    return false;
                }

                languages = catalogGroupCache.LanguageMapById.Values.ToList();
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
            }

            List<string> analyzers;
            List<string> filters;
            List<string> tokenizers;            
            GetAnalyzers(languages, out analyzers, out filters, out tokenizers);
            bool actionResult = api.BuildIndex(newIndexName, numOfShards, numOfReplicas, analyzers, filters, tokenizers);
            if (!actionResult)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return actionResult;
            }

            #endregion

            #region create mapping
            foreach (ApiObjects.LanguageObj language in languages)
            {
                MappingAnalyzers specificMappingAnlyzers = GetMappingAnalyzers(language, string.Empty);
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

                            if (!metas.ContainsKey(topic.SystemName))
                            {
                                metas.Add(topic.SystemName, new KeyValuePair<eESFieldType, string>(metaType, nullValue));
                            }
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
                    if (group.m_oMetasValuesByGroupId != null)
                    {
                        foreach (Dictionary<string, string> metaMap in group.m_oMetasValuesByGroupId.Values)
                        {
                            foreach (KeyValuePair<string, string> meta in metaMap)
                            {
                                string nullValue;
                                eESFieldType metaType;
                                serializer.GetMetaType(meta.Key, out metaType, out nullValue);

                                if (!metas.ContainsKey(meta.Value))
                                {
                                    metas.Add(meta.Value, new KeyValuePair<eESFieldType, string>(metaType, nullValue));
                                }
                            }
                        }
                    }

                    tags.AddRange(group.m_oGroupTags.Values);
                }

                string mapping = serializer.CreateMediaMapping(metas, tags, specificMappingAnlyzers, null);
                string type = (language.IsDefault) ? MEDIA : string.Concat(MEDIA, "_", language.Code);
                bool bMappingRes = api.InsertMapping(newIndexName, type, mapping.ToString());

                if (language.IsDefault && !bMappingRes)
                    actionResult = false;

                if (!bMappingRes)
                {
                    log.Error(string.Concat("Could not create mapping of type media for language ", language.Name));
                }

            }

            if (!actionResult)
                return actionResult;

            #endregion

            #region insert medias
            Dictionary<int, Dictionary<int, Media>> groupMedias = ElasticsearchTasksCommon.Utils.GetGroupMediasTotal(groupId, 0);

            if (groupMedias != null)
            {
                log.Debug("Info - " + string.Format("Start indexing medias. total medias={0}", groupMedias.Count));
                List<ESBulkRequestObj<int>> bulkList = new List<ESBulkRequestObj<int>>();

                foreach (int mediaId in groupMedias.Keys)
                {
                    foreach (int languageId in groupMedias[mediaId].Keys)
                    {
                        Media media = groupMedias[mediaId][languageId];

                        if (media != null)
                        {
                            string serializedMedia;

                            serializedMedia = serializer.SerializeMediaObject(media);
                            ApiObjects.LanguageObj language = doesGroupUsesTemplates ? catalogGroupCache.LanguageMapById[languageId] : group.GetLanguage(languageId);
                            string sType = ElasticSearchTaskUtils.GetTanslationType(MEDIA, language);

                            bulkList.Add(new ESBulkRequestObj<int>()
                            {
                                docID = media.m_nMediaID,
                                index = newIndexName,
                                type = sType,
                                document = serializedMedia
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

                if (bulkList.Count > 0)
                {
                    Task<object> t = Task<object>.Factory.StartNew(() => api.CreateBulkRequest(bulkList));
                    t.Wait();
                }
            }

            #endregion

            #region insert channel queries
            List<Channel> groupChannels = null;
            if (doesGroupUsesTemplates)
            {
                groupChannels = ChannelManager.GetGroupChannels(groupId);
            }
            else if (group.channelIDs != null)
            {
                groupChannels = groupManager.GetChannels(group.channelIDs.ToList(), groupId);
            }

            if (groupChannels != null)
            {
                log.Info(string.Format("Start indexing channels. total channels={0}", groupChannels.Count));

                List<KeyValuePair<int, string>> channelRequests = new List<KeyValuePair<int, string>>();
                try
                {
                    ESMediaQueryBuilder mediaQueryParser = new ESMediaQueryBuilder()
                    {
                        QueryType = eQueryType.EXACT
                    };
                    var unifiedQueryBuilder = new ESUnifiedQueryBuilder(null, groupId);

                    foreach (Channel currentChannel in groupChannels)
                    {
                        if (currentChannel == null)
                        {
                            log.ErrorFormat("BuildChannelQueries - All channels list has null or in-active channel, continuing");
                            continue;
                        }

                        // if group uses templates - index inactive channel as well
                        if (!doesGroupUsesTemplates && currentChannel.m_nIsActive != 1)
                        {
                            log.ErrorFormat("BuildChannelQueries - All channels list has null or in-active channel, continuing");
                            continue;
                        }

                        string channelQuery = string.Empty;

                        if (currentChannel.m_nChannelTypeID == (int)ChannelType.KSQL)
                        {
                            try
                            {
                                // If there is at least 1 media type, build its definitions
                                if (currentChannel.m_nMediaType == null || currentChannel.m_nMediaType.Count == 0 ||
                                    (currentChannel.m_nMediaType != null && currentChannel.m_nMediaType.Count > 0 &&
                                    currentChannel.m_nMediaType.Count(type => type != Channel.EPG_ASSET_TYPE) > 0))
                                {
                                    UnifiedSearchDefinitions definitions = ElasticsearchTasksCommon.Utils.BuildSearchDefinitions(currentChannel, true);

                                    unifiedQueryBuilder.SearchDefinitions = definitions;
                                    channelQuery = unifiedQueryBuilder.BuildSearchQueryString(true);
                                }
                            }
                            catch (KalturaException ex)
                            {
                                log.ErrorFormat("Tried to index an invalid KSQL Channel. ID = {0}, message = {1}, st = {2}", currentChannel.m_nChannelID, ex.Message, ex.StackTrace);
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat("Failed indexing KSQL channel. ID = {0}, message = {1}, message = {1}, st = {2}", currentChannel.m_nChannelID, ex.Message, ex.StackTrace);
                            }
                        }
                        else
                        {
                            try
                            {
                                mediaQueryParser.m_nGroupID = currentChannel.m_nGroupID;
                                MediaSearchObj mediaSearchObject = BuildBaseChannelSearchObject(currentChannel);

                                mediaQueryParser.oSearchObject = mediaSearchObject;
                                channelQuery = mediaQueryParser.BuildSearchQueryString(true);
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat("Failed indexing automatic/manual channel. ID = {0}, message = {1}", currentChannel.m_nChannelID, ex.Message, ex);
                            }
                        }

                        if (!string.IsNullOrEmpty(channelQuery))
                        {
                            channelRequests.Add(new KeyValuePair<int, string>(currentChannel.m_nChannelID, channelQuery));

                            if (channelRequests.Count > 50)
                            {
                                try
                                {
                                    api.CreateBulkIndexRequest("_percolator", newIndexName, channelRequests);
                                }
                                catch (Exception ex)
                                {
                                    log.ErrorFormat("Failed indexing channels in bulk. ex = {0}", ex);
                                }

                                channelRequests.Clear();
                            }
                        }
                    }

                    if (channelRequests.Count > 0)
                    {
                        try
                        {
                            api.CreateBulkIndexRequest("_percolator", newIndexName, channelRequests);
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Failed indexing channels in bulk. ex = {0}", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Caught exception while indexing channels. Ex={0};Stack={1}", ex.Message, ex.StackTrace));
                }
            }

            #endregion

            string alias = ElasticSearchTaskUtils.GetMediaGroupAliasStr(groupId);
            bool indexExists = api.IndexExists(alias);

            if (this.SwitchIndexAlias || !indexExists)
            {
                List<string> oldIndices = api.GetAliases(alias);

                Task<bool> taskSwitchIndex = Task<bool>.Factory.StartNew(() => api.SwitchIndex(newIndexName, alias, oldIndices));
                taskSwitchIndex.Wait();

                if (!taskSwitchIndex.Result)
                {
                    log.ErrorFormat("Failed switching index for new index name = {0}, group alias = {1}", newIndexName, alias);
                    actionResult = false;
                }

                if (this.DeleteOldIndices && taskSwitchIndex.Result && oldIndices.Count > 0)
                {
                    Task t = Task.Run(() => api.DeleteIndices(oldIndices));
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
