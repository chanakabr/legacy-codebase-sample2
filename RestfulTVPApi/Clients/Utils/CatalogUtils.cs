using RestfulTVPApi.Catalog;
using RestfulTVPApi.Objects.Models;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Clients.Utils
{
    public class CatalogUtils
    {
        private const string MEDIA_CACHE_KEY_PREFIX = "media";
        private const string EPG_CACHE_KEY_PREFIX = "epg";
        private const string CACHE_KEY_FORMAT = "{0}_lng{1}";

        private static readonly ILog logger = LogManager.GetLogger(typeof(CatalogUtils));

        public static bool GetBaseResponse<T>(RestfulTVPApi.Catalog.IserviceClient client, BaseRequest request, out T response, bool shouldSupportCaching = false, string cacheKey = null) where T : BaseResponse
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

        public static RestfulTVPApi.Objects.Responses.SearchAssetsResponse SearchAssets(RestfulTVPApi.Catalog.IserviceClient client, string signString, string signature, int cacheDuration, UnifiedSearchRequest request, string cacheKey, List<string> with)
        {
            RestfulTVPApi.Objects.Responses.SearchAssetsResponse result = new Objects.Responses.SearchAssetsResponse();

            UnifiedSearchResponse response;

            if (CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(client, request, out response, true, cacheKey) && response.status != null && response.status.Code == (int)RestfulTVPApi.Objects.Models.StatusCode.OK)
            {
                result.TotalItems = response.m_nTotalItems;

                List<MediaObj> medias = null;
                List<ProgramObj> epgs = null;
                List<long> missingMediaIds = null;
                List<long> missingEpgIds = null;

                if (!GetAssetsFromCache(response.searchResults, request.m_oFilter.m_nLanguage, out medias, out epgs, out missingMediaIds, out missingEpgIds))
                {
                    List<MediaObj> mediasFromCatalog;
                    List<ProgramObj> epgsFromCatalog;
                    GetAssetsFromCatalog(client, signString, signature, cacheDuration, request.m_nGroupID, request.m_oFilter.m_sPlatform, request.m_sSiteGuid, request.m_oFilter.m_sDeviceId, request.m_oFilter.m_nLanguage, missingMediaIds, missingEpgIds, out mediasFromCatalog, out epgsFromCatalog); // Get the assets that were missing in cache 

                    // Append the medias from Catalog to the medias from cache
                    medias.AddRange(mediasFromCatalog);

                    // Append the epgs from Catalog to the epgs from cache
                    epgs.AddRange(epgsFromCatalog);
                }

                result.Assets = MargeAndCompleteResults(response.searchResults, medias, epgs, with, request.m_nGroupID, request.m_oFilter.m_sPlatform, request.m_sSiteGuid, request.m_oFilter.m_sDeviceId); // Gets one list including both medias and epgds, ordered by Catalog order

                result.Status = new Objects.Models.Status((int)StatusCode.OK, "OK");
            }
            else
            {
                result.Status = RestfulTVPApi.Objects.Models.Status.CreateFromObject(response.status);
            }

            return result;
        }

        private static void GetAssetsFromCatalog(RestfulTVPApi.Catalog.IserviceClient client, string signString, string signature, int cacheDuration, int groupID, string platform, string siteGuid, string udid, int language, List<long> missingMediaIds, List<long> missingEpgIds, out List<MediaObj> mediasFromCatalog, out List<ProgramObj> epgsFromCatalog)
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
                        m_sPlatform = platform.ToString()
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
                    logger.Warn("CatalogClient: Received response from Catalog with null media objects");
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
                        case AssetType.Media:
                            mediaKeys.Add(key);
                            break;
                        case AssetType.Epg:
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
        private static List<AssetInfo> MargeAndCompleteResults(List<UnifiedSearchResult> orderedAssetsIds, List<MediaObj> medias, List<ProgramObj> epgs, List<string> with, 
            int groupID, string platform, string siteGuid, string udid)
        {
            List<AssetInfo> result = null;

            if (orderedAssetsIds == null || ((medias == null || medias.Count == 0) && (epgs == null || epgs.Count == 0)))
            {
                return null;
            }

            result = new List<AssetInfo>();
            AssetInfo asset = null;
            MediaObj media = null;
            ProgramObj epg = null;

            List<AssetStats> mediaAssetsStats = null;
            List<AssetStats> epgAssetsStats = null;

            bool shouldAddFiles = false;

            if (with != null)
            {
                if (with.Contains("stats")) // if stats are required - gets the stats from Catalog
                {
                    if (medias != null && medias.Count > 0)
                    {
                        mediaAssetsStats = ClientsManager.CatalogClient().
                            GetAssetsStats(groupID, ParsePlatformType(platform), siteGuid, udid, medias.Select(m => m.m_nID).ToList(), 
                            RestfulTVPApi.ServiceInterface.Utils.ConvertToUnixTimestamp(DateTime.MinValue), RestfulTVPApi.ServiceInterface.Utils.ConvertToUnixTimestamp(DateTime.MaxValue),
                            RestfulTVPApi.Objects.RequestModels.Enums.StatsType.Media);
                    }
                    if (epgs != null && epgs.Count > 0)
                    {
                        epgAssetsStats = ClientsManager.CatalogClient().
                            GetAssetsStats(groupID, ParsePlatformType(platform), siteGuid, udid, epgs.Select(e => e.m_nID).ToList(), 
                            RestfulTVPApi.ServiceInterface.Utils.ConvertToUnixTimestamp(DateTime.MinValue), RestfulTVPApi.ServiceInterface.Utils.ConvertToUnixTimestamp(DateTime.MaxValue),
                            RestfulTVPApi.Objects.RequestModels.Enums.StatsType.Epg);
                    }
                }
                if (with.Contains("files")) // if stats are required - add a flag 
                {
                    shouldAddFiles = true;
                }
            }

            // Build the AssetInfo objects
            foreach (var item in orderedAssetsIds)
            {
                if (item.type == AssetType.Media)
                {
                    media = medias.Where(m => m != null && m.m_nID == item.assetID).FirstOrDefault();
                    if (media != null)
                    {
                        asset = AssetInfo.CreateFromObject(media, shouldAddFiles, mediaAssetsStats != null ? mediaAssetsStats.Where(mas => mas.AssetId == media.m_nID).FirstOrDefault() : null);
                        result.Add(asset);
                        media = null;
                    }
                }
                else if (item.type == AssetType.Epg)
                {
                    epg = epgs.Where(p => p != null && p.m_nID == item.assetID).FirstOrDefault();
                    if (epg != null)
                    {
                        asset = AssetInfo.CreateFromObject(epg.m_oProgram, epgAssetsStats != null ? epgAssetsStats.Where(eas => eas.AssetId == epg.m_nID).FirstOrDefault() : null);
                        result.Add(asset);
                        epg = null;
                    }
                }
            }

            return result;
        }

        private static Objects.Enums.PlatformType ParsePlatformType(string platform)
        {
            return (RestfulTVPApi.Objects.Enums.PlatformType)Enum.Parse(typeof(RestfulTVPApi.Objects.Enums.PlatformType), platform);
        }
    }
}