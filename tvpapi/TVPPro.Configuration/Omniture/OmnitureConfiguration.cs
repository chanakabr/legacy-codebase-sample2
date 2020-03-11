using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Configuration;



namespace TVPPro.Configuration.Omniture
{
    public class OmnitureConfiguration : ConfigurationManager<Omniture>
    {
        static OmnitureConfiguration instance = null;
        static object instanceLock = new object();

        private OmnitureConfiguration()
        {
            base.SyncFromFile(System.Configuration.ConfigurationManager.AppSettings["TVPPro.Configuration.Omniture"], true);
        }
        private OmnitureConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }
        public static OmnitureConfiguration Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new OmnitureConfiguration();
                        }
                    }
                }

                return instance;
            }
        }
        public static OmnitureConfiguration GetInstance(string syncFile)
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new OmnitureConfiguration(syncFile);
                    }
                    else
                    {
                        lock (instanceLock)
                        {
                            instance.ReSyncFromFile(syncFile);
                        }
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
