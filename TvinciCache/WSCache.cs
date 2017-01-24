using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingProvider;
using TVinciShared;

namespace TvinciCache
{
    public class WSCache
    {
        #region Constants
        private static readonly uint DEFAULT_TIME_IN_CACHE_SECONDS = 7200;
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

        private uint GetDefaultCacheTimeInSeconds()
        {
            uint res = 0;
            string timeStr = WS_Utils.GetTcmConfigValue("CACHE_TIME_IN_MINUTES");
            if (timeStr.Length > 0 && uint.TryParse(timeStr, out res) && res > 0)
            {
                res *= 60;
                return res;
            }

            return DEFAULT_TIME_IN_CACHE_SECONDS;
        }

        private void InitializeCachingService(string cacheName, uint expirationInSeconds)
        {
            string res = WS_Utils.GetTcmConfigValue("CACHE_TYPE");

            switch (res)
            {
                case "OutOfProcess": 
                    //this.cache = new OutOfProcessCache
                    break;
                default:
                    this.cache = SingleInMemoryCacheManager.Instance(cacheName, expirationInSeconds);
                    break;

            }
        }

        private WSCache()
        {
            InitializeCachingService(GetCacheName(), GetDefaultCacheTimeInSeconds());
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

        /// <summary>
        /// Tried to get a value from the cache, and tells wether succeeded or failed
        /// </summary>
        /// <typeparam name="T">The type of the value that we get</typeparam>
        /// <param name="p_sKey"></param>
        /// <param name="p_tValue"></param>
        /// <returns>Wether the key exists in the cache or not</returns>
        public bool TryGet<T>(string p_sKey, out T p_tValue)
        {
            BaseModuleCache oInnerResult = cache.Get(p_sKey);
            p_tValue = default(T);

            if (oInnerResult != null && oInnerResult.result != null)
            {
                // cast the result from the inner cache
                p_tValue = (T)oInnerResult.result;

                return (true);
            }
            else
            {
                return (false);    
            }
        }

        public bool Add(string key, object obj)
        {
            BaseModuleCache bModule = new BaseModuleCache(obj);
            return obj != null && cache.Add(key, bModule);
        }

        public bool Add(string key, object obj, double nMinuteOffset)
        {
            BaseModuleCache bModule = new BaseModuleCache(obj);
            return obj != null && cache.Add(key, bModule, nMinuteOffset);
        }


        public IDictionary<string, object> GetValues(List<string> keys)
        {
            if (keys == null || keys.Count == 0)
                return null;

            return cache.GetValues(keys);
        }

        public static void ClearAll()
        {
            if (instance != null)
            {
                instance = null;
            }
        }

        public List<string> GetKeys()
        {            
            return cache.GetKeys();            
        }
    }
}
