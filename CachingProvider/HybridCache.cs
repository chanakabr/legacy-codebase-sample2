using CouchbaseManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;
namespace CachingProvider
{
    public class HybridCache<T> : OutOfProcessCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private SingleInMemoryCache inMemoryCache;
        private CouchBaseCache<T> couchbaseCache;
        private double secondsInMemory;

        /// <summary>
        /// Initializes a new instance of the hybrid cache
        /// </summary>
        /// <param name="externalCacheName"></param>
        private HybridCache(eCouchbaseBucket externalCacheName, string internalCacheName)
        {
            this.inMemoryCache = SingleInMemoryCacheManager.Instance(internalCacheName, 0);
            this.couchbaseCache = CouchBaseCache<T>.GetInstance(externalCacheName.ToString());
            this.secondsInMemory = Utils.GetDoubleValueFromTcm("Groups_Cache_TTL");

            // default value = 1 minute = 60 seconds
            if (this.secondsInMemory == 0)
            {
                this.secondsInMemory = 60;
            }
        }

        public static HybridCache<T> GetInstance(eCouchbaseBucket bucket, string internalCacheName)
        {
            HybridCache<T> cache = null;
            try
            {
                cache = new HybridCache<T>(bucket, internalCacheName);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Unable to create hybrid cache. Ex={0};\nCall stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return cache;
        }

        public override bool Add(string key, BaseModuleCache value, double minuteOffset)
        {
            bool couchBaseAdd = couchbaseCache.Add(key, value, minuteOffset);

            bool inMemoryAdd = false;

            if (couchBaseAdd)
            {
                inMemoryAdd = inMemoryCache.Add(key, value, minuteOffset);
            }

            return (inMemoryAdd && couchBaseAdd);
        }

        public override bool Set(string key, BaseModuleCache value, double minuteOffset)
        {
            bool couchBaseSet = couchbaseCache.Set(key, value, minuteOffset);

            bool inMemorySet = false;

            if (couchBaseSet)
            {
                inMemorySet = inMemoryCache.Set(key, value, minuteOffset);
            }

            return (inMemorySet && couchBaseSet);
        }

        public override bool Add(string key, BaseModuleCache value)
        {
            bool couchBaseAdd = couchbaseCache.Add(key, value);

            bool inMemoryAdd = false;

            if (couchBaseAdd)
            {
                inMemoryAdd = inMemoryCache.Add(key, value, this.secondsInMemory / 60);
            }

            return (inMemoryAdd && couchBaseAdd);
        }

        public override bool Set(string key, BaseModuleCache value)
        {
            bool couchBaseSet = couchbaseCache.Set(key, value);

            bool inMemorySet = false;

            if (couchBaseSet)
            {
                inMemorySet = inMemoryCache.Set(key, value, this.secondsInMemory / 60);
            }

            return (inMemorySet && couchBaseSet);
        }

        public override BaseModuleCache Get(string key)
        {
            BaseModuleCache result = inMemoryCache.Get(key);

            // If it isn't in in-memory, get it from couchbase and put in in-memory
            if (result == null || result.result == null)
            {
                result = couchbaseCache.Get(key);

                if (result != null && result.result != null)
                {
                    inMemoryCache.Add(key, result, this.secondsInMemory / 60);
                }
            }
            else if (result.result is VersionModuleCache)
            {
                result.result = (result.result as VersionModuleCache).result;
            }

            return result;
        }

        public override T Get<T>(string key)
        {
            T result = default(T);

            result = inMemoryCache.Get<T>(key);

            if (result == default(T))
            {
                result = couchbaseCache.Get<T>(key);

                if (result != null)
                {
                    BaseModuleCache newBaseModule = new BaseModuleCache(result);
                    inMemoryCache.Add(key, newBaseModule, this.secondsInMemory / 60);
                }
            }

            return result;
        }

        public override BaseModuleCache Remove(string key)
        {
            BaseModuleCache resultCouchbase = this.couchbaseCache.Remove(key);
            BaseModuleCache resultInMemory = this.inMemoryCache.Remove(key);

            if (resultInMemory != null && resultInMemory.result != null)
            {
                return resultInMemory;
            }
            else
            {
                return resultCouchbase;
            }
        }

        public override BaseModuleCache GetWithVersion<T>(string key)
        {
            BaseModuleCache result = inMemoryCache.GetWithVersion<T>(key);

            // If it isn't in in-memory, get it from couchbase and put in in-memory
            if (result == null || result.result == null)
            {
                result = couchbaseCache.GetWithVersion<T>(key);

                if (result != null && result.result != null)
                {
                    inMemoryCache.Add(key, result, this.secondsInMemory / 60);
                }
            }
            else if (result.result is VersionModuleCache)
            {
                result.result = (result.result as VersionModuleCache).result;
            }

            return result;
        }        

        public override bool AddWithVersion<T>(string key, BaseModuleCache value)
        {
            bool couchBaseAdd = couchbaseCache.AddWithVersion<T>(key, value);

            bool inMemoryAdd = false;

            if (couchBaseAdd)
            {
                inMemoryCache.AddWithVersion<T>(key, value, this.secondsInMemory / 60);
            }

            return (inMemoryAdd && couchBaseAdd);
        }

        public override bool AddWithVersion<T>(string key, BaseModuleCache value, double minuteOffset)
        {
            bool couchBaseAdd = couchbaseCache.AddWithVersion<T>(key, value, minuteOffset);

            bool inMemoryAdd = false;

            if (couchBaseAdd)
            {
                inMemoryAdd = inMemoryCache.AddWithVersion<T>(key, value, minuteOffset);
            }

            return (inMemoryAdd && couchBaseAdd);
        }

        public override bool SetWithVersion<T>(string key, BaseModuleCache value, double minuteOffset)
        {
            bool couchBaseSet = couchbaseCache.SetWithVersion<T>(key, value, minuteOffset);

            bool inMemorySet = false;

            if (couchBaseSet)
            {
                inMemorySet = inMemoryCache.SetWithVersion<T>(key, value, minuteOffset);
            }

            return (inMemorySet && couchBaseSet);
        }

        public override bool SetWithVersion<T>(string key, BaseModuleCache value)
        {
            bool couchBaseSet = couchbaseCache.SetWithVersion<T>(key, value);

            bool inMemorySet = false;

            if (couchBaseSet)
            {
                inMemorySet = inMemoryCache.SetWithVersion<T>(key, value);
            }

            return (inMemorySet && couchBaseSet);
        }

        public override IDictionary<string, object> GetValues(List<string> keys, bool asJson = false)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();

            IDictionary<string, object> dictionaryInMemory = this.inMemoryCache.GetValues(keys);

            List<string> missingKeys = new List<string>();

            // If we didn't get any key
            if (dictionaryInMemory == null || dictionaryInMemory.Count == 0)
            {
                missingKeys = keys;
            }
            // If we didn't get some of the keys
            else if (dictionaryInMemory.Count < keys.Count)
            {
                // Find out which of the keys didn't return from the in-memory
                foreach (string key in keys)
                {
                    if (!dictionaryInMemory.ContainsKey(key))
                    {
                        missingKeys.Add(key);
                    }
                }
            }

            // If everything is in-memory, just use it
            if (missingKeys.Count == 0)
            {
                result = dictionaryInMemory;
            }
            else
            {
                // If not, get missing keys from couchbase
                IDictionary<string, object> dictionaryCouchbase = this.couchbaseCache.GetValues(missingKeys, asJson);

                if (dictionaryCouchbase != null)
                {
                    // Put result from couchbase in result dictionary + in memory
                    foreach (var keyValue in dictionaryCouchbase)
                    {
                        result.Add(keyValue);
                        this.inMemoryCache.Add(keyValue.Key, new BaseModuleCache(keyValue.Value), this.secondsInMemory / 60);
                    }
                }

                // Union with whatever we have in-memory
                if (dictionaryInMemory != null)
                {
                    foreach (var keyValue in dictionaryInMemory)
                    {
                        result.Add(keyValue);
                    }
                }
            }

            return result;
        }

        public override bool SetJson<T>(string key, T obj, double cacheTT)
        {
            bool couchBaseSet = this.couchbaseCache.SetJson<T>(key, obj, cacheTT);

            bool inMemorySet = false;

            if (couchBaseSet)
            {
                inMemorySet = this.inMemoryCache.SetJson<T>(key, obj, cacheTT);
            }

            return inMemorySet && couchBaseSet;
        }

        public override bool GetJsonAsT<T>(string key, out T result)
        {
            bool success = false;

            success = this.inMemoryCache.GetJsonAsT<T>(key, out result);

            if (!success || result == default(T))
            {
                success = this.couchbaseCache.GetJsonAsT<T>(key, out result);
            }

            return success;
        }

        public override List<string> GetKeys()
        {
            return new List<string>();
        }

        public override bool Get<T>(string key, ref T result)
        {
            bool res = false;

            res = inMemoryCache.Get<T>(key, ref result);

            if (!res)
            {
                res = couchbaseCache.Get<T>(key, ref result);

                if (result != null)
                {
                    BaseModuleCache newBaseModule = new BaseModuleCache(result);
                    inMemoryCache.Add(key, newBaseModule, this.secondsInMemory / 60);
                }
            }
                        
            return result != null;
        }

        public override bool GetWithVersion<T>(string key, out ulong version, ref T result)
        {
            bool res = inMemoryCache.Get<T>(key, ref result);
            version = 0;
            // If it isn't in in-memory, get it from couchbase and put in in-memory
            if (!res)
            {
                res = couchbaseCache.GetWithVersion<T>(key, out version, ref result);

                if (res)
                {
                    res = inMemoryCache.Get<T>(key, ref result);
                }
            }

            return res;
        }

        public override bool RemoveKey(string key)
        {
            return this.couchbaseCache.RemoveKey(key) && this.inMemoryCache.RemoveKey(key);
        }

        public override bool Add<T>(string key, T value, uint expirationInSeconds)
        {
            return this.couchbaseCache.Add<T>(key, value, expirationInSeconds) && this.inMemoryCache.Add<T>(key, value, expirationInSeconds);
        }

        public override bool SetWithVersion<T>(string key, T value, ulong version, uint expirationInSeconds)
        {
            return this.couchbaseCache.SetWithVersion<T>(key, value, version, expirationInSeconds) && this.inMemoryCache.Add<T>(key, value, expirationInSeconds);
        }

    }
}
