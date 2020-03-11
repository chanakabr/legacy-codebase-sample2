using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Configuration;
using System.Threading;

namespace TVPPro.Configuration.Site
{
    public partial class SiteConfiguration : ConfigurationManager<SiteData>
    {
		static SiteConfiguration instance = null;
		static object instanceLock = new object();

		private SiteConfiguration()
		{
			base.SyncFromFile(System.Configuration.ConfigurationManager.AppSettings["TVPPro.Configuration.Site"], true);
		}

        private SiteConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }

		public static SiteConfiguration Instance
		{
			get
			{
				if (instance == null)
				{
					lock (instanceLock)
					{
						if (instance == null)
						{
							instance = new SiteConfiguration();
						}
					}
				}

				return instance;
			}
		}

        public static SiteConfiguration GetInstance(string syncFile)
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new SiteConfiguration(syncFile);
                    }
                }
            }
            else
            {
                lock (instanceLock)
                {
                    instance.ReSyncFromFile(syncFile);
                }
            }

            return instance;
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
