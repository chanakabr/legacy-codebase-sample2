using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel;
using System.Web;
using AutoMapper;
using WebAPI.Catalog;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Models.Catalog;
using WebAPI.ObjectsConvertor;
using WebAPI.Models.General;
using KLogMonitor;
using WebAPI.Managers.Models;
using WebAPI.Managers;

namespace WebAPI.Utils
{
    public class CatalogUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string MEDIA_CACHE_KEY_PREFIX = "media";
        private const string EPG_CACHE_KEY_PREFIX = "epg";
        private const string CACHE_KEY_FORMAT = "{0}_lng{1}";

        public static bool GetBaseResponse<T>(WebAPI.Catalog.IserviceClient client, BaseRequest request, out T response, bool shouldSupportFailOverCaching = false, string cacheKey = null) where T : BaseResponse
        {
            bool passed = false;
            response = null;
            BaseResponse baseResponse = new BaseResponse();
            if (request != null && client != null)
            {
                try
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        baseResponse = client.GetResponse(request);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Exception received while calling catalog service. user ID: {0}, request type: {1}, request address: {2}, exception: {3}",
                        request.m_sSiteGuid != null ? request.m_sSiteGuid : string.Empty,                            // 0
                        request.GetType(),                                                                           // 1
                        client.Endpoint != null &&
                        client.Endpoint.Address != null &&
                        client.Endpoint.Address.Uri != null ? client.Endpoint.Address.Uri.ToString() : string.Empty, // 2
                        ex);                                                                                         // 3


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

        private static void GetAssetsFromCatalog(IserviceClient client, BaseRequest request, int cacheDuration, List<long> missingMediaIds, List<long> missingEpgIds, out List<MediaObj> mediasFromCatalog, out List<ProgramObj> epgsFromCatalog)
        {
            mediasFromCatalog = new List<MediaObj>();
            epgsFromCatalog = new List<ProgramObj>(); ;

            if ((missingMediaIds != null && missingMediaIds.Count > 0) || (missingEpgIds != null && missingEpgIds.Count > 0))
            {
                // get group configuration 
                Group group = GroupsManager.GetGroup(request.m_nGroupID);

                // Build AssetInfoRequest with the missing ids
                AssetInfoRequest assetsRequest = new AssetInfoRequest()
                {
                    epgIds = missingEpgIds,
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
                };

                AssetInfoResponse response = new AssetInfoResponse();
                if (GetBaseResponse(client, assetsRequest, out response))
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
        private static bool GetAssetsFromCache(List<BaseObject> ids, int language, out List<MediaObj> medias, out List<ProgramObj> epgs, out List<long> missingMediaIds, out List<long> missingEpgIds)
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

                CacheKey key = null;

                // Separate media ids and epg ids and build the cache keys
                foreach (var id in ids)
                {
                    key = new CacheKey(id.AssetId, id.m_dUpdateDate);
                    switch (id.AssetType)
                    {
                        case eAssetTypes.MEDIA:
                            mediaKeys.Add(key);
                            break;
                        case eAssetTypes.EPG:
                            epgKeys.Add(key);
                            break;
                        default:
                            throw new Exception("Unexpected AsssetType");
                    }
                }

                // Media - Get the medias from cache, Cast the results, return false if at least one is missing
                result = RetriveAssetsFromCache(mediaKeys, MEDIA_CACHE_KEY_PREFIX, language, out medias, out missingMediaIds) &&
                    RetriveAssetsFromCache(epgKeys, EPG_CACHE_KEY_PREFIX, language, out epgs, out missingEpgIds);
            }

            return result;
        }

        private static bool RetriveAssetsFromCache<T>(List<CacheKey> mediaKeys, string cacheKeyPrefix, int language, out List<T> assets, out List<long> missingMediaIds) where T : BaseObject
        {
            bool result = true;
            assets = new List<T>();
            missingMediaIds = null;

            if (mediaKeys != null && mediaKeys.Count > 0)
            {
                List<BaseObject> cacheResults = null;

                cacheResults = CatalogCacheManager.Cache.GetObjects(mediaKeys, string.Format(CACHE_KEY_FORMAT, MEDIA_CACHE_KEY_PREFIX, language), out missingMediaIds);
                if (cacheResults != null && cacheResults.Count > 0)
                {
                    foreach (var res in cacheResults)
                    {
                        assets.Add((T)res);
                    }
                }

                if (missingMediaIds != null && missingMediaIds.Count > 0)
                {
                    result = false;
                }
            }

            return result;
        }

        

        internal static List<KalturaIAssetable> GetAssets(IserviceClient client, List<BaseObject> assetsBaseData, BaseRequest request, int cacheDuration, List<KalturaCatalogWith> withList, CatalogConvertor.ConvertAssetsDelegate convertAssets)
        {
            var assets = GetOrderedAssets(client, assetsBaseData, request, cacheDuration);
            if (assets != null)
            {
                return convertAssets(request.m_nGroupID, assets, withList);
            }
            else
                return null;
        }

        internal static List<KalturaAsset> GetAssets(IserviceClient client, List<BaseObject> assetsBaseData, BaseRequest request, int cacheDuration)
        {
            var assets = GetOrderedAssets(client, assetsBaseData, request, cacheDuration);

            if (assets != null)
            {
                return Mapper.Map<List<KalturaAsset>>(assets);
            }
            else
                return null;
        }

        private static List<BaseObject> GetOrderedAssets(IserviceClient client, List<BaseObject> assetsBaseData, BaseRequest request, int cacheDuration)
        {
            List<BaseObject> finalResult = new List<BaseObject>();
            List<MediaObj> medias = new List<MediaObj>();
            List<ProgramObj> epgs = new List<ProgramObj>();

            List<MediaObj> mediasFromCatalog = new List<MediaObj>();
            List<ProgramObj> epgsFromCatalog = new List<ProgramObj>();
            List<long> missingMediaIds = new List<long>();
            List<long> missingEpgIds = new List<long>();

            // get assets from cache
            if (!GetAssetsFromCache(assetsBaseData, request.m_oFilter.m_nLanguage, out medias, out epgs, out missingMediaIds, out missingEpgIds))
            {

                if ((missingMediaIds != null && missingMediaIds.Count > 0) ||
                    (missingEpgIds != null && missingEpgIds.Count > 0))
                {
                    // Get the assets from catalog that were missing in cache (and add them to cache) 
                    GetAssetsFromCatalog(client, request, cacheDuration, missingMediaIds, missingEpgIds, out mediasFromCatalog, out epgsFromCatalog);

                    // Append the medias from Catalog to the medias from cache
                    if (mediasFromCatalog != null)
                        medias.AddRange(mediasFromCatalog);

                    // Append the EPGs from Catalog to the EPGs from cache
                    if (epgs != null)
                        epgs.AddRange(epgsFromCatalog);
                }
            }

            // build combined EPG and Media results
            BaseObject baseObject = new BaseObject();
            foreach (var item in assetsBaseData)
            {
                if (item.AssetType == eAssetTypes.MEDIA)
                {
                    baseObject = medias.Where(m => m != null && m.AssetId.ToString() == item.AssetId).FirstOrDefault();
                    if (baseObject != null)
                        finalResult.Add(baseObject);
                }
                else
                {
                    if (item.AssetType == eAssetTypes.EPG)
                    {
                        baseObject = epgs.Where(p => p != null && p.AssetId.ToString() == item.AssetId).FirstOrDefault();
                        if (baseObject != null)
                            finalResult.Add(baseObject);
                    }
                }
            }

            return finalResult;
        }

        public static KalturaAssetInfoListResponse GetMedia(IserviceClient client, BaseRequest request, string key, int cacheDuration, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // fire request
            UnifiedSearchResponse mediaIdsResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(client, request, out mediaIdsResponse, true, key))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (mediaIdsResponse.searchResults != null && mediaIdsResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = mediaIdsResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(client, assetsBaseDataList, request, cacheDuration, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Objects = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
                }

                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }
            return result;
        }

        public static KalturaAssetListResponse GetMedia(IserviceClient client, BaseRequest request, string key, int cacheDuration)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // fire request
            UnifiedSearchResponse mediaIdsResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(client, request, out mediaIdsResponse, true, key))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (mediaIdsResponse.searchResults != null && mediaIdsResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = mediaIdsResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                result.Objects = CatalogUtils.GetAssets(client, assetsBaseDataList, request, cacheDuration);

                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }
            return result;
        }

        public static KalturaAssetInfoListResponse GetMediaWithStatus(IserviceClient client, BaseRequest request, string key, int cacheDuration, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // fire request
            MediaIdsStatusResponse mediaIdsResponse = new MediaIdsStatusResponse();
            if (!CatalogUtils.GetBaseResponse<MediaIdsStatusResponse>(client, request, out mediaIdsResponse, true, key)
                || mediaIdsResponse == null || mediaIdsResponse.Status.Code != (int)StatusCode.OK)
            {
                if (mediaIdsResponse == null)
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());

                // general error
                throw new ClientException((int)mediaIdsResponse.Status.Code, mediaIdsResponse.Status.Message);
            }

            if (mediaIdsResponse.m_nMediaIds != null && mediaIdsResponse.m_nMediaIds.Count > 0)
            {

                result.Objects = CatalogUtils.GetMediaByIds(client, mediaIdsResponse.m_nMediaIds, request, cacheDuration, with);
                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }

            result.RequestId = mediaIdsResponse.RequestId;
            return result;
        }

        public static List<KalturaAssetInfo> GetMediaByIds(IserviceClient client, List<SearchResult> mediaIds, BaseRequest request, int cacheDuration, List<KalturaCatalogWith> with)
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
            List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(client, assetsBaseDataList, request, cacheDuration, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

            // build AssetInfoWrapper response
            if (assetsInfo != null)
            {
                result = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
            }

            return result;
        }

        internal static KalturaAssetListResponse GetMediaWithStatus(IserviceClient client, BaseRequest request, string key, int cacheDuration)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // fire request
            MediaIdsStatusResponse mediaIdsResponse = new MediaIdsStatusResponse();
            if (!CatalogUtils.GetBaseResponse<MediaIdsStatusResponse>(client, request, out mediaIdsResponse, true, key)
                || mediaIdsResponse == null || mediaIdsResponse.Status.Code != (int)StatusCode.OK)
            {
                if (mediaIdsResponse == null)
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());

                // general error
                throw new ClientException((int)mediaIdsResponse.Status.Code, mediaIdsResponse.Status.Message);
            }

            if (mediaIdsResponse.m_nMediaIds != null && mediaIdsResponse.m_nMediaIds.Count > 0)
            {

                result.Objects = CatalogUtils.GetMediaByIds(client, mediaIdsResponse.m_nMediaIds, request, cacheDuration);
                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }
            return result;
        }

        private static List<KalturaAsset> GetMediaByIds(IserviceClient client, List<SearchResult> mediaIds, BaseRequest request, int cacheDuration)
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
            List<KalturaAsset> assetsInfo = CatalogUtils.GetAssets(client, assetsBaseDataList, request, cacheDuration);

            return result;
        }


        //internal static KalturaAssetListResponse GetBundleAssets(IserviceClient client, BundleAssetsRequest request, string key, int cacheDuration)
        //{


        //    // to do : casll BundleAssetsRequest
        //    KalturaAssetListResponse result = new KalturaAssetListResponse();

        //    // fire request
        //    UnifiedSearchResponse mediaIdsResponse = new UnifiedSearchResponse();
        //    if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(client, request, out mediaIdsResponse, true, key))
        //    {
        //        // general error
        //        throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
        //    }

        //    if (mediaIdsResponse.m_nMediaIds != null && mediaIdsResponse.m_nMediaIds.Count > 0)
        //    {
        //        // get base objects list
        //        List<BaseObject> assetsBaseDataList = mediaIdsResponse.m_nMediaIds.Select(x =>new BaseObject(){AssetId = x.assetID.ToString(), m_dUpdateDate = x.UpdateDate}).ToList();

        //        // get assets from catalog/cache
        //        result.Objects = CatalogUtils.GetAssets(client, assetsBaseDataList, request, cacheDuration);

        //        result.TotalCount = mediaIdsResponse.m_nTotalItems;
        //    }
        //    return result;
        //}
    }
}