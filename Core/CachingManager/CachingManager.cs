using System;
using System.Collections.Generic;
using System.Linq;
using KLogMonitor;
using System.Reflection;
using System.Runtime.Caching;
using CachingProvider;

namespace CachingManager
{

    public class CachingManager
    {
        private static readonly KLogger _Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        //static System.Collections.Hashtable cacheKeyList = new System.Collections.Hashtable();
        private static SingleInMemoryCache cache = SingleInMemoryCache.GetInstance(InMemoryCacheType.General, 0);
        private const string KEY_PREFIX = "CM";

        private static string FormatKey(string key)
        {
            return $"{KEY_PREFIX}_{key}"; ;
        }

        public static bool Exists(string key) 
        {
            key = FormatKey(key);
            return cache.Contains(key);
        }

        public static object GetCachedData(string key)
        {
            key = FormatKey(key);

            var cacheResult = cache.Get<object>(key);
            if (cacheResult != null)
            {
                return cacheResult;
            }

            return "";
        }

        public static object GetCachedDataNull(string key)
        {
            key = FormatKey(key);

            var cacheResult = cache.Get<object>(key);
            if (cacheResult != null)
            {
                return cacheResult;
            }

            return null;
        }


        /*GetCacheObject : this function return object for a specific key*/
        public static object GetCacheObject(string key)
        {
            key = FormatKey(key);

            var cacheResult = cache.Get<object>(key);
            if (cacheResult != null)
            {
                return cacheResult;
            }

            return null;
        }

        public static void SetCachedData(string key, object value, int expirationInSeconds, CacheItemPriority oPriority, int nMediaID, bool bToRenew)
        {
            key = FormatKey(key);

            cache.Set(key, value, DateTime.UtcNow.AddSeconds(expirationInSeconds));
        }

        public static void RemoveFromCache(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                key = FormatKey(key);
            }

            cache.RemoveKeysStartingWith(key);
        }

        public static List<string> GetCachedKeys()
        {
            var keys = new List<string>();
            try
            {
                keys = cache.GetCachedKeys();
            }
            catch (Exception ex)
            {
                _Log.Error("GetCachedKeys failed", ex);
            }

            return keys;
        }
    }
}
