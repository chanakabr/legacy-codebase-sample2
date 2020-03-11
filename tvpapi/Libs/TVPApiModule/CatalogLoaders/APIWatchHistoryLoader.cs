using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders;
using Core.Catalog;
using ApiObjects.SearchObjects;
using Core.Catalog.Request;
using Core.Catalog.Response;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;
using KLogMonitor;
using System.Reflection;
using ApiObjects;
using ConfigurationManager;

namespace TVPApiModule.CatalogLoaders
{
    public class APIWatchHistoryLoader : CatalogRequestManager
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string MEDIA_CACHE_KEY_PREFIX = "media";
        private const string CACHE_KEY_FORMAT = "{0}_lng{1}";

        public List<int> AssetTypes { get; set; }
        public OrderDir OrderDir { get; set; }
        public eWatchStatus FilterStatus { get; set; }
        public int NumOfDays { get; set; }
        public List<string> With { get; set; }


        public APIWatchHistoryLoader(int groupID, PlatformType platform, int domainId, string userIP, int pageSize, int pageIndex,
            List<int> assetTypes, eWatchStatus filterStatus, List<string> with, int numOfDays, OrderDir orderDir)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            this.OrderDir = orderDir;
            this.Platform = platform.ToString();
            this.AssetTypes = assetTypes;
            this.FilterStatus = filterStatus;
            this.With = with;
            this.NumOfDays = numOfDays;
        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new WatchHistoryRequest()
            {
                AssetTypes = AssetTypes,
                FilterStatus = FilterStatus,
                OrderDir = OrderDir,
                NumOfDays = NumOfDays
            };
        }

        public virtual object Execute()
        {
            WatchHistory result = new WatchHistory();
            BuildRequest();
            Log("TryExecuteGetBaseResponse APIWatchHistoryLoader:", m_oRequest);

            // get asset IDs from catalog
            var res = m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse);
            if (res == eProviderResult.Success)
            {
                WatchHistoryResponse response = (WatchHistoryResponse)m_oResponse;
                if (response.status.Code == (int)eStatus.OK && response.result.Count > 0)
                {
                    // request passed - process it
                    Log("APIWatchHistoryLoader Got:", m_oResponse);
                    result = (WatchHistory)Process();
                }
                else
                {
                    // request failed
                    result.Status = new Objects.Responses.Status() { Code = response.status.Code, Message = response.status.Message };
                }
            }
            else if (res == eProviderResult.TimeOut)
            {
                // request timed out
                result.Status = new Objects.Responses.Status() { Code = (int)eStatus.Timeout, Message = string.Empty };
            }

            return result;
        }

        protected virtual object Process()
        {
            WatchHistoryResponse catalogWatchHistoryResult = (WatchHistoryResponse)m_oResponse;

            // update total items, status and message
            WatchHistory clientResponse = new WatchHistory()
            {
                Status = new Objects.Responses.Status((int)catalogWatchHistoryResult.status.Code, catalogWatchHistoryResult.status.Message),
                TotalItems = catalogWatchHistoryResult.m_nTotalItems
            };

            if (catalogWatchHistoryResult.result != null && catalogWatchHistoryResult.result.Count > 0)
            {
                List<MediaObj> medias = new List<MediaObj>();
                List<long> missingMediaIds = new List<long>();

                // get medias from cache
                if (!GetAssetsFromCache(catalogWatchHistoryResult.result, out medias, out missingMediaIds))
                {
                    // Get the assets that were missing in cache 
                    List<MediaObj> mediasFromCatalog = new List<MediaObj>();
                    GetAssetsFromCatalog(missingMediaIds, out mediasFromCatalog);
                    medias.AddRange(mediasFromCatalog);
                }

                // Gets one list ordered by Catalog order
                clientResponse.Assets = BuildClientResult(catalogWatchHistoryResult.result, medias);
            }

            return clientResponse;
        }

        private bool GetAssetsFromCache(List<UserWatchHistory> userWatchHistory, out List<MediaObj> medias, out List<long> missingMediaIds)
        {
            bool result = true;
            medias = new List<MediaObj>();
            missingMediaIds = new List<long>();

            if (userWatchHistory != null && userWatchHistory.Count > 0)
            {
                List<BaseObject> cacheResults = null;
                CacheKey key = null;
                List<CacheKey> mediaKeys = new List<CacheKey>();

                // build cache keys for non NPVR
                foreach (var watchHistory in userWatchHistory)
                {
                    if (watchHistory.AssetTypeId != (int)TVPApiModule.Objects.Enums.eAssetFilterTypes.NPVR)
                    {
                        key = new CacheKey(watchHistory.AssetId, watchHistory.m_dUpdateDate);
                        mediaKeys.Add(key);
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
                            medias.Add((MediaObj)res);
                    }

                    if (missingMediaIds != null && missingMediaIds.Count > 0)
                        result = false;
                }
            }

            return result;
        }

        private void GetAssetsFromCatalog(List<long> missingMediaIds, out List<MediaObj> mediasFromCatalog)
        {
            mediasFromCatalog = null;

            if (missingMediaIds != null && missingMediaIds.Count > 0)
            {
                // Build AssetInfoRequest with the missing ids
                AssetInfoRequest request = new AssetInfoRequest()
                {
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
                            logger.Warn("APIWatchHistoryLoader: Received response from Catalog with null media objects");

                        mediasFromCatalog = assetInfoResponse.mediaList.Where(m => m != null).ToList();
                    }
                    else
                        mediasFromCatalog = new List<MediaObj>();


                    // Store in Cache the medias from Catalog
                    int duration = ApplicationConfiguration.Current.TVPApiConfiguration.CacheLiteDurationInMinutes.Value;

                    List<BaseObject> baseObjects = null;

                    if (mediasFromCatalog != null && mediasFromCatalog.Count > 0)
                    {
                        Log("Storing Medias in Cache", mediasFromCatalog);

                        baseObjects = new List<BaseObject>();
                        mediasFromCatalog.ForEach(m => baseObjects.Add(m));

                        CacheManager.Cache.StoreObjects(baseObjects, string.Format(CACHE_KEY_FORMAT, MEDIA_CACHE_KEY_PREFIX, Language), duration);
                    }
                }
            }
        }

        private List<WatchHistoryAsset> BuildClientResult(List<UserWatchHistory> watchHistoryResult, List<MediaObj> mediasInfo)
        {
            List<WatchHistoryAsset> result = new List<WatchHistoryAsset>();
            MediaObj media = new MediaObj();
            WatchHistoryAsset asset = null;
            List<AssetStatsResult> mediaAssetsStats = null;
            bool shouldAddFiles = false;

            if (With != null)
            {
                if (With.Contains("stats"))
                {
                    // gets the stats from Catalog
                    if (mediasInfo != null && mediasInfo.Count > 0)
                    {
                        mediaAssetsStats = new AssetStatsLoader(GroupID, m_sUserIP, 0, 0, mediasInfo.Select(m => int.Parse(m.AssetId)).ToList(),
                            StatsType.MEDIA, DateTime.MinValue, DateTime.MaxValue).Execute() as List<AssetStatsResult>;
                    }
                }

                if (With.Contains("files"))
                {
                    // stats are required - add a flag 
                    shouldAddFiles = true;
                }
            }

            // Build the AssetInfo objects
            foreach (var item in watchHistoryResult)
            {
                if (item.AssetTypeId == (int)TVPApiModule.Objects.Enums.eAssetFilterTypes.NPVR)
                {
                    // build NPVR assets objects
                    NpvrRecordAsset npvr = new NpvrRecordAsset() { Id = item.AssetId, Type = item.AssetTypeId };
                    result.Add(new WatchHistoryAsset()
                    {
                        Asset = new AssetInfo(npvr),
                        LastWatched = item.LastWatch,
                        Position = item.Location,
                        IsFinishedWatching = item.IsFinishedWatching,
                        Duration = item.Duration
                    });
                }
                else
                {
                    // build medias assets objects
                    media = mediasInfo.Where(m => m != null && m.AssetId == item.AssetId).FirstOrDefault();
                    if (media != null)
                    {
                        if (mediaAssetsStats != null && mediaAssetsStats.Count > 0)
                        {
                            // with stats
                            asset = new WatchHistoryAsset()
                            {
                                Asset = new AssetInfo(media, mediaAssetsStats.Where(mas => mas.m_nAssetID == int.Parse(media.AssetId)).FirstOrDefault(), shouldAddFiles),
                                LastWatched = item.LastWatch,
                                Position = item.Location,
                                IsFinishedWatching = item.IsFinishedWatching,
                                Duration = item.Duration
                            };
                        }
                        else
                        {
                            // without stats
                            asset = new WatchHistoryAsset()
                            {
                                Asset = new AssetInfo(media, shouldAddFiles),
                                LastWatched = item.LastWatch,
                                Position = item.Location,
                                IsFinishedWatching = item.IsFinishedWatching,
                                Duration = item.Duration
                            };
                        }

                        result.Add(asset);
                        media = null;
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
                    sText.AppendFormat("APIWatchHistoryLoader: GroupID = {0}, PageIndex = {1}, PageSize = {2}", GroupID, PageIndex, PageSize);
                    sText.Append(CatalogHelper.IDsToString(((List<MediaObj>)obj).Select(m => int.Parse(m.AssetId)).ToList(), "MediaIds"));
                }
            }
            logger.Debug(sText.ToString());
        }
    }
}
