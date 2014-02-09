using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Configuration;

namespace TVPPro.Configuration.PlatformServices
{
	public class PlatformServicesConfiguration : ConfigurationManager<PlatformServicesData>
    {
		static PlatformServicesConfiguration instance = null;
		static object instanceLock = new object();

		private PlatformServicesConfiguration()
		{
			base.SyncFromFile(ConfigurationManager.AppSettings["TVPPro.Configuration.PlatformServices"], true);
		}

        private PlatformServicesConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }

		public static PlatformServicesConfiguration Instance
		{
			get
			{
				if (instance == null)
				{
					lock (instanceLock)
					{
						if (instance == null)
						{
							instance = new PlatformServicesConfiguration();
						}
					}
				}

				return instance;
			}
		}

        public static PlatformServicesConfiguration GetInstance(string syncFile)
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new PlatformServicesConfiguration(syncFile);
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
	}
}
