using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Tvinci.Configuration;

namespace TVPPro.Configuration.OrcaRecommendations
{
    public class OrcaRecommendationsConfiguration : ConfigurationManager<OrcaRecommendations>
    {
        static OrcaRecommendationsConfiguration instance = null;
        static object instanceLock = new object();

        private OrcaRecommendationsConfiguration()
        {
            base.SyncFromFile(ConfigurationManager.AppSettings["TVPPro.Configuration.OrcaRecommendations"], true);
        }

        private OrcaRecommendationsConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }

        public static OrcaRecommendationsConfiguration Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new OrcaRecommendationsConfiguration();
                        }
                    }
                }

                return instance;
            }
        }

        public static OrcaRecommendationsConfiguration GetInstance(string syncFile)
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new OrcaRecommendationsConfiguration(syncFile);
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
