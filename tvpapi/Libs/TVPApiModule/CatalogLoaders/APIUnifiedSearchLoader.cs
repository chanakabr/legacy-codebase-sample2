using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Phx.Lib.Log;
using Tvinci.Data.Loaders;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Manager;
using ApiObjects;
using ApiObjects.SearchObjects;
using Core.Catalog.Request;
using Core.Catalog.Response;
using UnifiedSearchResponse = Core.Catalog.Response.UnifiedSearchResponse;
using Core.Catalog;
using Phx.Lib.Appconfig;

namespace TVPApiModule.CatalogLoaders
{
    public class APIUnifiedSearchLoader : CatalogRequestManager
    {
        protected const string MEDIA_CACHE_KEY_PREFIX = "media";
        protected const string EPG_CACHE_KEY_PREFIX = "epg";
        protected const string NPVR_CACHE_KEY_PREFIX = "npvr";
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

        public string RequestId
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
                personalFilters = PersonalFilters,
                requestId = this.RequestId
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
                UnifiedSearchResponse response = (UnifiedSearchResponse)m_oResponse;
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
                result = new Objects.Responses.UnifiedSearchResponse();
                result.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
                return result;
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
            // also add the request identifier to the reqsposne
            result.RequestId = response.requestId;

            if (response.searchResults != null && response.searchResults.Count > 0)
            {
                List<MediaObj> medias;
                List<ProgramObj> epgs;
                List<ProgramObj> recordings;

                GetAssets(response, out medias, out epgs, out recordings);

                result.Assets = OrderAndCompleteResults(response.searchResults, medias, epgs, recordings); // Gets one list including both medias and epgds, ordered by Catalog order
            }
            else
            {
                result.Assets = new List<AssetInfo>();
            }

            return result;
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
