using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CachingProvider;
using TVinciShared;

namespace TvinciCache
{
    public class WSCache
    {
        #region Constants
        private static readonly double DEFAULT_TIME_IN_CACHE_MINUTES = 120d;
        private static readonly string DEFAULT_CACHE_NAME = "Cache";
        #endregion
        #region Singleton properties
        private static object locker = new object();
        private static WSCache instance = null;
        #endregion
        private ICachingService cache = null;

        private string GetCacheName()
        {
            string res = WS_Utils.GetTcmConfigValue("CACHE_NAME");
            if (res.Length > 0)
                return res;
            return DEFAULT_CACHE_NAME;
        }

        private double GetDefaultCacheTimeInMinutes()
        {
            double res = 0d;
            string timeStr = WS_Utils.GetTcmConfigValue("CACHE_TIME_IN_MINUTES");
            if (timeStr.Length > 0 && Double.TryParse(timeStr, out res) && res > 0)
                return res;
            return DEFAULT_TIME_IN_CACHE_MINUTES;
        }

        private void InitializeCachingService(string cacheName, double cachingTimeMinutes)
        {
            string res = WS_Utils.GetTcmConfigValue("CACHE_TYPE");

            switch (res)
            {
                case "OutOfProcess":
                    //this.cache = new OutOfProcessCache(
                    break;
                default:
                    this.cache = new SingleInMemoryCache(cacheName, cachingTimeMinutes);
                    break;

            }
        }

        private WSCache()
        {
            InitializeCachingService(GetCacheName(), GetDefaultCacheTimeInMinutes());
        }

        public static WSCache Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new WSCache();
                        }
                    }
                }

                return instance;
            }
        }

        public T Get<T>(string key)
        {
            BaseModuleCache res = cache.Get(key);
            if (res != null && res.result != null)
            {
                return (T)res.result;
            }
            else
            {
                return default(T); 
            }
        }

        public bool TryGet<T>(string key, out T value)
        {
            bool res = false;

            BaseModuleCache obj = cache.Get(key);

            if (obj != null && obj.result != null)
            {
                value = (T)obj.result;
                res = true;
            }
            else
            {
                value = default(T);
            }

            return (res);
        }
        public bool Add(string key, object obj)
        {
            BaseModuleCache bModule = new BaseModuleCache(obj);
            return obj != null && cache.Add(key, bModule);
        }


        public IDictionary<string, object> GetValues(List<string> keys)
        {
            if (keys == null || keys.Count ==0)
                return null;

            return cache.GetValues(keys);
        }
    }
}
