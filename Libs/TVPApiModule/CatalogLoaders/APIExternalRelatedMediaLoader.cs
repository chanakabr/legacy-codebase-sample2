using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Manager;
using TVPApiModule.Objects.Responses;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;

namespace TVPApiModule.CatalogLoaders
{
    class APIExternalRelatedMediaLoader : ExternalRelatedMediaLoader
    {
        protected const string MEDIA_CACHE_KEY_PREFIX = "media";
        protected const string EPG_CACHE_KEY_PREFIX = "epg";
        protected const string CACHE_KEY_FORMAT = "{0}_lng{1}";

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private string m_sCulture;

        public string Culture
        {
            get { return m_sCulture; }
            set
            {
                m_sCulture = value;
                Language = TextLocalizationManager.Instance.GetTextLocalization(GroupIDParent, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).GetLanguageDBID(value);
            }
        }

        public int GroupIDParent { get; set; }

        #region Constructors
        public APIExternalRelatedMediaLoader(int mediaID, List<int> mediaTypes, int groupID, int groupIDParent, string platform, string userIP, int pageSize, int pageIndex, string picSize, string freeParam)
            : base(mediaID, mediaTypes, groupID, userIP, pageSize, pageIndex, picSize, freeParam)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
            Platform = platform;
        }
        #endregion

        public object ApiExecuteMultiMediaAdapter(List<BaseObject> medias)
        {
            FlashVars techConfigFlashVars = ConfigManager.GetInstance().GetConfig(GroupIDParent, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).TechnichalConfiguration.Data.TVM.FlashVars;
            string fileFormat = techConfigFlashVars.FileFormat;
            string subFileFormat = (techConfigFlashVars.SubFileFormat.Split(';')).FirstOrDefault();
            return CatalogHelper.MediaObjToDsItemInfo(medias, PicSize, fileFormat, subFileFormat);
        }

        protected override object Process()
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

            MediaIdsStatusResponse response = (MediaIdsStatusResponse)m_oResponse;

            if (response.Status.Code != (int)eStatus.OK) // Bad response from Catalog - return the status
            {
                result = new Objects.Responses.UnifiedSearchResponse();
                result.Status = new Objects.Responses.Status((int)response.Status.Code, response.Status.Message);
                return result;
            }

            // Add the status and the number of total items to the response
            result = new Objects.Responses.UnifiedSearchResponse();
            result.Status = new Objects.Responses.Status((int)response.Status.Code, response.Status.Message);
            result.TotalItems = response.m_nTotalItems;
            result.RequestId = response.RequestId;

            if (response.assetIds != null && response.assetIds.Count > 0)
            {
                CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey); // Insert the UnifiedSearchResponse to cache for failover support

                List<MediaObj> medias;
                List<ProgramObj> epgs;

                GetAssets(cacheKey, response, out medias, out epgs);

                // add extraData to tags only for EPG
                Util.UpdateEPGTags(epgs, response.assetIds);

                result.Assets = OrderAndCompleteResults(response.assetIds, medias, epgs); // Gets one list including both medias and epgds, ordered by Catalog order
            }
            else
            {
                result.Assets = new List<AssetInfo>();
            }

            return result;
        }

        protected void GetAssets(string cacheKey, Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaIdsStatusResponse response, out List<MediaObj> medias, out List<ProgramObj> epgs)
        {
            // Insert the UnifiedSearchResponse to cache for failover support
            CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey);

            medias = null;
            epgs = null;
            List<long> missingMediaIds = null;
            List<long> missingEpgIds = null;

            if (!GetAssetsFromCache(response.assetIds, out medias, out epgs, out missingMediaIds, out missingEpgIds))
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

    }
}
