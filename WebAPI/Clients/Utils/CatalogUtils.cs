using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Catalog;
using WebAPI.Clients.Exceptions;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.Clients.Utils
{
    public class CatalogUtils
    {
        private const string MEDIA_CACHE_KEY_PREFIX = "media";
        private const string EPG_CACHE_KEY_PREFIX = "epg";
        private const string CACHE_KEY_FORMAT = "{0}_lng{1}";

        //private static readonly ILog logger = LogManager.GetLogger(typeof(CatalogUtils));

        public static bool GetBaseResponse<T>(WebAPI.Catalog.IserviceClient client, BaseRequest request, out T response, bool shouldSupportCaching = false, string cacheKey = null) where T : BaseResponse
        {
            response = null;

            BaseResponse baseResponse = client.GetResponse(request);

            if (baseResponse == null && !shouldSupportCaching)
            {
                return false;
            }
            else if (baseResponse == null) // No response from Catalog, gets medias from cache
            {
                baseResponse = CatalogCacheManager.Cache.GetFailOverResponse(cacheKey);

                if (baseResponse == null) // No response from Catalog and no response from cache
                {
                    return false;
                }
            }

            if (baseResponse != null && baseResponse is T)
            {
                if (shouldSupportCaching)
                {
                    CatalogCacheManager.Cache.InsertFailOverResponse(baseResponse, cacheKey); // Insert the UnifiedSearchResponse to cache for failover support
                }
                response = baseResponse as T;
                return true;
            }

            return true;
        }

        public static T SearchAssets<T>(WebAPI.Catalog.IserviceClient client, string signString, string signature, int cacheDuration, UnifiedSearchRequest request, string cacheKey, List<With> with)
            where T : BaseListWrapper, new()
        {
            T result = new T();

            UnifiedSearchResponse response;

            try
            {
                if (GetBaseResponse<UnifiedSearchResponse>(client, request, out response, true, cacheKey) && response.status != null && response.status.Code == (int)WebAPI.Models.StatusCode.OK)
                {
                    List<MediaObj> medias = null;
                    List<ProgramObj> epgs = null;
                    List<long> missingMediaIds = null;
                    List<long> missingEpgIds = null;

                    if (!GetAssetsFromCache(response.searchResults, request.m_oFilter.m_nLanguage, out medias, out epgs, out missingMediaIds, out missingEpgIds))
                    {
                        List<MediaObj> mediasFromCatalog;
                        List<ProgramObj> epgsFromCatalog;

                        // Get the assets that were missing in cache 
                        GetAssetsFromCatalog(client, signString, signature, cacheDuration, request.m_nGroupID, request.m_oFilter.m_sPlatform, request.m_sSiteGuid, request.m_oFilter.m_sDeviceId, request.m_oFilter.m_nLanguage, missingMediaIds, missingEpgIds, out mediasFromCatalog, out epgsFromCatalog); 

                        // Append the medias from Catalog to the medias from cache
                        medias.AddRange(mediasFromCatalog);

                        // Append the epgs from Catalog to the epgs from cache
                        epgs.AddRange(epgsFromCatalog);
                    }

                    // Gets one list including both medias and epgds, ordered by Catalog order
                    if (typeof(T) == typeof(AssetInfoWrapper))
                    {
                        (result as AssetInfoWrapper).Assets = MargeAndCompleteResults(response.searchResults, medias, epgs, with, request.m_nGroupID, request.m_oFilter.m_sPlatform, request.m_sSiteGuid, request.m_oFilter.m_sDeviceId);
                    }
                    else if (typeof(T) == typeof(SlimAssetInfoWrapper))
                    {
                        (result as SlimAssetInfoWrapper).Assets = MargeAndCompleteSlimResults(response.searchResults, medias, epgs, with, request.m_nGroupID, request.m_oFilter.m_sPlatform, request.m_sSiteGuid, request.m_oFilter.m_sDeviceId);
                    }

                    result.TotalItems = response.m_nTotalItems;
                }
                else
                {
                    throw new ClientException(response.status.Code, response.status.Message);
                }
            }
            catch (Exception ex)
            {
                if (ex is ClientException)
                {
                    throw ex;
                }

                throw new ClientException((int)StatusCode.InternalConnectionIssue);
            }

            return (T)result;
        }

        private static void GetAssetsFromCatalog(IserviceClient client, string signString, string signature, int cacheDuration, int groupID, string platform, string siteGuid, string udid, int language, List<long> missingMediaIds, List<long> missingEpgIds, out List<MediaObj> mediasFromCatalog, out List<ProgramObj> epgsFromCatalog)
        {
            mediasFromCatalog = new List<MediaObj>();
            epgsFromCatalog = new List<ProgramObj>(); ;

            if ((missingMediaIds != null && missingMediaIds.Count > 0) || (missingEpgIds != null && missingEpgIds.Count > 0))
            {
                // Build AssetInfoRequest with the missing ids
                AssetInfoRequest request = new AssetInfoRequest()
                {
                    epgIds = missingEpgIds,
                    mediaIds = missingMediaIds,
                    m_nGroupID = groupID,
                    m_nPageIndex = 0,
                    m_nPageSize = 0,
                    m_oFilter = new Filter()
                    {
                        m_nLanguage = language,
                        m_sDeviceId = udid,
                        //m_sPlatform = platform.ToString()
                    },
                    m_sSignature = signature,
                    m_sSignString = signString,
                    m_sSiteGuid = siteGuid,
                };

                BaseResponse response = client.GetResponse(request);

                if (response != null)
                {
                    AssetInfoResponse assetInfoResponse = (AssetInfoResponse)response;

                    mediasFromCatalog = CleanNullsFromAssetsList(assetInfoResponse.mediaList);
                    epgsFromCatalog = CleanNullsFromAssetsList(assetInfoResponse.epgList);

                    // Store in Cache the medias and epgs from Catalog
                    StoreAssetsInCache(mediasFromCatalog, MEDIA_CACHE_KEY_PREFIX, language, cacheDuration);
                    StoreAssetsInCache(epgsFromCatalog, EPG_CACHE_KEY_PREFIX, language, cacheDuration);
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
        private static bool GetAssetsFromCache(List<UnifiedSearchResult> ids, int language, out List<MediaObj> medias, out List<ProgramObj> epgs, out List<long> missingMediaIds, out List<long> missingEpgIds)
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
                    key = new CacheKey(id.assetID, id.UpdateDate);
                    switch (id.type)
                    {
                        case WebAPI.Catalog.AssetType.Media:
                            mediaKeys.Add(key);
                            break;
                        case WebAPI.Catalog.AssetType.Epg:
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

        // Returns a list of AssetInfo results from the medias and epgs, ordered by the list of search results from Catalog
        // In case 'With' member contains "stats" - an AssetStatsRequest is made to complete the missing stats data from Catalog
        private static List<AssetInfo> MargeAndCompleteResults(List<UnifiedSearchResult> orderedAssetsIds, List<MediaObj> medias, List<ProgramObj> epgs, List<With> with,
            int groupID, string platform, string siteGuid, string udid)
        {
            List<AssetInfo> results = new List<AssetInfo>();

            if (orderedAssetsIds == null || ((medias == null || medias.Count == 0) && (epgs == null || epgs.Count == 0)))
            {
                return null;
            }

            AssetInfo asset = null;

            List<AssetInfo> mediaAssets = Mapper.Map<List<AssetInfo>>(medias);
            List<AssetInfo> epgAssets = Mapper.Map<List<AssetInfo>>(epgs);
            
            List<AssetStats> mediaAssetsStats = null;
            List<AssetStats> epgAssetsStats = null;

            if (with != null)
            {
                if (with.Contains(With.stats)) // if stats are required - gets the stats from Catalog
                {
                    if (medias != null && medias.Count > 0)
                    {
                        mediaAssetsStats = ClientsManager.CatalogClient().
                            GetAssetsStats(groupID, siteGuid, udid, medias.Select(m => m.m_nID).ToList(),
                            SerializationUtils.ConvertToUnixTimestamp(DateTime.MinValue), SerializationUtils.ConvertToUnixTimestamp(DateTime.MaxValue),
                            Mapper.Map<WebAPI.Catalog.StatsType>(WebAPI.Models.AssetType.Media));
                    }
                    if (epgs != null && epgs.Count > 0)
                    {
                        epgAssetsStats = ClientsManager.CatalogClient().
                            GetAssetsStats(groupID, siteGuid, udid, epgs.Select(e => e.m_nID).ToList(),
                            SerializationUtils.ConvertToUnixTimestamp(DateTime.MinValue), SerializationUtils.ConvertToUnixTimestamp(DateTime.MaxValue),
                            Mapper.Map<WebAPI.Catalog.StatsType>(WebAPI.Models.AssetType.Epg));
                    }
                }
                if (with.Contains(With.files)) // if files are required - parses the files
                {
                    for (int i = 0; i < medias.Count; i++)
                    {
                        mediaAssets[i].Files = Mapper.Map<List<File>>(medias[i].m_lFiles);
                    }
                }
                if (with.Contains(With.images)) // if images are required - gets the stats from Catalog
                {
                    for (int i = 0; i < medias.Count; i++)
                    {
                        mediaAssets[i].Images = Mapper.Map<List<Image>>(medias[i].m_lPicture);
                    }
                }
            }

            // order results
            foreach (var item in orderedAssetsIds)
            {
                if (item.type == WebAPI.Catalog.AssetType.Media)
                {
                    asset = mediaAssets.Where(m => m != null && m.Id == item.assetID).FirstOrDefault();
                    if (asset != null)
                    {
                        
                        asset.Statistics = mediaAssetsStats != null ? mediaAssetsStats.Where(mas => mas.AssetId == asset.Id).FirstOrDefault() : null;
                        results.Add(asset);
                        asset = null;
                    }
                }
                else if (item.type == WebAPI.Catalog.AssetType.Epg)
                {
                    asset = epgAssets.Where(p => p != null && p.Id == item.assetID).FirstOrDefault();
                    if (asset != null)
                    {
                        asset.Statistics = mediaAssetsStats != null ? epgAssetsStats.Where(eas => eas.AssetId == asset.Id).FirstOrDefault() : null;
                        results.Add(asset);
                        asset = null;
                    }
                }
            }

            return results;
        }

        private static List<SlimAssetInfo> MargeAndCompleteSlimResults(List<UnifiedSearchResult> orderedAssetsIds, List<MediaObj> medias, List<ProgramObj> epgs, List<With> with,
            int groupID, string platform, string siteGuid, string udid)
        {
            List<SlimAssetInfo> results = new List<SlimAssetInfo>();

            if (orderedAssetsIds == null || ((medias == null || medias.Count == 0) && (epgs == null || epgs.Count == 0)))
            {
                return null;
            }

            SlimAssetInfo asset = null;

            List<SlimAssetInfo> mediaAssets = Mapper.Map<List<SlimAssetInfo>>(medias);
            List<SlimAssetInfo> epgAssets = Mapper.Map<List<SlimAssetInfo>>(epgs);

            if (with != null)
            {
                if (with.Contains(With.images)) // if images are required - gets the stats from Catalog
                {
                    for (int i = 0; i < medias.Count; i++)
                    {
                        mediaAssets[i].Images = Mapper.Map<List<Image>>(medias[i].m_lPicture);
                    }
                }
            }

            // order results
            foreach (var item in orderedAssetsIds)
            {
                if (item.type == WebAPI.Catalog.AssetType.Media)
                {
                    asset = mediaAssets.Where(m => m != null && m.Id == item.assetID).FirstOrDefault();
                    if (asset != null)
                    {
                        results.Add(asset);
                        asset = null;
                    }
                }
                else if (item.type == WebAPI.Catalog.AssetType.Epg)
                {
                    asset = epgAssets.Where(p => p != null && p.Id == item.assetID).FirstOrDefault();
                    if (asset != null)
                    {
                        results.Add(asset);
                        asset = null;
                    }
                }
            }

            return results;
        }

        internal static WatchHistoryAssetWrapper WatchHistory(IserviceClient client, string signString, string signature, int cacheDuration, WatchHistoryRequest request, List<With> with)
        {
            WatchHistoryAssetWrapper result = new WatchHistoryAssetWrapper();

            WatchHistoryResponse response;

            try
            {
                if (GetBaseResponse<WatchHistoryResponse>(client, request, out response, true) && response.status != null && response.status.Code == (int)WebAPI.Models.StatusCode.OK)
                {
                    List<MediaObj> medias = null;
                    List<long> missingMediaIds = null;
                    List<ProgramObj> epgs = null;
                    List<long> missingEpgIds = null;


                    //if (!GetAssetsFromCache(response.result, request.m_oFilter.m_nLanguage, out medias, out epgs, out missingMediaIds, out missingEpgIds))
                    //{
                    //    List<MediaObj> mediasFromCatalog;
                    //    List<ProgramObj> epgsFromCatalog;

                    //    // Get the assets that were missing in cache 
                    //    GetAssetsFromCatalog(client, signString, signature, cacheDuration, request.m_nGroupID, request.m_oFilter.m_sPlatform, request.m_sSiteGuid, request.m_oFilter.m_sDeviceId, request.m_oFilter.m_nLanguage, missingMediaIds, missingEpgIds, out mediasFromCatalog, out epgsFromCatalog);

                    //    // Append the medias from Catalog to the medias from cache
                    //    medias.AddRange(mediasFromCatalog);
                    //}

                    // Gets one list including both medias and epgds, ordered by Catalog order
                    //result.WatchHistoryAssets = MargeAndCompleteHistoryResults(response.result, medias, epgs, with, request.m_nGroupID, request.m_oFilter.m_sPlatform, request.m_sSiteGuid, request.m_oFilter.m_sDeviceId);

                    result.TotalItems = response.m_nTotalItems;
                }
                else
                {
                    throw new ClientException(response.status.Code, response.status.Message);
                }
            }
            catch (Exception ex)
            {
                if (ex is ClientException)
                {
                    throw ex;
                }

                throw new ClientException((int)StatusCode.InternalConnectionIssue);
            }

            return result;
        }
    }
}