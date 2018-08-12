using CouchbaseManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;
using Newtonsoft.Json;
using System.Web;
using ConfigurationManager;

namespace CachingProvider.LayeredCache
{
    public class LayeredCache
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();
        private static LayeredCache instance = null;
        private static JsonSerializerSettings layeredCacheConfigSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
        private static LayeredCacheTcmConfig layeredCacheTcmConfig = null;
        public const string MISSING_KEYS = "NeededKeys";
        public const string IS_READ_ACTION = "IsReadAction";
        public static readonly HashSet<string> readActions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "get", "list"
        };

        private LayeredCache()
        {
            layeredCacheTcmConfig = GetLayeredCacheTcmConfig();
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="genericParameter"></param>
        /// <param name="fillObjectMethod"></param>
        /// <param name="funcParameters"></param>
        /// <param name="groupId"></param>
        /// <param name="layeredCacheConfigName"></param>
        /// <param name="inValidationKeys"></param>
        /// <returns></returns>
        public bool Get<T>(string key, ref T genericParameter, Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectMethod, Dictionary<string, object> funcParameters,
                            int groupId, string layeredCacheConfigName, List<string> inValidationKeys = null)
        {
            bool result = false;
            List<LayeredCacheConfig> insertToCacheConfig = null;
            try
            {
                bool isReadAction = IsReadAction();
                if (isReadAction && TryGetKeyFromSession<T>(key, ref genericParameter))
                {
                    return true;
                }

                Tuple<T, long> tuple = null;
                // save data in cache only if result is true!!!!
                result = TryGetFromCacheByConfig<T>(key, ref tuple, layeredCacheConfigName, out insertToCacheConfig, fillObjectMethod, funcParameters, groupId, inValidationKeys);
                genericParameter = tuple != null && tuple.Item1 != null ? tuple.Item1 : genericParameter;
                if (isReadAction && result)
                {
                    Dictionary<string, T> resultsToAdd = new Dictionary<string, T>();
                    resultsToAdd.Add(key, genericParameter);
                    InsertResultsToSession<T>(resultsToAdd);
                }

                if (insertToCacheConfig != null && insertToCacheConfig.Count > 0 && result && tuple != null && tuple.Item1 != null)
                {
                    // set validation to now
                    Tuple<T, long> tupleToInsert = new Tuple<T, long>(tuple.Item1, Utils.UnixTimeStampNow());
                    Dictionary<string, string> keyMappings = GetVersionKeyToOriginalKeyMap(new List<string>() { key }, groupId);
                    if (keyMappings != null && keyMappings.Count > 0)
                    {
                        foreach (LayeredCacheConfig cacheConfig in insertToCacheConfig)
                        {
                            if (!TryInsert<T>(keyMappings.Keys.First(), tupleToInsert, cacheConfig))
                            {
                                log.ErrorFormat("Failed inserting key {0} to {1}", keyMappings.Keys.First(), cacheConfig.Type.ToString());
                            }
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

        public bool GetValues<T>(Dictionary<string, string> KeyToOriginalValueMap, ref Dictionary<string, T> results, Func<Dictionary<string, object>, Tuple<Dictionary<string, T>, bool>> fillObjectsMethod,
                                    Dictionary<string, object> funcParameters, int groupId, string layeredCacheConfigName, Dictionary<string, List<string>> inValidationKeysMap = null)
        {
            bool res = false;
            Dictionary<LayeredCacheConfig, List<string>> insertToCacheConfigMappings = null;
            Dictionary<string, Tuple<T, long>> resultsMapping = null;
            Dictionary<string, T> sessionResultMapping = null;
            bool shouldAddSessionResults = false;
            try
            {
                bool isReadAction = IsReadAction();
                Dictionary<string, string> KeyToOriginalValueMapAfterSession = new Dictionary<string, string>(KeyToOriginalValueMap);
                if (isReadAction && TryGetKeysFromSession<T>(KeyToOriginalValueMap.Keys.ToList(), ref sessionResultMapping))
                {
                    shouldAddSessionResults = true;
                    foreach (string key in sessionResultMapping.Keys)
                    {
                        KeyToOriginalValueMapAfterSession.Remove(key);
                    }
                }

                if (KeyToOriginalValueMapAfterSession.Count > 0)
                {
                    res = TryGetValuesFromCacheByConfig<T>(KeyToOriginalValueMapAfterSession, ref resultsMapping, layeredCacheConfigName, out insertToCacheConfigMappings, fillObjectsMethod, funcParameters, groupId, inValidationKeysMap);
                    results = resultsMapping != null && resultsMapping.Count > 0 ? resultsMapping.ToDictionary(x => x.Key, x => x.Value.Item1) : null;
                    if (isReadAction && res)
                    {
                        InsertResultsToSession<T>(results);
                    }

                    if (insertToCacheConfigMappings != null && insertToCacheConfigMappings.Count > 0 && res && results != null)
                    {
                        Dictionary<string, string> keyToVersionMappings = GetOriginalKeyToVersionKeyMap(KeyToOriginalValueMapAfterSession.Keys.ToList(), groupId);
                        if (keyToVersionMappings != null && keyToVersionMappings.Count > 0)
                        {
                            foreach (KeyValuePair<LayeredCacheConfig, List<string>> pair in insertToCacheConfigMappings)
                            {
                                foreach (string key in pair.Value)
                                {
                                    if (results.ContainsKey(key) && results[key] != null)
                                    {
                                        // set validation to now
                                        Tuple<T, long> tupleToInsert = new Tuple<T, long>(results[key], Utils.UnixTimeStampNow());
                                        if (!TryInsert<T>(keyToVersionMappings[key], tupleToInsert, pair.Key))
                                        {
                                            log.ErrorFormat("GetValues<T> - Failed inserting key {0} to {1}", keyToVersionMappings[key], pair.Key.Type.ToString());
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
                }
                else
                {
                    res = true;
                }

                if (shouldAddSessionResults)
                {
                    if (results == null)
                    {
                        results = new Dictionary<string, T>();
                    }

                    foreach (KeyValuePair<string, T> pair in sessionResultMapping)
                    {
                        results.Add(pair.Key, pair.Value);
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetValues with keys {0} from LayeredCache, layeredCacheConfigName {1}, MethodName {2} and funcParameters {3}", string.Join(",", KeyToOriginalValueMap.Keys),
                                        string.IsNullOrEmpty(layeredCacheConfigName) ? string.Empty : layeredCacheConfigName,
                                        fillObjectsMethod.Method != null ? fillObjectsMethod.Method.Name : "No_Method_Name",
                                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters"), ex);
            }

            return res;
        }

        public bool GetWithAppDomainCache<T>(string key, ref T genericParameter, Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectMethod, Dictionary<string, object> funcParameters,
                                                int groupId, string layeredCacheConfigName, double ttlOnAppDomainInSeconds = 60, List<string> inValidationKeys = null)
        {
            bool result = false;
            try
            {
                if (TryGetKeyFromAppDomainCache<T>(key, ref genericParameter))
                {
                    result = true;
                }
                else if (Get<T>(key, ref genericParameter, fillObjectMethod, funcParameters, groupId, layeredCacheConfigName, inValidationKeys) && ttlOnAppDomainInSeconds > 0)
                {
                    Dictionary<string, T> resultsToAdd = new Dictionary<string, T>();
                    resultsToAdd.Add(key, genericParameter);
                    InsertResultsToAppDomainCache<T>(resultsToAdd, ttlOnAppDomainInSeconds);
                    result = true;
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed to get key {0} from LayeredCache on GetWithAppDomainCache, layeredCacheConfigName {1}, MethodName {2} and funcParameters {3}", key,
                                        string.IsNullOrEmpty(layeredCacheConfigName) ? string.Empty : layeredCacheConfigName,
                                        fillObjectMethod.Method != null ? fillObjectMethod.Method.Name : "No_Method_Name",
                                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters"), ex);
            }

            return result;
        }

        public bool SetInvalidationKey(string key, DateTime? updatedAt = null)
        {
            bool res = false;
            try
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Response.Headers.Set("no-cache", "true");
                }

                long valueToUpdate = Utils.UnixTimeStampNow();
                if (updatedAt.HasValue)
                {
                    valueToUpdate = Utils.DateTimeToUnixTimestamp(updatedAt.Value);
                }

                res = TrySetInValidationKey(key, valueToUpdate);
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed SetInvalidationKey, key: {0}, updatedAt: {1}", key, updatedAt.HasValue ? updatedAt.Value.ToString() : "null"), ex);
            }

            return res;
        }

        public bool InvalidateKeys(List<string> keys, DateTime? updatedAt = null)
        {
            if (keys == null || keys.Count == 0)
            {
                return false;
            }

            bool result = true;

            try
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Response.Headers.Set("no-cache", "true");
                }

                long valueToUpdate = Utils.UnixTimeStampNow();

                if (updatedAt.HasValue)
                {
                    valueToUpdate = Utils.DateTimeToUnixTimestamp(updatedAt.Value);
                }

                foreach (var key in keys)
                {
                    result &= TrySetInValidationKey(key, valueToUpdate);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed InvalidateKeys, keys: {0}, updatedAt: {1}",
                    string.Join(";", keys), updatedAt.HasValue ? updatedAt.Value.ToString() : "null"), ex);
            }

            return result;
        }

        public void SetReadingInvalidationKeys(List<string> invalidationKeys)
        {
            try
            {
                if (invalidationKeys != null)
                {

                    if (HttpContext.Current != null)
                    {
                        var invalidationKeysHeader = HttpContext.Current.Response.Headers["invalidationKeys"];

                        if (invalidationKeysHeader == null)
                        {
                            string invalidationKeysString = string.Join(";", invalidationKeys);

                            HttpContext.Current.Response.Headers.Add("invalidationKeys", invalidationKeysString);
                        }
                        else
                        {
                            // Split and create hashset of all current invalidation keys - to avoid duplications
                            HashSet<string> invalidationKeysHashSet = new HashSet<string>(invalidationKeysHeader.Split(';'));

                            invalidationKeysHashSet.UnionWith(invalidationKeys);

                            string invalidationKeysString = string.Join(";", invalidationKeysHashSet);

                            HttpContext.Current.Response.Headers.Set("invalidationKeys", invalidationKeysString);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed setting reading invalidation keys, ex = {0}", ex);
            }
        }

        public bool SetLayeredCacheGroupConfig(int groupId, int? version = null, bool? shouldDisableLayeredCache = null,
                                                List<string> layeredCacheSettingsToExclude = null, bool? shouldOverrideExistingExludeSettings = false,
                                                List<string> layeredCacheInvalidationKeySettingsToExclude = null, bool? shouldOverrideExistingInvalidationKeyExludeSettings = false)
        {
            bool res = false;
            try
            {
                string key = GetLayeredCacheGroupConfigKey(groupId);
                LayeredCacheGroupConfig groupConfig;
                if (TryGetLayeredCacheGroupConfig(groupId, out groupConfig, false))
                {
                    if (version.HasValue)
                    {
                        groupConfig.Version = version.Value;
                    }

                    if (shouldDisableLayeredCache.HasValue)
                    {
                        groupConfig.DisableLayeredCache = shouldDisableLayeredCache.Value;
                    }

                    if (layeredCacheSettingsToExclude != null)
                    {
                        if (shouldOverrideExistingExludeSettings.HasValue && shouldOverrideExistingExludeSettings.Value)
                        {
                            groupConfig.LayeredCacheSettingsToExclude = new HashSet<string>(layeredCacheSettingsToExclude, StringComparer.InvariantCultureIgnoreCase);
                        }
                        else
                        {
                            foreach (string keyToAdd in layeredCacheSettingsToExclude)
                            {
                                if (!groupConfig.LayeredCacheSettingsToExclude.Contains(keyToAdd))
                                {
                                    groupConfig.LayeredCacheSettingsToExclude.Add(keyToAdd);
                                }
                            }
                        }
                    }

                    if (layeredCacheInvalidationKeySettingsToExclude != null)
                    {
                        if (shouldOverrideExistingInvalidationKeyExludeSettings.HasValue && shouldOverrideExistingInvalidationKeyExludeSettings.Value)
                        {
                            groupConfig.LayeredCacheInvalidationKeySettingsToExclude = new HashSet<string>(layeredCacheInvalidationKeySettingsToExclude, StringComparer.InvariantCultureIgnoreCase);
                        }
                        else
                        {
                            foreach (string keyToAdd in layeredCacheInvalidationKeySettingsToExclude)
                            {
                                if (!groupConfig.LayeredCacheInvalidationKeySettingsToExclude.Contains(keyToAdd))
                                {
                                    groupConfig.LayeredCacheInvalidationKeySettingsToExclude.Add(keyToAdd);
                                }
                            }
                        }
                    }
                }

                if (groupConfig != null)
                {

                    res = TrySetLayeredGroupCacheConfig(key, groupConfig);
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed SetLayeredCacheGroupConfig, groupId: {0}, version: {1}, shouldDisableLayeredCache: {2}, layeredCacheSettingsToExclude: {3}", groupId, version,
                                            shouldDisableLayeredCache, layeredCacheSettingsToExclude != null ? string.Join(",", layeredCacheSettingsToExclude) : "null"), ex);
            }

            return res;
        }

        public LayeredCacheGroupConfig GetLayeredCacheGroupConfig(int groupId)
        {
            LayeredCacheGroupConfig groupConfig = null;
            try
            {
                string key = GetLayeredCacheGroupConfigKey(groupId);                
                if (!TryGetLayeredCacheGroupConfig(groupId, out groupConfig, false))
                {
                    log.DebugFormat("Failed getting GetLayeredCacheGroupConfig for groupId: {0}", groupId);
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetLayeredCacheGroupConfig, groupId: {0}", groupId), ex);
            }

            return groupConfig;
        }

        // Is it needed? or we will just change version value
        public bool Remove(string key, int groupId, string layeredCacheConfigName = null, string version = null)
        {
            bool result = false;
            List<LayeredCacheConfig> layeredCacheConfig;
            try
            {
                if (TryGetLayeredCacheConfig(layeredCacheConfigName, out layeredCacheConfig))
                {
                    result = true;
                    foreach (LayeredCacheConfig cacheConfig in layeredCacheConfig)
                    {
                        result = TryRemove(key, cacheConfig, groupId, version) && result;
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

        public bool IsReadAction()
        {
            return HttpContext.Current != null && 
                   HttpContext.Current.Items != null && 
                   HttpContext.Current.Items[LayeredCache.IS_READ_ACTION] != null ? (bool)HttpContext.Current.Items[LayeredCache.IS_READ_ACTION] : false;
        }

        public bool TryGetKeyFromSession<T>(string key, ref T genericParameter)
        {
            bool res = false;
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Items[key] != null)
                {
                    genericParameter = (T)HttpContext.Current.Items[key];
                    res = genericParameter != null && true;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to get key {0} from TryGetFromSession", key), ex);
            }

            return res;
        }

        public void InsertResultsToSession<T>(Dictionary<string, T> results)
        {
            if (HttpContext.Current != null && HttpContext.Current.Items != null)
            {
                foreach (KeyValuePair<string, T> pair in results)
                {
                    HttpContext.Current.Items[pair.Key] = pair.Value;
                }
            }
        }

        public bool ShouldGoToCache(string layeredCacheConfigName, int groupId)
        {
            List<LayeredCacheConfig> layeredCacheConfig = null;

            return ShouldGoToCache(layeredCacheConfigName, groupId, ref layeredCacheConfig);
        }

        #endregion

        #region Static Methods

        public static string GetBucketFromLayeredCacheConfig(LayeredCacheType cacheType)
        {
            string bucketName = string.Empty;
            try
            {
                if (layeredCacheTcmConfig != null && layeredCacheTcmConfig.BucketSettings != null && layeredCacheTcmConfig.BucketSettings.Count > 0)
                {
                    LayeredCacheBucketSettings bucketSettings = layeredCacheTcmConfig.BucketSettings.Where(x => x.CacheType == cacheType).FirstOrDefault();
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
                string layeredCacheConfigurationString = ApplicationConfiguration.LayeredCacheConfigurationValidation.Value;
                if (!string.IsNullOrEmpty(layeredCacheConfigurationString))
                {
                    layeredCacheTcmConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<LayeredCacheTcmConfig>(layeredCacheConfigurationString, layeredCacheConfigSerializerSettings);
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed GetLayeredCacheTcmConfig", ex);
            }

            return layeredCacheTcmConfig;
        }

        #endregion

        #region Private Methods

        #region Get

        private bool TryGetFromCacheByConfig<T>(string key, ref Tuple<T, long> tupleResult, string layeredCacheConfigName, out List<LayeredCacheConfig> insertToCacheConfig,
                                                Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectMethod, Dictionary<string, object> funcParameters,
                                                int groupId, List<string> inValidationKeys = null)
        {
            bool result = false;
            List<LayeredCacheConfig> layeredCacheConfig = null;
            insertToCacheConfig = new List<LayeredCacheConfig>();
            try
            {
                if (ShouldGoToCache(layeredCacheConfigName, groupId, ref layeredCacheConfig))
                {
                    long maxInValidationDate = 0;
                    bool hasMaxInvalidationDate = false;
                    foreach (LayeredCacheConfig cacheConfig in layeredCacheConfig)
                    {
                        if (TryGetFromICachingService<T>(key, ref tupleResult, cacheConfig, groupId))
                        {
                            if (!hasMaxInvalidationDate)
                            {
                                hasMaxInvalidationDate = TryGetMaxInValidationKeysDate(layeredCacheConfigName, groupId, inValidationKeys, out maxInValidationDate);
                                if (!hasMaxInvalidationDate)
                                {
                                    log.ErrorFormat("Error getting inValidationKeysMaxDate for key: {0}, layeredCacheConfigName: {1}, groupId: {2}, invalidationKeys: {3}",
                                                    key, layeredCacheConfigName, groupId, inValidationKeys != null ? string.Join(",", inValidationKeys) : "null");
                                    insertToCacheConfig.Add(cacheConfig);
                                    continue;
                                }

                            }
                            // we add 1 sec to the current time in case inValidationKey was created exactly at the same time as the cache itself was created
                            if (tupleResult != null && tupleResult.Item2 > maxInValidationDate + 1)
                            {
                                result = true;
                                break;
                            }
                        }

                        // if result=true we won't get here (break) and if it isn't we need to insert into this cache later
                        insertToCacheConfig.Add(cacheConfig);
                    }
                }
                if (!result && fillObjectMethod != null)
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

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetFromCacheByConfig with key {0}, LayeredCacheTypes {1}, MethodName {2} and funcParameters {3}", key, GetLayeredCacheConfigTypesForLog(layeredCacheConfig),
                        fillObjectMethod.Method != null ? fillObjectMethod.Method.Name : "No_Method_Name",
                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters"), ex);
            }

            return result;
        }

        private bool TryGetValuesFromCacheByConfig<T>(Dictionary<string, string> KeyToOriginalValueMap, ref Dictionary<string, Tuple<T, long>> tupleResults, string layeredCacheConfigName,
                                                        out Dictionary<LayeredCacheConfig, List<string>> insertToCacheConfig, Func<Dictionary<string, object>, Tuple<Dictionary<string, T>, bool>> fillObjectsMethod,
                                                        Dictionary<string, object> funcParameters, int groupId, Dictionary<string, List<string>> inValidationKeysMap = null)
        {
            bool result = false;
            List<LayeredCacheConfig> layeredCacheConfig = null;
            insertToCacheConfig = new Dictionary<LayeredCacheConfig, List<string>>();
            try
            {
                HashSet<string> keysToGet = new HashSet<string>(KeyToOriginalValueMap.Keys);
                Dictionary<string, Tuple<T, long>> resultsToAdd = new Dictionary<string, Tuple<T, long>>();
                tupleResults = new Dictionary<string, Tuple<T, long>>();
                if (ShouldGoToCache(layeredCacheConfigName, groupId, ref layeredCacheConfig))
                {
                    Dictionary<string, KeyValuePair<bool, long>> inValidationKeysMaxDateMapping = null;
                    bool hasMaxInvalidationDates = false;
                    foreach (LayeredCacheConfig cacheConfig in layeredCacheConfig)
                    {
                        if (TryGetValuesFromICachingService<T>(keysToGet.ToList(), ref resultsToAdd, cacheConfig, groupId) && resultsToAdd != null && resultsToAdd.Count > 0)
                        {
                            if (!hasMaxInvalidationDates)
                            {
                                hasMaxInvalidationDates = TryGetInValidationKeysMaxDateMapping(layeredCacheConfigName, groupId, inValidationKeysMap, ref inValidationKeysMaxDateMapping);
                                if (!hasMaxInvalidationDates)
                                {
                                    log.ErrorFormat("Error getting inValidationKeysMaxDateMapping for keys: {0}, layeredCacheConfigName: {1}, groupId: {2}",
                                                     string.Join(",", keysToGet), layeredCacheConfigName, groupId);
                                    insertToCacheConfig.Add(cacheConfig, new List<string>(keysToGet));
                                    continue;
                                }
                            }

                            List<string> keysToIterate = new List<string>(keysToGet);
                            foreach (string keyToGet in keysToIterate)
                            {
                                bool isKeyInTupleResult = false;
                                if (resultsToAdd.ContainsKey(keyToGet))
                                {
                                    Tuple<T, long> tuple = resultsToAdd[keyToGet];
                                    long maxInValidationDate = inValidationKeysMaxDateMapping != null && inValidationKeysMaxDateMapping.Count > 0 && inValidationKeysMaxDateMapping.ContainsKey(keyToGet) ? inValidationKeysMaxDateMapping[keyToGet].Value + 1 : 0;
                                    if (tuple != null && tuple.Item1 != null && tuple.Item2 > maxInValidationDate)
                                    {
                                        tupleResults.Add(keyToGet, new Tuple<T, long>(tuple.Item1, tuple.Item2));
                                        isKeyInTupleResult = true;
                                        keysToGet.Remove(keyToGet);
                                    }
                                }

                                if (!isKeyInTupleResult)
                                {
                                    if (insertToCacheConfig.ContainsKey(cacheConfig))
                                    {
                                        insertToCacheConfig[cacheConfig].Add(keyToGet);
                                    }
                                    else
                                    {
                                        insertToCacheConfig.Add(cacheConfig, new List<string>() { keyToGet });
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
                            insertToCacheConfig.Add(cacheConfig, new List<string>(keysToGet));
                        }
                    }
                }
                else
                {
                    log.ErrorFormat("Didn't go to cache for key: {0}, layeredCacheConfigName: {1}, groupId: {2}", string.Join(",", keysToGet), layeredCacheConfigName, groupId);
                }

                if (!result)
                {
                    if (keysToGet.Count < KeyToOriginalValueMap.Count)
                    {
                        if (funcParameters != null && !funcParameters.ContainsKey(MISSING_KEYS))
                        {
                            funcParameters.Add(MISSING_KEYS, KeyToOriginalValueMap.Where(x => keysToGet.Contains(x.Key)).Select(x => x.Value).ToList());
                        }
                    }

                    Tuple<Dictionary<string, T>, bool> tuple = fillObjectsMethod(funcParameters);
                    if (tuple != null)
                    {
                        result = tuple.Item2;
                        if (tuple.Item1 != null)
                        {
                            foreach (KeyValuePair<string, T> pair in tuple.Item1.Where(x => keysToGet.Contains(x.Key)))
                            {
                                tupleResults.Add(pair.Key, new Tuple<T, long>(pair.Value, Utils.UnixTimeStampNow()));
                            }
                        }
                    }

                    if (!result)
                    {
                        log.ErrorFormat("Failed fillingObjectFromDbMethod for key {0} with MethodName {1} and funcParameters {2}", string.Join(",", KeyToOriginalValueMap.Keys),
                                        fillObjectsMethod.Method != null ? fillObjectsMethod.Method.Name : "No_Method_Name",
                                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters");
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetValuesFromCacheByConfig with keys {0}, LayeredCacheTypes {1}, MethodName {2} and funcParameters {3}", string.Join(",", KeyToOriginalValueMap.Keys), GetLayeredCacheConfigTypesForLog(layeredCacheConfig),
                        fillObjectsMethod.Method != null ? fillObjectsMethod.Method.Name : "No_Method_Name",
                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters"), ex);
            }

            return result;
        }

        private bool TryGetFromICachingService<T>(string key, ref Tuple<T, long> tupleResult, LayeredCacheConfig cacheConfig, int groupId)
        {
            bool res = false;
            try
            {
                ICachingService cache = cacheConfig.GetICachingService();
                if (cache != null)
                {
                    Dictionary<string, string> keysMapping = GetVersionKeyToOriginalKeyMap(new List<string>() { key }, groupId);
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

        private bool TryGetValuesFromICachingService<T>(List<string> keys, ref Dictionary<string, Tuple<T, long>> tupleResultsMap, LayeredCacheConfig cacheConfig, int groupId)
        {
            bool res = false;
            try
            {
                ICachingService cache = cacheConfig.GetICachingService();
                if (cache != null)
                {
                    Dictionary<string, string> keysMapping = GetVersionKeyToOriginalKeyMap(keys, groupId);
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

        private bool TryGetMaxInValidationKeysDate(string layeredCacheConfigName, int groupId, List<string> keys, out long MaxInValidationDate)
        {
            bool res = false;
            MaxInValidationDate = 0;
            List<LayeredCacheConfig> invalidationKeyCacheConfig = null;
            try
            {
                if (keys == null || keys.Count == 0)
                {
                    return true;
                }

                if (ShouldCheckInvalidationKey(layeredCacheConfigName, groupId, ref invalidationKeyCacheConfig))
                {
                    if (invalidationKeyCacheConfig == null)
                    {
                        return false;
                    }

                    Dictionary<LayeredCacheConfig, List<string>> insertToCacheConfig = new Dictionary<LayeredCacheConfig, List<string>>();
                    HashSet<string> keysToGet = new HashSet<string>(keys);
                    Dictionary<string, long> compeleteResultMap = new Dictionary<string, long>();
                    foreach (LayeredCacheConfig cacheConfig in invalidationKeyCacheConfig)
                    {
                        ICachingService cache = cacheConfig.GetICachingService();
                        if (cache != null)
                        {
                            IDictionary<string, long> resultMap = null;
                            bool getSuccess = cache.GetValues<long>(keysToGet.ToList(), ref resultMap, true);
                            if (getSuccess && resultMap != null)
                            {
                                bool shouldSearchKeyInResult = cacheConfig.Type == LayeredCacheType.CbCache || cacheConfig.Type == LayeredCacheType.CbMemCache;
                                foreach (string keyToGet in keys)
                                {
                                    if (shouldSearchKeyInResult || resultMap.ContainsKey(keyToGet))
                                    {
                                        compeleteResultMap[keyToGet] = resultMap.ContainsKey(keyToGet) ? resultMap[keyToGet] : 0;
                                        keysToGet.Remove(keyToGet);
                                    }
                                    else
                                    {
                                        if (insertToCacheConfig.ContainsKey(cacheConfig))
                                        {
                                            insertToCacheConfig[cacheConfig].Add(keyToGet);
                                        }
                                        else
                                        {
                                            insertToCacheConfig[cacheConfig] = new List<string>() { keyToGet };
                                        }
                                    }
                                }

                                // found all keys
                                if (keysToGet.Count == 0)
                                {
                                    res = true;
                                    foreach (object obj in compeleteResultMap.Values)
                                    {
                                        long inValidationDate;
                                        if (long.TryParse(obj.ToString(), out inValidationDate) && inValidationDate > MaxInValidationDate)
                                        {
                                            MaxInValidationDate = inValidationDate;
                                        }
                                    }

                                    break;
                                }
                            }
                            else
                            {
                                insertToCacheConfig.Add(cacheConfig, new List<string>(keys));
                                continue;
                            }
                        }
                    }

                    if (insertToCacheConfig != null && insertToCacheConfig.Count > 0 && res && compeleteResultMap != null)
                    {
                        foreach (KeyValuePair<LayeredCacheConfig, List<string>> pair in insertToCacheConfig)
                        {
                            foreach (string keyToInsert in pair.Value)
                            {
                                // in case invalidation key value wasn't found on CB, we know it was never set and we can put the value 0
                                long invalidationDateToInsert = compeleteResultMap.ContainsKey(keyToInsert) ? compeleteResultMap[keyToInsert] : 0;
                                if (!TrySetInvalidationKeyWithCacheConfig(keyToInsert, invalidationDateToInsert, pair.Key))
                                {
                                    log.ErrorFormat("Failed inserting key {0} to {1}", keyToInsert, pair.Key.Type.ToString());
                                }
                            }
                        }
                    }
                }
                else
                {
                    res = true;
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetMaxInValidationKeysDate with keys {0}", string.Join(",", keys)), ex);
            }

            return res;
        }

        private bool TryGetInValidationKeysMaxDateMapping(string layeredCacheConfigName, int groupId, Dictionary<string, List<string>> keyMappings, ref Dictionary<string, KeyValuePair<bool, long>> inValidationKeysMaxDateMapping)
        {
            bool res = true;
            try
            {
                if (keyMappings == null || keyMappings.Count == 0)
                {
                    return res;
                }

                inValidationKeysMaxDateMapping = new Dictionary<string, KeyValuePair<bool, long>>();
                foreach (KeyValuePair<string, List<string>> pair in keyMappings)
                {
                    long maxInvalidationKeyDate = 0;
                    if (TryGetMaxInValidationKeysDate(layeredCacheConfigName, groupId, pair.Value, out maxInvalidationKeyDate))
                    {
                        inValidationKeysMaxDateMapping.Add(pair.Key, new KeyValuePair<bool, long>(true, maxInvalidationKeyDate));
                    }
                    else
                    {
                        res = false;
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetInValidationKeysMaxDateMapping with keys {0}", keyMappings != null ? string.Join(",", keyMappings.Values) : ""), ex);
            }

            return res;
        }

        private bool TryGetLayeredCacheConfig(string configurationName, out List<LayeredCacheConfig> layeredCacheConfig)
        {
            layeredCacheConfig = null;
            try
            {
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
                log.Error(string.Format("Failed TryGetLayeredCacheConfig for configurationName: {0}", configurationName), ex);
            }

            return layeredCacheConfig != null;
        }

        private bool TryGetLayeredCacheGroupConfig(int groupId, out LayeredCacheGroupConfig groupConfig, bool shouldCreateIfNoneExists = true)
        {
            bool res = false;
            groupConfig = null;
            string key = GetLayeredCacheGroupConfigKey(groupId);
            try
            {
                List<LayeredCacheConfig> GroupCacheSettings = layeredCacheTcmConfig.GroupCacheSettings;
                List<LayeredCacheConfig> insertToCacheConfig = new List<LayeredCacheConfig>();
                if (GroupCacheSettings == null)
                {
                    return res;
                }

                if (TryGetKeyFromSession<LayeredCacheGroupConfig>(key, ref groupConfig))
                {
                    return true;
                }

                foreach (LayeredCacheConfig cacheConfig in GroupCacheSettings)
                {
                    ICachingService cache = cacheConfig.GetICachingService();
                    if (cache != null)
                    {
                        if (cache.Get<LayeredCacheGroupConfig>(key, ref groupConfig) && groupConfig != null)
                        {
                            res = true;
                            Dictionary<string, LayeredCacheGroupConfig> groupConfigToAdd = new Dictionary<string, LayeredCacheGroupConfig>();
                            groupConfigToAdd.Add(key, groupConfig);
                            InsertResultsToSession(groupConfigToAdd);
                            break;
                        }

                        // if result=true we won't get here (break) and if it isn't we need to insert into this cache later
                        insertToCacheConfig.Add(cacheConfig);
                    }
                }

                if (res && groupConfig != null && insertToCacheConfig != null && insertToCacheConfig.Count > 0)
                {
                    if (!TrySetLayeredGroupCacheConfig(key, groupConfig, insertToCacheConfig))
                    {
                        log.ErrorFormat("Failed inserting LayeredCacheGroupConfig into cache, key: {0}, groupId: {1}, LayeredCacheTypes: {2}", key, groupId, GetLayeredCacheConfigTypesForLog(insertToCacheConfig));
                    }
                }

                if (shouldCreateIfNoneExists && !res && groupConfig == null)
                {
                    groupConfig = new LayeredCacheGroupConfig()
                    {
                        GroupId = groupId,
                        Version = 0,
                        DisableLayeredCache = false,
                        LayeredCacheSettingsToExclude = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase),
                        LayeredCacheInvalidationKeySettingsToExclude = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
                    };

                    res = TrySetLayeredGroupCacheConfig(key, groupConfig, insertToCacheConfig);
                    if (!res)
                    {
                        log.ErrorFormat("Failed inserting Default LayeredCacheGroupConfig into cache, key: {0}, groupId: {1}, LayeredCacheTypes: {2}", key, groupId, GetLayeredCacheConfigTypesForLog(insertToCacheConfig));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetLayeredCacheGroupConfig for groupId: {0}", groupId), ex);
            }

            return res;
        }

        private bool TryGetInvalidationKeyLayeredCacheConfig(string configurationName, out List<LayeredCacheConfig> layeredCacheConfig)
        {
            layeredCacheConfig = null;
            try
            {
                if (layeredCacheTcmConfig != null)
                {
                    if (!string.IsNullOrEmpty(configurationName) && layeredCacheTcmConfig.LayeredCacheInvalidationKeySettings != null
                        && layeredCacheTcmConfig.LayeredCacheInvalidationKeySettings.ContainsKey(configurationName))
                    {
                        layeredCacheConfig = layeredCacheTcmConfig.LayeredCacheInvalidationKeySettings[configurationName];
                    }
                    else if (layeredCacheTcmConfig.InvalidationKeySettings != null && layeredCacheTcmConfig.InvalidationKeySettings.Count > 0)
                    {
                        layeredCacheConfig = layeredCacheTcmConfig.InvalidationKeySettings;
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetInvalidationKeyLayeredCacheConfig for configurationName: {0}", configurationName), ex);
            }

            return layeredCacheConfig != null;
        }

        private Dictionary<string, string> GetVersionKeyToOriginalKeyMap(List<string> keys, int groupId, string versionToAdd = null)
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
                    versionValue = layeredCacheTcmConfig != null ? layeredCacheTcmConfig.Version : string.Empty;
                }

                List<string> distinctKeys = keys.Distinct().ToList();
                LayeredCacheGroupConfig groupConfig;
                if (TryGetLayeredCacheGroupConfig(groupId, out groupConfig) && groupConfig != null)
                {
                    if (!string.IsNullOrEmpty(versionValue))
                    {
                        res = distinctKeys.ToDictionary(x => string.Format("{0}_GV{1}_V{2}", x, groupConfig.Version, versionValue), x => x);
                    }
                    else
                    {
                        res = distinctKeys.ToDictionary(x => string.Format("{0}_GV{1}", x, groupConfig.Version), x => x);
                    }
                }
                else
                {
                    res = distinctKeys.ToDictionary(x => x, x => x);
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetVersionKeyToOriginalKeyMap for the following keys: {0}", string.Join(",", keys)), ex);
            }

            return res;
        }

        private Dictionary<string, string> GetOriginalKeyToVersionKeyMap(List<string> keys, int groupId, string versionToAdd = null)
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
                    versionValue = layeredCacheTcmConfig != null ? layeredCacheTcmConfig.Version : string.Empty;
                }

                List<string> distinctKeys = keys.Distinct().ToList();
                LayeredCacheGroupConfig groupConfig;
                if (TryGetLayeredCacheGroupConfig(groupId, out groupConfig) && groupConfig != null)
                {
                    if (!string.IsNullOrEmpty(versionValue))
                    {
                        res = distinctKeys.ToDictionary(x => x, x => string.Format("{0}_GV{1}_V{2}", x, groupConfig.Version, versionValue));
                    }
                    else
                    {
                        res = distinctKeys.ToDictionary(x => x, x => string.Format("{0}_GV{1}", x, groupConfig.Version));
                    }
                }
                else
                {
                    res = distinctKeys.ToDictionary(x => x, x => x);
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetOriginalKeyToVersionKeyMap for the following keys: {0}", string.Join(",", keys)), ex);
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

        private string GetLayeredCacheGroupConfigKey(int groupId)
        {
            return string.Format("layeredCacheGroupConfig_V1_{0}", groupId);
        }

        private bool TryGetKeysFromSession<T>(List<string> keys, ref Dictionary<string, T> sessionResultMapping)
        {
            bool res = false;
            try
            {
                sessionResultMapping = new Dictionary<string, T>();
                foreach (string key in keys)
                {
                    if (HttpContext.Current != null && HttpContext.Current.Items != null && HttpContext.Current.Items[key] != null)
                    {
                        T genericParameter = (T)HttpContext.Current.Items[key];
                        if (genericParameter != null)
                        {
                            sessionResultMapping.Add(key, genericParameter);
                        }
                    }
                }

                res = sessionResultMapping.Count > 0;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to get keys {0} from TryGetKeysFromSession", string.Join(",", keys)), ex);
            }

            return res;
        }

        private bool TryGetKeyFromAppDomainCache<T>(string key, ref T genericParameter)
        {
            bool res = false;
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Cache != null)
                {
                    object cachedObj = HttpContext.Current.Cache.Get(key);
                    if (cachedObj != null)
                    {
                        genericParameter = (T)cachedObj;
                        res = genericParameter != null && true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to get key {0} from TryGetFromSession", key), ex);
            }

            return res;
        }

        private void InsertResultsToAppDomainCache<T>(Dictionary<string, T> results, double ttlOnAppDomainInSeconds)
        {
            if (HttpContext.Current != null && HttpContext.Current.Cache != null)
            {
                foreach (KeyValuePair<string, T> pair in results)
                {
                    HttpContext.Current.Cache.Insert(pair.Key, pair.Value, null, DateTime.UtcNow.AddSeconds(ttlOnAppDomainInSeconds), System.Web.Caching.Cache.NoSlidingExpiration);
                }
            }
        }

        #endregion

        #region Insert

        private bool TryInsert<T>(string key, Tuple<T, long> tuple, LayeredCacheConfig cacheConfig)
        {
            bool res = false;
            try
            {
                ICachingService cache = cacheConfig.GetICachingService();
                if (cache != null)
                {
                    ulong version = 0;
                    if (cacheConfig.Type == LayeredCacheType.CbCache || cacheConfig.Type == LayeredCacheType.CbMemCache)
                    {
                        Tuple<T, long> getResult = default(Tuple<T, long>);
                        cache.GetWithVersion<Tuple<T, long>>(key, out version, ref getResult);
                    }

                    res = cache.SetWithVersion<Tuple<T, long>>(key, tuple, version, cacheConfig.TTL);
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryInsert with key {0}, LayeredCacheTypes {1}", key,
                                        GetLayeredCacheConfigTypesForLog(new List<LayeredCacheConfig>() { cacheConfig }), ex));
            }

            return res;
        }

        #endregion

        #region Set

        private bool TrySetInValidationKey(string key, long valueToUpdate)
        {
            bool res = false;
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return res;
                }

                LayeredCacheConfig invalidationKeyCacheConfig = layeredCacheTcmConfig.InvalidationKeySettings.Where(x => x.Type == LayeredCacheType.CbCache
                                                                                                    || x.Type == LayeredCacheType.CbMemCache).DefaultIfEmpty(null).First();
                if (invalidationKeyCacheConfig == null)
                {
                    return res;
                }

                res = TrySetInvalidationKeyWithCacheConfig(key, valueToUpdate, invalidationKeyCacheConfig);
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TrySetInValidationKey with key {0}", key), ex);
            }

            return res;
        }

        private static bool TrySetInvalidationKeyWithCacheConfig(string key, long valueToUpdate, LayeredCacheConfig invalidationKeyCacheConfig)
        {
            bool res = false;
            try
            {
                ICachingService cache = invalidationKeyCacheConfig.GetICachingService();
                if (cache != null)
                {
                    ulong version = 0;
                    long getResult = 0;
                    cache.GetWithVersion<long>(key, out version, ref getResult);
                    res = cache.SetWithVersion<long>(key, valueToUpdate, version, invalidationKeyCacheConfig.TTL);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TrySetInvalidationKeyWithCacheConfig with key {0}", key), ex);
            }

            return res;
        }

        private bool TrySetLayeredGroupCacheConfig(string key, LayeredCacheGroupConfig groupConfig, List<LayeredCacheConfig> layeredCacheConfig = null)
        {
            bool res = false;

            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return res;
                }

                List<LayeredCacheConfig> GroupCacheSettings = layeredCacheConfig == null || layeredCacheConfig.Count == 0 ? layeredCacheTcmConfig.GroupCacheSettings : layeredCacheConfig;
                if (GroupCacheSettings == null)
                {
                    return res;
                }

                bool insertResult = true;
                foreach (LayeredCacheConfig cacheConfig in GroupCacheSettings)
                {
                    ICachingService cache = cacheConfig.GetICachingService();
                    if (cache != null)
                    {
                        ulong version = 0;
                        if (cacheConfig.Type == LayeredCacheType.CbCache || cacheConfig.Type == LayeredCacheType.CbMemCache)
                        {

                            LayeredCacheGroupConfig getResult = null;
                            cache.GetWithVersion<LayeredCacheGroupConfig>(key, out version, ref getResult);

                        }

                        insertResult = insertResult && cache.SetWithVersion<LayeredCacheGroupConfig>(key, groupConfig, version, cacheConfig.TTL);
                    }
                }

                res = insertResult;
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TrySetLayeredGroupCacheConfig with key {0}", key), ex);
            }

            return res;
        }

        #endregion

        #region Remove

        private bool TryRemove(string key, LayeredCacheConfig cacheConfig, int groupId, string version = null)
        {
            bool result = false;
            try
            {
                bool res = false;
                ICachingService cache = cacheConfig.GetICachingService();
                if (cache != null)
                {
                    Dictionary<string, string> keyMappings = GetVersionKeyToOriginalKeyMap(new List<string>() { key }, groupId);
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

        #endregion

        #region Other

        private bool ShouldGoToCache(string layeredCacheConfigName, int groupId, ref List<LayeredCacheConfig> layeredCacheConfig)
        {
            bool res = false;
            try
            {
                LayeredCacheGroupConfig layeredCacheGroupConfig;
                res = TryGetLayeredCacheConfig(layeredCacheConfigName, out layeredCacheConfig) && TryGetLayeredCacheGroupConfig(groupId, out layeredCacheGroupConfig)
                        && !layeredCacheGroupConfig.DisableLayeredCache && !layeredCacheGroupConfig.LayeredCacheSettingsToExclude.Contains(layeredCacheConfigName);
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed ShouldGoToCache with layeredCacheConfigName {0}, groupId {1}", layeredCacheConfigName, groupId), ex);
            }

            return res;
        }

        private bool ShouldCheckInvalidationKey(string layeredCacheConfigName, int groupId, ref List<LayeredCacheConfig> layeredCacheConfig)
        {
            bool res = false;
            try
            {
                LayeredCacheGroupConfig layeredCacheGroupConfig;
                res = TryGetInvalidationKeyLayeredCacheConfig(layeredCacheConfigName, out layeredCacheConfig) && TryGetLayeredCacheGroupConfig(groupId, out layeredCacheGroupConfig)
                        && !layeredCacheGroupConfig.DisableLayeredCache && !layeredCacheGroupConfig.LayeredCacheInvalidationKeySettingsToExclude.Contains(layeredCacheConfigName);
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed ShouldCheckInvalidationKey with layeredCacheConfigName {0}, groupId {1}", layeredCacheConfigName, groupId), ex);
            }

            return res;
        }

        #endregion

        #endregion

    }
}
