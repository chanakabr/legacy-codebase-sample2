using ConfigurationManager;
using StackExchange.Redis;
using System.Reflection;
using KLogMonitor;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace RedisManager
{
    public enum RedisClientType
    {
        Persistent = 0,
        Cache = 1
    }

    public class RedisClientManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static RedisClientManager cacheInstance = null;
        private static RedisClientManager persistentInstance = null;
        private static object locker = new object();
        private readonly IConnectionMultiplexer connection;
        private readonly IDatabase database;

        private RedisClientManager(RedisClientType redisClientType)
        {
            string address = string.Empty;
            EndPointCollection epc = new EndPointCollection();
            // persistent redis address
            if (redisClientType == RedisClientType.Persistent)
            {
                address = ApplicationConfiguration.Current.RedisClientConfiguration.PersistentAddress.Value;
            }
            // persistent redis address
            else
            {
                address = ApplicationConfiguration.Current.RedisClientConfiguration.CacheAddress.Value;
            }

            ConfigurationOptions configOptions = new ConfigurationOptions()
            {
                EndPoints = { { address } }
            };

            connection = ConnectionMultiplexer.Connect(configOptions);
            database = connection.GetDatabase();
        }

        public static RedisClientManager CacheInstance
        {
            get
            {
                if (cacheInstance == null)
                {
                    lock (locker)
                    {
                        if (cacheInstance == null)
                        {
                            cacheInstance = new RedisClientManager(RedisClientType.Cache);
                        }
                    }
                }

                return cacheInstance;
            }
        }

        public static RedisClientManager PersistenceInstance
        {
            get
            {
                if (persistentInstance == null)
                {
                    lock (locker)
                    {
                        if (persistentInstance == null)
                        {
                            persistentInstance = new RedisClientManager(RedisClientType.Persistent);
                        }
                    }
                }

                return persistentInstance;
            }
        }

        public bool HealthCheck()
        {
            bool result = false;

            try
            {
                TimeSpan timeSpan = database.Ping();
                result = true;
            }
            catch (Exception ex)
            {
                log.Error("Health check failed for Redis", ex);
            }

            return result;
        }

        #region Util Methods

        private static string ObjectToJson<T>(T value)
        {
            if (value != null)
                return JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.None);
            else
                return string.Empty;
        }

        private static string ObjectToJson<T>(T value, JsonSerializerSettings jsonSerializerSettings)
        {
            if (value != null && jsonSerializerSettings != null)
                return JsonConvert.SerializeObject(value, jsonSerializerSettings);
            else
                return string.Empty;
        }

        private static T JsonToObject<T>(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                return default(T);
            }
        }

        private static T JsonToObject<T>(string json, JsonSerializerSettings jsonSerializerSettings)
        {
            if (!string.IsNullOrEmpty(json) && jsonSerializerSettings != null)
            {
                return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
            }
            else
            {
                return default(T);
            }
        }

        private static HashEntry[] ObjectToHashFields<T>(string key, T value, bool getPropNameFromJsonPropAttribute)
        {            
            if (value == null)
            {
                log.Warn($"{key} is null, can not convert to hash fields");
                return null;
            }

            List<HashEntry> hashFields = new List<HashEntry>();

            try
            {
                foreach (PropertyInfo prop in value.GetType().GetProperties())
                {
                    var name = prop.Name;
                    if (getPropNameFromJsonPropAttribute)
                    {    
                        var jsonProperty = prop.GetCustomAttribute<JsonPropertyAttribute>(true);
                        if (jsonProperty != null)
                        {
                            name = jsonProperty.PropertyName;
                        }
                    }

                    hashFields.Add(new HashEntry(name, prop.GetValue(value)?.ToString()));
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed ObjectToHashFields", ex);
            }

            return hashFields?.ToArray();
        }

        private static HashEntry[] DictionaryToHashFields(string key, Dictionary<string, object> hashSetEntries)
        {
            if (hashSetEntries == null)
            {
                log.Warn($"{key} is null, can not convert hashSetEntries to hash fields");
                return null;
            }

            List<HashEntry> hashFields = new List<HashEntry>();

            try
            {
                foreach (KeyValuePair<string, object> pair in hashSetEntries)
                {
                    hashFields.Add(new HashEntry(pair.Key, RedisValue.Unbox(pair.Value)));
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed ObjectToHashFields", ex);
            }

            return hashFields?.ToArray();
        }

        #endregion

        #region Exist Methods

        public bool IsKeyExists(string key)
        {
            bool result = false;
            try
            {
                result = database.KeyExists(key);
            }
            catch (Exception ex)
            {
                log.Error($"Failed checking if key exists for key {key}", ex);
            }

            return result;
        }

        #endregion

        #region Get Methods

        public RedisClientResponse<string> Get(string key)
        {
            RedisClientResponse<string> result = new RedisClientResponse<string>();
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_REDIS, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = $"key {key}" })
                {
                    RedisValue redisValue = database.StringGet(key);
                    if (redisValue.HasValue)
                    {
                        result.SetResponse(true, redisValue);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed fetching key {key}", ex);
            }

            return result;
        }

        public RedisClientResponse<T> Get<T>(string key, JsonSerializerSettings jsonSerializerSettings = null)
        {
            RedisClientResponse<T> result = new RedisClientResponse<T>();
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_REDIS, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = $"key {key}" })
                {
                    RedisClientResponse<string> getResponse = Get(key);
                    if (getResponse.IsSuccess && !string.IsNullOrEmpty(getResponse.Result))
                    {
                        result.SetResponse(true, JsonConvert.DeserializeObject<T>(getResponse.Result));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed fetching key {key}", ex);
            }

            return result;
        }

        #endregion

        #region Set Methods

        public bool Set(string key, string value, double ttlInSeconds)
        {
            bool result = false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_REDIS, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = $"key {key}, value {value}, ttlInSeconds {ttlInSeconds}" })
                {
                    TimeSpan? expiry = null;
                    if (ttlInSeconds > 0)
                    {
                        expiry = TimeSpan.FromSeconds(ttlInSeconds);
                    }

                    result = database.StringSet(key, value, expiry);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed setting key {key}", ex);
            }

            return result;
        }

        public bool Set<T>(string key, T value, double ttlInSeconds, JsonSerializerSettings jsonSerializerSettings = null)
        {
            bool result = false;
            try
            {
                string serializedValue = jsonSerializerSettings != null ? ObjectToJson<T>(value, jsonSerializerSettings) : ObjectToJson<T>(value);
                result = Set(key, serializedValue, ttlInSeconds);
            }
            catch (Exception ex)
            {
                log.Error($"Failed setting key {key}", ex);
            }

            return result;
        }

        #endregion

        #region Delete Methods

        public bool Delete(string key)
        {
            bool result = false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_REDIS, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.DELETE, Database = $"key {key}" })
                {
                    result = database.KeyDelete(key);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed deleting key {key}", ex);
            }

            return result;
        }

        #endregion

        #region HashSet Methods

        private bool UpsertHashSet(string key, HashEntry[] hashFields, double ttlInSeconds)
        {
            bool result = false;
            if (hashFields?.Length > 0)
            {
                try
                {
                    database.HashSet(key, hashFields);
                    if (ttlInSeconds > 0)
                    {
                        result = database.KeyExpire(key, TimeSpan.FromSeconds(ttlInSeconds));
                    }
                    else
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Failed UpsertHashSet key {key}, ttlInSeconds {ttlInSeconds}", ex);
                }
            }

            return result;
        }

        public bool UpsertHashSet(string key, Dictionary<string, object> hashSetEntries, double ttlInSeconds)
        {
            bool result = false;
            try
            {
                result = UpsertHashSet(key, DictionaryToHashFields(key, hashSetEntries), ttlInSeconds);
            }
            catch (Exception ex)
            {
                log.Error($"Failed UpsertHashSet key {key}, ttlInSeconds {ttlInSeconds}", ex);
            }

            return result;
        }

        public bool UpsertHashSet<T>(string key, T value, double ttlInSeconds, bool getPropNameFromJsonPropAttribute = true)
        {
            bool result = false;
            try
            {
                result = UpsertHashSet(key, ObjectToHashFields<T>(key, value, getPropNameFromJsonPropAttribute), ttlInSeconds);
            }
            catch (Exception ex)
            {
                log.Error($"Failed UpsertHashSet key {key}, ttlInSeconds {ttlInSeconds}", ex);
            }

            return result;
        }

        public bool IncrementHashSetField(string key, string field, double incrementAmount = 1)
        {
            bool result = false;
            try
            {
                if (!IsKeyExists(key))
                {
                    log.Debug($"{key} does not exist, can not increment hash set field {field}");
                    result = false;
                }

                // ***** This is a big assumption that the operation was successful... currently the only other option is storing the previous value and then comparing, bad for performance *****
                result = database.HashIncrement(key, field, incrementAmount) > 0;
            }
            catch (Exception ex)
            {
                log.Error($"Failed IncrementHashSetField, key {key}, field {field}, incrementAmount {incrementAmount}", ex);
            }

            return result;
        }

        public bool IncrementHashSetField<T>(string key, string field, T value, double ttlInSeconds = 0, double incrementAmount = 1)
        {
            bool result = false;
            try
            {
                if (!IsKeyExists(key))
                {
                    result = UpsertHashSet<T>(key, value, ttlInSeconds);
                }
                else
                {
                    // ***** This is a big assumption that the operation was successful... currently the only other option is storing the previous value and then comparing, bad for performance *****
                    result = database.HashIncrement(key, field, incrementAmount) > 0;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed IncrementHashSetField, key {key}, field {field}, ttlInSeconds {ttlInSeconds}, incrementAmount {incrementAmount}", ex);
            }

            return result;
        }

        public object GetHashSetFieldValue(string key, string field)
        {
            object result = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_REDIS, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = $"key {key}, field {field}" })
                {
                    Type a = typeof(long);
                    RedisValue redisValue = database.HashGet(key, field);
                    if (redisValue.HasValue)
                    {
                        result = redisValue;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed fetching key {key}", ex);
            }

            return result;
        }

        #endregion
    }
}