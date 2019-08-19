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
using UnifiedSearchResponse = Core.Catalog.Response.UnifiedSearchResponse;

namespace TVPApiModule.CatalogLoaders
{
    class APIExternalSearchMediaLoader : MultiMediaLoader
    {
        protected const string MEDIA_CACHE_KEY_PREFIX = "media";
        protected const string EPG_CACHE_KEY_PREFIX = "epg";
        protected const string NPVR_CACHE_KEY_PREFIX = "npvr";
        protected const string CACHE_KEY_FORMAT = "{0}_lng{1}";
        private string m_sCulture;
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string Query { get; set; }
        public List<int> MediaTypes { get; set; }
        public List<string> With { get; set; }

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
        public APIExternalSearchMediaLoader(string query, List<int> mediaTypes, int groupID, int groupIDParent, string platform, string userIP, int pageSize, int pageIndex)
            : base(groupID, userIP, pageSize, pageIndex, "0")
        {
            MediaTypes = mediaTypes;
            Query = query;
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

        public override string GetLoaderCachekey()
        {
            StringBuilder key = new StringBuilder();
            key.AppendFormat("external_search_index{0}_size{1}_group{2}", PageIndex, PageSize, GroupID);
            if (MediaTypes != null && MediaTypes.Count > 0)
                key.AppendFormat("_mt={0}", string.Join(",", MediaTypes.Select(type => type.ToString()).ToArray()));
            return key.ToString();

        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new MediaSearchExternalRequest()
            {
                m_nMediaTypes = MediaTypes,
                m_sQuery = Query
            };
        }

        public override object Execute()
        {
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            TVPApiModule.Objects.Responses.UnifiedSearchResponse response = null;

            m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse);
            {
                Log("Got:", m_oResponse);
                response = (TVPApiModule.Objects.Responses.UnifiedSearchResponse)Process();
            }
            return response;
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

            UnifiedSearchResponse response = (UnifiedSearchResponse)m_oResponse;

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
            result.RequestId = response.requestId;

            if (response.searchResults != null && response.searchResults.Count > 0)
            {
                CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey); // Insert the UnifiedSearchResponse to cache for failover support

                List<MediaObj> medias;
                List<ProgramObj> epgs;
                List<ProgramObj> recordings;

                GetAssets(cacheKey, response, out medias, out epgs, out recordings);

                // add extraData to tags only for EPG           
                Util.UpdateProgramsTags(epgs, eAssetTypes.EPG, response.searchResults);
                Util.UpdateProgramsTags(recordings, eAssetTypes.NPVR, response.searchResults);
                //Util.UpdateEPGAndRecordingTags(epgs, recordings, response.searchResults);

                result.Assets = OrderAndCompleteResults(response.searchResults, medias, epgs, recordings); // Gets one list including both medias and epgds, ordered by Catalog order
            }
            else
            {
                result.Assets = new List<AssetInfo>();
            }

            return result;
        }

        protected void GetAssets(string cacheKey, UnifiedSearchResponse response, out List<MediaObj> medias,
                                out List<ProgramObj> epgs, out List<ProgramObj> recordings)
        {
            // Insert the UnifiedSearchResponse to cache for failover support
            CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey);

            medias = null;
            epgs = null;
            recordings = null;
            List<long> missingMediaIds = null;
            List<long> missingEpgIds = null;
            List<long> missingRecordingIds = null;

            if (!GetAssetsFromCache(response.searchResults, out medias, out epgs, out recordings, out missingMediaIds, out missingEpgIds, out missingRecordingIds))
            {
                List<MediaObj> mediasFromCatalog;
                List<ProgramObj> epgsFromCatalog;
                List<ProgramObj> recordingsFromCatalog;

                GetAssetsFromCatalog(missingMediaIds, missingEpgIds, missingRecordingIds, out mediasFromCatalog, out epgsFromCatalog, out recordingsFromCatalog); // Get the assets that were missing in cache 

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

                // Append the recordings from Catalog to the recordings from cache
                if (recordings == null && recordingsFromCatalog != null)
                {
                    recordings = new List<ProgramObj>();
                }

                recordings.AddRange(recordingsFromCatalog);
            }
        }

        // Returns a list of AssetInfo results from the medias and epgs, ordered by the list of search results from Catalog
        // In case 'With' member contains "stats" - an AssetStatsRequest is made to complete the missing stats data from Catalog
        protected List<AssetInfo> OrderAndCompleteResults(List<UnifiedSearchResult> order, List<MediaObj> medias, List<ProgramObj> epgs, List<ProgramObj> recordings)
        {
            List<AssetInfo> result = null;

            if (order == null || ((medias == null || medias.Count == 0) && (epgs == null || epgs.Count == 0) && (recordings == null || recordings.Count == 0)))
            {
                return null;
            }

            result = new List<AssetInfo>();
            AssetInfo asset = null;
            MediaObj media = null;
            ProgramObj epg = null;
            ProgramObj rec = null;

            List<AssetStatsResult> mediaAssetsStats = null;
            List<AssetStatsResult> epgAssetsStats = null;
            List<AssetStatsResult> recAssetsStats = null;

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
                    if (recordings != null && recordings.Count > 0)
                    {
                        recAssetsStats = new AssetStatsLoader(GroupID, m_sUserIP, 0, 0, recordings.Select(p => int.Parse(p.AssetId)).ToList(),
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
                switch (item.AssetType)
                {
                    case eAssetTypes.MEDIA:
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
                        break;
                    case eAssetTypes.EPG:                                            
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
                        break;
                    case eAssetTypes.NPVR:
                        rec = recordings.Where(p => p != null && p.AssetId == item.AssetId).FirstOrDefault();
                        if (rec != null)
                        {
                            if (recAssetsStats != null && recAssetsStats.Count > 0)
                            {
                                asset = new AssetInfo(rec.m_oProgram, recAssetsStats.Where(eas => eas.m_nAssetID.ToString() == rec.AssetId).FirstOrDefault());
                            }
                            else
                            {
                                asset = new AssetInfo(rec.m_oProgram);
                            }
                            result.Add(asset);
                            rec = null;
                        }
                        break;
                    case eAssetTypes.UNKNOWN:
                    default:
                        break;
                }
            }

            return result;
        }

        protected void GetAssetsFromCatalog(List<long> missingMediaIds, List<long> missingEpgIds, List<long> missingRecordingIds, out List<MediaObj> mediasFromCatalog,
                                            out List<ProgramObj> epgsFromCatalog, out List<ProgramObj> recordingsFromCatalog)
        {
            mediasFromCatalog = null;
            epgsFromCatalog = null;
            recordingsFromCatalog = new List<ProgramObj>();

            if ((missingMediaIds != null && missingMediaIds.Count > 0) || (missingEpgIds != null && missingEpgIds.Count > 0) || (missingRecordingIds != null && missingRecordingIds.Count > 0))
            {
                List<long> requestEpgIds = new List<long>();
                if (missingEpgIds != null)
                {
                    requestEpgIds.AddRange(missingEpgIds);
                }

                if (missingRecordingIds != null)
                {
                    requestEpgIds.AddRange(missingRecordingIds);
                }

                // Build AssetInfoRequest with the missing ids
                AssetInfoRequest request = new AssetInfoRequest()
                {
                    epgIds = requestEpgIds,
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
                    int.TryParse(System.Configuration.ConfigurationManager.AppSettings["Tvinci.DataLoader.CacheLite.DurationInMinutes"], out duration);

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
                        recordingsFromCatalog = new List<ProgramObj>();
                        if (missingRecordingIds != null)
                        {
                            foreach (long recordingId in missingRecordingIds)
                            {
                                try
                                {
                                    int index = epgsFromCatalog.FindIndex(x => x.AssetId == recordingId.ToString());
                                    recordingsFromCatalog.Add(epgsFromCatalog[index]);
                                    epgsFromCatalog.RemoveAt(index);
                                }
                                catch (Exception ex)
                                {
                                    Log(string.Format("recording with id {0} wasn't found", recordingId), null);
                                }

                            }
                        }

                        if (recordingsFromCatalog != null && recordingsFromCatalog.Count > 0)
                        {
                            List<BaseObject> npvrBaseObjects = new List<BaseObject>();
                            recordingsFromCatalog.ForEach(p => npvrBaseObjects.Add(p));
                            Log("Storing NPVRs in Cache", recordingsFromCatalog);
                            CacheManager.Cache.StoreObjects(npvrBaseObjects, string.Format(CACHE_KEY_FORMAT, NPVR_CACHE_KEY_PREFIX, Language), duration);
                        }

                        baseObjects = new List<BaseObject>();
                        epgsFromCatalog.ForEach(p => baseObjects.Add(p));
                        Log("Storing EPGs in Cache", epgsFromCatalog);
                        CacheManager.Cache.StoreObjects(baseObjects, string.Format(CACHE_KEY_FORMAT, EPG_CACHE_KEY_PREFIX, Language), duration);
                    }
                }
            }
        }


        // Gets medias and epgs from cache
        // Returns true if all assets were found in cache, false if at least one is missing or not up to date
        protected bool GetAssetsFromCache(List<UnifiedSearchResult> ids, out List<MediaObj> medias, out List<ProgramObj> epgs, out List<ProgramObj> recordings,
                                            out List<long> missingMediaIds, out List<long> missingEpgIds, out List<long> missingRecordingIds)
        {
            bool result = true;
            medias = null;
            epgs = null;
            recordings = null;
            missingMediaIds = null;
            missingEpgIds = null;
            missingRecordingIds = null;

            if (ids != null && ids.Count > 0)
            {
                List<BaseObject> cacheResults = null;

                List<CacheKey> mediaKeys = new List<CacheKey>();
                List<CacheKey> epgKeys = new List<CacheKey>();
                List<CacheKey> recordingKeys = new List<CacheKey>();

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
                        case eAssetTypes.NPVR:
                            recordingKeys.Add(key);
                            break;
                        case eAssetTypes.UNKNOWN:
                        default:
                            break;
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

                // RECORDINGS - Get the recordings from cache, Cast the results, return false if at least one is missing
                if (recordingKeys != null && recordingKeys.Count > 0)
                {
                    cacheResults = CacheManager.Cache.GetObjects(recordingKeys, string.Format(CACHE_KEY_FORMAT, NPVR_CACHE_KEY_PREFIX, Language), out missingRecordingIds);
                    if (cacheResults != null && cacheResults.Count > 0)
                    {
                        recordings = new List<ProgramObj>();
                        foreach (var res in cacheResults)
                        {
                            recordings.Add((ProgramObj)res);
                        }
                    }

                    if (missingRecordingIds != null && missingRecordingIds.Count > 0)
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
                switch (obj.GetType().ToString())
                {
                    case "MediaSearchExternalRequest":
                        MediaSearchExternalRequest searchRquest = obj as MediaSearchExternalRequest;
                        sText.AppendFormat("MediaExternalSearchRequest: Query = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}", searchRquest.m_sQuery, searchRquest.m_nGroupID, searchRquest.m_nPageIndex, searchRquest.m_nPageSize);
                        break;
                    case "MediaIdsStatusResponse":
                        //MediaIdsStatusResponse mediaIdsResponse = obj as MediaIdsStatusResponse;
                        //sText.AppendFormat("MediaIdsResponse for Ralated: TotalItems = {0}, ", mediaIdsResponse.m_nTotalItems);
                        //sText.AppendLine(mediaIdsResponse.m_nMediaIds.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }


    }
}
