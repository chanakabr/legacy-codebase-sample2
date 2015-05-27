using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using log4net;
using WebAPI.Catalog;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Models.Catalog;
using WebAPI.ObjectsConvertor;
using WebAPI.Models.General;

namespace WebAPI.Utils
{
    public class CatalogUtils
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
                    baseResponse = client.GetResponse(request);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Exception received while calling catalog service. user ID: {0}, group ID: {1}, request type: {2}, request address: {3} Exception: {4}",
                        request.m_sSiteGuid != null ? request.m_sSiteGuid : string.Empty,                            // 0
                        request.m_nGroupID,                                                                          // 1
                        request.GetType(),                                                                           // 2
                        client.Endpoint != null &&
                        client.Endpoint.Address != null &&
                        client.Endpoint.Address.Uri != null ? client.Endpoint.Address.Uri.ToString() : string.Empty, // 3
                        ex);

                    if (ex is CommunicationException)
                    {
                        throw new ClientException((int)StatusCode.InternalConnectionIssue, StatusCode.InternalConnectionIssue.ToString());
                    }

                    if (ex is TimeoutException)
                    {
                        throw new ClientException((int)StatusCode.Timeout, StatusCode.Timeout.ToString());
                    }

                    if (ex is Exception)
                    {
                        throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                    }
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
                        m_sDeviceId = request.m_oFilter.m_sDeviceId,
                        //m_sPlatform = platform.ToString()
                    },
                    m_sSignature = request.m_sSignature,
                    m_sSignString = request.m_sSignString,
                    m_sSiteGuid = request.m_sSiteGuid,
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
        
        internal static int GetLanguageId(int groupId, string language)
        {
            // get all group languages
            var languages = GroupsManager.GetGroup(groupId).Languages;

            // get default/specific language
            Models.Language langModel = new Models.Language();
            if (string.IsNullOrEmpty(language))
                langModel = languages.Where(l => l.IsDefault).FirstOrDefault();
            else
                langModel = languages.Where(l => l.Code == language).FirstOrDefault();

            if (langModel != null)
                return langModel.Id;
            else
                return 0;
        }

        internal static List<IAssetable> GetAssets(IserviceClient client, List<BaseObject> assetsBaseData, BaseRequest request, int cacheDuration, List<With> withList, CatalogConvertor.ConvertAssetsDelegate convertAssets)
        {
            var assets = GetOrderedAssets(client, assetsBaseData, request, cacheDuration);
            if (assets != null)
            {
                return convertAssets(request.m_nGroupID, assets, withList);
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
    }
}