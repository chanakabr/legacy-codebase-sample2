using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Threading;
using System.Configuration;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.Configuration.ResourcesConfiguration
{
    public class ResourcesConfiguration : ConfigurationManager<ResourceConfiguration>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();

        static ResourcesConfiguration instance = null;
        static object instanceLock = new object();

        private ResourcesConfiguration()
        {
            base.SyncFromFile(System.Configuration.ConfigurationManager.AppSettings["TVPPro.Configuration.ResourcesConfiguration"], true);
            m_syncFile = System.Configuration.ConfigurationManager.AppSettings["TVPPro.Configuration.ResourcesConfiguration"];
        }

        private ResourcesConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }

        public static ResourcesConfiguration Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new ResourcesConfiguration();
                        }
                    }
                }

                return instance;
            }
        }

        public static ResourcesConfiguration GetInstance(string syncFile)
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new ResourcesConfiguration(syncFile);
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
