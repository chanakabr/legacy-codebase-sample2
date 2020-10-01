using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Tvinci.Data.Loaders;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.Catalog;
using ApiObjects;
using ApiObjects.Response;
using TVPApi;
using TVPApiModule.Manager;
using TVPApiModule.Objects.Responses;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;
using ConfigurationManager;

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
            var techConfigFlashVars = GroupsManager.GetGroup(GroupIDParent).GetFlashVars((PlatformType)Enum.Parse(typeof(PlatformType), Platform));
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
                Util.UpdateProgramsTags(epgs, eAssetTypes.EPG, response.assetIds);

                result.Assets = OrderAndCompleteResults(response.assetIds, medias, epgs); // Gets one list including both medias and epgds, ordered by Catalog order
            }
            else
            {
                result.Assets = new List<AssetInfo>();
            }

            return result;
        }

        protected void GetAssets(string cacheKey, MediaIdsStatusResponse response, out List<MediaObj> medias, out List<ProgramObj> epgs)
        {
            // Insert the UnifiedSearchResponse to cache for failover support
            CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey);

            List<long> mediaIds = response.assetIds.Where(asset => asset.AssetType == eAssetTypes.MEDIA).Select(asset => long.Parse(asset.AssetId)).ToList();
            List<long> epgIds = response.assetIds.Where(asset => asset.AssetType == eAssetTypes.EPG).Select(asset => long.Parse(asset.AssetId)).ToList(); ;
            List<ProgramObj> recordings;

            GetAssetsFromCatalog(mediaIds, epgIds, null, out medias, out epgs, out recordings);
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
                if (item.AssetType == eAssetTypes.MEDIA)
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
                else if (item.AssetType == eAssetTypes.EPG)
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
    }
}
