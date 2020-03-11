using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Threading;
using System.Configuration;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.Configuration.PageStruct
{
    public class PageConfiguration : ConfigurationManager<PageStruct>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();

        static PageConfiguration instance = null;
        static object instanceLock = new object();

        private PageConfiguration()
        {
            base.SyncFromFile(System.Configuration.ConfigurationManager.AppSettings["TVPPro.Configuration.PageStruct"], true);
            m_syncFile = System.Configuration.ConfigurationManager.AppSettings["TVPPro.Configuration.PageStruct"];
        }

        private PageConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }

        public static PageConfiguration Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new PageConfiguration();
                        }
                    }
                }

                return instance;
            }
        }

        public static PageConfiguration GetInstance(string syncFile)
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new PageConfiguration(syncFile);
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
