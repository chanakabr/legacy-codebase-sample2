using CachingProvider.LayeredCache;
using Phx.Lib.Log;
using Newtonsoft.Json;
using RedisManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CachingProvider
{
    public class RedisCache : ILayeredCacheService
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public GetOperationStatus Get<T>(string key, ref T result, JsonSerializerSettings jsonSerializerSettings)
        {
            RedisClientResponse<T> getResponse = RedisClientManager.CacheInstance.Get<T>(key, jsonSerializerSettings);
            if (getResponse.IsSuccess)
            {
                result = getResponse.Result;
                return GetOperationStatus.Success;
            }

            return GetOperationStatus.NotFound;
        }

        public bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, JsonSerializerSettings jsonSerializerSettings = null, bool shouldAllowPartialQuery = false)
        {
            bool res = false;
            results = new Dictionary<string, T>();
            try
            {
                foreach (string key in keys)
                {
                    RedisClientResponse<T> getResponse = RedisClientManager.CacheInstance.Get<T>(key, jsonSerializerSettings);
                    if (getResponse.IsSuccess)
                    {
                        results[key] = getResponse.Result;
                    }
                    else if (!getResponse.IsSuccess && !shouldAllowPartialQuery)
                    {
                        results = null;
                        break;
                    }
                }

                res = results != null || shouldAllowPartialQuery ? true : false;
            }
            catch (Exception ex)
            {
                results = null;
                string jsonSerializerSettingsNull = jsonSerializerSettings != null ? "not null" : "null";
                log.Error($"Failed GetValues<T> for keys {string.Join(",", keys.ToArray().Take(20))}, jsonSerializerSettings are {jsonSerializerSettingsNull}, shouldAllowPartialQuery is {shouldAllowPartialQuery}", ex);
            }

            return res;
        }

        public bool Set<T>(string key, T value, uint expirationInSeconds, JsonSerializerSettings jsonSerializerSettings = null)
        {
            bool res = false;
            try
            {
                res = RedisClientManager.CacheInstance.Set<T>(key, value, expirationInSeconds, jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                string jsonSerializerSettingsNull = jsonSerializerSettings != null ? "not null" : "null";
                log.Error($"Failed Set<T> for key {key}, jsonSerializerSettings are {jsonSerializerSettingsNull}", ex);
            }

            return res;
        }
    }
}
