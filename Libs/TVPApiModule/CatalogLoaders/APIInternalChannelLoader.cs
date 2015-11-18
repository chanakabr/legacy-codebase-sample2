using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;

namespace TVPApiModule.CatalogLoaders
{
    public class APIInternalChannelLoader : APIUnifiedSearchLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        #region Data Members

        protected string internalChannelId;
        protected string externalChannelId;

        #endregion

        #region Ctor

        public APIInternalChannelLoader(int groupId, PlatformType platform, string userIP, int pageSize, int pageIndex, int domainId, string siteGuid, 
            string localeLanguage, List<string> with, string externalChannelId, string filterQuery)
            : base(groupId, platform, domainId, userIP, pageSize, pageIndex, new List<int>(), string.Empty, with, null, localeLanguage)
        {
            this.SiteGuid = siteGuid;
            this.DomainId = domainId;
            this.externalChannelId = externalChannelId;
            this.Filter = filterQuery;
        }

        #endregion

        #region Override Methods

        protected override void BuildSpecificRequest()
        {
            // build request
            m_oRequest = new InternalChannelRequest()
            {
                type = eChannelType.Internal,
                m_sSignature = SignatureKey,
                m_sSignString = m_sSignString,
                domainId = DomainId,
                internalChannelID = internalChannelId,
                externalChannelID = externalChannelId,
                m_nGroupID = GroupID,
                m_nPageIndex = PageIndex,
                m_nPageSize = PageSize,
                m_oFilter = m_oFilter,
                m_sSiteGuid = SiteGuid,
                m_sUserIP = m_sUserIP,
                filterQuery = this.Filter,
                order = this.Order
            };
        }

        public override string GetLoaderCachekey()
        {
            // g = GroupId
            // ps = PageSize
            // pi = PageIndex
            // sg = SiteGuid
            // ec = internal Channel ID
            // f = filter query
            string key = string.Format("Internal_Channel_g={0}_ps={1}_pi={2}_sg={3}_ic={4}_f={5}", GroupID, PageSize, PageIndex, SiteGuid, internalChannelId, this.Filter);

            return key;
        }

        #endregion
    }
}
