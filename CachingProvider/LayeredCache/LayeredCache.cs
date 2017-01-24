using CouchbaseManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;
using Newtonsoft.Json;
namespace CachingProvider.LayeredCache
{
    public class LayeredCache
    {        
        private const string IN_MEMORY_CACHE_NAME = "LayeredInMemoryCache";
        private const string CACHE_VERSION = "LayeredCache.Version";
        private const string BUCKET_SETTINGS = "LayeredCache.BucketSettings.{0}";
        private const string DEFAULT_CACHE_SETTINGS = "LayeredCache.DefaultSettings";
        private const string INVALIDATION_KEY_SETTINGS = "LayeredCache.InvalidationKeySettings";        

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());                 
        private static object locker = new object();
        private static LayeredCache instance = null;
        private static JsonSerializerSettings layeredCacheConfigSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };

        private LayeredCache() { }

        public static LayeredCache Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new LayeredCache();
                        }
                    }
                }

                return instance;
            }
        }

        #region Public Methods

        public bool Get<T>(string key, ref T genericParameter, Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectMethod, Dictionary<string, object> funcParameters, string layeredCacheConfigName = null)
        {
            bool result = false;            
            List<LayeredCacheConfig> insertToCacheConfig = null;
            try
            {
                key = AddVersionOnKey(key);
                result = TryGetFromCacheByConfig<T>(key, ref genericParameter, layeredCacheConfigName, out insertToCacheConfig, fillObjectMethod, funcParameters);
                if (insertToCacheConfig != null && insertToCacheConfig.Count > 0 && result && genericParameter != null)
                {
                    foreach (LayeredCacheConfig cacheConfig in insertToCacheConfig)
                    {
                        if (!TryInsert<T>(key, genericParameter, cacheConfig))
                        {
                            log.ErrorFormat("Failed inserting key {0} to {1}", key, cacheConfig.Type.ToString());
                        }
                        else
                        {
                            insertToCacheConfig.Add(cacheConfig);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed to get key {0} from LayeredCache, layeredCacheConfigName {1}, MethodName {2} and funcParameters {3}", key,
                                        string.IsNullOrEmpty(layeredCacheConfigName) ? string.Empty : layeredCacheConfigName,
                                        fillObjectMethod.Method != null ? fillObjectMethod.Method.Name : "No_Method_Name",
                                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters"), ex);
            }

            return result;
        }        

        // Is it needed? or we will just change version value
        public bool Remove(string key, string layeredCacheConfigName = null, string version = null)
        {
            key = AddVersionOnKey(key, version);
            bool result = false;
            List<LayeredCacheConfig> layeredCacheConfig = null;
            try
            {
                if (GetLayeredCacheConfig(layeredCacheConfigName, out layeredCacheConfig))
                {
                    result = true;
                    foreach (LayeredCacheConfig cacheConfig in layeredCacheConfig)
                    {
                        result = TryRemove(key, cacheConfig) && result;
                    }
                }                
            }

            catch (Exception ex)
            {
                result = false;
                log.Error(string.Format("Failed to remove key {0} from LayeredCache with layeredCacheConfigName {1} and version {2}", key,
                                        string.IsNullOrEmpty(layeredCacheConfigName) ? string.Empty : layeredCacheConfigName,
                                        string.IsNullOrEmpty(version) ? string.Empty : version), ex);
            }

            return result;
        }

        #endregion

        #region Private Methods

        private bool TryGetFromCacheByConfig<T>(string key, ref T genericParameter, string layeredCacheConfigName, out List<LayeredCacheConfig> insertToCacheConfig, Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectMethod, Dictionary<string, object> funcParameters)
        {
            bool result = false;
            List<LayeredCacheConfig> layeredCacheConfig = null;
            insertToCacheConfig = new List<LayeredCacheConfig>();
            try
            {
                if (GetLayeredCacheConfig(layeredCacheConfigName, out layeredCacheConfig))
                {                    
                    foreach (LayeredCacheConfig cacheConfig in layeredCacheConfig)
                    {
                        if (TryGetFromICachingService<T>(key, ref genericParameter, cacheConfig))
                        {
                            result = true;
                            break;
                        }
                        else
                        {
                            insertToCacheConfig.Add(cacheConfig);
                        }
                    }

                    if (!result)
                    {
                        Tuple<T, bool> tuple = fillObjectMethod(funcParameters);
                        genericParameter = tuple.Item1;
                        result = tuple.Item2;
                        if (!result)
                        {
                            log.ErrorFormat("Failed fillingObjectFromDbMethod for key {0} with MethodName {1} and funcParameters {2}", key,
                                            fillObjectMethod.Method != null ? fillObjectMethod.Method.Name : "No_Method_Name",
                                            funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters");
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetFromCacheByConfig with key {0}, LayeredCacheTypes {1}, MethodName {2} and funcParameters {3}", key, GetLayeredCacheConfigTypesForLog(layeredCacheConfig),
                        fillObjectMethod.Method != null ? fillObjectMethod.Method.Name : "No_Method_Name",
                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters"), ex);
            }

            return result;
        }

        private bool TryGetFromICachingService<T>(string key, ref T genericParameter, LayeredCacheConfig cacheConfig)
        {
            bool res = false;             
            try
            {
                ICachingService cache = GetICachingServiceByCacheConfig(cacheConfig);
                if (cache != null)
                {
                    res = cache.Get<T>(key, ref genericParameter);
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetFromICachingService with key {0}, LayeredCacheTypes {1}", key, GetLayeredCacheConfigTypesForLog(new List<LayeredCacheConfig>() { cacheConfig }), ex));
            }

            return res;
        }                

        private bool TryInsert<T>(string key, T genericParameter, LayeredCacheConfig cacheConfig)
        {
            bool res = false;
            try
            {
                ICachingService cache = GetICachingServiceByCacheConfig(cacheConfig);
                if (cache != null)
                {
                    if (cacheConfig.Type.HasFlag(LayeredCacheType.CbCache | LayeredCacheType.CbMemCache))
                    {
                        ulong version;
                        T getResult = default(T);
                        if (cache.GetWithVersion<T>(key, out version, ref getResult))
                        {
                            cache.SetWithVersion<T>(key, genericParameter, version, cacheConfig.TTL);
                        }
                    }
                    else
                    {
                        res = cache.Add<T>(key, genericParameter, cacheConfig.TTL);
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryInsert with key {0}, LayeredCacheTypes {1}", key,
                                        GetLayeredCacheConfigTypesForLog(new List<LayeredCacheConfig>() { cacheConfig }), ex));
            }

            return res;
            //if (insertToCacheTypes.HasFlag(LayeredCacheType.InMemoryCache))
            //{
            //    if (!inMemoryCache.AddGenericType<T>(key, genericParameter, this.innerCacheTTL))
            //    {
            //        log.ErrorFormat("Failed inserting key {0} to {1}", key, "LayeredCacheType.InMemoryCache");
            //    }
            //}

            //if (insertToCacheTypes.HasFlag(LayeredCacheType.CbCache))
            //{
            //    if (!cbClient.Add<T>(key, genericParameter, this.cbCacheTTL))
            //    {
            //        log.ErrorFormat("Failed inserting key {0} to {1}", key, "LayeredCacheType.CbCache");
            //    }
            //}

            //if (insertToCacheTypes.HasFlag(LayeredCacheType.CbMemCache))
            //{
            //    if (!memCacheCbClient.Add<T>(key, genericParameter, this.cbCacheTTL))
            //    {
            //        log.ErrorFormat("Failed inserting key {0} to {1}", key, "LayeredCacheType.CbMemCache");
            //    }
            //}
            throw new NotImplementedException();
        }

        private bool TryRemove(string key, LayeredCacheConfig cacheConfig)
        {
            bool result = false;            
            try
            {
                bool res = false;
                ICachingService cache = null;
                if (cache != null)
                {
                    res = cache.RemoveKey(key);
                }

                return res;
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryRemove with key {0}, LayeredCacheTypes {1}", key, GetLayeredCacheConfigTypesForLog(new List<LayeredCacheConfig>() { cacheConfig }), ex));
            }

            return result;
        }

        private ICachingService GetICachingServiceByCacheConfig(LayeredCacheConfig cacheConfig)
        {
            ICachingService cache = null;
            try
            {
                switch (cacheConfig.Type)
                {
                    case LayeredCacheType.None:
                        break;
                    case LayeredCacheType.InMemoryCache:
                        InMemoryLayeredCacheConfig inMemoryCache = cacheConfig as InMemoryLayeredCacheConfig;
                        if (inMemoryCache != null)
                        {
                            cache = SingleInMemoryCacheManager.Instance(inMemoryCache.CacheName, cacheConfig.TTL);
                        }
                        break;
                    case LayeredCacheType.CbCache:
                    case LayeredCacheType.CbMemCache:
                        CbLayeredCacheConfig cbCache = cacheConfig as CbLayeredCacheConfig;
                        if (cbCache != null)
                        {
                            string bucketName = string.IsNullOrEmpty(cbCache.Bucket) ? Utils.GetTcmGenericValue<string>(string.Format(BUCKET_SETTINGS, cbCache.Type.ToString())) : cbCache.Bucket;
                            cache = CouchBaseCache<object>.GetInstance(bucketName);
                        }
                        break;
                    default:
                        break;
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetICachingServiceByCacheConfig with LayeredCacheTypes {0}", GetLayeredCacheConfigTypesForLog(new List<LayeredCacheConfig>() { cacheConfig }), ex));
            }

            return cache;
        }

        private bool GetLayeredCacheConfig(string configurationName, out List<LayeredCacheConfig> layeredCacheConfig)
        {
            layeredCacheConfig = null;
            try
            {
                string configurationValue = Utils.GetTcmGenericValue<string>(string.Format("LayeredCache.{0}", configurationName));
                if (!string.IsNullOrEmpty(configurationValue))
                {
                    string testConfigurationValue = @"[{""Type"": ""InMemoryCache"",""TTL"": 30},{ ""Type"": ""CbMemCache"", ""TTL"": 600 }]";
                    LayeredCacheConfig testConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<LayeredCacheConfig>(testConfigurationValue, layeredCacheConfigSerializerSettings);
                    layeredCacheConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LayeredCacheConfig>>(configurationValue, layeredCacheConfigSerializerSettings);
                }
                else
                {
                    layeredCacheConfig = GetDefaultCacheConfigSettings();
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetLayeredCacheConfig for configurationName: {0}", configurationName), ex);
            }

            return layeredCacheConfig != null;
        }

        private List<LayeredCacheConfig> GetDefaultCacheConfigSettings()
        {
            List<LayeredCacheConfig> layeredCacheConfig = null;
            try
            {
                string defaultSettings = Utils.GetTcmGenericValue<string>(DEFAULT_CACHE_SETTINGS);
                if (!string.IsNullOrEmpty(defaultSettings))
                {
                    layeredCacheConfig = layeredCacheConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LayeredCacheConfig>>(defaultSettings, layeredCacheConfigSerializerSettings);
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetDefaultCacheConfigSettings for configurationName: {0}", DEFAULT_CACHE_SETTINGS), ex);
            }

            return layeredCacheConfig;
        }

        private string AddVersionOnKey(string key, string versionToAdd = null)
        {
            if (!string.IsNullOrEmpty(versionToAdd))
            {
                key = string.Format("{0}_V{1}", key, versionToAdd);
            }
            else
            {
                string layeredCacheVersion = TCMClient.Settings.Instance.GetValue<string>(CACHE_VERSION);
                key = !string.IsNullOrEmpty(layeredCacheVersion) ? string.Format("{0}_V{1}", key, layeredCacheVersion) : key;
            }

            return key;
        }

        private string GetLayeredCacheConfigTypesForLog(List<LayeredCacheConfig> layeredCacheConfig)
        {
            if (layeredCacheConfig != null && layeredCacheConfig.Count > 0)
            {
                return string.Join(",", layeredCacheConfig.Select(x => x.Type.ToString()).ToList());
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion
              
    }
}
