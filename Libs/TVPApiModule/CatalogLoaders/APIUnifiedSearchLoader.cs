using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Manager;

namespace TVPApiModule.CatalogLoaders
{
    public class APIUnifiedSearchLoader : CatalogRequestManager
    {
        protected const string MEDIA_CACHE_KEY_PREFIX = "media";
        protected const string EPG_CACHE_KEY_PREFIX = "epg";
        protected const string CACHE_KEY_FORMAT = "{0}_lng{1}";

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public List<int> AssetTypes { get; set; }
        public OrderObj Order { get; set; }
        public string Filter { get; set; }
        public string Query { get; set; }
        public List<string> With { get; set; }
        public List<ePersonalFilter> PersonalFilters
        {
            get;
            set;
        }

        public APIUnifiedSearchLoader(int groupID, PlatformType platform, int domainId, string userIP, int pageSize, int pageIndex,
            List<int> assetTypes, string filter, List<string> with, List<ePersonalFilter> personalFilters, string localeLanguage)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            //DomainId = domainId
            Platform = platform.ToString();
            AssetTypes = assetTypes;
            Filter = filter;
            With = with;
            PersonalFilters = personalFilters;

            Language = TextLocalizationManager.Instance.GetTextLocalization(GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform, true)).GetLanguageDBID(localeLanguage);
        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new UnifiedSearchRequest()
            {
                assetTypes = AssetTypes,
                filterQuery = Filter,
                order = Order,
                nameAndDescription = Query,
                personalFilters = PersonalFilters
            };
        }

        // Cache key for failover support 
        public virtual string GetLoaderCachekey()
        {
            StringBuilder key = new StringBuilder();

            // g = GroupId
            // ps = PageSize
            // pi = PageIndex
            // ob = OrderBy
            // od = OrderDir
            // ov = OrderValue 
            // at = AssetTypes
            //f = filter

            key.AppendFormat("Unified_search_g={0}_ps={1}_pi={2}", GroupID, PageSize, PageIndex);
            if (Order != null)
            {
                key.AppendFormat("_ob={0}_od={1}", Order.m_eOrderBy, Order.m_eOrderDir);
                if (!string.IsNullOrEmpty(Order.m_sOrderValue))
                    key.AppendFormat("_ov={0}", Order.m_sOrderValue);
            }
            if (AssetTypes != null && AssetTypes.Count > 0)
                key.AppendFormat("_at={0}", string.Join(",", AssetTypes.Select(at => at.ToString()).ToArray()));
            if (!string.IsNullOrEmpty(Filter))
                key.AppendFormat("_f={0}", Filter);
            return key.ToString();
        }

        public virtual object Execute()
        {
            object result = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            var res = m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse); // Get the assets ids + update dates from catalog
            if (res == eProviderResult.Success)
            {
                Tvinci.Data.Loaders.TvinciPlatform.Catalog.UnifiedSearchResponse response = (Tvinci.Data.Loaders.TvinciPlatform.Catalog.UnifiedSearchResponse)m_oResponse;
                if (response.status.Code == (int)eStatus.OK)
                {
                    Log("Got:", m_oResponse);
                    result = Process(); // Try get assets from cache, get the missing assets from catalog
                }
                else
                {
                    return new TVPApiModule.Objects.Responses.UnifiedSearchResponse()
                    {
                        Status = new Objects.Responses.Status(response.status.Code, response.status.Message)
                    };
                }
            }
            else if (res == eProviderResult.TimeOut)
            {
                result = new TVPApiModule.Objects.Responses.UnifiedSearchResponse()
                {
                    Status = new Objects.Responses.Status((int)eStatus.Timeout, string.Empty)
                };
            }
            return result;
        }


        // Try get assets from cache
        // Get the missing / not up to date assets from Catalog
        // Return the combined results in the same order returned in UnifiedSearchResponse from Catalog
        protected virtual object Process()
        {
            TVPApiModule.Objects.Responses.UnifiedSearchResponse result = null;

            string cacheKey = GetLoaderCachekey();

            if (m_oResponse == null)// No response from Catalog, gets medias from cache
            {
                m_oResponse = CacheManager.Cache.GetFailOverResponse(cacheKey);

                if (m_oResponse == null)// No response from Catalog and no response from cache
                {
                    result = new Objects.Responses.UnifiedSearchResponse();
                    result.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
                    return result;
                }
            }

            Tvinci.Data.Loaders.TvinciPlatform.Catalog.UnifiedSearchResponse response = (Tvinci.Data.Loaders.TvinciPlatform.Catalog.UnifiedSearchResponse)m_oResponse;

            if (response.status.Code != (int)eStatus.OK) // Bad response from Catalog - return the status
            {
                result = new Objects.Responses.UnifiedSearchResponse();
                result.Status = new Objects.Responses.Status((int)response.status.Code, response.status.Message);
                return result;
            }

            // Add the status and the number of total items to the response
            result = new Objects.Responses.UnifiedSearchResponse();
            result.Status = new Objects.Responses.Status((int)response.status.Code, response.status.Message);
            result.TotalItems = response.m_nTotalItems;

            if (response.searchResults != null && response.searchResults.Count > 0)
            {
                CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey); // Insert the UnifiedSearchResponse to cache for failover support

                List<MediaObj> medias;
                List<ProgramObj> epgs;

                GetAssets(cacheKey, response, out medias, out epgs);

                result.Assets = OrderAndCompleteResults(response.searchResults, medias, epgs); // Gets one list including both medias and epgds, ordered by Catalog order
            }
            else
            {
                result.Assets = new List<AssetInfo>();
            }

            return result;
        }

        /// <summary>
        /// Gets media and epg objects according to list of Ids from search result
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="response"></param>
        /// <param name="medias"></param>
        /// <param name="epgs"></param>
        protected void GetAssets(string cacheKey, Tvinci.Data.Loaders.TvinciPlatform.Catalog.UnifiedSearchResponse response, out List<MediaObj> medias, out List<ProgramObj> epgs)
        {
            // Insert the UnifiedSearchResponse to cache for failover support
            CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey);

            medias = null;
            epgs = null;
            List<long> missingMediaIds = null;
            List<long> missingEpgIds = null;

            if (!GetAssetsFromCache(response.searchResults, out medias, out epgs, out missingMediaIds, out missingEpgIds))
            {
                List<MediaObj> mediasFromCatalog;
                List<ProgramObj> epgsFromCatalog;

                GetAssetsFromCatalog(missingMediaIds, missingEpgIds, out mediasFromCatalog, out epgsFromCatalog); // Get the assets that were missing in cache 

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
        }

        // Returns a list of AssetInfo results from the medias and epgs, ordered by the list of search results from Catalog
        // In case 'With' member contains "stats" - an AssetStatsRequest is made to complete the missing stats data from Catalog
        protected List<AssetInfo> OrderAndCompleteResults(List<UnifiedSearchResult> order, List<MediaObj> medias, List<ProgramObj> epgs)
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

            List<AssetStatsResult> mediaAssetsStats = null;
            List<AssetStatsResult> epgAssetsStats = null;

            bool shouldAddFiles = false;

            if (With != null)
            {
                if (With.Contains("stats")) // if stats are required - gets the stats from Catalog
                {
                    if (medias != null && medias.Count > 0)
                    {
                        mediaAssetsStats = new AssetStatsLoader(GroupID, m_sUserIP, 0, 0, medias.Select(m => int.Parse(m.AssetId)).ToList(),
                            StatsType.MEDIA, DateTime.MinValue, DateTime.MaxValue).Execute() as List<AssetStatsResult>;
                    }
                    if (epgs != null && epgs.Count > 0)
                    {
                        epgAssetsStats = new AssetStatsLoader(GroupID, m_sUserIP, 0, 0, epgs.Select(p => int.Parse(p.AssetId)).ToList(),
                            StatsType.EPG, DateTime.MinValue, DateTime.MaxValue).Execute() as List<AssetStatsResult>;
                    }
                }
                if (With.Contains("files")) // if stats are required - add a flag 
                {
                    shouldAddFiles = true;
                }
            }

            // Build the AssetInfo objects
            foreach (var item in order)
            {
                if (item.AssetType == Tvinci.Data.Loaders.TvinciPlatform.Catalog.eAssetTypes.MEDIA)
                {
                    media = medias.Where(m => m != null && m.AssetId == item.AssetId).FirstOrDefault();
                    if (media != null)
                    {
                        if (mediaAssetsStats != null && mediaAssetsStats.Count > 0)
                        {
                            asset = new AssetInfo(media, mediaAssetsStats.Where(mas => mas.m_nAssetID.ToString() == media.AssetId).FirstOrDefault(), shouldAddFiles);
                        }
                        else
                        {
                            asset = new AssetInfo(media, shouldAddFiles);
                        }
                        result.Add(asset);
                        media = null;
                    }
                }
                else if (item.AssetType == Tvinci.Data.Loaders.TvinciPlatform.Catalog.eAssetTypes.EPG)
                {
                    epg = epgs.Where(p => p != null && p.AssetId == item.AssetId).FirstOrDefault();
                    if (epg != null)
                    {
                        if (epgAssetsStats != null && epgAssetsStats.Count > 0)
                        {
                            asset = new AssetInfo(epg.m_oProgram, epgAssetsStats.Where(eas => eas.m_nAssetID.ToString() == epg.AssetId).FirstOrDefault());
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

        protected void GetAssetsFromCatalog(List<long> missingMediaIds, List<long> missingEpgIds, out List<MediaObj> mediasFromCatalog, out List<ProgramObj> epgsFromCatalog)
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
                    m_nGroupID = GroupID,
                    m_nPageIndex = PageIndex,
                    m_nPageSize = PageSize,
                    m_oFilter = m_oFilter,
                    m_sSignature = m_sSignature,
                    m_sSignString = m_sSignString,
                    m_sSiteGuid = SiteGuid,
                    m_sUserIP = m_sUserIP
                };

                BaseResponse response = null;
                eProviderResult providerResult = m_oProvider.TryExecuteGetBaseResponse(request, out response);
                if (providerResult == eProviderResult.Success && response != null)
                {
                    AssetInfoResponse assetInfoResponse = (AssetInfoResponse)response;

                    if (assetInfoResponse.mediaList != null)
                    {
                        if (assetInfoResponse.mediaList.Any(m => m == null))
                        {
                            logger.Warn("APIUnifiedSearchLoader: Received response from Catalog with null media objects");
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
                            logger.Warn("APIUnifiedSearchLoader: Received response from Catalog with null EPG objects");
                        }

                        epgsFromCatalog = assetInfoResponse.epgList.Where(m => m != null).ToList();
                    }
                    else
                    {
                        epgsFromCatalog = new List<ProgramObj>();
                    }

                    // Store in Cache the medias and epgs from Catalog
                    int duration;
                    int.TryParse(ConfigurationManager.AppSettings["Tvinci.DataLoader.CacheLite.DurationInMinutes"], out duration);

                    List<BaseObject> baseObjects = null;

                    if (mediasFromCatalog != null && mediasFromCatalog.Count > 0)
                    {
                        Log("Storing Medias in Cache", mediasFromCatalog);

                        baseObjects = new List<BaseObject>();
                        mediasFromCatalog.ForEach(m => baseObjects.Add(m));

                        CacheManager.Cache.StoreObjects(baseObjects, string.Format(CACHE_KEY_FORMAT, MEDIA_CACHE_KEY_PREFIX, Language), duration);
                    }

                    if (epgsFromCatalog != null && epgsFromCatalog.Count > 0)
                    {
                        Log("Storing EPGs in Cache", epgsFromCatalog);

                        baseObjects = new List<BaseObject>();
                        epgsFromCatalog.ForEach(p => baseObjects.Add(p));

                        CacheManager.Cache.StoreObjects(baseObjects, string.Format(CACHE_KEY_FORMAT, EPG_CACHE_KEY_PREFIX, Language), duration);
                    }
                }
            }
        }


        // Gets medias and epgs from cache
        // Returns true if all assets were found in cache, false if at least one is missing or not up to date
        protected bool GetAssetsFromCache(List<UnifiedSearchResult> ids, out List<MediaObj> medias, out List<ProgramObj> epgs, out List<long> missingMediaIds, out List<long> missingEpgIds)
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
                    key = new CacheKey(id.AssetId, id.m_dUpdateDate);

                    if (id.AssetType == Tvinci.Data.Loaders.TvinciPlatform.Catalog.eAssetTypes.MEDIA)
                    {
                        mediaKeys.Add(key);
                    }

                    else if (id.AssetType == Tvinci.Data.Loaders.TvinciPlatform.Catalog.eAssetTypes.EPG)
                    {
                        epgKeys.Add(key);
                    }
                }

                // Media - Get the medias from cache, Cast the results, return false if at least one is missing
                if (mediaKeys != null && mediaKeys.Count > 0)
                {
                    cacheResults = CacheManager.Cache.GetObjects(mediaKeys, string.Format(CACHE_KEY_FORMAT, MEDIA_CACHE_KEY_PREFIX, Language), out missingMediaIds);
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
                    cacheResults = CacheManager.Cache.GetObjects(epgKeys, string.Format(CACHE_KEY_FORMAT, EPG_CACHE_KEY_PREFIX, Language), out missingEpgIds);
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

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);

            if (obj != null)
            {
                if (obj is List<MediaObj>)
                {
                    sText.AppendFormat("APIUnifiedSearchLoader: GroupID = {0}, PageIndex = {1}, PageSize = {2}", GroupID, PageIndex, PageSize);
                    sText.Append(CatalogHelper.IDsToString(((List<MediaObj>)obj).Select(m => int.Parse(m.AssetId)).ToList(), "MediaIds"));
                }
                if (obj is List<ProgramObj>)
                {
                    sText.AppendFormat("APIUnifiedSearchLoader: GroupID = {0}, PageIndex = {1}, PageSize = {2}", GroupID, PageIndex, PageSize);
                    sText.Append(CatalogHelper.IDsToString(((List<ProgramObj>)obj).Select(p => int.Parse(p.AssetId)).ToList(), "EpgIds"));
                }

            }
            logger.Debug(sText.ToString());
        }
    }


}
