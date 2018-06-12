using KLogMonitor;
using System.Collections.Generic;
using System.Reflection;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects.Responses;

namespace TVPApiModule.CatalogLoaders
{
    public class APIRecommendationsLoader : APIUnifiedSearchLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Data Members

        protected string deviceType;
        protected string internalChannelId;
        protected string externalChannelId;
        protected string utcOffset;
        protected string free;

        #endregion

        #region Ctor

        public APIRecommendationsLoader(int groupId, PlatformType platform, string userIP, int pageSize, int pageIndex, int domainId, string siteGuid,
            string localeLanguage, List<string> with, string udid,
            string deviceType, string externalChannelId, string utcOffset, string filterQuery, string internalChannelId, string free)
            : base(groupId, platform, domainId, userIP, pageSize, pageIndex, new List<int>(), string.Empty, with, null, localeLanguage)
        {
            this.SiteGuid = siteGuid;
            this.DomainId = domainId;
            this.DeviceId = udid;

            this.deviceType = deviceType;
            this.externalChannelId = externalChannelId;
            this.internalChannelId = internalChannelId;
            this.utcOffset = utcOffset;
            this.Filter = filterQuery;
            this.free = free;
        }

        #endregion

        #region Override Methods

        protected override void BuildSpecificRequest()
        {
            // build request
            m_oRequest = new ExternalChannelRequest()
            {
                type = eChannelType.External,
                m_sSignature = SignatureKey,
                m_sSignString = m_sSignString,
                deviceId = DeviceId,
                deviceType = deviceType,
                domainId = DomainId,
                internalChannelID = internalChannelId,
                externalChannelID = externalChannelId,
                m_nGroupID = GroupID,
                m_nPageIndex = PageIndex,
                m_nPageSize = PageSize,
                m_oFilter = m_oFilter,
                m_sSiteGuid = SiteGuid,
                m_sUserIP = m_sUserIP,
                utcOffset = utcOffset,
                filterQuery = this.Filter,
                free = this.free
            };
        }

        /// <summary>
        /// Correct casting of response object to include request ID
        /// </summary>
        /// <returns></returns>
        public override object Execute()
        {
            TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId response = null;

            var baseResult = base.Execute();

            if (baseResult is TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId)
            {
                response = baseResult as TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId;
            }
            else if (baseResult is TVPApiModule.Objects.Responses.UnifiedSearchResponse)
            {
                var unifiedResponse = baseResult as TVPApiModule.Objects.Responses.UnifiedSearchResponse;

                response = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId()
                {
                    Assets = unifiedResponse.Assets,
                    RequestId = string.Empty,
                    Status = unifiedResponse.Status,
                    TotalItems = unifiedResponse.TotalItems
                };
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
            // also add the request identifier to the response
            result.RequestId = response.requestId;

            if (response.searchResults != null && response.searchResults.Count > 0)
            {
                CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey); // Insert the UnifiedSearchResponse to cache for failover support

                List<MediaObj> medias;
                List<ProgramObj> epgs;

                GetAssets(cacheKey, response, out medias, out epgs);

                // add extraData to tags only for EPG
                Util.UpdateEPGTags(epgs, response.searchResults);

                result.Assets = OrderAndCompleteResults(response.searchResults, medias, epgs); // Gets one list including both medias and epgds, ordered by Catalog order
            }
            else
            {
                result.Assets = new List<AssetInfo>();
            }

            return result;
        }

        public override string GetLoaderCachekey()
        {
            // g = GroupId
            // ps = PageSize
            // pi = PageIndex
            // sg = SiteGuid
            // ec = External Channel ID
            // f = filter query
            string key = string.Format("Recommendations_g={0}_ps={1}_pi={2}_sg={3}_ec={4}_f={5}", GroupID, PageSize, PageIndex, SiteGuid, externalChannelId, this.Filter);

            return key;
        }

        #endregion
    }
}
