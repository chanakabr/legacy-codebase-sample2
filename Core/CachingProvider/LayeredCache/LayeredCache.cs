using CouchbaseManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phx.Lib.Log;
using System.Reflection;
using Newtonsoft.Json;
using System.Web;
using Phx.Lib.Appconfig;
using System.Runtime.Caching;
using CachingProvider.LayeredCache.Helper;
using EventBus.Kafka;
using EventBus.Abstraction;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CachingProvider.LayeredCache
{
    public interface ILayeredCache
    {
        bool Get<T>(string key, ref T genericParameter, Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectMethod, Dictionary<string, object> funcParameters,
                            int groupId, string layeredCacheConfigName, List<string> inValidationKeys = null, bool shouldUseAutoNameTypeHandling = false);

        bool SetInvalidationKey(string key, DateTime? updatedAt = null);

        long GetInvalidationKeyValue(int groupId, string layeredCacheConfigName, string invalidationKey);
        
        bool GetValues<T>(Dictionary<string, string> keyToOriginalValueMap, ref Dictionary<string, T> results, Func<Dictionary<string, object>, Tuple<Dictionary<string, T>, bool>> fillObjectsMethod,
            Dictionary<string, object> funcParameters, int groupId, string layeredCacheConfigName, Dictionary<string, List<string>> inValidationKeysMap = null,
            bool shouldUseAutoNameTypeHandling = false);
    }

    public class LayeredCache : ILayeredCache
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();
        private static LayeredCache instance = null;
        private static JsonSerializerSettings layeredCacheConfigSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
        private static LayeredCacheTcmConfig layeredCacheTcmConfig = null;
        private static JsonSerializerSettings jsonSerializerSettings = null;
        public const string INVALIDATION_KEYS_HEADER = "X-Kaltura-InvalidationKeys";
        public const string MISSING_KEYS = "NeededKeys";
        public const string IS_READ_ACTION = "IsReadAction";
        public const string CURRENT_REQUEST_LAYERED_CACHE = "CurrentRequestLayeredCache";
        public const string DATABASE_ERROR_DURING_SESSION = "DATABASE_ERROR_DURING_SESSION";
        public const string CONTEXT_KEY_SHOULD_ROUTE_DB_TO_SECONDARY = "ShouldRouteDbToSecondary";
        private const string REQUEST_TAGS = "request_tags";
        private const string REQUEST_TAGS_PARTNER_ROLE = "partner_role";

        public static readonly HashSet<string> readActions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "get", "list", "getContext", "playManifest"
        };

        private readonly bool ShouldProduceInvalidationEventsToKafka;
        private readonly string InvalidationEventsTopic = ApplicationConfiguration.Current.MicroservicesClientConfiguration.LayeredCacheConfiguration.InvalidationEventsTopic.Value;
        private List<Regex> _InvalidationEventsRegexRules;

        public LayeredCache()
        {
            layeredCacheTcmConfig = GetLayeredCacheTcmConfig();
            jsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
            ShouldProduceInvalidationEventsToKafka = GetShouldProduceInvalidationEventsToKafkaValue();
            if (ShouldProduceInvalidationEventsToKafka)
            {
                LoadInvalidationEventsRules();
            }
        }

        private bool GetShouldProduceInvalidationEventsToKafkaValue()
        {
            bool shouldProduceInvalidationEventsToKafka = ApplicationConfiguration.Current.MicroservicesClientConfiguration.LayeredCacheConfiguration.ShouldProduceInvalidationEventsToKafka.Value;            
            string envVariableShouldProduceInvalidationEventsToKafka = Environment.GetEnvironmentVariable("SHOULD_PRODUCE_INVALIDATION_EVENTS_TO_KAFKA");
            bool shouldProduceInvalidationEventsToKafkaOverride;
            if (!string.IsNullOrEmpty(envVariableShouldProduceInvalidationEventsToKafka) && bool.TryParse(envVariableShouldProduceInvalidationEventsToKafka, out shouldProduceInvalidationEventsToKafkaOverride))
            {
                shouldProduceInvalidationEventsToKafka = shouldProduceInvalidationEventsToKafkaOverride;
            }

            return shouldProduceInvalidationEventsToKafka;
        }

        private void LoadInvalidationEventsRules()
        {
            _InvalidationEventsRegexRules = new List<Regex>();
            foreach (var regexRule in ApplicationConfiguration.Current.MicroservicesClientConfiguration.LayeredCacheConfiguration.InvalidationEventsMatchRules.Value)
            {
                try
                {
                    var regex = new Regex(regexRule, RegexOptions.Compiled);
                    _InvalidationEventsRegexRules.Add(regex);
                }
                catch (Exception e)
                {
                    log.Error($"error parsing invalidation event rule:[{regexRule}], skipping the rule.", e);
                }
            }
        }

        public static LayeredCache Instance
        {
            get
            {
                if (instance == null || layeredCacheTcmConfig == null)
                {
                    lock (locker)
                    {
                        if (instance == null || layeredCacheTcmConfig == null || jsonSerializerSettings == null)
                        {
                            instance = new LayeredCache();
                        }
                    }
                }

                return instance;
            }
        }

        #region Public Methods

        public bool Get<T>(string key, ref T genericParameter, Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectMethod, Dictionary<string, object> funcParameters,
                            int groupId, string layeredCacheConfigName, List<string> inValidationKeys = null, bool shouldUseAutoNameTypeHandling = false)
        {
            bool result = false;
            List<LayeredCacheConfig> insertToCacheConfig = null;
            try
            {
                if (TryGetKeyFromCurrentRequest(key, ref genericParameter))
                {
                    return true;
                }

                Tuple<T, long> tuple = null;

                // save data in cache only if result is true!!!!
                result = TryGetFromCacheByConfig<T>(key, ref tuple, layeredCacheConfigName, out insertToCacheConfig, fillObjectMethod, funcParameters, groupId, inValidationKeys, shouldUseAutoNameTypeHandling);
                genericParameter = tuple != null && tuple.Item1 != null ? tuple.Item1 : genericParameter;

                // if we successfully got data from cache / delegate, insert results to current request items
                if (result)
                {
                    Dictionary<string, T> resultsToAdd = new Dictionary<string, T>();
                    resultsToAdd.Add(key, genericParameter);
                    Dictionary<string, List<string>> invalidationKeyToAdd = null;

                    if (inValidationKeys != null && inValidationKeys.Count > 0)
                    {
                        invalidationKeyToAdd = new Dictionary<string, List<string>>();

                        foreach (var invalidationKey in inValidationKeys)
                        {
                            invalidationKeyToAdd.Add(invalidationKey, new List<string>() { key });
                        }
                    }

                    InsertResultsToCurrentRequest(resultsToAdd, invalidationKeyToAdd);
                }

                if (insertToCacheConfig != null && insertToCacheConfig.Count > 0 && result && tuple != null && tuple.Item1 != null &&
                    // insert to cache only if no errors during session
                    !(HttpContext.Current != null && HttpContext.Current.Items.ContainsKey(DATABASE_ERROR_DURING_SESSION) &&
                    HttpContext.Current.Items[DATABASE_ERROR_DURING_SESSION] is bool &&
                    (bool)HttpContext.Current.Items[DATABASE_ERROR_DURING_SESSION]))
                {
                    // set validation to now
                    Tuple<T, long> tupleToInsert = new Tuple<T, long>(tuple.Item1, Utils.GetUtcUnixTimestampNow());
                    Dictionary<string, string> keyMappings = GetVersionKeyToOriginalKeyMap(new List<string>() { key }, groupId);
                    if (keyMappings != null && keyMappings.Count > 0)
                    {
                        foreach (LayeredCacheConfig cacheConfig in insertToCacheConfig)
                        {
                            if (!TryInsert<T>(keyMappings.Keys.First(), tupleToInsert, cacheConfig, shouldUseAutoNameTypeHandling))
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


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyToOriginalValueMap"></param>
        /// <param name="results"></param>
        /// <param name="fillObjectsMethod"></param>
        /// <param name="funcParameters"></param>
        /// <param name="groupId"></param>
        /// <param name="layeredCacheConfigName"></param>
        /// <param name="inValidationKeysMap"></param>
        /// <param name="shouldUseAutoNameTypeHandling"></param>
        /// <returns></returns>
        public bool GetValues<T>(Dictionary<string, string> keyToOriginalValueMap, ref Dictionary<string, T> results, Func<Dictionary<string, object>, Tuple<Dictionary<string, T>, bool>> fillObjectsMethod,
                                    Dictionary<string, object> funcParameters, int groupId, string layeredCacheConfigName, Dictionary<string, List<string>> inValidationKeysMap = null,
                                    bool shouldUseAutoNameTypeHandling = false)
        {
            bool res = false;
            Dictionary<LayeredCacheConfig, List<string>> insertToCacheConfigMappings = null;
            Dictionary<string, Tuple<T, long>> resultsMapping = null;
            Dictionary<string, T> currentRequestResultMapping = null;
            bool shouldAddCurrentRequestResults = false;
            try
            {
                Dictionary<string, string> keyToOriginalValueMapAfterCurrentRequest = new Dictionary<string, string>(keyToOriginalValueMap);

                if (TryGetKeysFromCurrentRequest<T>(keyToOriginalValueMap.Keys.ToList(), ref currentRequestResultMapping))
                {
                    shouldAddCurrentRequestResults = true;

                    foreach (string key in currentRequestResultMapping.Keys)
                    {
                        keyToOriginalValueMapAfterCurrentRequest.Remove(key);
                    }
                }

                if (keyToOriginalValueMapAfterCurrentRequest.Count > 0)
                {
                    res = TryGetValuesFromCacheByConfig<T>(keyToOriginalValueMapAfterCurrentRequest, ref resultsMapping, layeredCacheConfigName, out insertToCacheConfigMappings, fillObjectsMethod,
                                                            funcParameters, groupId, inValidationKeysMap, shouldUseAutoNameTypeHandling);
                    results = resultsMapping != null && resultsMapping.Count > 0 ? resultsMapping.ToDictionary(x => x.Key, x => x.Value.Item1) : null;

                    // if we successfully got data from cache / delegate, insert results to current request items
                    if (res)
                    {
                        Dictionary<string, List<string>> invalidationKeysToKeys = new Dictionary<string, List<string>>();

                        if (inValidationKeysMap != null)
                        {
                            foreach (var objectKey in inValidationKeysMap.Keys)
                            {
                                var invalidationKeysOfSpecificKey = inValidationKeysMap[objectKey];

                                foreach (var invalidationKey in invalidationKeysOfSpecificKey)
                                {
                                    if (!invalidationKeysToKeys.ContainsKey(invalidationKey))
                                    {
                                        invalidationKeysToKeys[invalidationKey] = new List<string>();
                                    }

                                    invalidationKeysToKeys[invalidationKey].Add(objectKey);
                                }
                            }
                        }

                        InsertResultsToCurrentRequest(results, invalidationKeysToKeys);
                    }

                    if (insertToCacheConfigMappings != null && insertToCacheConfigMappings.Count > 0 && res && results != null &&
                        // insert to cache only if no errors during session
                        !(HttpContext.Current != null && HttpContext.Current.Items.ContainsKey(DATABASE_ERROR_DURING_SESSION) &&
                        HttpContext.Current.Items[DATABASE_ERROR_DURING_SESSION] is bool &&
                        (bool)HttpContext.Current.Items[DATABASE_ERROR_DURING_SESSION]))
                    {
                        Dictionary<string, string> keyToVersionMappings = GetOriginalKeyToVersionKeyMap(keyToOriginalValueMapAfterCurrentRequest.Keys.ToList(), groupId);

                        if (keyToVersionMappings != null && keyToVersionMappings.Count > 0)
                        {
                            foreach (KeyValuePair<LayeredCacheConfig, List<string>> pair in insertToCacheConfigMappings)
                            {
                                foreach (string key in pair.Value)
                                {
                                    if (results.ContainsKey(key))
                                    {
                                        if (results[key] != null)
                                        {
                                            // set validation to now
                                            Tuple<T, long> tupleToInsert = new Tuple<T, long>(results[key], Utils.GetUtcUnixTimestampNow());
                                            if (!TryInsert<T>(keyToVersionMappings[key], tupleToInsert, pair.Key, shouldUseAutoNameTypeHandling))
                                            {
                                                log.ErrorFormat("GetValues<T> - Failed inserting key {0} to {1}", keyToVersionMappings[key], pair.Key.Type.ToString());
                                            }
                                        }
                                        else
                                        {
                                            log.LogTrace($"GetValues<T> - key: {key} in results is null");
                                        }
                                    }
                                    else
                                    {
                                        log.LogTrace($"GetValues<T> - key: {key} isn't contained in results");
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

                if (shouldAddCurrentRequestResults)
                {
                    if (results == null)
                    {
                        results = new Dictionary<string, T>();
                    }

                    foreach (KeyValuePair<string, T> pair in currentRequestResultMapping)
                    {
                        results.Add(pair.Key, pair.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                var _cacheConfigName = string.IsNullOrEmpty(layeredCacheConfigName) ? string.Empty : layeredCacheConfigName;
                var _methodName = fillObjectsMethod.Method != null ? fillObjectsMethod.Method.Name : "No_Method_Name";
                var _funcParameters = funcParameters != null && funcParameters.Count > 0 ? string.Join(", ", funcParameters.Keys.ToList()) : "No_Func_Parameters";
                var message = $"Failed GetValues with keys {string.Join(",", keyToOriginalValueMap.Keys).Take(20)} " +
                    $"from LayeredCache, layeredCacheConfigName {_cacheConfigName}, " +
                    $"MethodName {_methodName} " +
                    $"and funcParameters {_funcParameters}";
                
                //BEO-10787
                if (ex is InvalidOperationException)
                {
                    log.Debug(message, ex);
                }
                else
                {
                    log.Error(message, ex);
                }
            }

            return res;
        }

        public long GetInvalidationKeyValue(int groupId, string layeredCacheConfigName, string invalidationKey)
        {
            long result = -1;
            try
            {
                long m;
                TryGetMaxInValidationKeysDate(layeredCacheConfigName, groupId, new List<string>() { invalidationKey }, out result, out m);
            }
            catch (Exception ex)
            {
                log.Error($"failed getting invalidation key value for groupId {groupId}, layeredCacheConfigName {layeredCacheConfigName} and invalidationKey {invalidationKey}", ex);
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
                    HttpContext.Current.Response.Headers.Remove("no-cache");
                    HttpContext.Current.Response.Headers.Add("no-cache", "true");
                }

                long valueToUpdate = Utils.GetUtcUnixTimestampNow();
                if (updatedAt.HasValue)
                {
                    valueToUpdate = Utils.DateTimeToUtcUnixTimestampSeconds(updatedAt.Value);
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
                    HttpContext.Current.Response.Headers.Remove("no-cache");
                    HttpContext.Current.Response.Headers.Add("no-cache", "true");
                }

                long valueToUpdate = Utils.GetUtcUnixTimestampNow();

                if (updatedAt.HasValue)
                {
                    valueToUpdate = Utils.DateTimeToUtcUnixTimestampSeconds(updatedAt.Value);
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

        public void DisableInMemoryCache()
        {
            if (layeredCacheTcmConfig.DefaultSettings.Any(x => x.Type == LayeredCacheType.InMemoryCache))
            {
                layeredCacheTcmConfig.DefaultSettings.RemoveAll(x => x.Type == LayeredCacheType.InMemoryCache);
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

        public bool IsReadAction()
        {
            return HttpContext.Current != null && HttpContext.Current.Items != null &&
                   HttpContext.Current.Items[LayeredCache.IS_READ_ACTION] != null ? (bool)HttpContext.Current.Items[LayeredCache.IS_READ_ACTION] : false;
        }

        public bool TryGetKeyFromCurrentRequest<T>(string key, ref T genericParameter)
        {
            List<string> keys = new List<string>() { key };
            Dictionary<string, T> currentRequestResultMapping = new Dictionary<string, T>();

            bool success = TryGetKeysFromCurrentRequest(keys, ref currentRequestResultMapping);

            if (currentRequestResultMapping != null && currentRequestResultMapping.ContainsKey(key))
            {
                genericParameter = (T)currentRequestResultMapping[key];
            }

            return success;
        }

        private static bool DoesHttpContextContainsRequestLayeredCache()
        {
            return HttpContext.Current != null && HttpContext.Current.Items != null && HttpContext.Current.Items.ContainsKey(CURRENT_REQUEST_LAYERED_CACHE) &&
                    HttpContext.Current.Items[CURRENT_REQUEST_LAYERED_CACHE] != null && HttpContext.Current.Items[CURRENT_REQUEST_LAYERED_CACHE] is RequestLayeredCache;
        }

        public bool TryGetKeysFromCurrentRequest<T>(List<string> keys, ref Dictionary<string, T> currentRequestResultMapping)
        {
            bool success = false;

            try
            {
                currentRequestResultMapping = new Dictionary<string, T>();

                if (DoesHttpContextContainsRequestLayeredCache() && keys != null && keys.Count > 0)
                {
                    RequestLayeredCache requestLayeredCache = HttpContext.Current.Items[CURRENT_REQUEST_LAYERED_CACHE] as RequestLayeredCache;

                    foreach (string key in keys)
                    {
                        if (requestLayeredCache.cachedObjects.ContainsKey(key))
                        {
                            T genericParameter = (T)requestLayeredCache.cachedObjects[key];

                            if (genericParameter != null)
                            {
                                currentRequestResultMapping.Add(key, genericParameter);
                            }
                        }
                    }

                    success = currentRequestResultMapping.Count > 0;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed TryGetKeysFromCurrentRequest for keys = {0}; error = {1}", keys != null ? string.Join(",", keys) : "null", ex);
            }

            return success;
        }

        public static List<string> GetInvalidationKeyFromRequest()
        {
            RequestLayeredCache requestLayeredCache = null;
            if (DoesHttpContextContainsRequestLayeredCache())
            {
                requestLayeredCache = HttpContext.Current.Items[CURRENT_REQUEST_LAYERED_CACHE] as RequestLayeredCache;
            }

            return requestLayeredCache?.invalidationKeysToKeys?.Keys.ToList();
        }

        public void InsertResultsToCurrentRequest<T>(Dictionary<string, T> results, Dictionary<string, List<string>> invalidationKeysToKeys)
        {
            var requestLayeredCache = DoesHttpContextContainsRequestLayeredCache()
                ? HttpContext.Current.Items[CURRENT_REQUEST_LAYERED_CACHE] as RequestLayeredCache
                : new RequestLayeredCache();

            if (results != null)
            {
                // save cached objects
                foreach (var result in results)
                {
                    requestLayeredCache.cachedObjects[result.Key] = result.Value;
                }
            }

            if (invalidationKeysToKeys != null)
            {
                // save invalidation keys (merge two dictionaries)
                foreach (var invalidationKey in invalidationKeysToKeys)
                {
                    if (invalidationKey.Value != null && invalidationKey.Value.Count > 0)
                    {
                        var hashset = requestLayeredCache.invalidationKeysToKeys.GetOrAdd(invalidationKey.Key,
                            _ => new ConcurrentDictionary<string, byte>());
                        foreach (var key in invalidationKey.Value)
                        {
                            hashset.TryAdd(key, 0);
                        }
                    }
                }
            }

            if (HttpContext.Current?.Items != null)
            {
                HttpContext.Current.Items[CURRENT_REQUEST_LAYERED_CACHE] = requestLayeredCache;
            }
        }

        private void RemoveCachedObjectsAndInvalidationKeysFromCurrentRequest(string invalidationKey)
        {
            RequestLayeredCache requestLayeredCache = null;

            if (DoesHttpContextContainsRequestLayeredCache())
            {
                requestLayeredCache = HttpContext.Current.Items[CURRENT_REQUEST_LAYERED_CACHE] as RequestLayeredCache;

                ConcurrentDictionary<string, byte> keysToRemove;

                if (requestLayeredCache.invalidationKeysToKeys.TryGetValue(invalidationKey, out keysToRemove))
                {
                    foreach (var keyToRemove in keysToRemove)
                    {
                        requestLayeredCache.cachedObjects.TryRemove(keyToRemove.Key, out _);
                    }
                }

                requestLayeredCache.invalidationKeysValues.TryRemove(invalidationKey, out _);
            }
        }

        public bool TryGetInvalidationKeysFromCurrentRequest(HashSet<string> invalidationKeys, ref Dictionary<string, long> currentRequestResultMapping)
        {
            bool success = false;

            try
            {
                currentRequestResultMapping = new Dictionary<string, long>();

                if (DoesHttpContextContainsRequestLayeredCache() && invalidationKeys != null && invalidationKeys.Count > 0)
                {
                    RequestLayeredCache requestLayeredCache = HttpContext.Current.Items[CURRENT_REQUEST_LAYERED_CACHE] as RequestLayeredCache;

                    foreach (string invalidationKey in invalidationKeys)
                    {
                        if (requestLayeredCache.invalidationKeysValues.ContainsKey(invalidationKey))
                        {
                            currentRequestResultMapping[invalidationKey] = requestLayeredCache.invalidationKeysValues[invalidationKey];
                        }
                    }

                    success = currentRequestResultMapping.Count > 0;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed TryGetInvalidationKeysFromCurrentRequest for invalidationKeys = {0}; error = {1}", invalidationKeys != null ? string.Join(",", invalidationKeys) : "null", ex);
            }

            return success;
        }

        public void InsertInvalidationKeysToCurrentRequest(Dictionary<string, long> invalidationKeys)
        {
            RequestLayeredCache requestLayeredCache = null;

            if (DoesHttpContextContainsRequestLayeredCache())
            {
                requestLayeredCache = HttpContext.Current.Items[CURRENT_REQUEST_LAYERED_CACHE] as RequestLayeredCache;
            }
            else
            {
                requestLayeredCache = new RequestLayeredCache();
            }

            if (invalidationKeys != null)
            {
                // save invalidationKeys
                foreach (var result in invalidationKeys)
                {
                    requestLayeredCache.invalidationKeysValues[result.Key] = result.Value;
                }
            }

            if (HttpContext.Current?.Items != null)
            {
                HttpContext.Current.Items[CURRENT_REQUEST_LAYERED_CACHE] = requestLayeredCache;
            }
        }

        public bool ShouldGoToCache(string layeredCacheConfigName, int groupId)
        {
            if (layeredCacheConfigName == LayeredCacheConfigNames.UNIFIED_SEARCH_WITH_PERSONAL_DATA && isPartnerRequest())
            {
                log.Debug($"BEO-10994 skipCache for partner name={layeredCacheConfigName}");
                return false;
            }

            List<LayeredCacheConfig> layeredCacheConfig = null;

            return ShouldGoToCache(layeredCacheConfigName, groupId, ref layeredCacheConfig);
        }

        public bool IncrementLayeredCacheGroupConfigVersion(int groupId)
        {
            bool result = false;
            try
            {
                string key = GetLayeredCacheGroupConfigKey(groupId);
                LayeredCacheGroupConfig groupConfig;
                if (TryGetLayeredCacheGroupConfig(groupId, out groupConfig, false))
                {
                    groupConfig.Version++;
                    result = TrySetLayeredGroupCacheConfig(key, groupConfig);
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed IncrementLayeredCacheGroupConfigVersion, groupId: {0}", groupId), ex);
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
                if (layeredCacheTcmConfig != null && layeredCacheTcmConfig.BucketSettings != null && layeredCacheTcmConfig.BucketSettings.Count > 0)
                {
                    LayeredCacheBucketSettings bucketSettings = layeredCacheTcmConfig.BucketSettings.FirstOrDefault(x => x.CacheType == cacheType);
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
                string layeredCacheConfigurationString = ApplicationConfiguration.Current.LayeredCacheConfigurationValidation.JsonConfig.Value.ToString();
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
                                                int groupId, List<string> inValidationKeys = null, bool shouldUseAutoNameTypeHandling = false)
        {
            bool result = false;
            List<LayeredCacheConfig> layeredCacheConfig = null;
            insertToCacheConfig = new List<LayeredCacheConfig>();
            try
            {
                long maxExternalInvalidationDate = 0;
                bool hasMaxInvalidationDate = false;

                bool shouldGoToCache = ShouldGoToCache(layeredCacheConfigName, groupId, ref layeredCacheConfig);

                if (shouldGoToCache)
                {
                    long maxInValidationDate = 0;

                    foreach (LayeredCacheConfig cacheConfig in layeredCacheConfig)
                    {
                        if (TryGetFromICachingService<T>(key, ref tupleResult, cacheConfig, groupId, shouldUseAutoNameTypeHandling))
                        {
                            if (!hasMaxInvalidationDate)
                            {
                                long currentMaxExternalInvalidationDate = 0;

                                hasMaxInvalidationDate = TryGetMaxInValidationKeysDate(layeredCacheConfigName, groupId, inValidationKeys, out maxInValidationDate, out currentMaxExternalInvalidationDate);

                                if (currentMaxExternalInvalidationDate > maxExternalInvalidationDate)
                                {
                                    maxExternalInvalidationDate = currentMaxExternalInvalidationDate;
                                }

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
                    bool initialStateWasRoutedToSecondary = checkShouldRouteDBToSecondary(key, layeredCacheConfigName, groupId, 
                        hasMaxInvalidationDate, maxExternalInvalidationDate,
                        inValidationKeys, null);

                    Tuple<T, bool> tuple = fillObjectMethod(funcParameters);

                    resetThreadSpecificRouting(key, initialStateWasRoutedToSecondary);

                    if (tuple != null)
                    {
                        tupleResult = new Tuple<T, long>(tuple.Item1, Utils.GetUtcUnixTimestampNow());
                        result = tuple.Item2;
                    }

                    if (!result)
                    {
                        log.DebugFormat("Failed fillingObjectFromDbMethod for key: {0}, with MethodName: {1}, and funcParameters: {2}.",
                                        key,
                                        fillObjectMethod.Method != null ? fillObjectMethod.Method.Name : "No_Method_Name",
                                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys) : "No_Func_Parameters");
                    }
                }
            }
            catch (Exception ex)
            {
                insertToCacheConfig = new List<LayeredCacheConfig>(0);
                log.Error(string.Format("Failed TryGetFromCacheByConfig with key {0}, LayeredCacheTypes {1}, MethodName {2} and funcParameters {3}", key, GetLayeredCacheConfigTypesForLog(layeredCacheConfig),
                        fillObjectMethod.Method != null ? fillObjectMethod.Method.Name : "No_Method_Name",
                        funcParameters != null && funcParameters.Count > 0 ? string.Join(",", funcParameters.Keys.ToList()) : "No_Func_Parameters"), ex);
            }

            return result;
        }

        private bool TryGetValuesFromCacheByConfig<T>(Dictionary<string, string> KeyToOriginalValueMap, ref Dictionary<string, Tuple<T, long>> tupleResults, string layeredCacheConfigName,
                                                        out Dictionary<LayeredCacheConfig, List<string>> insertToCacheConfig, Func<Dictionary<string, object>, Tuple<Dictionary<string, T>, bool>> fillObjectsMethod,
                                                        Dictionary<string, object> funcParameters, int groupId, Dictionary<string, List<string>> inValidationKeysMap = null,
                                                        bool shouldUseAutoNameTypeHandling = false)
        {
            bool result = false;
            List<LayeredCacheConfig> layeredCacheConfig = null;
            insertToCacheConfig = new Dictionary<LayeredCacheConfig, List<string>>();
            try
            {
                HashSet<string> keysToGet = new HashSet<string>(KeyToOriginalValueMap.Keys);
                Dictionary<string, Tuple<T, long>> resultsToAdd = new Dictionary<string, Tuple<T, long>>();
                tupleResults = new Dictionary<string, Tuple<T, long>>();
                long maxExternalInvalidationDate = 0;
                bool hasMaxInvalidationDates = false;

                bool shouldGoToCache = ShouldGoToCache(layeredCacheConfigName, groupId, ref layeredCacheConfig);

                if (shouldGoToCache)
                {
                    Dictionary<string, long> inValidationKeysMaxDateMapping = null;

                    foreach (LayeredCacheConfig cacheConfig in layeredCacheConfig)
                    {
                        if (TryGetValuesFromICachingService<T>(keysToGet.ToList(), ref resultsToAdd, cacheConfig, groupId, shouldUseAutoNameTypeHandling) && resultsToAdd != null && resultsToAdd.Count > 0)
                        {
                            if (!hasMaxInvalidationDates)
                            {
                                long currentMaxExternalInvalidationDate = 0;
                                hasMaxInvalidationDates = TryGetInValidationKeysMaxDateMapping(layeredCacheConfigName, groupId, inValidationKeysMap, ref inValidationKeysMaxDateMapping, out currentMaxExternalInvalidationDate);

                                if (currentMaxExternalInvalidationDate > maxExternalInvalidationDate)
                                {
                                    maxExternalInvalidationDate = currentMaxExternalInvalidationDate;
                                }

                                if (!hasMaxInvalidationDates)
                                {
                                    log.ErrorFormat("Error getting inValidationKeysMaxDateMapping for keys: {0}, layeredCacheConfigName: {1}, groupId: {2}",
                                        string.Join(",", keysToGet).Take(20),
                                        layeredCacheConfigName,
                                        groupId);
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
                                    long maxInValidationDate = inValidationKeysMaxDateMapping != null && inValidationKeysMaxDateMapping.Count > 0 && inValidationKeysMaxDateMapping.ContainsKey(keyToGet) ? inValidationKeysMaxDateMapping[keyToGet] + 1 : 0;
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
                    log.ErrorFormat("Didn't go to cache for key: {0}, layeredCacheConfigName: {1}, groupId: {2}",
                        string.Join(",", keysToGet).Take(20), layeredCacheConfigName, groupId);
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

                    bool initialState = checkShouldRouteDBToSecondary(keysToGet.ElementAt(0), layeredCacheConfigName, groupId, hasMaxInvalidationDates, maxExternalInvalidationDate, 
                        null, inValidationKeysMap);

                    Tuple<Dictionary<string, T>, bool> tuple = fillObjectsMethod(funcParameters);

                    resetThreadSpecificRouting(keysToGet.ElementAt(0), initialState);

                    if (tuple != null)
                    {
                        result = tuple.Item2;
                        if (tuple.Item1 != null)
                        {
                            foreach (KeyValuePair<string, T> pair in tuple.Item1.Where(x => keysToGet.Contains(x.Key)))
                            {
                                tupleResults.Add(pair.Key, new Tuple<T, long>(pair.Value, Utils.GetUtcUnixTimestampNow()));
                            }
                        }
                    }

                    if (!result)
                    {
                        log.DebugFormat("fillObjectsMethod returned false for keys: {0}, with MethodName: {1}",
                                        // take only 20 first keys - to avoid flood of log
                                        string.Join(",", KeyToOriginalValueMap.Keys.Take(20)),
                                        fillObjectsMethod.Method != null ? fillObjectsMethod.Method.Name : "No_Method_Name");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetValuesFromCacheByConfig with keys {0}, LayeredCacheTypes {1}, MethodName {2}",
                        // take only 20 first keys - to avoid flood of log
                        string.Join(",", KeyToOriginalValueMap.Keys.Take(20)),
                        GetLayeredCacheConfigTypesForLog(layeredCacheConfig),
                        fillObjectsMethod.Method != null ? fillObjectsMethod.Method.Name : "No_Method_Name"), ex);
            }

            return result;
        }

        private bool TryGetFromICachingService<T>(string key, ref Tuple<T, long> tupleResult, LayeredCacheConfig cacheConfig, int groupId, bool shouldUseAutoNameTypeHandling = false)
        {
            bool res = false;
            try
            {
                ILayeredCacheService cache = cacheConfig.GetILayeredCachingService();
                if (cache != null)
                {
                    Dictionary<string, string> keysMapping = GetVersionKeyToOriginalKeyMap(new List<string>() { key }, groupId);
                    if (keysMapping != null && keysMapping.Count > 0)
                    {
                        res = cache.Get(keysMapping.Keys.First(), ref tupleResult, shouldUseAutoNameTypeHandling ? jsonSerializerSettings : null) == GetOperationStatus.Success;
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetFromICachingService with key {0}, LayeredCacheTypes {1}", key, GetLayeredCacheConfigTypesForLog(new List<LayeredCacheConfig>() { cacheConfig }), ex));
            }

            return res;
        }

        private bool TryGetValuesFromICachingService<T>(List<string> keys, ref Dictionary<string, Tuple<T, long>> tupleResultsMap, LayeredCacheConfig cacheConfig,
                                                        int groupId, bool shouldUseAutoNameTypeHandling = false)
        {
            bool res = false;
            try
            {
                ILayeredCacheService cache = cacheConfig.GetILayeredCachingService();
                if (cache != null)
                {
                    Dictionary<string, string> keysMapping = GetVersionKeyToOriginalKeyMap(keys, groupId);
                    if (keysMapping != null && keysMapping.Count > 0)
                    {
                        IDictionary<string, Tuple<T, long>> getResults = null;
                        res = cache.GetValues<Tuple<T, long>>(keysMapping.Keys.ToList(), ref getResults, shouldUseAutoNameTypeHandling ? jsonSerializerSettings : null, true);
                        if (getResults != null)
                        {
                            tupleResultsMap = getResults.ToDictionary(x => keysMapping[x.Key], x => x.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetValuesFromICachingService with keys {0}, LayeredCacheTypes {1}",
                    // take only 20 first keys - to avoid flood of log
                    string.Join(",", keys).Take(20),
                    GetLayeredCacheConfigTypesForLog(new List<LayeredCacheConfig>() { cacheConfig }), ex));
            }

            return res;
        }

        private bool TryGetMaxInValidationKeysDate(string layeredCacheConfigName, int groupId, List<string> keys, out long MaxInValidationDate, out long maxExternalInvalidationDate,
            bool shouldGetExternalInvalidationKeyDate = false)
        {
            bool res = false;
            MaxInValidationDate = 0;
            maxExternalInvalidationDate = 0;

            try
            {
                if (keys == null || keys.Count == 0)
                {
                    return true;
                }

                List<LayeredCacheConfig> invalidationKeyCacheConfig = null;
                Dictionary<string, long> currentRequestResultMap = null;
                Dictionary<string, long> compeleteResultMap = new Dictionary<string, long>();
                HashSet<string> keysToGet = new HashSet<string>(keys);

                if (!shouldGetExternalInvalidationKeyDate)
                {
                    if (TryGetInvalidationKeysFromCurrentRequest(keysToGet, ref currentRequestResultMap))
                    {
                        foreach (KeyValuePair<string, long> pair in currentRequestResultMap)
                        {
                            keysToGet.Remove(pair.Key);
                            compeleteResultMap.Add(pair.Key, pair.Value);
                        }
                    }
                }

                if (ShouldCheckInvalidationKey(layeredCacheConfigName, groupId, ref invalidationKeyCacheConfig) && keysToGet.Count > 0)
                {
                    if (invalidationKeyCacheConfig == null)
                    {
                        return false;
                    }

                    Dictionary<LayeredCacheConfig, List<string>> insertToCacheConfig = new Dictionary<LayeredCacheConfig, List<string>>();

                    foreach (LayeredCacheConfig cacheConfig in invalidationKeyCacheConfig)
                    {
                        bool notInMemoryInvalidationKey = cacheConfig.Type != LayeredCacheType.InMemoryCache;

                        // if we want only external invalidation keys and this config is in memory - skip it
                        if (!notInMemoryInvalidationKey && shouldGetExternalInvalidationKeyDate)
                        {
                            continue;
                        }

                        ILayeredCacheService cache = cacheConfig.GetILayeredCachingService();

                        if (cache != null)
                        {
                            IDictionary<string, long> resultMap = null;
                            bool getSuccess = cache.GetValues<long>(keysToGet.ToList(), ref resultMap, null, true);
                            if (getSuccess && resultMap != null)
                            {
                                foreach (string keyToGet in keys)
                                {
                                    bool keyExistsInResult = resultMap.ContainsKey(keyToGet);

                                    if (keyExistsInResult || notInMemoryInvalidationKey)
                                    {
                                        // in case invalidation key value wasn't found on CB, we know it was never set and we can put the value 0
                                        long invalidationKeyValue = keyExistsInResult ? resultMap[keyToGet] : 0;
                                        compeleteResultMap[keyToGet] = invalidationKeyValue;
                                        keysToGet.Remove(keyToGet);
                                        maxExternalInvalidationDate = Math.Max(maxExternalInvalidationDate, invalidationKeyValue);
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
                                    break;
                                }
                            }
                            else
                            {
                                insertToCacheConfig.Add(cacheConfig, new List<string>(keys));
                                continue;
                            }
                        }

                        if (!shouldGetExternalInvalidationKeyDate)
                        {
                            InsertInvalidationKeysToCurrentRequest(compeleteResultMap);
                        }
                    }

                    if (!shouldGetExternalInvalidationKeyDate &&
                        insertToCacheConfig != null && insertToCacheConfig.Count > 0 && compeleteResultMap?.Count > 0)
                    {
                        foreach (KeyValuePair<LayeredCacheConfig, List<string>> pair in insertToCacheConfig)
                        {
                            // insert only to in memory cache, for CB there is no point to insert invalidation keys that were never set
                            if (pair.Key.Type == LayeredCacheType.InMemoryCache)
                            {
                                foreach (string keyToInsert in pair.Value)
                                {
                                    // in case invalidation key value wasn't found at all, it means it wasn't set and we can assign 0
                                    long invalidationDateToInsert = compeleteResultMap.ContainsKey(keyToInsert) ? compeleteResultMap[keyToInsert] : 0;
                                    if (!TrySetInvalidationKeyWithCacheConfig(keyToInsert, invalidationDateToInsert, pair.Key))
                                    {
                                        log.ErrorFormat("Failed inserting key {0} to {1}", keyToInsert, pair.Key.Type.ToString());
                                    }
                                }
                            }
                        }
                    }
                }

                if (compeleteResultMap?.Count > 0)
                {
                    MaxInValidationDate = compeleteResultMap.Values.Max();
                }

                res = true;
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetMaxInValidationKeysDate with keys {0}", string.Join(",", keys)), ex);
            }

            return res;
        }

        private bool TryGetInValidationKeysMaxDateMapping(string layeredCacheConfigName, 
            int groupId, 
            Dictionary<string, List<string>> keyMappings, 
            ref Dictionary<string, long> inValidationKeysMaxDateMapping, 
            out long maxExternalInvalidationDate)
        {
            bool res = true;
            maxExternalInvalidationDate = 0;

            try
            {
                if (keyMappings == null || keyMappings.Count == 0)
                {
                    return res;
                }

                inValidationKeysMaxDateMapping = new Dictionary<string, long>();
                foreach (KeyValuePair<string, List<string>> pair in keyMappings)
                {
                    long maxInvalidationKeyDate = 0;
                    if (TryGetMaxInValidationKeysDate(layeredCacheConfigName, groupId, pair.Value, out maxInvalidationKeyDate, out maxExternalInvalidationDate))
                    {
                        inValidationKeysMaxDateMapping.Add(pair.Key, maxInvalidationKeyDate);
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
                    else if (InternalLayeredCacheSettings.CacheSettings != null && InternalLayeredCacheSettings.CacheSettings.ContainsKey(configurationName))
                    {
                        layeredCacheConfig = InternalLayeredCacheSettings.CacheSettings[configurationName];
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

                if (TryGetKeyFromCurrentRequest<LayeredCacheGroupConfig>(key, ref groupConfig))
                {
                    return true;
                }

                var getOperationStatus = GetOperationStatus.NotFound;
                foreach (LayeredCacheConfig cacheConfig in GroupCacheSettings)
                {
                    ILayeredCacheService cache = cacheConfig.GetILayeredCachingService();
                    if (cache != null)
                    {
                        getOperationStatus = cache.Get(key, ref groupConfig, null);
                        if (getOperationStatus == GetOperationStatus.Success && groupConfig != null)
                        {
                            res = true;
                            Dictionary<string, LayeredCacheGroupConfig> groupConfigToAdd = new Dictionary<string, LayeredCacheGroupConfig>();
                            groupConfigToAdd.Add(key, groupConfig);
                            InsertResultsToCurrentRequest(groupConfigToAdd, null);

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

                if (shouldCreateIfNoneExists && getOperationStatus == GetOperationStatus.NotFound && groupConfig == null)
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
                    else if (insertToCacheConfig?.Any(x => x.Type != LayeredCacheType.InMemoryCache) == true)
                    {
                        log.Warn($"BEO-10978 inserting Default LayeredCacheGroupConfig into cache, key: {key}, groupId: {groupId}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetLayeredCacheGroupConfig for groupId: {0}", groupId), ex);
            }

            return res;
        }

        public bool TryGetInvalidationKeyLayeredCacheConfig(string configurationName, out List<LayeredCacheConfig> layeredCacheConfig)
        {
            layeredCacheConfig = null;
            try
            {
                if (layeredCacheTcmConfig != null)
                {
                    bool isPartner = isPartnerRequest(); //BEO-7703 - No cache for operator+ 

                    if (!string.IsNullOrEmpty(configurationName) && layeredCacheTcmConfig.LayeredCacheInvalidationKeySettings != null
                        && layeredCacheTcmConfig.LayeredCacheInvalidationKeySettings.ContainsKey(configurationName) && !isPartner)
                    {
                        layeredCacheConfig = layeredCacheTcmConfig.LayeredCacheInvalidationKeySettings[configurationName];
                    }
                    else if (InternalLayeredCacheSettings.InvalidationCacheSettings != null && InternalLayeredCacheSettings.InvalidationCacheSettings.ContainsKey(configurationName))
                    {
                        layeredCacheConfig = InternalLayeredCacheSettings.InvalidationCacheSettings[configurationName];
                    }
                    else if (layeredCacheTcmConfig.InvalidationKeySettings != null && layeredCacheTcmConfig.InvalidationKeySettings.Count > 0)
                    {
                        layeredCacheConfig = layeredCacheTcmConfig.InvalidationKeySettings;

                        if (isPartner)
                        {
                            layeredCacheConfig = layeredCacheConfig.Where(x => x.Type == LayeredCacheType.CbCache || x.Type == LayeredCacheType.CbMemCache).ToList();
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetInvalidationKeyLayeredCacheConfig for configurationName: {0}", configurationName), ex);
            }

            return layeredCacheConfig?.Count > 0;
        }

        // TODO: remove key, it's only for debugging purposes
        private bool checkShouldRouteDBToSecondary(string key,
            string layeredCacheConfigName, int groupId,
            bool hasMaximumExternalInvalidationDate, long maxExternalInvalidationDate,
            List<string> invalidationKeys = null, Dictionary<string, List<string>> invalidationKeysMap = null)
        {
            bool previousStateWasRoutedToSecondary = false;

            // not configured - nothing to do
            if (!ApplicationConfiguration.Current.SqlTrafficConfiguration.ShouldUseTrafficHandler.Value)
            {
                return previousStateWasRoutedToSecondary;
            }

            // either we don't have an http context
            // or if we do but we already decided that we should route to secondary - remember that previous state was routing to secondary
            if (HttpContext.Current == null || HttpContext.Current.Items == null || 
                Convert.ToBoolean(HttpContext.Current.Items[CONTEXT_KEY_SHOULD_ROUTE_DB_TO_SECONDARY]) ||
                Convert.ToBoolean(HttpContext.Current.Items[GetCurrentThreadDbRoutingContextKey()]))
            {
                previousStateWasRoutedToSecondary = true;
            }

            // #4: MaxInvalidationKeyStalnessInSeconds (default 3) that tells us 
            // if one of the current object invalidation keys changed in that time period we have to fetch the data from master 
            // (Assuming we are going to the SQL and not fetching from cache)
            // check CB (last layer) invalidation keys and fetch the max value to compare with #4 and update the request context to master if required

            var nowTimeStamp = Utils.GetUtcUnixTimestampNow();

            //// no invaldidation keys at all - don't take a risk and use primary
            //if ((invalidationKeys == null || invalidationKeys.Count == 0) &&
            //    (invalidationKeysMap == null || invalidationKeysMap.Count == 0))
            //{
            //    log.Debug($"Sql Traffic Handler for key {key}: no invalidation keys at all, so now routing next DB queries on this thread ({Thread.CurrentThread.ManagedThreadId}) to primary.");
            //    HttpContext.Current.Items[GetCurrentThreadDbRoutingContextKey()] = true;
            //    return previousState;
            //}

            // if function was not provided with max external invalidation date, find it now
            if (!hasMaximumExternalInvalidationDate)
            {
                List<string> invalidationKeysToCheck = invalidationKeys;

                // if we have invalidation keys mapping and not list - let's flatten the dictionary to a list
                if (invalidationKeysToCheck == null && invalidationKeysMap != null)
                {
                    invalidationKeysToCheck = new List<string>();

                    foreach (var invalidationKeysList in invalidationKeysMap.Values)
                    {
                        invalidationKeysToCheck.AddRange(invalidationKeysList);
                    }
                }

                if (invalidationKeysToCheck != null)
                {
                    // avoid duplications
                    invalidationKeysToCheck = invalidationKeysToCheck.Distinct().ToList();

                    long maxInvalidationKeyDate;
                    TryGetMaxInValidationKeysDate(layeredCacheConfigName, groupId, invalidationKeysToCheck,
                        out maxInvalidationKeyDate, out maxExternalInvalidationDate, true);
                }
            }

            if ((nowTimeStamp - maxExternalInvalidationDate) >= ApplicationConfiguration.Current.SqlTrafficConfiguration.MaxInvalidationKeyStalenessInSeconds.Value)
            {
                log.Debug($"Sql Traffic Handler for key {key}: maxExternalInvalidationDate is {maxExternalInvalidationDate} and now is {nowTimeStamp}. " + 
                    $"Difference is larger than config value, so now routing next DB queries on this thread ({Thread.CurrentThread.ManagedThreadId}) to secondary.");
                HttpContext.Current.Items[GetCurrentThreadDbRoutingContextKey()] = true;
            }

            return previousStateWasRoutedToSecondary;
        }

        // TODO: remove key, it's only for debugging purposes
        private void resetThreadSpecificRouting(string key, bool previousStateShouldRouteToSecondary)
        {
            // only reset if we previously routed requests to primary
            if (!previousStateShouldRouteToSecondary && ApplicationConfiguration.Current.SqlTrafficConfiguration.ShouldUseTrafficHandler.Value && 
                HttpContext.Current != null && HttpContext.Current.Items != null &&
                HttpContext.Current.Items.ContainsKey(GetCurrentThreadDbRoutingContextKey()))
            {
                log.Debug($"Sql Traffic Handler for key {key}: Resetting this thread ({Thread.CurrentThread.ManagedThreadId}) to primary.");

                HttpContext.Current.Items.Remove(GetCurrentThreadDbRoutingContextKey());
            }
        }

        public static string GetCurrentThreadDbRoutingContextKey()
        {
            return $"ShouldRouteDbToSecondary_{Thread.CurrentThread.ManagedThreadId}";
        }

        // TODO duplicate with RequestContextUtils.IsPartnerRequest
        public bool isPartnerRequest()
        {
            bool isPartner = false;

            if (HttpContext.Current != null && HttpContext.Current.Items != null && HttpContext.Current.Items.ContainsKey(REQUEST_TAGS))
            {
                var tags = (HashSet<string>)HttpContext.Current.Items[REQUEST_TAGS];
                isPartner = tags != null && tags.Contains(REQUEST_TAGS_PARTNER_ROLE);
            }

            return isPartner;
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
		
        #endregion

        #region Insert

        private bool TryInsert<T>(string key, Tuple<T, long> tuple, LayeredCacheConfig cacheConfig, bool shouldUseAutoNameTypeHandling = false)
        {
            bool res = false;
            try
            {
                ILayeredCacheService cache = cacheConfig.GetILayeredCachingService();
                if (cache != null)
                {
                    res = cache.Set<Tuple<T, long>>(key, tuple, cacheConfig.TTL, shouldUseAutoNameTypeHandling ? jsonSerializerSettings : null);
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
            if (ShouldProduceInvalidationEventsToKafka)
            {
                ProduceInvalidationEvent(key);
            }

            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return res;
                }

                LayeredCacheConfig invalidationKeyCacheConfig = layeredCacheTcmConfig.InvalidationKeySettings
                    .FirstOrDefault(x => x.Type == LayeredCacheType.CbCache || x.Type == LayeredCacheType.CbMemCache || x.Type == LayeredCacheType.Redis);

                if (invalidationKeyCacheConfig == null)
                {
                    return res;
                }

                res = TrySetInvalidationKeyWithCacheConfig(key, valueToUpdate, invalidationKeyCacheConfig);

                RemoveCachedObjectsAndInvalidationKeysFromCurrentRequest(key);
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TrySetInValidationKey with key {0}", key), ex);
            }

            return res;
        }

        private void ProduceInvalidationEvent(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key)) { return; }

                // if we have some rules we have to verify the key is matched before we send invalidation event
                if (_InvalidationEventsRegexRules.Any())
                {
                    if (!_InvalidationEventsRegexRules.Any(r => r.IsMatch(key)))
                    {
                        return;
                    }
                }

                var invalidationEvent = new CacheInvalidationEvent(key, InvalidationEventsTopic);
                KafkaPublisher.GetFromTcmConfiguration(invalidationEvent).PublishHeadersOnly(invalidationEvent);
            }
            catch (Exception e)
            {
                log.Error($"Error while trying to produce cache invalidation event for key:[{key}]", e);
            }
        }

        private static bool TrySetInvalidationKeyWithCacheConfig(string key, long valueToUpdate, LayeredCacheConfig invalidationKeyCacheConfig)
        {
            bool res = false;
            try
            {
                ILayeredCacheService cache = invalidationKeyCacheConfig.GetILayeredCachingService();
                if (cache != null)
                {
                    res = cache.Set<long>(key, valueToUpdate, invalidationKeyCacheConfig.TTL);
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
                    ILayeredCacheService cache = cacheConfig.GetILayeredCachingService();
                    if (cache != null)
                    {
                        insertResult = insertResult && cache.Set<LayeredCacheGroupConfig>(key, groupConfig, cacheConfig.TTL);

                        if (insertResult && (cacheConfig.Type != LayeredCacheType.InMemoryCache))
                        {
                            log.Warn($"BEO-10978 TrySetLayeredGroupCacheConfig, key:{key}, type:{cacheConfig.Type}, Version:{groupConfig.Version}, TTL: {cacheConfig.TTL}");
                        }
                    }
                    else
                    {
                        insertResult = false;
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

                //BEO-7703 - No cache for operator+ 
                if (res && isPartnerRequest())
                {
                    layeredCacheConfig = layeredCacheConfig.Where(x => x.Type == LayeredCacheType.CbCache || x.Type == LayeredCacheType.CbMemCache || x.Type == LayeredCacheType.Redis).ToList();
                    res = layeredCacheConfig != null && layeredCacheConfig.Count > 0;
                }
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
