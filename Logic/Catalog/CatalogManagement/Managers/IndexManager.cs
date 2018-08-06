using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Catalog.Cache;
using Core.Catalog.Request;
using Core.Catalog.Response;
using ElasticSearch.Common;
using ElasticSearch.Common.DeleteResults;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Tvinci.Core.DAL;

namespace Core.Catalog.CatalogManagement
{
    public class IndexManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string MEDIA = "media";
        private const string CHANNEL = "channel";
        private const string EPG = "epg";
        private const string PERCOLATOR = ".percolator";

        #region Public Methods

        public static Dictionary<int, Dictionary<int, Media>> GetGroupMedias(int groupId, int mediaId)
        {
            //dictionary contains medias such that first key is media_id, which returns a dictionary with a key language_id and value Media object.
            //E.g. mediaTranslations[123][2] --> will return media 123 of the hebrew language
            Dictionary<int, Dictionary<int, Media>> mediaTranslations = new Dictionary<int, Dictionary<int, Media>>();

            //temporary media dictionary
            Dictionary<int, Media> medias = new Dictionary<int, Media>();

            try
            {

                if (Core.Catalog.CatalogManagement.CatalogManager.DoesGroupUsesTemplates(groupId))
                {
                    return Core.Catalog.CatalogManagement.AssetManager.GetMediaForElasticSearchIndex(groupId, mediaId);
                }

                GroupManager groupManager = new GroupManager();
                Group group = groupManager.GetGroup(groupId);
                if (group == null)
                {
                    log.Error("Error - Could not load group from cache in GetGroupMedias");
                    return mediaTranslations;
                }

                ApiObjects.LanguageObj defaultLangauge = group.GetGroupDefaultLanguage();
                if (defaultLangauge == null)
                {
                    log.Error("Error - Could not get group default language from cache in GetGroupMedias");
                    return mediaTranslations;
                }

                ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_GroupMedias_ml");
                storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
                storedProcedure.AddParameter("@GroupID", groupId);
                storedProcedure.AddParameter("@MediaID", mediaId);

                DataSet dataSet = storedProcedure.ExecuteDataSet();
                //Task<DataSet> dataSetTask = Task<DataSet>.Factory.StartNew(() => storedProcedure.ExecuteDataSet());
                //dataSetTask.Wait();
                //DataSet dataSet = dataSetTask.Result;

                Core.Catalog.Utils.BuildMediaFromDataSet(ref mediaTranslations, ref medias, group, dataSet);

                // get media update dates
                DataTable updateDates = CatalogDAL.Get_MediaUpdateDate(new List<int>() { (int)mediaId });
            }
            catch (Exception ex)
            {
                log.Error("Media Exception", ex);
            }

            return mediaTranslations;
        }

        public static bool UpsertMedia(int groupId, int assetId)
        {
            bool result = false;
            ElasticSearch.Common.ESSerializerV2 esSerializer = new ElasticSearch.Common.ESSerializerV2();
            ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

            if (assetId <= 0)
            {
                log.WarnFormat("Received media request of invalid media id {0} when calling UpsertMedia", assetId);
                return result;
            }
            
            Dictionary<int, LanguageObj> languagesMap = null;            
            bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
            if (doesGroupUsesTemplates)
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpsertMedia", groupId);
                    return false;
                }

                languagesMap = new Dictionary<int, LanguageObj>(catalogGroupCache.LanguageMapById);
            }
            else
            {
                GroupManager groupManager = new GroupManager();                
                Group group = groupManager.GetGroup(groupId);
                if (group == null)
                {
                    log.ErrorFormat("Could not load group {0} in upsertMedia", groupId);
                    return false;
                }
                List<LanguageObj> languages = group.GetLangauges();
                languagesMap = languages.ToDictionary(x => x.ID, x => x);
            }


            try
            {
                //Create Media Object
                Dictionary<int, Dictionary<int, Media>> mediaDictionary = GetGroupMedias(groupId, assetId);
                if (mediaDictionary != null && mediaDictionary.Count > 0 && mediaDictionary.ContainsKey(assetId))
                {
                    foreach (int languageId in mediaDictionary[assetId].Keys)
                    {
                        LanguageObj language = languagesMap.ContainsKey(languageId) ? languagesMap[languageId] : null;
                        if (language != null)
                        {
                            string suffix = null;
                            if (!language.IsDefault)
                            {
                                suffix = language.Code;
                            }

                            Media media = mediaDictionary[assetId][languageId];
                            if (media != null)
                            {
                                string serializedMedia = esSerializer.SerializeMediaObject(media, suffix);
                                string type = GetTanslationType(MEDIA, language);
                                if (!string.IsNullOrEmpty(serializedMedia))
                                {
                                    result = esApi.InsertRecord(groupId.ToString(), type, media.m_nMediaID.ToString(), serializedMedia);
                                    if (!result)
                                    {
                                        log.Error("Error - " + string.Format("Could not update media in ES. GroupID={0};Type={1};MediaID={2};serializedObj={3};",
                                                                                groupId, type, media.m_nMediaID, serializedMedia));
                                    }
                                    // support for old invalidation keys
                                    else
                                    {
                                        LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(groupId, assetId));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Upsert Media threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
        }
        
        public static bool DeleteMedia(int groupId, int assetId)
        {
            bool result = false;
            string index = groupId.ToString();
            ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

            if (assetId <= 0)
            {
                log.WarnFormat("Received media request of invalid media id {0} when calling DeleteMedia", assetId);
                return result;
            }

            List<LanguageObj> languages = null;
            bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
            if (doesGroupUsesTemplates)
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteMedia", groupId);
                    return false;
                }

                languages = catalogGroupCache.LanguageMapById.Values.ToList();
            }
            else
            {
                GroupManager groupManager = new GroupManager();
                Group group = groupManager.GetGroup(groupId);
                if (group == null)
                {
                    log.ErrorFormat("Could not load group {0} in upsertMedia", groupId);
                    return false;
                }

                languages = group.GetLangauges();
            }

            try
            {
                if (languages != null && languages.Count > 0)
                {
                    result = true;

                    foreach (LanguageObj lang in languages)
                    {
                        string type = GetTanslationType(MEDIA, lang);
                        ESDeleteResult deleteResult = esApi.DeleteDoc(index, type, assetId.ToString());
                        result = deleteResult.Ok && result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Could not delete media from ES. Media id={0}, ex={1}", assetId, ex);
            }

            if (!result)
            {
                log.ErrorFormat("Delete media with id {0} failed", assetId);
            }
            // support for old invalidation keys
            else
            {
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(groupId, assetId));
            }

            return result;
        }

        public static bool UpsertChannel(int groupId, int channelId, Channel channel = null)
        {
            bool result = false;
            ElasticSearch.Common.ESSerializerV2 esSerializer = new ElasticSearch.Common.ESSerializerV2();
            ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

            if (channelId <= 0)
            {
                log.WarnFormat("Received channel request of invalid channel id {0} when calling UpsertChannel", channelId);
                return result;
            }

            try
            {
                if (channel == null)
                {
                    // isAllowedToViewInactiveAssets = true because only operator can cause upsert of channel
                    channel = ChannelManager.GetChannelById(groupId, channelId, true);
                    if (channel == null)
                    {
                        log.ErrorFormat("failed to get channel object for groupId: {0}, channelId: {1} when calling UpsertChannel", groupId, channelId);
                        return result;
                    }
                }

                string index = ElasticSearch.Common.Utils.GetGroupChannelIndex(groupId);
                string type = "channel";
                string serializedChannel = esSerializer.SerializeChannelObject(channel);
                if (esApi.InsertRecord(index, type, channelId.ToString(), serializedChannel))
                {
                    result = true;                    
                    if ((channel.m_nChannelTypeID != (int)ChannelType.Manual || (channel.m_lChannelTags != null && channel.m_lChannelTags.Count > 0))
                            && !UpdateChannelPercolator(groupId, new List<int>() { channelId }, channel))
                    {
                        log.ErrorFormat("Update channel percolator failed for Upsert Channel with channelId: {0}", channelId);                        
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Upsert Channel threw an exception. channelId: {0}, Exception={1};Stack={2}", channelId, ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Upsert channel with id {0} failed", channelId);
            }

            return result;
        }

        public static bool DeleteChannel(int groupId, int channelId)
        {
            bool result = false;            
            ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

            if (channelId <= 0)
            {
                log.WarnFormat("Received channel request of invalid channel id {0} when calling DeleteChannel", channelId);
                return result;
            }

            try
            {
                string index = ElasticSearch.Common.Utils.GetGroupChannelIndex(groupId);
                ESDeleteResult deleteResult = esApi.DeleteDoc(index, CHANNEL, channelId.ToString());
                if (deleteResult.Ok)
                {
                    result = true;
                    if (DeleteChannelPercolator(groupId, new List<int>() { channelId }))
                    {
                        log.ErrorFormat("Delete channel percolator failed for Delete Channel");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Delete Channel threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Delete channel with id {0} failed", channelId);
            }

            return result;
        }

        public static bool UpdateChannelPercolator(int groupId, List<int> channelIds, Channel channel = null)
        {
            bool result = false;
            ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            List<string> mediaAliases = esApi.GetAliases(groupId.ToString());
            List<string> epgAliases = esApi.GetAliases(string.Format("{0}_epg", groupId));
            try
            {
                if (mediaAliases != null && mediaAliases.Count > 0)
                {
                    if (channel != null)
                    {
                        result = UpdateChannelPercolator(esApi, channel, new List<int>() { groupId }, mediaAliases, epgAliases);
                    }
                    else
                    {
                        GroupManager groupManager = new GroupManager();
                        Group group = groupManager.GetGroup(groupId);

                        if (group == null || group.channelIDs == null || group.channelIDs.Count == 0)
                        {
                            return result;
                        }

                        result = true;
                        foreach (int channelId in channelIds)
                        {
                            Channel channelToUpdate = ChannelRepository.GetChannel(channelId, group);

                            if (channelToUpdate != null)
                            {
                                result = result && UpdateChannelPercolator(esApi, channelToUpdate, group.m_nSubGroup, mediaAliases, epgAliases);
                            }
                        }
                    }
                }

                // Set invalidation for the entire group
                string invalidationKey = LayeredCacheKeys.GetGroupChannelsInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to invalidate key: {0} after UpdateChannelPercolator", invalidationKey);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Update Channel Percolator threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Update Channel Percolator with ids {0} failed", channelIds != null && channelIds.Count > 0 ? string.Join(",", channelIds) : string.Empty);
            }

            return result;
        }

        public static bool DeleteChannelPercolator(int groupId, List<int> channelIds)
        {
            bool result = false;
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearchApi();
            string mediaIndex = groupId.ToString();
            string epgIndex = string.Format("{0}_epg", groupId);
            ESDeleteResult deleteResult;

            try
            {
                bool epgExists = esApi.IndexExists(epgIndex);
                List<string> mediaAliases = esApi.GetAliases(mediaIndex);
                List<string> epgAliases = null;

                if (epgExists)
                {
                    epgAliases = esApi.GetAliases(epgIndex);
                }

                // If we found aliases to both, or if we don't have EPG at all
                if (mediaAliases != null && epgAliases != null &&
                    (!epgExists || (mediaAliases.Count > 0 && epgAliases.Count > 0)))
                {
                    result = true;
                }

                if (mediaAliases != null && mediaAliases.Count > 0)
                {
                    foreach (int channelID in channelIds)
                    {
                        foreach (string index in mediaAliases)
                        {
                            deleteResult = esApi.DeleteDoc(index, PERCOLATOR, channelID.ToString());
                            result &= deleteResult.Ok;

                            if (!deleteResult.Ok)
                            {
                                log.Error("Error - " + string.Concat("Could not delete channel from elasticsearch. ID=", channelID));
                            }
                        }
                    }
                }
                else
                {
                    log.Error("Error - " + string.Concat("Could not find indices for alias ", mediaIndex));
                }

                if (epgAliases != null && epgAliases.Count > 0)
                {
                    foreach (int channelId in channelIds)
                    {
                        foreach (string index in epgAliases)
                        {
                            deleteResult = esApi.DeleteDoc(index, PERCOLATOR, channelId.ToString());
                            result &= deleteResult.Ok;

                            if (!deleteResult.Ok)
                            {
                                log.Error("Error - " + string.Concat("Could not delete channel from elasticsearch. ID=", channelId));
                            }
                        }
                    }
                }
                else
                {
                    log.Error("Error - " + string.Concat("Could not find indices for alias ", epgIndex));
                }

                // Set invalidation for the entire group
                string invalidationKey = LayeredCacheKeys.GetGroupChannelsInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to invalidate key: {0} after UpdateChannelPercolator", invalidationKey);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Delete Channel Percolator threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Delete Channel Percolator with ids {0} failed", channelIds != null && channelIds.Count > 0 ? string.Join(",", channelIds) : string.Empty);
            }

            return result;
        }

        public static List<EpgCB> GetEpgPrograms(int groupId, int epgId, List<string> languages, EpgBL.BaseEpgBL epgBL = null)
        {
            throw new NotImplementedException("GetEpgPrograms should be implemented to new TVM logic");
            //List<EpgCB> results = new List<EpgCB>();

            //// If no language was received - just get epg program by old method
            //if (languages == null || languages.Count == 0)
            //{
            //    EpgCB program = GetEpgProgram(groupId, epgId);

            //    results.Add(program);
            //}
            //else
            //{
            //    try
            //    {
            //        if (epgBL == null)
            //        {
            //            epgBL = EpgBL.Utils.GetInstance(groupId);
            //        }

            //        ulong uEpgID = (ulong)epgId;
            //        results = epgBL.GetEpgCB(uEpgID, languages);
            //    }
            //    catch (Exception ex)
            //    {
            //        log.Error("Error (GetEpgProgram) - " + string.Format("epg:{0}, msg:{1}, st:{2}", epgId, ex.Message, ex.StackTrace), ex);
            //    }
            //}

            //return results;
        }

        #endregion

        #region Private Methods

        private static string GetTanslationType(string type, LanguageObj language)
        {
            if (language.IsDefault)
            {
                return type;
            }
            else
            {
                return string.Concat(type, "_", language.Code);
            }
        }

        private static bool UpdateChannelPercolator(ElasticSearchApi esApi, Channel channel, List<int> subGroupIds, List<string> mediaAliases, List<string> epgAliases)
        {
            bool result = false;
            if (channel != null)
            {
                bool isMedia = false;
                bool isEpg = false;

                string channelQuery = string.Empty;

                if (channel.m_nChannelTypeID == (int)ChannelType.KSQL)
                {
                    UnifiedSearchDefinitions definitions = BuildSearchDefinitions(channel, true);

                    isMedia = definitions.shouldSearchMedia;
                    isEpg = definitions.shouldSearchEpg;

                    ESUnifiedQueryBuilder unifiedQueryBuilder = new ESUnifiedQueryBuilder(definitions);
                    channelQuery = unifiedQueryBuilder.BuildSearchQueryString(true);
                }
                else
                {
                    isMedia = true;
                    ESMediaQueryBuilder mediaQueryParser = new ESMediaQueryBuilder()
                    {
                        QueryType = eQueryType.EXACT
                    };

                    mediaQueryParser.m_nGroupID = channel.m_nGroupID;
                    MediaSearchObj mediaSearchObject = BuildBaseChannelSearchObject(channel, subGroupIds);

                    mediaQueryParser.oSearchObject = mediaSearchObject;
                    channelQuery = mediaQueryParser.BuildSearchQueryString(true);
                }

                log.DebugFormat("Update channel with query: {0}", channelQuery);

                if (isMedia)
                {
                    foreach (string alias in mediaAliases)
                    {
                        result = esApi.AddQueryToPercolatorV2(alias, channel.m_nChannelID.ToString(), ref channelQuery);
                    }
                }

                if (isEpg)
                {
                    foreach (string alias in epgAliases)
                    {
                        result = esApi.AddQueryToPercolatorV2(alias, channel.m_nChannelID.ToString(), ref channelQuery);
                    }
                }
            }

            return result;
        }

        private static UnifiedSearchDefinitions BuildSearchDefinitions(Channel channel, bool useMediaTypes)
        {
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();

            definitions.groupId = channel.m_nGroupID;

            if (useMediaTypes)
            {
                definitions.mediaTypes = new List<int>(channel.m_nMediaType);
            }

            if (channel.m_nMediaType != null)
            {
                // Nothing = all
                if (channel.m_nMediaType.Count == 0)
                {
                    definitions.shouldSearchEpg = true;
                    definitions.shouldSearchMedia = true;
                }
                else
                {
                    if (channel.m_nMediaType.Contains(Channel.EPG_ASSET_TYPE))
                    {
                        definitions.shouldSearchEpg = true;
                    }

                    // If there's anything besides EPG
                    if (channel.m_nMediaType.Count(type => type != Channel.EPG_ASSET_TYPE) > 0)
                    {
                        definitions.shouldSearchMedia = true;
                    }
                }
            }

            definitions.permittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID);
            definitions.order = new OrderObj();

            definitions.shouldUseStartDateForMedia = false;
            definitions.shouldUseFinalEndDate = false;

            if (!string.IsNullOrEmpty(channel.filterQuery))
            {
                BooleanPhraseNode filterTree = null;
                Status parseStatus = BooleanPhraseNode.ParseSearchExpression(channel.filterQuery, ref filterTree);

                if (parseStatus.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException(parseStatus.Message, parseStatus.Code);
                }
                else
                {
                    definitions.filterPhrase = filterTree;
                }

                GroupManager groupManager = new GroupManager();

                Group group = groupManager.GetGroup(channel.m_nParentGroupID);

                if (group == null)
                {
                    log.Error("Error - Could not load group from cache in GetGroupMedias");
                }

                BaseRequest dummyRequest = new BaseRequest()
                {
                    domainId = 0,
                    m_nGroupID = channel.m_nParentGroupID,
                    m_nPageIndex = 0,
                    m_nPageSize = 0,
                    m_oFilter = new Filter(),
                    m_sSiteGuid = string.Empty,
                    m_sUserIP = string.Empty
                };

                CatalogLogic.UpdateNodeTreeFields(dummyRequest,
                    ref definitions.filterPhrase, definitions, group, channel.m_nParentGroupID);
            }

            return definitions;
        }

        private static string GetPermittedWatchRules(int nGroupId, List<int> lSubGroup = null)
        {
            DataTable permittedWathRulesDt = CatalogDAL.GetPermittedWatchRulesByGroupId(nGroupId, lSubGroup);
            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
                {
                    lWatchRulesIds.Add(ODBCWrapper.Utils.GetSafeStr(permittedWatchRuleRow["RuleID"]));
                }
            }

            string sRules = string.Empty;

            if (lWatchRulesIds != null && lWatchRulesIds.Count > 0)
            {
                sRules = string.Join(" ", lWatchRulesIds);
            }

            return sRules;
        }

        private static MediaSearchObj BuildBaseChannelSearchObject(Channel channel, List<int> lSubGroups)
        {
            ApiObjects.SearchObjects.MediaSearchObj searchObject = new ApiObjects.SearchObjects.MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;

            if (channel.m_nMediaType != null)
            {
                searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType.Select(type => type.ToString()));
            }

            searchObject.m_sPermittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID, lSubGroups);
            searchObject.m_oOrder = new ApiObjects.SearchObjects.OrderObj();

            searchObject.m_bUseStartDate = false;
            searchObject.m_bUseFinalEndDate = false;

            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);

            // If it is a manual channel without media, make it an empty request
            if (channel.m_nChannelTypeID == (int)ChannelType.Manual &&
                (channel.m_lChannelTags == null || channel.m_lChannelTags.Count == 0))
            {
                searchObject.m_eCutWith = CutWith.AND;
                searchObject.m_eFilterTagsAndMetasCutWith = CutWith.AND;
                searchObject.m_lFilterTagsAndMetas = new List<SearchValue>()
                {
                    new SearchValue("media_id", "0")
                    {
                        m_eInnerCutWith = CutWith.AND,
                        m_lValue = new List<string>()
                        {
                            "0"
                        }
                    }
                };
            }

            return searchObject;
        }

        public static void CopySearchValuesToSearchObjects(ref MediaSearchObj searchObject, CutWith cutWith, List<SearchValue> channelSearchValues)
        {
            List<SearchValue> m_dAnd = new List<SearchValue>();
            List<SearchValue> m_dOr = new List<SearchValue>();

            SearchValue search = new SearchValue();
            if (channelSearchValues != null && channelSearchValues.Count > 0)
            {
                foreach (SearchValue searchValue in channelSearchValues)
                {
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        search = new SearchValue();
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
