using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;

namespace TVPApiModule.CatalogLoaders
{
    public class APIUnifiedSearchLoader : CatalogRequestManager
    {
        private const string MEDIA_CACHE_KEY_PREFIX = "media";
        private const string EPG_CACHE_KEY_PREFIX = "epg";

        private static ILog logger = log4net.LogManager.GetLogger(typeof(APIUnifiedSearchLoader));

        public List<int> AssetTypes { get; set; }
        public Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy OrderBy { get; set; }
        public OrderObj Order { get; set; }
        public string Filter{ get; set; }
        public string Query { get; set; }
        public List<string> With { get; set; }

        public APIUnifiedSearchLoader(int groupID, PlatformType platform, int domainId, string userIP, int pageSize, int pageIndex,
            List<int> assetTypes, string query, string filter, List<string> with)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            //DomainId = domainId
            Platform = platform.ToString();
            AssetTypes = assetTypes;
            Filter = filter;
            Query = query;
            With = with;

        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new UnifiedSearchRequest()
            {
                assetTypes = AssetTypes,
                filterQuery = Filter,
                order = Order,
                nameAndDescription = Query
            };
        }

        // for failover support
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
            // q = query
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
            if (!string.IsNullOrEmpty(Query))
                key.AppendFormat("_q={0}", Query);
            if (!string.IsNullOrEmpty(Filter))
                key.AppendFormat("_f={0}", Filter);
            return key.ToString();
        }

        public virtual object Execute()
        {
            object result = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            var res = m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse);
            if (res == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                result = Process();
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

        protected virtual object Process()
        {
            TVPApiModule.Objects.Responses.UnifiedSearchResponse result = null;
            //List<AssetInfo> result = null;

            string cacheKey = GetLoaderCachekey();

            if (m_oResponse == null)// No Response from Catalog, gets medias from cache
            {
                m_oResponse = CacheManager.Cache.GetFailOverResponse(cacheKey);
            }

            if (m_oResponse == null)// No Response from Catalog and no response from cache
            {
                result = new Objects.Responses.UnifiedSearchResponse();
                result.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
                return result;
            }

            Tvinci.Data.Loaders.TvinciPlatform.Catalog.UnifiedSearchResponse response = (Tvinci.Data.Loaders.TvinciPlatform.Catalog.UnifiedSearchResponse)m_oResponse;

            if (response.status.Code != (int)eStatus.OK) // bad response
            {
                result = new Objects.Responses.UnifiedSearchResponse();
                result.Status = new Objects.Responses.Status((int)response.status.Code, response.status.Message);
                return result;
            }

            result = new Objects.Responses.UnifiedSearchResponse();
            //result.Status = new Status((int)m_oResponse.status.Code, m_oResponse.status.Message)
            result.TotalItems = response.m_nTotalItems;

            if (response.searchResults!= null && response.searchResults.Count > 0)
            {
                CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey);

                List<MediaObj> medias = null;
                List<ProgramObj> epgs = null;
                List<long> missingMediaIds = null;
                List<long> missingEpgIds = null;

                if (!GetAssetsFromCache(response.searchResults, out medias, out epgs, out missingMediaIds, out missingEpgIds))
                {
                    GetAssetsFromCatalog(missingMediaIds, missingEpgIds, out medias, out epgs);
                }

                result.Assets = OrderResults(response.searchResults, medias, epgs);
            }
            else
            {
                result.Assets = new List<AssetInfo>();
            }

            return result;
        }

        private List<AssetInfo> OrderResults(List<UnifiedSearchResult> order, List<MediaObj> medias, List<ProgramObj> epgs)
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

            if (With != null && With.Contains("stats"))
            {
                if (medias != null && medias.Count > 0)
                {
                    mediaAssetsStats = new AssetStatsLoader(GroupID, m_sUserIP, 0, 0, medias.Select(m => m.m_nID).ToList(),
                        StatsType.MEDIA, DateTime.MinValue, DateTime.MaxValue).Execute() as List<AssetStatsResult>;
                }
                if (epgs != null && epgs.Count > 0)
                {
                    epgAssetsStats = new AssetStatsLoader(GroupID, m_sUserIP, 0, 0, medias.Select(m => m.m_nID).ToList(),
                        StatsType.EPG, DateTime.MinValue, DateTime.MaxValue).Execute() as List<AssetStatsResult>;
                }
            }

            foreach (var item in order)
            {
                if (item.type == Tvinci.Data.Loaders.TvinciPlatform.Catalog.AssetType.Media)
                {
                    media = medias.Where(m =>  m != null && m.m_nID == item.assetID).FirstOrDefault();
                    if (media != null)
                    {
                        if (mediaAssetsStats != null && mediaAssetsStats.Count > 0)
                        {
                            asset = new AssetInfo(media, mediaAssetsStats.Where(mas => mas.m_nAssetID == media.m_nID).FirstOrDefault());
                        }
                        else
                        {
                            asset = new AssetInfo(media);
                        }
                        result.Add(asset);
                        media = null;
                    }
                }
                else if (item.type == Tvinci.Data.Loaders.TvinciPlatform.Catalog.AssetType.Epg)
                {
                    epg = epgs.Where(p => p!= null && p.m_nID == item.assetID).FirstOrDefault();
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

        private void GetAssetsFromCatalog(List<long> missingMediaIds, List<long> missingEpgIds, out List<MediaObj> medias, out List<ProgramObj> epgs)
        {
            medias = null; 
            epgs = null;

            if ((missingMediaIds != null && missingMediaIds.Count > 0) || (missingEpgIds != null && missingEpgIds.Count > 0))
            {
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
                    medias = assetInfoResponse.mediaList;
                    epgs = assetInfoResponse.epgList;
                   
                    // Store in Cache the medias from Catalog
                    int duration;
                    int.TryParse(ConfigurationManager.AppSettings["Tvinci.DataLoader.CacheLite.DurationInMinutes"], out duration);

                    List<BaseObject> baseObjects = null;

                    if (medias != null && medias.Count > 0)
                    {
                        //Log("Storing Medias in Cache", medias);

                        baseObjects = new List<BaseObject>();
                        medias.ForEach(m => baseObjects.Add(m));

                        CacheManager.Cache.StoreObjects(baseObjects, string.Format("{0}_lng{1}", MEDIA_CACHE_KEY_PREFIX, Language), duration);
                    }

                    if (epgs != null && epgs.Count > 0)
                    {
                        //Log("Storing EPGs in Cache", epgs);

                        baseObjects = new List<BaseObject>();
                        epgs.ForEach(p => baseObjects.Add(p));

                        CacheManager.Cache.StoreObjects(baseObjects, string.Format("{0}_lng{1}", EPG_CACHE_KEY_PREFIX, Language), duration);
                    }
                }
            }
        }
        
        private bool GetAssetsFromCache(List<UnifiedSearchResult> ids, out List<MediaObj> medias, out List<ProgramObj> epgs, out List<long> missingMediaIds, out List<long> missingEpgIds)
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

                foreach (var id in ids)
                {
                    if (id.type == Tvinci.Data.Loaders.TvinciPlatform.Catalog.AssetType.Media)
                    {
                        key = new CacheKey(id.assetID, id.UpdateDate);
                        mediaKeys.Add(key);
                    }

                    else if (id.type == Tvinci.Data.Loaders.TvinciPlatform.Catalog.AssetType.Epg)
                    {
                        key = new CacheKey(id.assetID, id.UpdateDate);
                        epgKeys.Add(key);
                    }
                    key = null;
                }

                // Media
                if (mediaKeys != null && mediaKeys.Count > 0)
                {
                    cacheResults = CacheManager.Cache.GetObjects(mediaKeys, string.Format("{0}_lng{1}", MEDIA_CACHE_KEY_PREFIX, Language), out missingMediaIds);
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

                // EPG
                if (epgKeys != null && epgKeys.Count > 0)
                {
                    cacheResults = CacheManager.Cache.GetObjects(epgKeys, string.Format("{0}_lng{1}", EPG_CACHE_KEY_PREFIX, Language), out missingEpgIds);
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
                    sText.Append(CatalogHelper.IDsToString(((List<MediaObj>)obj).Select(m => m.m_nID).ToList(), "MediaIds"));
                }
                if (obj is List<ProgramObj>)
                {
                    sText.AppendFormat("APIUnifiedSearchLoader: GroupID = {0}, PageIndex = {1}, PageSize = {2}", GroupID, PageIndex, PageSize);
                    sText.Append(CatalogHelper.IDsToString(((List<ProgramObj>)obj).Select(p => p.m_nID).ToList(), "EpgIds"));
                }
                   
            }
            logger.Debug(sText.ToString());
        }
    }


}
