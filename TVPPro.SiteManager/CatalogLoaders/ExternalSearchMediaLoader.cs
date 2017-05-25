using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Objects;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class ExternalSearchMediaLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string Query { get; set; }
        public List<int> MediaTypes { get; set; }
        public string RequestId { get; set; }
        public Status Status { get; set; }
        public int TotalResults { get; set; }

        #region Constructors
        public ExternalSearchMediaLoader(string query, List<int> mediaTypes, int groupID, string userIP, int pageSize, int pageIndex)
            : base(groupID, userIP, pageSize, pageIndex, "0")
        {
            MediaTypes = mediaTypes;
            Query = query;
        }

        public ExternalSearchMediaLoader(string query, List<int> mediaTypes, string userName, string userIP, int pageSize, int pageIndex)
            : this(query, mediaTypes, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex)
        {
        }
        #endregion

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

        public virtual object Execute()
        {
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            MediaIdsStatusResponse obj = null;

            m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse);
            {
                Log("Got:", m_oResponse);
                obj = (MediaIdsStatusResponse)Process();
            }

            MediaIdsStatusResponse response = m_oResponse as MediaIdsStatusResponse;

            if (response != null)
            {
                this.RequestId = response.RequestId;
                this.Status = response.Status;
                this.TotalResults = response.m_nTotalItems;
            }
            return obj;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaSearchExternalRequest":
                        MediaSearchExternalRequest searchRquest = obj as MediaSearchExternalRequest;
                        sText.AppendFormat("MediaExternalSearchRequest: Query = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}", searchRquest.m_sQuery, searchRquest.m_nGroupID, searchRquest.m_nPageIndex, searchRquest.m_nPageSize);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaIdsStatusResponse":
                        MediaIdsStatusResponse mediaIdsResponse = obj as MediaIdsStatusResponse;
                        sText.AppendFormat("MediaIdsResponse for Ralated: TotalItems = {0}, ", mediaIdsResponse.m_nTotalItems);
                        sText.AppendLine(mediaIdsResponse.m_nMediaIds.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }

        protected virtual object Process()
        {
            string cacheKey = GetLoaderCachekey();

            if (m_oResponse is UnifiedSearchResponse)
            {
                UnifiedSearchResponse usr = (m_oResponse as UnifiedSearchResponse);
                MediaIdsStatusResponse newResp = new MediaIdsStatusResponse()
                {
                    ExtensionData = usr.ExtensionData,
                    m_lObj = usr.m_lObj,
                    Status = usr.status,
                    m_nTotalItems = usr.m_nTotalItems,
                    m_nMediaIds = usr.searchResults.Select(x => new SearchResult() { assetID = int.Parse(x.AssetId), ExtensionData = x.ExtensionData, UpdateDate = x.m_dUpdateDate }).ToList()
                };
                m_oResponse = newResp;
            }

            if (m_oResponse != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds.Count > 0)
            {
                CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey);
                m_oMediaCache = new MediaCache(((MediaIdsResponse)m_oResponse).m_nMediaIds, GroupID, m_sUserIP, m_oFilter);
            }
            else if (m_oResponse == null)// No Response from Catalog, gets medias from cache
            {
                m_oResponse = CacheManager.Cache.GetFailOverResponse(cacheKey);
                if (m_oResponse != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds.Count > 0)
                {
                    m_oMediaCache = new MediaCache(((MediaIdsResponse)m_oResponse).m_nMediaIds, GroupID, m_sUserIP, m_oFilter);
                }
            }
            if (m_oMediaCache != null)
            {
                m_oMediaCache.BuildRequest();
                m_oResponse.m_lObj = (List<BaseObject>)m_oMediaCache.Execute();
            }
            return m_oResponse;

        }
    }
}
