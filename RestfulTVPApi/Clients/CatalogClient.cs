using RestfulTVPApi.Catalog;
using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.Objects.Responses;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace RestfulTVPApi.Clients
{
    public class CatalogClient : BaseClient
    {
        #region Variables
        private const string MEDIA_CACHE_KEY_PREFIX = "media";
        private const string EPG_CACHE_KEY_PREFIX = "epg";
        private const string CACHE_KEY_FORMAT = "{0}_lng{1}";

        private readonly ILog logger = LogManager.GetLogger(typeof(CatalogClient));
        
        public string Signature { get; set; }
        public string SignString { get; set; }
        public string SignatureKey
        {
            set
            {
                SignString = Guid.NewGuid().ToString();
                Signature = GetSignature(SignString, value);
            }
        }


        public int CacheDuration { get; set; }


        #endregion

        #region CTOR

        public CatalogClient(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
            
        }

        public CatalogClient()
        {

        }

        #endregion

        #region Properties

        protected RestfulTVPApi.Catalog.IserviceClient Catalog
        {
            get
            {
                return (Module as RestfulTVPApi.Catalog.IserviceClient);
            }
        }

        #endregion

        private string GetSignature(string signString, string signatureKey)
        {
            string retVal;
            //Get key from DB
            string hmacSecret = signatureKey;
            // The HMAC secret as configured in the skin
            // Values are always transferred using UTF-8 encoding
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Calculate the HMAC
            // signingString is the SignString from the request
            HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
            retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signString)));
            myhmacsha1.Clear();
            return retVal;
        }

        private bool GetResponseWithFailOverSupport<T>(BaseRequest request, string cacheKey, out T response) where T : BaseResponse
        {
            bool res = false;
            response = null;

            BaseResponse baseResponse = Catalog.GetResponse(request);

            if (baseResponse == null)// No response from Catalog, gets medias from cache
            {
                baseResponse = CatalogCacheManager.Cache.GetFailOverResponse(cacheKey);

                if (baseResponse == null)// No response from Catalog and no response from cache
                {
                    res = false;
                }
            }

            if (baseResponse != null && baseResponse is T)
            {
                CatalogCacheManager.Cache.InsertFailOverResponse(baseResponse, cacheKey); // Insert the UnifiedSearchResponse to cache for failover support

                response = baseResponse as T;
                res = true;
            }

            return res;
        }

        private void GetAssetsFromCatalog(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform, string siteGuid, string udid, int language, List<long> missingMediaIds, List<long> missingEpgIds, out List<MediaObj> mediasFromCatalog, out List<ProgramObj> epgsFromCatalog)
        {
            mediasFromCatalog = null;
            epgsFromCatalog = null;

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
                    m_sSignature = Signature,
                    m_sSignString = SignString,
                    m_sSiteGuid = siteGuid,
                };

                BaseResponse response = Catalog.GetResponse(request);

                if (response != null)
                {
                    AssetInfoResponse assetInfoResponse = (AssetInfoResponse)response;

                    if (assetInfoResponse.mediaList != null)
                    {
                        if (assetInfoResponse.mediaList.Any(m => m == null))
                        {
                            logger.Warn("CatalogClient: Received response from Catalog with null media objects");
                        }

                        mediasFromCatalog = assetInfoResponse.mediaList.Where(m => m != null).ToList();
                    }
                    else
                    {
                        mediasFromCatalog = new List<MediaObj>();
                    }

                    if (assetInfoResponse.epgList != null)
                    {
                        if (assetInfoResponse.epgList.Any(m => m == null))
                        {
                            logger.Warn("CatalogClient: Received response from Catalog with null EPG objects");
                        }

                        epgsFromCatalog = assetInfoResponse.epgList.Where(m => m != null).ToList();
                    }
                    else
                    {
                        epgsFromCatalog = new List<ProgramObj>();
                    }

                    // Store in Cache the medias and epgs from Catalog
                    
                    List<BaseObject> baseObjects = null;

                    if (mediasFromCatalog != null && mediasFromCatalog.Count > 0)
                    {
                        baseObjects = new List<BaseObject>();
                        mediasFromCatalog.ForEach(m => baseObjects.Add(m));

                        CatalogCacheManager.Cache.StoreObjects(baseObjects, string.Format(CACHE_KEY_FORMAT, MEDIA_CACHE_KEY_PREFIX, language), CacheDuration);
                    }

                    if (epgsFromCatalog != null && epgsFromCatalog.Count > 0)
                    {
                        baseObjects = new List<BaseObject>();
                        epgsFromCatalog.ForEach(p => baseObjects.Add(p));

                        CatalogCacheManager.Cache.StoreObjects(baseObjects, string.Format(CACHE_KEY_FORMAT, EPG_CACHE_KEY_PREFIX, language), CacheDuration);
                    }
                }
            }
        }

        // Gets medias and epgs from cache
        // Returns true if all assets were found in cache, false if at least one is missing or not up to date
        private bool GetAssetsFromCache(List<UnifiedSearchResult> ids, int language, out List<MediaObj> medias, out List<ProgramObj> epgs, out List<long> missingMediaIds, out List<long> missingEpgIds)
        {
            bool result = true;
            medias = null;
            epgs = null;
            missingMediaIds = null;
            missingEpgIds = null;

            if (ids != null && ids.Count > 0)
            {
                List<BaseObject> cacheResults = null;

                List<CacheKey> mediaKeys = new List<CacheKey>();
                List<CacheKey> epgKeys = new List<CacheKey>();

                CacheKey key = null;

                // Separate media ids and epg ids and build the cache keys
                foreach (var id in ids)
                {
                    if (id.type == AssetType.Media)
                    {
                        key = new CacheKey(id.assetID, id.UpdateDate);
                        mediaKeys.Add(key);
                    }

                    else if (id.type == AssetType.Epg)
                    {
                        key = new CacheKey(id.assetID, id.UpdateDate);
                        epgKeys.Add(key);
                    }
                }

                // Media - Get the medias from cache, Cast the results, return false if at least one is missing
                if (mediaKeys != null && mediaKeys.Count > 0)
                {
                    cacheResults = CatalogCacheManager.Cache.GetObjects(mediaKeys, string.Format(CACHE_KEY_FORMAT, MEDIA_CACHE_KEY_PREFIX, language), out missingMediaIds);
                    if (cacheResults != null && cacheResults.Count > 0)
                    {
                        medias = new List<MediaObj>();
                        foreach (var res in cacheResults)
                        {
                            medias.Add((MediaObj)res);
                        }
                    }

                    if (missingMediaIds != null && missingMediaIds.Count > 0)
                    {
                        result = false;
                    }
                }

                // EPG - Get the epgs from cache, Cast the results, return false if at least one is missing
                if (epgKeys != null && epgKeys.Count > 0)
                {
                    cacheResults = CatalogCacheManager.Cache.GetObjects(epgKeys, string.Format(CACHE_KEY_FORMAT, EPG_CACHE_KEY_PREFIX, language), out missingEpgIds);
                    if (cacheResults != null && cacheResults.Count > 0)
                    {
                        epgs = new List<ProgramObj>();
                        foreach (var res in cacheResults)
                        {
                            epgs.Add((ProgramObj)res);
                        }
                    }

                    if (missingEpgIds != null && missingEpgIds.Count > 0)
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        // Returns a list of AssetInfo results from the medias and epgs, ordered by the list of search results from Catalog
        // In case 'With' member contains "stats" - an AssetStatsRequest is made to complete the missing stats data from Catalog
        private List<AssetInfo> OrderAndCompleteResults(List<UnifiedSearchResult> order, List<MediaObj> medias, List<ProgramObj> epgs, List<string> with)
        {
            List<AssetInfo> result = null;

            if (order == null || ((medias == null || medias.Count == 0) && (epgs == null || epgs.Count == 0)))
            {
                return null;
            }

            result = new List<AssetInfo>();
            AssetInfo asset = null;
            MediaObj media = null;
            ProgramObj epg = null;

            List<RestfulTVPApi.Catalog.AssetStatsResult> mediaAssetsStats = null;
            List<RestfulTVPApi.Catalog.AssetStatsResult> epgAssetsStats = null;

            bool shouldAddFiles = false;

            if (with != null)
            {
                if (with.Contains("stats")) // if stats are required - gets the stats from Catalog
                {
                    //if (medias != null && medias.Count > 0)
                    //{
                    //    mediaAssetsStats = new AssetStatsLoader(GroupID, m_sUserIP, 0, 0, medias.Select(m => m.m_nID).ToList(),
                    //        StatsType.MEDIA, DateTime.MinValue, DateTime.MaxValue).Execute() as List<RestfulTVPApi.Objects.Responses.AssetStatsResult>;
                    //}
                    //if (epgs != null && epgs.Count > 0)
                    //{
                    //    epgAssetsStats = new AssetStatsLoader(GroupID, m_sUserIP, 0, 0, epgs.Select(p => p.m_nID).ToList(),
                    //        StatsType.EPG, DateTime.MinValue, DateTime.MaxValue).Execute() as List<AssetStatsResult>;
                    //}
                }
                if (with.Contains("files")) // if stats are required - add a flag 
                {
                    shouldAddFiles = true;
                }
            }

            // Build the AssetInfo objects
            foreach (var item in order)
            {
                if (item.type == AssetType.Media)
                {
                    media = medias.Where(m => m != null && m.m_nID == item.assetID).FirstOrDefault();
                    if (media != null)
                    {
                        if (mediaAssetsStats != null && mediaAssetsStats.Count > 0)
                        {
                            asset = new AssetInfo(media, mediaAssetsStats.Where(mas => mas.m_nAssetID == media.m_nID).FirstOrDefault(), shouldAddFiles);
                        }
                        else
                        {
                            asset = new AssetInfo(media, shouldAddFiles);
                        }
                        result.Add(asset);
                        media = null;
                    }
                }
                else if (item.type == AssetType.Epg)
                {
                    epg = epgs.Where(p => p != null && p.m_nID == item.assetID).FirstOrDefault();
                    if (epg != null)
                    {
                        if (epgAssetsStats != null && epgAssetsStats.Count > 0)
                        {
                            asset = new AssetInfo(epg.m_oProgram, epgAssetsStats.Where(eas => eas.m_nAssetID == epg.m_nID).FirstOrDefault());
                        }
                        else
                        {
                            asset = new AssetInfo(epg.m_oProgram);
                        }
                        result.Add(asset);
                        epg = null;
                    }
                }
            }

            return result;
        }

        public List<AssetInfo> SearchAssets(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform,  string siteGuid, string udid, int language, int pageIndex, int pageSize, 
            string filter, OrderObj order, List<int> assetTypes, List<string> with)
        {
            List<AssetInfo> result = null;

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter() 
                { 
                    m_sPlatform = platform.ToString(),
                    m_sDeviceId = udid,
                    m_nLanguage = language,
                },
                m_nGroupID = groupID,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize,
                filterQuery = filter,
                order = order,
                assetTypes = assetTypes,
            };


            // build failover cahce key
            StringBuilder key = new StringBuilder();

            // g = GroupId
            // ps = PageSize
            // pi = PageIndex
            // ob = OrderBy
            // od = OrderDir
            // ov = OrderValue 
            // at = AssetTypes
            //f = filter

            key.AppendFormat("Unified_search_g={0}_ps={1}_pi={2}", groupID, pageSize, pageIndex);
            if (order != null)
            {
                key.AppendFormat("_ob={0}_od={1}", order.m_eOrderBy, order.m_eOrderDir);
                if (!string.IsNullOrEmpty(order.m_sOrderValue))
                    key.AppendFormat("_ov={0}", order.m_sOrderValue);
            }
            if (assetTypes != null && assetTypes.Count > 0)
                key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));
            if (!string.IsNullOrEmpty(filter))
                key.AppendFormat("_f={0}", filter);

            UnifiedSearchResponse response;

            if (GetResponseWithFailOverSupport<UnifiedSearchResponse>(request, key.ToString(), out response))
            {
                List<MediaObj> medias = null;
                List<ProgramObj> epgs = null;
                List<long> missingMediaIds = null;
                List<long> missingEpgIds = null;

                if (!GetAssetsFromCache(response.searchResults, language, out medias, out epgs, out missingMediaIds, out missingEpgIds))
                {
                    List<MediaObj> mediasFromCatalog;
                    List<ProgramObj> epgsFromCatalog;
                    GetAssetsFromCatalog(groupID, platform, siteGuid, udid, language, missingMediaIds, missingEpgIds, out mediasFromCatalog, out epgsFromCatalog); // Get the assets that were missing in cache 

                    // Append the medias from Catalog to the medias from cache
                    if (medias == null && mediasFromCatalog != null)
                    {
                        medias = new List<MediaObj>();
                    }
                    medias.AddRange(mediasFromCatalog);

                    // Append the epgs from Catalog to the epgs from cache
                    if (epgs == null && epgsFromCatalog != null)
                    {
                        epgs = new List<ProgramObj>();
                    }
                    epgs.AddRange(epgsFromCatalog);
                }

                result = OrderAndCompleteResults(response.searchResults, medias, epgs, with); // Gets one list including both medias and epgds, ordered by Catalog order
            }

            return result;
        }

        public string MediaMark(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform,  string siteGuid, string udid, int language, int mediaId, int mediaFileId, int location,
            string mediaCdn, string errorMessage, string errorCode, string mediaDuration, string action, int totalBitRate, int currentBitRate, int avgBitRate, string npvrId = null)
        {
            string res = null;

            MediaMarkRequest request = new MediaMarkRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oMediaPlayRequestData = new MediaPlayRequestData()
                {
                    m_nAvgBitRate = avgBitRate,
                    m_nCurrentBitRate = currentBitRate,
                    m_nLoc = location,
                    m_nMediaFileID = mediaFileId,
                    m_nMediaID = mediaId,
                    m_nTotalBitRate = totalBitRate,
                    m_sAction = action,
                    m_sMediaDuration = mediaDuration,
                    m_sSiteGuid = siteGuid,
                    m_sUDID = udid,
                    m_sNpvrID = npvrId
                },
                m_sErrorCode = errorCode,
                m_sErrorMessage = errorMessage,
                m_sMediaCDN = mediaCdn,
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupID,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = language, 
                    m_sPlatform = platform.ToString()
                }
            };
            request.m_oFilter.m_sDeviceId = udid;

            var response = Catalog.GetResponse(request) as MediaMarkResponse;

            if (response != null)
            {
                res = response.m_sStatus;
            }

            return res;
        }
    }
}