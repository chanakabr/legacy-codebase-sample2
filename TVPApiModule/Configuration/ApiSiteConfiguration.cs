using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Configuration;
using System.Threading;
using TVPPro.Configuration.Site;

namespace TVPApi.Configuration.Site
{
    public partial class ApiSiteConfiguration : ConfigurationManager<SiteData>
    {

		public ApiSiteConfiguration()
		{
			base.SyncFromFile(ConfigurationManager.AppSettings["TVPPro.Configuration.Site"], true);
		}

        public ApiSiteConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }

		public bool SupportPricing
		{
			get
			{
				return this.Data.Features.Pricing.SupportFeature;
			}
		}
	}
}
