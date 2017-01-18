using CachingProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Social.Cache
{
    public sealed class SocialCache
    {
        private static object syncRoot = new Object();

        private static volatile SocialCache instance = null;

        public static SocialCache Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SocialCache();
                    }
                }

                return instance;
            }
        }


        private readonly double nDocTTL = GetDocTTLSettings();
        private ICachingService m_oCacheProvider;
        private SocialCache()
        {
            m_oCacheProvider = CouchBaseCache<bool>.GetInstance("social");
        }

        public object Get(string sKey)
        {
            return m_oCacheProvider.Get(sKey);
        }

        public T Get<T>(string sKey) where T : class
        {
            return m_oCacheProvider.Get<T>(sKey);
        }

        public bool Set(string sKey, object oValue)
        {
            BaseModuleCache bModule = new BaseModuleCache(oValue);
            return m_oCacheProvider.Set(sKey, bModule, nDocTTL);
        }

        public object Remove(string sKey)
        {
            return m_oCacheProvider.Remove(sKey);
        }

        private static double GetDocTTLSettings()
        {
            double nResult;
            if (!double.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue("socialCacheDocTimeout"), out nResult))
            {
                nResult = 1440.0;
            }

            return nResult;
        }
    }
}
