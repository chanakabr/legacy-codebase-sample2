using ApiObjects;
using ApiObjects.SearchObjects;
using AutoMapper;
using Catalog.Response;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor;
using WebAPI.ObjectsConvertor.Mapping;

namespace WebAPI.Utils
{
    public class CatalogUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string MEDIA_CACHE_KEY_PREFIX = "media";
        private const string EPG_CACHE_KEY_PREFIX = "epg";
        private const string RECORDING_CACHE_KEY_PREFIX = "recording";
        private const string CACHE_KEY_FORMAT = "{0}_lng{1}";
        private const string OPC_MERGE_VERSION = "5.0.0.0";
        private static readonly Version opcMergeVersion = new Version(OPC_MERGE_VERSION);

        public static bool GetBaseResponse<T>(BaseRequest request, out T response, bool shouldSupportFailOverCaching = false, string cacheKey = null) where T : BaseResponse
        {
            bool passed = false;
            response = null;
            BaseResponse baseResponse = new BaseResponse();
            if (request != null)
            {
                try
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        baseResponse = request.GetResponse(request);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Exception received while calling catalog service. user ID: {0}, request type: {1}, exception: {3}",
                        request.m_sSiteGuid != null ? request.m_sSiteGuid : string.Empty,                            // 0
                        request.GetType(),                                                                           // 1
                        ex);                                                                                         // 2


                    ErrorUtils.HandleWSException(ex);
                }

                if (baseResponse == null && shouldSupportFailOverCaching && !string.IsNullOrEmpty(cacheKey))
                {
                    // No response from Catalog -> gets medias from cache
                    baseResponse = CatalogCacheManager.Cache.GetFailOverResponse(cacheKey);
                }

                if (baseResponse != null && baseResponse is T)
                {
                    // response received
                    if (shouldSupportFailOverCaching && !string.IsNullOrEmpty(cacheKey))
                    {
                        // insert to cache for failover support
                        CatalogCacheManager.Cache.InsertFailOverResponse(baseResponse, cacheKey);
                    }

                    // convert response to requires object
                    response = baseResponse as T;
                    passed = true;
                }
            }
            return passed;
        }

        private static void GetAssetsFromCatalog(BaseRequest request, int cacheDuration,
            List<long> missingMediaIds, List<long> missingEpgIds,
            out List<MediaObj> mediasFromCatalog, out List<ProgramObj> epgsFromCatalog,
            bool managementData = false)
        {
            mediasFromCatalog = new List<MediaObj>();
            epgsFromCatalog = new List<ProgramObj>();

            if ((missingMediaIds != null && missingMediaIds.Count > 0) || (missingEpgIds != null && missingEpgIds.Count > 0))
            {
                // get group configuration 
                Group group = GroupsManager.GetGroup(request.m_nGroupID);

                List<long> epgIds = new List<long>();

                if (missingEpgIds != null)
                {
                    epgIds.AddRange(missingEpgIds);
                }

                // Build AssetInfoRequest with the missing ids
                AssetInfoRequest assetsRequest = new AssetInfoRequest()
                {
                    epgIds = epgIds,
                    mediaIds = missingMediaIds,
                    m_nGroupID = request.m_nGroupID,
                    m_nPageIndex = 0,
                    m_nPageSize = 0,
                    m_oFilter = new Filter()
                    {
                        m_nLanguage = request.m_oFilter.m_nLanguage,
                        m_bUseStartDate = group.UseStartDate,
                        m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                    },
                    m_sSignature = request.m_sSignature,
                    m_sSignString = request.m_sSignString,
                    ManagementData = managementData
                };

                AssetInfoResponse response = new AssetInfoResponse();
                if (GetBaseResponse(assetsRequest, out response))
                {
                    mediasFromCatalog = CleanNullsFromAssetsList(response.mediaList);
                    epgsFromCatalog = CleanNullsFromAssetsList(response.epgList);

                    // Store in Cache the medias and epgs from Catalog
                    StoreAssetsInCache(mediasFromCatalog, MEDIA_CACHE_KEY_PREFIX, request.m_oFilter.m_nLanguage, cacheDuration);
                    StoreAssetsInCache(epgsFromCatalog, EPG_CACHE_KEY_PREFIX, request.m_oFilter.m_nLanguage, cacheDuration);
                }
            }
        }

        private static void StoreAssetsInCache<T>(List<T> assets, string cacheKeyPrefix, int language, int cacheDuration) where T : BaseObject
        {
            List<BaseObject> baseObjects = null;

            if (assets != null && assets.Count > 0)
            {
                baseObjects = new List<BaseObject>();
                assets.ForEach(m => baseObjects.Add(m));

                CatalogCacheManager.Cache.StoreObjects(baseObjects, string.Format(CACHE_KEY_FORMAT, cacheKeyPrefix, language), cacheDuration);
            }
        }

        private static List<T> CleanNullsFromAssetsList<T>(List<T> assetsList) where T : BaseObject
        {
            List<T> result = null;

            if (assetsList != null)
            {
                if (assetsList.Any(m => m == null))
                {
                    //logger.Warn("CatalogClient: Received response from Catalog with null media objects");
                }

                result = assetsList.Where(m => m != null).ToList();
            }
            else
            {
                result = new List<T>();
            }

            return result;
        }

        // Gets medias and epgs from cache
        // Returns true if all assets were found in cache, false if at least one is missing or not up to date
        private static bool GetAssetsFromCache(List<BaseObject> ids, int language, out List<MediaObj> medias,
            out List<ProgramObj> epgs,
            out List<long> missingMediaIds, out List<long> missingEpgIds)
        {
            bool result = true;
            medias = new List<MediaObj>();
            epgs = new List<ProgramObj>();

            missingMediaIds = null;
            missingEpgIds = null;

            if (ids != null && ids.Count > 0)
            {
                List<CacheKey> mediaKeys = new List<CacheKey>();
                List<CacheKey> epgKeys = new List<CacheKey>();
                List<CacheKey> recordingKeys = new List<CacheKey>();

                CacheKey key = null;

                // Separate media ids and epg ids and build the cache keys
                foreach (var id in ids)
                {
                    if (id.AssetType == eAssetTypes.NPVR)
                    {
                        var searchResult = id as RecordingSearchResult;

                        if (searchResult != null)
                        {
                            key = new CacheKey(searchResult.EpgId, searchResult.m_dUpdateDate);
                        }
                        else
                        {
                            var watchHistory = id as UserWatchHistory;

                            if (watchHistory != null)
                            {
                                key = new CacheKey(watchHistory.EpgId.ToString(), watchHistory.m_dUpdateDate);
                            }
                        }
                    }
                    else
                    {
                        key = new CacheKey(id.AssetId, id.m_dUpdateDate);
                    }

                    switch (id.AssetType)
                    {
                        case eAssetTypes.MEDIA:
                            mediaKeys.Add(key);
                            break;
                        case eAssetTypes.EPG:
                        case eAssetTypes.NPVR:
                            epgKeys.Add(key);
                            break;
                        default:
                            throw new Exception("Unexpected AsssetType");
                    }
                }

                // Media - Get the medias from cache, Cast the results, return false if at least one is missing
                bool mediaCacheResult = RetriveAssetsFromCache(mediaKeys, MEDIA_CACHE_KEY_PREFIX, language, out medias, out missingMediaIds);
                bool epgCacheResult = RetriveAssetsFromCache(epgKeys, EPG_CACHE_KEY_PREFIX, language, out epgs, out missingEpgIds);
                result = mediaCacheResult && epgCacheResult;

            }

            return result;
        }

        private static bool RetriveAssetsFromCache<T>(List<CacheKey> assetKeys, string cacheKeyPrefix, int language, out List<T> assets, out List<long> missingAssetIds) where T : BaseObject
        {
            bool result = true;
            assets = new List<T>();
            missingAssetIds = null;

            if (assetKeys != null && assetKeys.Count > 0)
            {
                List<BaseObject> cacheResults = null;

                cacheResults = CatalogCacheManager.Cache.GetObjects(assetKeys, string.Format(CACHE_KEY_FORMAT, cacheKeyPrefix, language), out missingAssetIds);
                if (cacheResults != null && cacheResults.Count > 0)
                {
                    foreach (var res in cacheResults)
                    {
                        assets.Add((T)res);
                    }
                }

                if (missingAssetIds != null && missingAssetIds.Count > 0)
                {
                    result = false;
                }
            }

            return result;
        }

        internal static List<KalturaIAssetable> GetAssets(List<BaseObject> assetsBaseData, BaseRequest request, List<KalturaCatalogWith> withList, CatalogConvertor.ConvertAssetsDelegate convertAssets)
        {
            List<BaseObject> assets = Core.Catalog.Utils.GetOrderedAssets(request.m_nGroupID,assetsBaseData, request.m_oFilter);
            if (assets != null)
            {
                return convertAssets(request.m_nGroupID, assets, withList);
            }
            else
                return null;
        }

        internal static List<KalturaAsset> GetAssets(List<BaseObject> assetsBaseData, BaseRequest request, bool managementData = false)
        {
            List<BaseObject> assets = Core.Catalog.Utils.GetOrderedAssets(request.m_nGroupID, assetsBaseData, request.m_oFilter, managementData);            

            if (assets != null)
                return MapAssets(request.m_nGroupID, assets);
            else
                return null;
        }
        
        private static void BuildMediasFromCatalogAccordingAssetsBaseData(List<BaseObject> assetsBaseDataList, out List<long> missingMediaIds)
        {
            missingMediaIds = new List<long>();
            long assetId = 0;
            foreach (var assetsBaseData in assetsBaseDataList)
            {
                if (assetsBaseData.AssetType == eAssetTypes.MEDIA)
                {
                    long.TryParse(assetsBaseData.AssetId, out assetId);
                    missingMediaIds.Add(assetId);
                }
            }
        }

        public static KalturaAssetInfoListResponse GetMedia(BaseRequest request, string key, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // fire request
            UnifiedSearchResponse mediaIdsResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out mediaIdsResponse, true, key))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (mediaIdsResponse.searchResults != null && mediaIdsResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = mediaIdsResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(assetsBaseDataList, request, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Objects = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
                }

                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }
            return result;
        }

        public static KalturaAssetListResponse GetMedia(BaseRequest request, string key, KalturaBaseResponseProfile responseProfile = null)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // fire request
            UnifiedSearchResponse mediaIdsResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out mediaIdsResponse, true, key))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (mediaIdsResponse.status != null && mediaIdsResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(mediaIdsResponse.status.Code, mediaIdsResponse.status.Message);
            }

            if (mediaIdsResponse.aggregationResults != null && mediaIdsResponse.aggregationResults.Count > 0 &&
                mediaIdsResponse.aggregationResults[0].results != null && mediaIdsResponse.aggregationResults[0].results.Count > 0 && responseProfile != null)
            {
                // build the assetsBaseDataList from the hit array 
                result.Objects = CatalogUtils.GetAssets(mediaIdsResponse.aggregationResults[0].results, request, false, responseProfile);
                result.TotalCount = mediaIdsResponse.aggregationResults[0].totalItems;
            }

            else if (mediaIdsResponse.searchResults != null && mediaIdsResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = mediaIdsResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                result.Objects = CatalogUtils.GetAssets(assetsBaseDataList, request);

                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }
            return result;
        }

        public static KalturaAssetInfoListResponse GetMediaWithStatus(BaseRequest request, string key, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // fire request
            MediaIdsStatusResponse mediaIdsResponse = new MediaIdsStatusResponse();
            if (!CatalogUtils.GetBaseResponse<MediaIdsStatusResponse>(request, out mediaIdsResponse, true, key)
                || mediaIdsResponse == null || mediaIdsResponse.Status.Code != (int)StatusCode.OK)
            {
                if (mediaIdsResponse == null)
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());

                // general error
                throw new ClientException((int)mediaIdsResponse.Status.Code, mediaIdsResponse.Status.Message);
            }

            if (mediaIdsResponse.m_nMediaIds != null && mediaIdsResponse.m_nMediaIds.Count > 0)
            {

                result.Objects = CatalogUtils.GetMediaByIds(mediaIdsResponse.m_nMediaIds, request, with);
                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }

            result.RequestId = mediaIdsResponse.RequestId;
            return result;
        }

        public static List<KalturaAssetInfo> GetMediaByIds(List<SearchResult> mediaIds, BaseRequest request, List<KalturaCatalogWith> with)
        {
            List<KalturaAssetInfo> result = null;

            // get base objects list
            List<BaseObject> assetsBaseDataList = mediaIds.Select(x => new BaseObject()
            {
                AssetId = x.assetID.ToString(),
                AssetType = eAssetTypes.MEDIA,
                m_dUpdateDate = x.UpdateDate
            }).ToList();

            // get assets from catalog/cache
            List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(assetsBaseDataList, request, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

            // build AssetInfoWrapper response
            if (assetsInfo != null)
            {
                result = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
            }

            return result;
        }

        internal static KalturaAssetListResponse GetMediaWithStatus(BaseRequest request, string key)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // fire request
            MediaIdsStatusResponse mediaIdsResponse = new MediaIdsStatusResponse();
            if (!CatalogUtils.GetBaseResponse<MediaIdsStatusResponse>(request, out mediaIdsResponse, true, key)
                || mediaIdsResponse == null || mediaIdsResponse.Status.Code != (int)StatusCode.OK)
            {
                if (mediaIdsResponse == null)
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());

                // general error
                throw new ClientException((int)mediaIdsResponse.Status.Code, mediaIdsResponse.Status.Message);
            }

            if (mediaIdsResponse.m_nMediaIds != null && mediaIdsResponse.m_nMediaIds.Count > 0)
            {

                result.Objects = CatalogUtils.GetMediaByIds(mediaIdsResponse.m_nMediaIds, request);
                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }
            return result;
        }

        internal static List<KalturaAsset> GetMediaByIds(List<SearchResult> mediaIds, BaseRequest request)
        {
            List<KalturaAsset> result = null;

            // get base objects list
            List<BaseObject> assetsBaseDataList = mediaIds.Select(x => new BaseObject()
            {
                AssetId = x.assetID.ToString(),
                AssetType = eAssetTypes.MEDIA,
                m_dUpdateDate = x.UpdateDate
            }).ToList();

            // get assets from catalog/cache
            result = CatalogUtils.GetAssets(assetsBaseDataList, request);
            return result;
        }

        //internal static KalturaAssetListResponse GetBundleAssets(BundleAssetsRequest request, string key, int cacheDuration)
        //{


        //    // to do : casll BundleAssetsRequest
        //    KalturaAssetListResponse result = new KalturaAssetListResponse();

        //    // fire request
        //    UnifiedSearchResponse mediaIdsResponse = new UnifiedSearchResponse();
        //    if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out mediaIdsResponse, true, key))
        //    {
        //        // general error
        //        throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
        //    }

        //    if (mediaIdsResponse.m_nMediaIds != null && mediaIdsResponse.m_nMediaIds.Count > 0)
        //    {
        //        // get base objects list
        //        List<BaseObject> assetsBaseDataList = mediaIdsResponse.m_nMediaIds.Select(x =>new BaseObject(){AssetId = x.assetID.ToString(), m_dUpdateDate = x.UpdateDate}).ToList();

        //        // get assets from catalog/cache
        //        result.Objects = CatalogUtils.GetAssets(assetsBaseDataList, request, cacheDuration);

        //        result.TotalCount = mediaIdsResponse.m_nTotalItems;
        //    }
        //    return result;
        //}

        internal static List<KalturaAsset> GetAssets(List<AggregationResult> results, BaseRequest request, bool managementData, KalturaBaseResponseProfile responseProfile)
        {
            // get base objects list

            List<BaseObject> assetsBaseDataList = new List<BaseObject>();
            foreach (AggregationResult aggregationResult in results)
            {
                if (aggregationResult.topHits != null && aggregationResult.topHits.Count > 0)
                {
                    assetsBaseDataList.Add(aggregationResult.topHits[0] as BaseObject);
                }
            }

            List<BaseObject> assets = Core.Catalog.Utils.GetOrderedAssets(request.m_nGroupID, assetsBaseDataList, request.m_oFilter, managementData); 

            if (assets != null)
            {
                List<KalturaAsset> tempAssets = MapAssets(request.m_nGroupID, assets);

                if (responseProfile != null)
                {
                    string profileName = string.Empty;
                    KalturaDetachedResponseProfile profile = (KalturaDetachedResponseProfile)responseProfile; // always KalturaDetachedResponseProfile
                    if (profile != null)
                    {
                        List<KalturaDetachedResponseProfile> profiles = profile.RelatedProfiles;
                        if (profiles != null && profiles.Count > 0)
                        {
                            profileName = profiles.Where(x => x.Filter is KalturaAggregationCountFilter).Select(x => x.Name).FirstOrDefault();
                        }
                    }

                    Dictionary<string, int> assetIdToCount =
                        results.Where(x => x.topHits != null && x.topHits.Count > 0).
                                ToDictionary(x => (x.topHits[0] as BaseObject).AssetId, x => x.count);

                    foreach (KalturaAsset asset in tempAssets)
                    {
                        string id = string.Empty;

                        if (asset is KalturaRecordingAsset)
                        {
                            id = (asset as KalturaRecordingAsset).RecordingId;
                        }
                        else
                        {
                            if (asset.Id.HasValue)
                            {
                                id = asset.Id.Value.ToString();
                            }
                        }

                        if (assetIdToCount.ContainsKey(id))
                        {
                            var item = assetIdToCount[id];

                            asset.relatedObjects = new SerializableDictionary<string, KalturaListResponse>();
                            KalturaIntegerValueListResponse kiv = new KalturaIntegerValueListResponse()
                            {
                                Values = new List<KalturaIntegerValue>()
                            {
                                new KalturaIntegerValue()
                                {
                                    value = item
                                }
                            },
                                TotalCount = item
                            };

                            asset.relatedObjects.Add(profileName, kiv);
                        }
                    }
                }

                return tempAssets;
            }
            else
            {
                return null;
            }
            return null;
        }

        internal static List<KalturaAsset> MapAssets(int groupId, List<BaseObject> assets)
        {
            GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
            int linearMediaTypeId = groupManager.GetLinearMediaTypeId(groupId);
            Version requestVersion = Managers.Scheme.OldStandardAttribute.getCurrentRequestVersion();
            bool isNewerThenOpcMergeVersion = requestVersion.CompareTo(opcMergeVersion) > 0;
            List<KalturaAsset> result = new List<KalturaAsset>();
            if (assets != null)
            {
                foreach (BaseObject asset in assets)
                {
                    if (asset != null)
                    {
                        KalturaAsset assetToAdd = null;
                        if (asset.AssetType == eAssetTypes.MEDIA && asset is MediaObj)
                        {
                            assetToAdd = AutoMapper.Mapper.Map<KalturaMediaAsset>(asset);
                            MediaObj mediaObj = asset as MediaObj;
                            if (isNewerThenOpcMergeVersion && linearMediaTypeId > 0 && mediaObj.m_oMediaType.m_nTypeID == linearMediaTypeId)
                            {
                                assetToAdd = AutoMapper.Mapper.Map<KalturaLiveAsset>(mediaObj);
                            }
                        }
                        else
                        {
                            assetToAdd = AutoMapper.Mapper.Map<KalturaAsset>(asset);
                        }

                        result.Add(assetToAdd);
                    }
                    else
                    {
                        log.WarnFormat("found null asset while mapping from internal asset object to KalturaAsset");
                    }
                }
            }            

            return result;
        }

        public static UnifiedSearchResponse SearchAssets(int groupId, int userId, int domainId, string udid, string language, int pageIndex, int? pageSize, string filter, List<int> assetTypes,
            DateTime serverTime, OrderObj order, Group group, string signature, string signString, string failoverCacheKey, ref UnifiedSearchRequest request)
        {
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();

            // build request
            request = new UnifiedSearchRequest()
            {
                m_sSignature = signature,
                m_sSignString = signString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                filterQuery = filter,
                m_dServerTime = serverTime,
                order = order,
                assetTypes = assetTypes,
                m_sSiteGuid = userId.ToString(),
                domainId = domainId
            };

            // fire unified search request
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out searchResponse, true, failoverCacheKey))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status.Code, searchResponse.status.Message);
            }

            return searchResponse;
        }

        internal static List<long> GetUserWatchedMediaIds(int groupId, int userId)
        {
            List<long> mediaIds = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    mediaIds = Core.Api.Module.GetUserWatchedMediaIds(groupId, userId);
                    if (mediaIds == null)
                    {
                        log.ErrorFormat("Error while getting user watchedMediaIds. groupId: {0}, userId:{1}", groupId, userId);
                    }
                }

                log.DebugFormat("return from Api.GetUserWatchedMediaIds. groupId: {0}, userId: {1}", groupId, userId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            return mediaIds;
        }

        public static UnifiedSearchResponse GetChannelAssets(int groupId, int userId, int domainId, string udid, string language, int pageIndex, int? pageSize, int internalChannelId,
            string filterQuery, DateTime serverTime, OrderObj order, Group group, string signature, string signString, string failoverCacheKey, ref InternalChannelRequest request)
        {
            UnifiedSearchResponse channelResponse = new UnifiedSearchResponse();

            // build request
            request = new InternalChannelRequest()
            {
                m_sSignature = signature,
                m_sSignString = signString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = userId.ToString(),
                domainId = domainId,
                order = order,
                internalChannelID = internalChannelId.ToString(),
                filterQuery = filterQuery,
                m_dServerTime = serverTime,
                m_bIgnoreDeviceRuleID = false
            };

            // fire request
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out channelResponse, true, failoverCacheKey))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (channelResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(channelResponse.status.Code, channelResponse.status.Message);
            }

            return channelResponse;
        }

        internal static UnifiedSearchResponse GetMediaExcludeWatched(int groupId, int userId, int domainId, string udid, string language,
            int pageIndex, int? pageSize, int mediaId, string filter, List<int> mediaTypes, DateTime dateTime, OrderObj order, Group group,
            string signature, string signString, string failoverCacheKey, ref MediaRelatedRequest request)
        {
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();

            // build request
            request = new MediaRelatedRequest()
            {
                m_sSignature = signature,
                m_sSignString = signString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = userId.ToString(),
                domainId = domainId,
                m_sFilter = filter,
                OrderObj = order
            };

            // fire request            
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out searchResponse, true, failoverCacheKey))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (searchResponse.status != null && searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status.Code, searchResponse.status.Message);
            }

            return searchResponse;
        }

        internal static void UpdateEpgTags(List<KalturaAsset> assets, List<BaseObject> searchResults)
        {
            if (assets == null)
            {
                return;
            }

            BaseObject unifiedSearchResult = null;
            RecommendationSearchResult recommendationSearchResult = null;

            foreach (var asset in assets)
            {
                unifiedSearchResult = searchResults.FirstOrDefault(x => x.AssetId == asset.Id.Value.ToString());
                if (unifiedSearchResult is RecommendationSearchResult)
                {
                    recommendationSearchResult = unifiedSearchResult as RecommendationSearchResult;
                    if (recommendationSearchResult.TagsExtraData != null)
                    {
                        if (asset.Tags == null)
                        {
                            asset.Tags = new SerializableDictionary<string, KalturaMultilingualStringValueArray>();
                        }

                        foreach (var extraData in recommendationSearchResult.TagsExtraData)
                        {
                            if (!asset.Tags.ContainsKey(extraData.Key))
                            {
                                asset.Tags.Add(extraData.Key, new KalturaMultilingualStringValueArray());
                                LanguageContainer lc = new LanguageContainer() { LanguageCode = WebAPI.Utils.Utils.GetDefaultLanguage(), Value = extraData.Value };
                                asset.Tags[extraData.Key].Objects.Add(new KalturaMultilingualStringValue() { value = new KalturaMultilingualString(new LanguageContainer[1] { lc }) });
                            }
                        }
                    }
                }
            }
        }

    }
}