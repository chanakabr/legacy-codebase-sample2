using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.Threading;
using System.Security.Principal;
using System.Security.AccessControl;
using Phx.Lib.Log;
using System.Reflection;
using Phx.Lib.Appconfig;
using CachingProvider.LayeredCache;
using Newtonsoft.Json;

namespace CachingProvider
{
    public class SingleInMemoryCache : ICachingService, IDisposable, ILayeredCacheService
    {
        /*
         * Pay attention !
         * 1. MemoryCache is threadsafe, however the references it holds are not necessarily thread safe.
         * 2. MemoryCache should be properly disposed.
         */

        private static readonly string DEFAULT_CACHE_NAME = "SVInMemoryCache";        
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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

        private static object locker = new object();
        
        private static Dictionary<InMemoryCacheType, SingleInMemoryCache> instances = new Dictionary<InMemoryCacheType, SingleInMemoryCache>();
        public static SingleInMemoryCache GetInstance(InMemoryCacheType type, uint defaultExpirationInSeconds)
        {
            if (!instances.ContainsKey(type))
            {
                lock (locker)
                {
                    if (!instances.ContainsKey(type))
                    {
                        long cacheMemoryLimit = 0;
                        int pollingIntervalSeconds = 0;

                        switch (type)
                        {
                            case InMemoryCacheType.LayeredCache:
                                cacheMemoryLimit = ApplicationConfiguration.Current.LayeredCacheInMemoryCacheConfiguration.CacheMemoryLimit.Value;
                                pollingIntervalSeconds = ApplicationConfiguration.Current.LayeredCacheInMemoryCacheConfiguration.PollingIntervalSeconds.Value;
                                break;
                            case InMemoryCacheType.General:
                                cacheMemoryLimit = ApplicationConfiguration.Current.GeneralInMemoryCacheConfiguration.CacheMemoryLimit.Value;
                                pollingIntervalSeconds = ApplicationConfiguration.Current.GeneralInMemoryCacheConfiguration.PollingIntervalSeconds.Value;
                                break;
                            default:
                                break;
                        }

                        instances[type] = new SingleInMemoryCache(defaultExpirationInSeconds, cacheMemoryLimit, pollingIntervalSeconds);
                    }
                }
            }

            return instances[type];
        }

        #region Ctors

        private SingleInMemoryCache(uint defaultExpirationInSeconds, long cacheMemoryLimitMegabytes = 0, int pollingIntervalSeconds = 0)
        {
            CacheName = GetCacheName();
            DefaultMinOffset = (double)defaultExpirationInSeconds / 60;
            var config = new System.Collections.Specialized.NameValueCollection();

            if (cacheMemoryLimitMegabytes > 0)
            {
                config["cacheMemoryLimitMegabytes"] = cacheMemoryLimitMegabytes.ToString();
            }

            if (pollingIntervalSeconds > 0)
            {
                var timeSpan = TimeSpan.FromSeconds(pollingIntervalSeconds);
                config["pollingInterval"] = timeSpan.ToString();
            }

            cache = new MemoryCache(CacheName, config);
        }

        #endregion

        private string GetCacheName()
        {
            return DEFAULT_CACHE_NAME;
        }

        public bool Add<T>(string sKey, T value, uint expirationInSeconds)
        {
            if (string.IsNullOrEmpty(sKey))
                return false;
            return cache.Add(sKey, value, DateTime.UtcNow.AddMinutes((double)expirationInSeconds / 60));
        }

        public bool Add(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            if (string.IsNullOrEmpty(sKey))
                return false;
            return cache.Add(sKey, oValue.result, DateTime.UtcNow.AddMinutes(nMinuteOffset));
        }

        public bool Add(string sKey, BaseModuleCache oValue)
        {
            return Add(sKey, oValue, DefaultMinOffset);
        }

        public bool Set(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool res = false;
            if (string.IsNullOrEmpty(sKey))
                return false;
            try
            {
                cache.Set(sKey, oValue.result, DateTime.UtcNow.AddMinutes(nMinuteOffset));
                res = true;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at Set. ");
                sb.Append(String.Concat(" Key: ", sKey));
                sb.Append(String.Concat(" Val: ", oValue != null ? oValue.ToString() : "null"));
                sb.Append(String.Concat(" Min Offset: ", nMinuteOffset));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
            }

            return res;
        }

        public bool Set<T>(string key, T value, DateTimeOffset absoluteExpiration)
        {
            try
            {
                cache.Set(key, value, absoluteExpiration);
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error when setting memory cache value. key = {key}, ex = {ex}", ex);
                return false;
            }
        }

        public bool Set(string sKey, BaseModuleCache oValue)
        {
            return Set(sKey, oValue, DefaultMinOffset);
        }

        public BaseModuleCache Get(string sKey)
        {
            BaseModuleCache baseModule = new BaseModuleCache();
            if (string.IsNullOrEmpty(sKey))
                return null;
            baseModule.result = cache.Get(sKey);
            return baseModule;
        }

        public BaseModuleCache Remove(string sKey)
        {
            BaseModuleCache baseModule = new BaseModuleCache();
            baseModule.result = cache.Remove(sKey);
            return baseModule;
        }        

        public T Get<T>(string sKey) where T : class
        {
            if (string.IsNullOrEmpty(sKey))
                return default(T);
            return cache.Get(sKey) as T;
        }

        public bool Contains(string key)
        {
            return cache.Contains(key);
        }

        public BaseModuleCache GetWithVersion<T>(string sKey)
        {         
            try
            {
                if (string.IsNullOrEmpty(sKey))
                    return null;
                VersionModuleCache baseModule = (VersionModuleCache)cache.Get(sKey);

                if (baseModule == null)
                {
                    baseModule = new VersionModuleCache();
                }
                return baseModule;
            }
            catch
            {
                return null;
            }
        }

        public bool AddWithVersion<T>(string sKey, BaseModuleCache oValue)
        {
            return AddWithVersion<T>(sKey, oValue, DefaultMinOffset);
        }

        public bool AddWithVersion<T>(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            if (string.IsNullOrEmpty(sKey))
                return false;

            return cache.Add(sKey, oValue, DateTime.UtcNow.AddMinutes(nMinuteOffset));
        }

        public bool SetWithVersion<T>(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool bRes = false;
            try
            {
                DateTime dtExpiresAt = DateTime.UtcNow.AddMinutes(nMinuteOffset);
                BaseModuleCache vModule = GetWithVersion<T>(sKey);
                // memory is empty for this key OR the object must have the same version 
                if (vModule == null || vModule.result == null || (vModule != null && vModule.result != null))
                {
                    Guid versionGuid = Guid.NewGuid();
                    cache.Set(sKey, oValue, DateTime.UtcNow.AddMinutes(nMinuteOffset));
                    return true;
                }

                return bRes;
            }
            catch (Exception ex)
            {
                log.Error("AddWithVersion", ex);
                return false;
            }
        }

        public bool SetWithVersion<T>(string sKey, BaseModuleCache oValue)
        {
            return SetWithVersion<T>(sKey, oValue, DefaultMinOffset);
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

        public IDictionary<string, object> GetValues(List<string> keys, bool asJson = false)
        {
            IDictionary<string, object> iDict = null;
            try
            {
                if (keys == null || keys.Count == 0)
                    return null;

                iDict = cache.GetValues(keys);

                return iDict;
            }
            catch
            {
                return null;
            }

        }

        public bool SetJson<T>(string sKey, T obj, double dCacheTT)
        {
            return false;
        }

        public bool GetJsonAsT<T>(string sKey, out T res) where T : class
        {
            res = null;
            return false;
        }

        public List<string> GetKeys()
        {                        
            List<string> keys = new List<string>();
            foreach (var item in cache)
            {                
                keys.Add(item.Key);
            }
            return keys;
        }

        public GetOperationStatus Get<T>(string key, ref T result, JsonSerializerSettings jsonSerializerSettings)
        {
            result = default(T);
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    object cacheResult = cache.Get(key);
                    if (cacheResult != null)
                    {
                        result = (T) cacheResult;
                        return result != null ? GetOperationStatus.Success : GetOperationStatus.NotFound;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Get<T> with key: {0}", key), ex);
                return GetOperationStatus.Error;
            }

            return GetOperationStatus.NotFound;
        }

        public bool RemoveKey(string sKey)
        {
            bool? result = false;
            result = cache.Remove(sKey) as bool?;
            return result.HasValue && result.Value;
        }

        public bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, bool shouldAllowPartialQuery = false)
        {
            bool res = false;
            try
            {
                IDictionary<string, object> getResults = null;
                getResults = GetValues(keys, shouldAllowPartialQuery);
                if (getResults != null && getResults.Count > 0)
                {
                    results = getResults.ToDictionary(x => x.Key, x => (T)x.Value);
                    if (results != null)
                    {
                        if (shouldAllowPartialQuery)
                        {
                            res = results.Count > 0;
                        }
                        else
                        {
                            res = keys.Count == results.Count;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetValues<T> from InMemoryCache while getting the following keys: {0}", string.Join(",", keys)), ex);
            }

            return res;
        }

        public bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings, bool shouldAllowPartialQuery = false)
        {
            return GetValues(keys, ref results, shouldAllowPartialQuery);
        }

        public bool Set<T>(string key, T value, uint ttlInSeconds, JsonSerializerSettings jsonSerializerSettings = null)
        {
            bool bRes = false;
            try
            {
                DateTime dtExpiresAt = DateTime.UtcNow.AddMinutes((double)ttlInSeconds / 60);
                cache.Set(key, value, dtExpiresAt);
                bRes = true;
            }

            catch (Exception ex)
            {
                log.Error("SetWithVersion<T>", ex);
                return false;
            }

            return bRes;
        }

        public bool RemoveKeysStartingWith(string keyPrefix)
        {
            try
            {
                List<KeyValuePair<string, object>> removeList = null;
                if (!string.IsNullOrEmpty(keyPrefix))
                {
                    removeList = cache.Where(item => item.Key.StartsWith(keyPrefix)).ToList();
                }
                else
                {
                    removeList = cache.ToList();
                }

                foreach (var item in removeList)
                {
                    try
                    {
                        cache.Remove(item.Key);
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error when removing key from cache. key = {item.Key}, ex = {ex}", ex);

                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error when removing keys from cache. start key = {keyPrefix}, ex = {ex}", ex);
                return false;
            }
        }

        public List<string> GetCachedKeys()
        {
            return this.cache.Select(item => item.Key).ToList();
        }
    }

    public enum InMemoryCacheType
    {
        LayeredCache,
        General
    }
}
