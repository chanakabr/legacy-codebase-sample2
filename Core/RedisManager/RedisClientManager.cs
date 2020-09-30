using ConfigurationManager;
using StackExchange.Redis;
using System.Reflection;
using KLogMonitor;
using System;
using Newtonsoft.Json;

namespace RedisManager
{
    public class RedisClientManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static RedisClientManager instance = null;
        private static object locker = new object();
        private readonly IConnectionMultiplexer connection;
        private readonly IDatabase database;

        private RedisClientManager()
        {
            ConfigurationOptions configOptions = new ConfigurationOptions()
            {
                EndPoints = { { ApplicationConfiguration.Current.RedisClientConfiguration.HostName.Value, ApplicationConfiguration.Current.RedisClientConfiguration.Port.Value } }
            };

            connection = ConnectionMultiplexer.Connect(configOptions);
            database = connection.GetDatabase();
        }

        public static RedisClientManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new RedisClientManager();
                        }
                    }
                }

                return instance;
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

        public bool Set(string key, string value, double ttlInSeconds)
        {
            bool result = false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_REDIS, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = $"key {key}, value {value}, ttlInSeconds {ttlInSeconds}" })
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

    }
}
