using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CachingProvider;

namespace ConditionalAccess
{
    public class CasCache
    {
        #region Constants
        private static readonly double DEFAULT_TIME_IN_CACHE_MINUTES = 120d;
        private static readonly string DEFAULT_CACHE_NAME = "PricingCache";
        #endregion
        #region Singleton properties
        private static object locker = new object();
        private static CasCache instance = null;
        #endregion
        private ICachingService cache = null;

        private string GetCacheName()
        {
            string res = Utils.GetValueFromConfig("PRICING_CACHE_NAME");
            if(res.Length > 0)
                return res;
            return DEFAULT_CACHE_NAME;
        }

        private double GetDefaultCacheTimeInMinutes()
        {
            double res = 0d;
            string timeStr = Utils.GetValueFromConfig("PRICING_CACHE_TIME_IN_MINUTES");
            if (timeStr.Length > 0 && Double.TryParse(timeStr, out res) && res > 0)
                return res;
            return DEFAULT_TIME_IN_CACHE_MINUTES;
        }

        private void InitializeCachingService(string cacheName, double cachingTimeMinutes)
        {
            this.cache = new SingleInMemoryCache(cacheName, cachingTimeMinutes);
        }

        private CasCache()
        {
            InitializeCachingService(GetCacheName(), GetDefaultCacheTimeInMinutes());
        }

        internal static CasCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new CasCache();
                    }
                }
            }

            return instance;
        }

        /*
        internal bool TryGetSubscription(string key, out Subscription sub)
        {
            
        }

        internal bool TryAddSubscription(string key, Subscription sub)
        {
            return sub != null && cache.Add(key, sub);
        }
        */ 
    }
}
