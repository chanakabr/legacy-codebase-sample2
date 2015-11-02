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
        protected string externalChannelId;
        protected string utcOffset;

        #endregion

        #region Ctor

        public APIRecommendationsLoader(int groupId, PlatformType platform, string userIP, int pageSize, int pageIndex, int domainId, string siteGuid, 
            string localeLanguage, List<string> with, string udid, 
            string deviceType, string externalChannelId, string utcOffset)
            : base(groupId, platform, domainId, userIP, pageSize, pageIndex, new List<int>(), string.Empty, with, null, localeLanguage)
        {
            this.SiteGuid = siteGuid;
            this.DomainId = domainId;
            this.DeviceId = udid;

            this.deviceType = deviceType;
            this.externalChannelId = externalChannelId;
            this.utcOffset = utcOffset;
        }

        #endregion

        #region Override Methods

        protected override void BuildSpecificRequest()
        {
            // build request
            m_oRequest = new ExternalChannelRequest()
            {
                m_sSignature = SignatureKey,
                m_sSignString = m_sSignString,
                deviceId = DeviceId,
                deviceType = deviceType,
                domainId = DomainId,
                externalChannelId = externalChannelId,
                m_nGroupID = GroupID,
                m_nPageIndex = PageIndex,
                m_nPageSize = PageSize,
                m_oFilter = m_oFilter,
                m_sSiteGuid = SiteGuid,
                m_sUserIP = m_sUserIP,
                utcOffset = utcOffset
            };
        }

        public override string GetLoaderCachekey()
        {
            // g = GroupId
            // ps = PageSize
            // pi = PageIndex
            // sg = SiteGuid
            // ec = External Channel ID
            string key = string.Format("Recommendations_g={0}_ps={1}_pi={2}_sg={3}_ec={4}", GroupID, PageSize, PageIndex, SiteGuid, externalChannelId);

            return key;
        }

        #endregion
    }
}
