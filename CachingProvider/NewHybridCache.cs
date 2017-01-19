using CouchbaseManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;
namespace CachingProvider
{
    public class NewHybridCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();
        private static NewHybridCache instance = null;
        private const string IN_MEMORY_CACHE_NAME = "HybridInMemoryCache";
        private const uint DEFAULT_CB_CACHE_TTL_SEC = 300;
        private const double DEFAULT_INNER_CACHE_TTL_MIN = 1;

        private SingleInMemoryCache inMemoryCache;
        private CouchbaseManager.CouchbaseManager cbClient;
        private CouchbaseManager.CouchbaseManager memCacheCbClient;
        private uint cbCacheTTL;
        private double innerCacheTTL;
        private string version;

        /// <summary>
        /// Initializes a new instance of the hybrid cache
        /// </summary>
        /// <param name="externalCacheName"></param>
        private NewHybridCache()
        {
            this.version = TCMClient.Settings.Instance.GetValue<string>("Hybrid_Cache_Version");
            //TODO change name of TCM value   
            this.cbCacheTTL = TCMClient.Settings.Instance.GetValue<uint>("Hybrid_Cache_CB_TTL_SEC");
            if (this.cbCacheTTL == 0)
            {
                this.cbCacheTTL = DEFAULT_CB_CACHE_TTL_SEC;
            }

            this.innerCacheTTL = TCMClient.Settings.Instance.GetValue<double>("Hybrid_Cache_InnerCache_TTL_MIN");
            if (this.innerCacheTTL == 0)
            {
                this.innerCacheTTL = DEFAULT_INNER_CACHE_TTL_MIN;
            }
            
            this.inMemoryCache = new SingleInMemoryCache(IN_MEMORY_CACHE_NAME, innerCacheTTL);
            this.cbClient = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.CACHE);
            this.memCacheCbClient = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEMCACHED);
        }

        public static NewHybridCache GetInstance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new NewHybridCache();
                    }
                }
            }

            return instance;
        }

        public bool Get<T>(string key, ref T genericParameter, HybridCacheType hybridCacheTypes, Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectFromDbMethod, Dictionary<string, object> funcParameters)
        {
            bool result = false;
            bool shouldInsertToSomeCache = false;
            HybridCacheType insertToCacheTypes = HybridCacheType.None;
            key = AddVersionOnKey(key);

            try
            {
                if (hybridCacheTypes.HasFlag(HybridCacheType.InMemoryCache))
                {
                    result = inMemoryCache.GetGenericType<T>(key, ref genericParameter);
                    shouldInsertToSomeCache = !result;
                    insertToCacheTypes = !result ? HybridCacheType.InMemoryCache : insertToCacheTypes;
                }

                if (!result && hybridCacheTypes.HasFlag(HybridCacheType.CbCache))
                {
                    result = cbClient.Get<T>(key, ref genericParameter);
                    shouldInsertToSomeCache = !result;
                    insertToCacheTypes = !result ? HybridCacheType.CbCache | insertToCacheTypes : insertToCacheTypes;
                }

                if (!result && hybridCacheTypes.HasFlag(HybridCacheType.CbMemCache))
                {
                    result = memCacheCbClient.Get<T>(key, ref genericParameter);
                    shouldInsertToSomeCache = !result;
                    insertToCacheTypes = !result ? HybridCacheType.CbMemCache | insertToCacheTypes : insertToCacheTypes;
                }

                if (!result)
                {
                    Tuple<T, bool> tuple = fillObjectFromDbMethod(funcParameters);
                    genericParameter = tuple.Item1;
                    result = tuple.Item2;
                    if (!result)
                    {
                        log.ErrorFormat("Failed fillingObjectFromDbMethod for key {0} with MethodName {1} and funcParameters {2}", key,
                                        fillObjectFromDbMethod.Method != null ? fillObjectFromDbMethod.Method.Name : "No_Method_Name",
                                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters");
                    }
                }

                if (shouldInsertToSomeCache && result && genericParameter != null)
                {
                    bool insertResult = false;
                    HybridCacheType failedToInsertCacheTypes = HybridCacheType.None;
                    if (insertToCacheTypes.HasFlag(HybridCacheType.InMemoryCache))
                    {
                        insertResult = inMemoryCache.AddGenericType<T>(key, genericParameter, this.innerCacheTTL);
                        failedToInsertCacheTypes = !insertResult ? HybridCacheType.InMemoryCache : failedToInsertCacheTypes;
                    }

                    if (insertToCacheTypes.HasFlag(HybridCacheType.CbCache))
                    {
                        insertResult = cbClient.Add<T>(key, genericParameter, this.cbCacheTTL);
                        failedToInsertCacheTypes = !insertResult ? HybridCacheType.CbCache | failedToInsertCacheTypes : failedToInsertCacheTypes;
                    }

                    if (insertToCacheTypes.HasFlag(HybridCacheType.CbMemCache))
                    {
                        insertResult = memCacheCbClient.Add<T>(key, genericParameter, this.cbCacheTTL);
                        failedToInsertCacheTypes = !insertResult ? HybridCacheType.CbMemCache | failedToInsertCacheTypes : failedToInsertCacheTypes;
                    }

                    if (!insertResult)
                    {
                        log.ErrorFormat("Failed inserting key {0} on the following hybridCacheTypes {1}", key, hybridCacheTypes.ToString());
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed to get key {0} from NewHybridCache with hybridCacheTypes {1}, MethodName {2} and funcParameters {3}", key, hybridCacheTypes.ToString(),
                                        fillObjectFromDbMethod.Method != null ? fillObjectFromDbMethod.Method.Name : "No_Method_Name",
                                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters"), ex);
            }

            return result;
        }

        public bool GetWithVersion<T>(string key, ref T genericParameter, out ulong version, HybridCacheType hybridCacheTypes, Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectFromDbMethod, Dictionary<string, object> funcParameters)
        {
            bool result = false;
            bool shouldInsertToSomeCache = false;
            HybridCacheType insertToCacheTypes = HybridCacheType.None;
            key = AddVersionOnKey(key);
            version = 0;

            try
            {
                if (hybridCacheTypes.HasFlag(HybridCacheType.InMemoryCache))
                {
                    result = inMemoryCache.GetGenericType<T>(key, ref genericParameter);
                    shouldInsertToSomeCache = !result;
                    insertToCacheTypes = !result ? HybridCacheType.InMemoryCache : insertToCacheTypes;
                }

                if (!result && hybridCacheTypes.HasFlag(HybridCacheType.CbCache))
                {
                    result = cbClient.GetWithVersion<T>(key, out version, ref genericParameter);
                    shouldInsertToSomeCache = !result;
                    insertToCacheTypes = !result ? HybridCacheType.CbCache | insertToCacheTypes : insertToCacheTypes;
                }

                if (!result && hybridCacheTypes.HasFlag(HybridCacheType.CbMemCache))
                {
                    result = memCacheCbClient.GetWithVersion<T>(key, out version, ref genericParameter);
                    shouldInsertToSomeCache = !result;
                    insertToCacheTypes = !result ? HybridCacheType.CbMemCache | insertToCacheTypes : insertToCacheTypes;
                }

                if (!result)
                {
                    Tuple<T, bool> tuple = fillObjectFromDbMethod(funcParameters);
                    genericParameter = tuple.Item1;
                    result = tuple.Item2;
                    if (!result)
                    {
                        log.ErrorFormat("Failed fillingObjectFromDbMethod for key {0}, version {1} with MethodName {2} and funcParameters {3}", key, version,
                                        fillObjectFromDbMethod.Method != null ? fillObjectFromDbMethod.Method.Name : "No_Method_Name",
                                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters");
                    }
                }

                if (shouldInsertToSomeCache && result && genericParameter != null)
                {
                    bool insertResult = false;
                    HybridCacheType failedToInsertCacheTypes = HybridCacheType.None;
                    if (insertToCacheTypes.HasFlag(HybridCacheType.InMemoryCache))
                    {
                        insertResult = inMemoryCache.AddGenericType<T>(key, genericParameter, this.innerCacheTTL);
                        failedToInsertCacheTypes = !insertResult ? HybridCacheType.InMemoryCache : failedToInsertCacheTypes;
                    }

                    if (insertToCacheTypes.HasFlag(HybridCacheType.CbCache))
                    {
                        insertResult = cbClient.Add<T>(key, genericParameter, this.cbCacheTTL);
                        failedToInsertCacheTypes = !insertResult ? HybridCacheType.CbCache | failedToInsertCacheTypes : failedToInsertCacheTypes;
                    }

                    if (insertToCacheTypes.HasFlag(HybridCacheType.CbMemCache))
                    {
                        insertResult = memCacheCbClient.Add<T>(key, genericParameter, this.cbCacheTTL);
                        failedToInsertCacheTypes = !insertResult ? HybridCacheType.CbMemCache | failedToInsertCacheTypes : failedToInsertCacheTypes;
                    }

                    if (!insertResult)
                    {
                        log.ErrorFormat("Failed inserting key {0} on the following hybridCacheTypes {1}", key, hybridCacheTypes.ToString());
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed to get key {0} from NewHybridCache with hybridCacheTypes {1}, MethodName {2} and funcParameters {3}", key, hybridCacheTypes.ToString(),
                                        fillObjectFromDbMethod.Method != null ? fillObjectFromDbMethod.Method.Name : "No_Method_Name",
                                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters"), ex);
            }

            return result;
        }

        // Is it needed? or we will just change version value
        public bool Remove(string key, HybridCacheType hybridCacheTypes, string version = null)
        {
            key = AddVersionOnKey(key, version);
            bool genericParameter = false;
            try
            {
                if (hybridCacheTypes.HasFlag(HybridCacheType.InMemoryCache))
                {
                    genericParameter = inMemoryCache.RemoveWithBoolResult(key);
                }

                if (hybridCacheTypes.HasFlag(HybridCacheType.CbCache))
                {
                    genericParameter = cbClient.Remove(key) && genericParameter;
                }

                if (!genericParameter && hybridCacheTypes.HasFlag(HybridCacheType.CbMemCache))
                {
                    genericParameter = memCacheCbClient.Remove(key) && genericParameter;
                }
            }

            catch (Exception ex)
            {
                genericParameter = false;
                log.Error(string.Format("Failed to remove key {0} from NewHybridCache with hybridCacheTypes {1}", key, hybridCacheTypes.ToString()), ex);
            }

            return genericParameter;
        }

        private string AddVersionOnKey(string key, string versionToAdd = null)
        {
            if (!string.IsNullOrEmpty(versionToAdd))
            {
                key = string.Format("{0}_V{1}", key, versionToAdd);
            }
            else if (!string.IsNullOrEmpty(this.version))
            {
                key = string.Format("{0}_V{1}", key, this.version);
            }
            
            return key;
        }

        [Flags]
        public enum HybridCacheType
        {
            None = 0,
            InMemoryCache = 1,
            CbCache = 2,
            CbMemCache = 4
        }
    }
}
