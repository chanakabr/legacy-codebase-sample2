using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Manager;

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
            TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId response = null;

            var baseProcess = base.Process();

            if (baseProcess is TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId)
            {
                response = baseProcess as TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId;
            }
            else if (baseProcess is TVPApiModule.Objects.Responses.UnifiedSearchResponse)
            {
                var unifiedResponse = baseProcess as TVPApiModule.Objects.Responses.UnifiedSearchResponse;

                response = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId()
                {
                    Assets = unifiedResponse.Assets,
                    RequestId = string.Empty,
                    Status = unifiedResponse.Status,
                    TotalItems = unifiedResponse.TotalItems
                };
            }

            // If we have request id from catalog response - use it
            if (m_oResponse is UnifiedSearchExternalResponse)
            {
                response.RequestId = (m_oResponse as UnifiedSearchExternalResponse).requestId;
            }

            return response;
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
