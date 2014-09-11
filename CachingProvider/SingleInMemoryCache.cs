using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace CachingProvider
{
    public class SingleInMemoryCache : ICachingService, IDisposable
    {
        /*
         * Pay attention !
         * 1. MemoryCache is threadsafe, however the references it holds are not necessarily thread safe.
         * 2. MemoryCache should be properly disposed.
         */
        private static readonly string SINGLE_IN_MEM_CACHE_LOG_FILE = "SingleInMemoryCache";
        private MemoryCache cache = null;
        public string CacheName
        {
            get;
            private set;
        }
        public double DefaultMinOffset
        {
            get;
            private set;
        }

        public SingleInMemoryCache(string name, double defaultMinOffset)
        {
            CacheName = name;
            DefaultMinOffset = defaultMinOffset;
            cache = new MemoryCache(name);
        }

        public bool Add(string sKey, object oValue, double nMinuteOffset)
        {
            if (string.IsNullOrEmpty(sKey))
                return false;
            return cache.Add(sKey, oValue, DateTime.Now.AddMinutes(nMinuteOffset));
        }

        public bool Add(string sKey, object oValue)
        {
            return Add(sKey, oValue, DefaultMinOffset);
        }

        public bool Set(string sKey, object oValue, double nMinuteOffset)
        {
            bool res = false;
            if (string.IsNullOrEmpty(sKey))
                return false;
            try
            {
                cache.Set(sKey, oValue, DateTime.Now.AddMinutes(nMinuteOffset));
                res = true;
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at Set. ");
                sb.Append(String.Concat(" Key: ", sKey));
                sb.Append(String.Concat(" Val: ", oValue != null ? oValue.ToString() : "null"));
                sb.Append(String.Concat(" Min Offset: ", nMinuteOffset));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                Logger.Logger.Log("Exception", sb.ToString(), SINGLE_IN_MEM_CACHE_LOG_FILE);
                #endregion
            }

            return res;
        }

        public bool Set(string sKey, object oValue)
        {
            return Set(sKey, oValue, DefaultMinOffset);
        }

        public object Get(string sKey)
        {
            if (string.IsNullOrEmpty(sKey))
                return null;
            return cache.Get(sKey);
        }

        public object Remove(string sKey)
        {
            return cache.Remove(sKey);
        }

        public T Get<T>(string sKey) where T : class
        {
            if (string.IsNullOrEmpty(sKey))
                return default(T);
            return cache.Get(sKey) as T;
        }

        public void Dispose()
        {
            if (cache != null)
            {
                cache.Dispose();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("SingleInMemoryCache. ");
            sb.Append(String.Concat(" Cache Name: ", CacheName));
            sb.Append(String.Concat(" DefaultMinOffset: ", DefaultMinOffset));
            sb.Append(String.Concat(" Items in cache: ", cache.GetCount()));
            sb.Append(String.Concat(" Total amt of bytes on machine the cache can use: ", cache.CacheMemoryLimit));
            sb.Append(String.Concat(" Total percentage of physical memory the cache can use: ", cache.PhysicalMemoryLimit));
            sb.Append(String.Concat(" Polling Interval: ", cache.PollingInterval.ToString()));

            return sb.ToString();
        }
    }
}
