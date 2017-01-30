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
        private const string LAYERED_CACHE_TCM_CONFIG = "LayeredCache";

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

        public bool Get<T>(string key, ref T genericParameter, Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectMethod, Dictionary<string, object> funcParameters, string layeredCacheConfigName = null, List<string> inValidationKeys = null)
        {
            bool result = false;            
            List<LayeredCacheConfig> insertToCacheConfig = null;
            try
            {
                Tuple<T, long> tuple = null;
                result = TryGetFromCacheByConfig<T>(key, ref tuple, layeredCacheConfigName, out insertToCacheConfig, fillObjectMethod, funcParameters, inValidationKeys);
                genericParameter = tuple != null && tuple.Item1 != null ? tuple.Item1 : genericParameter;
                if (insertToCacheConfig != null && insertToCacheConfig.Count > 0 && result && tuple != null && tuple.Item1 != null)
                {
                    // set validation to now
                    Tuple<T, long> tupleToInsert = new Tuple<T, long>(tuple.Item1, Utils.UnixTimeStampNow());
                    foreach (LayeredCacheConfig cacheConfig in insertToCacheConfig)
                    {
                        if (!TryInsert<T>(key, tupleToInsert, cacheConfig))
                        {
                            log.ErrorFormat("Failed inserting key {0} to {1}", key, cacheConfig.Type.ToString());
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

        public bool GetValues<T>(List<string> keys, ref Dictionary<string, T> results, Func<Dictionary<string, object>, Tuple<Dictionary<string,T>, bool>> fillObjectsMethod,
                                    Dictionary<string, object> funcParameters, string layeredCacheConfigName = null, Dictionary<string, List<string>> inValidationKeysMap = null)
        {
            bool res = false;
            Dictionary<LayeredCacheConfig, List<string>> insertToCacheConfigMappings = null;
            Dictionary<string, Tuple<T, long>> resultsMapping = null;
            try
            {                
                res = TryGetValuesFromCacheByConfig<T>(keys, ref resultsMapping, layeredCacheConfigName, out insertToCacheConfigMappings, fillObjectsMethod, funcParameters, inValidationKeysMap);
                results = resultsMapping != null && resultsMapping.Count > 0 ? resultsMapping.ToDictionary(x => x.Key, x => x.Value.Item1) : null;
                if (insertToCacheConfigMappings != null && insertToCacheConfigMappings.Count > 0 && res && results != null)
                {
                    foreach (KeyValuePair<LayeredCacheConfig, List<string>> pair in insertToCacheConfigMappings)
                    {
                        foreach (string key in pair.Value)
                        {
                            if (results.ContainsKey(key) && results[key] != null)
                            {
                                // set validation to now
                                Tuple<T, long> tupleToInsert = new Tuple<T, long>(results[key], Utils.UnixTimeStampNow());
                                if (!TryInsert<T>(key, tupleToInsert, pair.Key))
                                {
                                    log.ErrorFormat("GetValues<T> - Failed inserting key {0} to {1}", key, pair.Key.Type.ToString());
                                }
                            }
                            else
                            {
                                log.ErrorFormat("GetValues<T> - key: {0} isn't contained in results or contained with null value", key);
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetValues with keys {0} from LayeredCache, layeredCacheConfigName {1}, MethodName {2} and funcParameters {3}", string.Join(",", keys),
                                        string.IsNullOrEmpty(layeredCacheConfigName) ? string.Empty : layeredCacheConfigName,
                                        fillObjectsMethod.Method != null ? fillObjectsMethod.Method.Name : "No_Method_Name",
                                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters"), ex);
            }            

            return res;
        }

        public bool SetInvalidationKey(string key, DateTime? updatedAt = null)
        {
            long valueToUpdate = Utils.UnixTimeStampNow();
            if (updatedAt.HasValue)
            {
                valueToUpdate = Utils.DateTimeToUnixTimestamp(updatedAt.Value);
            }

            return TrySetInValidationKey(key, valueToUpdate);
        }

        // Is it needed? or we will just change version value
        public bool Remove(string key, string layeredCacheConfigName = null, string version = null)
        {
            bool result = false;
            List<LayeredCacheConfig> layeredCacheConfig = null;
            try
            {
                if (GetLayeredCacheConfig(layeredCacheConfigName, out layeredCacheConfig))
                {
                    result = true;
                    foreach (LayeredCacheConfig cacheConfig in layeredCacheConfig)
                    {
                        result = TryRemove(key, cacheConfig, version) && result;
                    }
                }
                else
                {
                    log.ErrorFormat("Failed getting LayeredCacheConfig for configName: {0}", layeredCacheConfigName);
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

        #region Static Methods

        public static string GetBucketFromLayeredCacheConfig(LayeredCacheType cacheType)
        {
            string bucketName = string.Empty;
            try
            {
                LayeredCacheTcmConfig layeredCacheTcmConfig = GetLayeredCacheTcmConfig();
                if (layeredCacheTcmConfig != null && layeredCacheTcmConfig.BucketSettings != null && layeredCacheTcmConfig.BucketSettings.Count > 0)
                {
                    LayeredCacheBucketSettings bucketSettings = layeredCacheTcmConfig.BucketSettings.Where(x => x.CacheType.HasFlag(cacheType)).FirstOrDefault();
                    if (bucketSettings != null && bucketSettings.Bucket != eCouchbaseBucket.DEFAULT)
                    {
                        bucketName = bucketSettings.Bucket.ToString();
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetBucketFromLayeredCacheConfig for cacheType: {0}", cacheType.ToString()), ex);
            }

            return bucketName;
        }

        private static LayeredCacheTcmConfig GetLayeredCacheTcmConfig()
        {
            LayeredCacheTcmConfig layeredCacheTcmConfig = null;
            try
            {
                object obj = Utils.GetTcmGenericValue<object>(LAYERED_CACHE_TCM_CONFIG);
                if (obj != null)
                {
                    layeredCacheTcmConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<LayeredCacheTcmConfig>(obj.ToString(), layeredCacheConfigSerializerSettings);
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetLayeredCacheTcmConfig for key: {0}", LAYERED_CACHE_TCM_CONFIG), ex);
            }

            return layeredCacheTcmConfig;
        }        

        #endregion

        #region Private Methods

        private bool TryGetFromCacheByConfig<T>(string key, ref Tuple<T, long> tupleResult, string layeredCacheConfigName, out List<LayeredCacheConfig> insertToCacheConfig,
                                                Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectMethod, Dictionary<string, object> funcParameters, List<string> inValidationKeys = null)
        {
            bool result = false;
            List<LayeredCacheConfig> layeredCacheConfig = null;
            insertToCacheConfig = new List<LayeredCacheConfig>();
            try
            {
                if (GetLayeredCacheConfig(layeredCacheConfigName, out layeredCacheConfig))
                {
                    List<long> inValidationKeysResult = null;
                    if (TryGetInValidationKeys(inValidationKeys, ref inValidationKeysResult))
                    {
                        // if inValidationKeysResult.Max() we add 1 sec to the current time in case inValidationKey was created exactly at the same time as the cache itself was created
                        long maxInValidationKey = inValidationKeysResult != null && inValidationKeysResult.Count > 0 ? inValidationKeysResult.Max() + 1 : 0;
                        foreach (LayeredCacheConfig cacheConfig in layeredCacheConfig)
                        {
                            if (TryGetFromICachingService<T>(key, ref tupleResult, cacheConfig))
                            {
                                if (tupleResult != null && tupleResult.Item2 > maxInValidationKey)
                                {
                                    result = true;
                                    break;
                                }
                            }

                            // if result=true we won't get here (break) and if it isn't we need to insert into this cache later
                            insertToCacheConfig.Add(cacheConfig);
                        }

                        if (!result)
                        {
                            Tuple<T, bool> tuple = fillObjectMethod(funcParameters);
                            if (tuple != null)
                            {
                                tupleResult = new Tuple<T, long>(tuple.Item1, Utils.UnixTimeStampNow());
                                result = tuple.Item2;
                            }

                            if (!result)
                            {
                                log.ErrorFormat("Failed fillingObjectFromDbMethod for key {0} with MethodName {1} and funcParameters {2}", key,
                                                fillObjectMethod.Method != null ? fillObjectMethod.Method.Name : "No_Method_Name",
                                                funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters");
                            }
                        }
                    }
                }
                else
                {
                    log.ErrorFormat("Failed getting LayeredCacheConfig for configName: {0}", layeredCacheConfigName);
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

        private bool TryGetValuesFromCacheByConfig<T>(List<string> keys, ref Dictionary<string, Tuple<T, long>> tupleResults, string layeredCacheConfigName,
                                                        out Dictionary<LayeredCacheConfig, List<string>> insertToCacheConfig, Func<Dictionary<string, object>, Tuple<Dictionary<string, T>, bool>> fillObjectsMethod,
                                                        Dictionary<string, object> funcParameters, Dictionary<string, List<string>> inValidationKeysMap = null)
        {
            bool result = false;
            List<LayeredCacheConfig> layeredCacheConfig = null;
            insertToCacheConfig = new Dictionary<LayeredCacheConfig, List<string>>();
            try
            {
                if (GetLayeredCacheConfig(layeredCacheConfigName, out layeredCacheConfig))
                {
                    Dictionary<string, List<long>> inValidationKeysResultMap = null;
                    if (TryGetInValidationKeysMapping(inValidationKeysMap, ref inValidationKeysResultMap))
                    {
                        foreach (LayeredCacheConfig cacheConfig in layeredCacheConfig)
                        {
                            if (TryGetValuesFromICachingService<T>(keys, ref tupleResults, cacheConfig) && tupleResults != null)
                            {
                                foreach (KeyValuePair<string, Tuple<T, long>> pair in tupleResults)
                                {
                                    // if inValidationKeysResult.Max() we add 1 sec to the current time in case inValidationKey was created exactly at the same time as the cache itself was created
                                    long maxInValidationKey = inValidationKeysResultMap != null && inValidationKeysResultMap.Count > 0 && inValidationKeysResultMap.ContainsKey(pair.Key) ? inValidationKeysResultMap[pair.Key].Max() + 1 : 0;
                                    if (pair.Value == null || pair.Value.Item1 == null || pair.Value.Item2 <= maxInValidationKey)
                                    {
                                        if (insertToCacheConfig.ContainsKey(cacheConfig))
                                        {
                                            insertToCacheConfig[cacheConfig].Add(pair.Key);
                                        }
                                        else
                                        {
                                            insertToCacheConfig.Add(cacheConfig, new List<string>() { pair.Key });
                                        }
                                    }
                                }

                                if (!insertToCacheConfig.ContainsKey(cacheConfig) || insertToCacheConfig[cacheConfig].Count == 0)
                                {
                                    result = true;
                                    break;
                                }
                            }
                            else
                            {
                                // if result=true we won't get here (break) and if it isn't we need to insert all the keys into this cache later
                                insertToCacheConfig.Add(cacheConfig, new List<string>(keys));
                            }
                        }

                        if (!result)
                        {
                            Tuple<Dictionary<string, T>, bool> tuple = fillObjectsMethod(funcParameters);
                            if (tuple != null)
                            {
                                result = tuple.Item2;
                                if (tuple.Item1 != null)
                                {
                                    tupleResults = tuple.Item1.ToDictionary(x => x.Key, x => new Tuple<T, long>(x.Value, Utils.UnixTimeStampNow()));
                                }
                            }
                                                        
                            if (!result)
                            {
                                log.ErrorFormat("Failed fillingObjectFromDbMethod for key {0} with MethodName {1} and funcParameters {2}", string.Join(",", keys),
                                                fillObjectsMethod.Method != null ? fillObjectsMethod.Method.Name : "No_Method_Name",
                                                funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters");
                            }
                        }
                    }
                }
                else
                {
                    log.ErrorFormat("Failed getting LayeredCacheConfig for configName: {0}", layeredCacheConfigName);
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetValuesFromCacheByConfig with keys {0}, LayeredCacheTypes {1}, MethodName {2} and funcParameters {3}", string.Join(",", keys), GetLayeredCacheConfigTypesForLog(layeredCacheConfig),
                        fillObjectsMethod.Method != null ? fillObjectsMethod.Method.Name : "No_Method_Name",
                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters"), ex);
            }

            return result;            
        }

        private bool TryGetFromICachingService<T>(string key, ref Tuple<T, long> tupleResult, LayeredCacheConfig cacheConfig)
        {
            bool res = false;
            try
            {
                ICachingService cache = cacheConfig.GetICachingService();
                if (cache != null)
                {
                    Dictionary<string, string> keysMapping = GetVersionToOriginalKeyMap(new List<string>() { key });
                    if (keysMapping != null && keysMapping.Count > 0)
                    {
                        res = cache.Get<Tuple<T, long>>(keysMapping.Keys.First(), ref tupleResult);
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetFromICachingService with key {0}, LayeredCacheTypes {1}", key, GetLayeredCacheConfigTypesForLog(new List<LayeredCacheConfig>() { cacheConfig }), ex));
            }

            return res;
        }

        private bool TryGetValuesFromICachingService<T>(List<string> keys, ref Dictionary<string, Tuple<T, long>> tupleResultsMap, LayeredCacheConfig cacheConfig)
        {
            bool res = false;
            try
            {
                ICachingService cache = cacheConfig.GetICachingService();
                if (cache != null)
                {
                    Dictionary<string, string> keysMapping = GetVersionToOriginalKeyMap(keys);
                    if (keysMapping != null && keysMapping.Count > 0)
                    {
                        IDictionary<string, Tuple<T, long>> getResults = null;
                        res = cache.GetValues<Tuple<T, long>>(keysMapping.Keys.ToList(), ref getResults, true);
                        if (getResults != null)
                        {
                            tupleResultsMap = getResults.ToDictionary(x => keysMapping[x.Key], x => x.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetValuesFromICachingService with keys {0}, LayeredCacheTypes {1}", string.Join(",", keys), GetLayeredCacheConfigTypesForLog(new List<LayeredCacheConfig>() { cacheConfig }), ex));
            }

            return res;
        }

        private bool TryInsert<T>(string key, Tuple<T, long> tuple, LayeredCacheConfig cacheConfig)
        {
            bool res = false;
            try
            {
                ICachingService cache = cacheConfig.GetICachingService();                                
                if (cache != null)
                {
                    Dictionary<string, string> keyMappings = GetVersionToOriginalKeyMap(new List<string>() { key });
                    if (keyMappings != null && keyMappings.Count > 0)
                    {
                        string keyToInsert = keyMappings.Keys.First();
                        ulong version = 0;
                        if (cacheConfig.Type.HasFlag(LayeredCacheType.CbCache | LayeredCacheType.CbMemCache))
                        {

                            Tuple<T, long> getResult = default(Tuple<T, long>);
                            cache.GetWithVersion<Tuple<T, long>>(keyToInsert, out version, ref getResult);

                        }

                        res = cache.SetWithVersion<Tuple<T, long>>(keyToInsert, tuple, version, cacheConfig.TTL);
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryInsert with key {0}, LayeredCacheTypes {1}", key,
                                        GetLayeredCacheConfigTypesForLog(new List<LayeredCacheConfig>() { cacheConfig }), ex));
            }

            return res;

        }

        private bool TryRemove(string key, LayeredCacheConfig cacheConfig, string version = null)
        {
            bool result = false;            
            try
            {
                bool res = false;
                ICachingService cache = cacheConfig.GetICachingService();
                if (cache != null)
                {
                    Dictionary<string, string> keyMappings = GetVersionToOriginalKeyMap(new List<string>() { key });
                    if (keyMappings != null && keyMappings.Count > 0)
                    {
                        res = cache.RemoveKey(keyMappings.Keys.First());
                    }
                }

                return res;
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryRemove with key {0}, LayeredCacheTypes {1}", key, GetLayeredCacheConfigTypesForLog(new List<LayeredCacheConfig>() { cacheConfig }), ex));
            }

            return result;
        }

        private bool TryGetInValidationKeys(List<string> keys, ref List<long> inValidationKeys)
        {
            bool res = false;
            try
            {
                if (keys == null || keys.Count == 0)
                {
                    return true;
                }

                LayeredCacheConfig invalidationKeyCacheConfigSettings = GetLayeredCacheTcmConfig().InvalidationKeySettings;
                if (invalidationKeyCacheConfigSettings == null)
                {
                    return false;
                }

                ICachingService cache = invalidationKeyCacheConfigSettings.GetICachingService();
                if (cache != null)
                {
                    IDictionary<string, object> resultMap = cache.GetValues(keys);
                    if (resultMap != null)
                    {
                        inValidationKeys = new List<long>();
                        foreach (object obj in resultMap.Values)
                        {
                            long inValidationDate;
                            if (long.TryParse(obj.ToString(), out inValidationDate))
                            {
                                inValidationKeys.Add(inValidationDate);
                            }
                        }

                        res = true;
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetInValidationKeys with keys {0}", string.Join(",", keys)), ex);
            }

            return res;
        }

        private bool TryGetInValidationKeysMapping(Dictionary<string, List<string>> keyMappings, ref Dictionary<string, List<long>> inValidationKeysMapping)
        {
            bool res = true;
            try
            {
                if (keyMappings == null || keyMappings.Count == 0)
                {
                    return res;
                }

                inValidationKeysMapping = new Dictionary<string, List<long>>();
                foreach (KeyValuePair<string, List<string>> pair in keyMappings)
                {
                    List<long> invalidationKeys = null;
                    if (TryGetInValidationKeys(pair.Value, ref invalidationKeys))
                    {
                        inValidationKeysMapping.Add(pair.Key, invalidationKeys);
                    }
                    else
                    {
                        res = false;
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetInValidationKeys with keys {0}", keyMappings != null ? string.Join(",", keyMappings.Values) : ""), ex);
            }

            return res;
        }

        private bool TrySetInValidationKey(string key, long valueToUpdate)
        {
            bool res = false;
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return res;
                }

                LayeredCacheConfig invalidationKeyCacheConfigSettings = GetLayeredCacheTcmConfig().InvalidationKeySettings;
                if (invalidationKeyCacheConfigSettings == null)
                {
                    return res;
                }

                ICachingService cache = invalidationKeyCacheConfigSettings.GetICachingService();
                if (cache != null)
                {
                    ulong version = 0;
                    long getResult = 0;
                    cache.GetWithVersion<long>(key, out version, ref getResult);
                    res = cache.SetWithVersion<long>(key, valueToUpdate, version, invalidationKeyCacheConfigSettings.TTL);
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TrySetInValidationKey with key {0}", key), ex);
            }

            return res;
        }

        private bool GetLayeredCacheConfig(string configurationName, out List<LayeredCacheConfig> layeredCacheConfig)
        {            
            layeredCacheConfig = null;
            try
            {
                LayeredCacheTcmConfig layeredCacheTcmConfig = GetLayeredCacheTcmConfig();
                if (layeredCacheTcmConfig != null)
                {
                    if (!string.IsNullOrEmpty(configurationName) && layeredCacheTcmConfig.LayeredCacheSettings != null && layeredCacheTcmConfig.LayeredCacheSettings.ContainsKey(configurationName))
                    {
                        layeredCacheConfig = layeredCacheTcmConfig.LayeredCacheSettings[configurationName];
                    }
                    else if (layeredCacheTcmConfig.DefaultSettings != null && layeredCacheTcmConfig.DefaultSettings.Count > 0)
                    {
                        layeredCacheConfig = layeredCacheTcmConfig.DefaultSettings;
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetLayeredCacheConfig for configurationName: {0}", configurationName), ex);
            }

            return layeredCacheConfig != null;
        }        

        private Dictionary<string, string> GetVersionToOriginalKeyMap(List<string> keys, string versionToAdd = null)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            string versionValue = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(versionToAdd))
                {
                    versionValue = versionToAdd;
                }
                else
                {
                    LayeredCacheTcmConfig layeredCacheTcmConfig = GetLayeredCacheTcmConfig();
                    versionValue = layeredCacheTcmConfig != null ? layeredCacheTcmConfig.Version : string.Empty;
                }

                if (!string.IsNullOrEmpty(versionValue))
                {
                    res = keys.ToDictionary(x => x, x => string.Format("{0}_V{1}", x, versionValue));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddVersionOnKeysMap for the following keys: {0}", string.Join(",", keys)), ex);
            }

            return res;
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
