using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Threading;
using System.Configuration;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.Configuration.Media
{
    public class MediaConfiguration : ConfigurationManager<MediaData>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();

        static MediaConfiguration instance = null;
        static object instanceLock = new object();

        private MediaConfiguration()
        {
            base.SyncFromFile(System.Configuration.ConfigurationManager.AppSettings["TVPPro.Configuration.Media"], true);
            m_syncFile = System.Configuration.ConfigurationManager.AppSettings["TVPPro.Configuration.Media"];
        }

        private MediaConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }

        public static MediaConfiguration Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new MediaConfiguration();
                        }
                    }
                }

                return instance;
            }
        }

        public static MediaConfiguration GetInstance(string syncFile)
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new MediaConfiguration(syncFile);
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
