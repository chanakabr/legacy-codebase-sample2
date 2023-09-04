using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CachingProvider.LayeredCache;
using CouchbaseManager;
using Phx.Lib.Log;
using Newtonsoft.Json;

namespace CachingProvider
{
    public class CouchBaseCache<TO> : OutOfProcessCache, ILayeredCacheService
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int RETRY_LIMIT = 2;
        private const int RETRY_INTERVAL = 30;

        eCouchbaseBucket bucket = eCouchbaseBucket.DEFAULT;

        private CouchBaseCache(eCouchbaseBucket eCacheName)
        {
            bucket = eCacheName;
        }

        public static CouchBaseCache<TO> GetInstance(string sCacheName)
        {
            CouchBaseCache<TO> cache = null;
            try
            {
                eCouchbaseBucket eCacheName;
                if (Enum.TryParse<eCouchbaseBucket>(sCacheName.ToUpper(), out eCacheName))
                {
                    cache = new CouchBaseCache<TO>(eCacheName);
                }
                else
                {
                    log.Error("Error - " + string.Format("Unable to create OOP cache. Please check that cache of type {0} exists.", sCacheName));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error - " + string.Format("Unable to create OOP cache. Ex={0};\nCall stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return cache;
        }

        public override bool Add(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            return new CouchbaseManager.CouchbaseManager(bucket).Add(sKey, oValue.result, (uint)(nMinuteOffset * 60));
        }

        public override bool Set(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            return new CouchbaseManager.CouchbaseManager(bucket).Set(sKey, oValue.result, (uint)(nMinuteOffset * 60));
        }

        public override bool Add(string key, BaseModuleCache oValue)
        {
            return new CouchbaseManager.CouchbaseManager(bucket).Add(key, oValue.result);
        }

        public override bool Set(string sKey, BaseModuleCache oValue)
        {
            return new CouchbaseManager.CouchbaseManager(bucket).Set(sKey, oValue.result);
        }

        public override BaseModuleCache Get(string key)
        {
            BaseModuleCache baseModule = new BaseModuleCache();

            TO result = new CouchbaseManager.CouchbaseManager(bucket).Get<TO>(key);

            baseModule.result = result;

            return baseModule;
        }

        public override T Get<T>(string key)
        {
            T result = new CouchbaseManager.CouchbaseManager(bucket).Get<T>(key);

            return result;
        }

        public override BaseModuleCache Remove(string key)
        {
            BaseModuleCache baseModule = new BaseModuleCache();

            bool result = new CouchbaseManager.CouchbaseManager(bucket).Remove(key);
            baseModule.result = result;

            return baseModule;
        }

        public override BaseModuleCache GetWithVersion<T>(string key)
        {
            VersionModuleCache baseModule = new VersionModuleCache();
            ulong version;

            T result = new CouchbaseManager.CouchbaseManager(bucket).GetWithVersion<T>(key, out version);

            baseModule.result = result;
            baseModule.version = version.ToString();

            return baseModule;
        }

        /// <summary>
        /// Add - insert the value + key to cache only if the key doesn't exists already
        /// (does nothing (returns false) if there is already a value for that key )
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sKey"></param>
        /// <param name="oValue"></param>
        /// <param name="nMinuteOffset"></param>
        /// <returns></returns>
        public override bool AddWithVersion<T>(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool result = false;

            result = new CouchbaseManager.CouchbaseManager(bucket).Add(sKey, oValue.result, (uint)(nMinuteOffset * 60));

            return result;
        }

        public override bool AddWithVersion<T>(string key, BaseModuleCache oValue)
        {
            return this.Add(key, oValue);
        }

        public override bool SetWithVersion<T>(string key, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool result = false;
            
            result = new CouchbaseManager.CouchbaseManager(bucket).SetWithVersion(key, oValue.result, 
                ulong.Parse((oValue as VersionModuleCache).version),
                (uint)(nMinuteOffset * 60));

            return result;
        }

        public override bool SetWithVersion<T>(string key, BaseModuleCache oValue)
        {
            bool result = false;

            result = new CouchbaseManager.CouchbaseManager(bucket).SetWithVersion(key, oValue.result,
                ulong.Parse((oValue as VersionModuleCache).version));

            return result;
        }

        public override IDictionary<string, object> GetValues(List<string> keys, bool asJson = false)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();

            var innerResult = new CouchbaseManager.CouchbaseManager(bucket).GetValues<TO>(keys, asJson);

            if (innerResult != null)
            {
                foreach (var item in innerResult)
                {
                    result.Add(item.Key, item.Value);
                }
            }

            return result;
        }

        public override bool SetJson<T>(string sKey, T obj, double dCacheTT)
        {
            bool result = false;

            result = new CouchbaseManager.CouchbaseManager(bucket).SetJson<T>(sKey, obj, (uint)(dCacheTT * 60));

            return result;
        }

        public override bool GetJsonAsT<T>(string sKey, out T res)
        {
            var json = Get<object>(sKey);
            try
            {
                string jsonString = Convert.ToString(json);

                if (!string.IsNullOrEmpty(jsonString))
                {
                    res = JsonToObject<T>(jsonString);
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetJsonAsT on key = {0}", sKey, ex);
            }

            res = default(T);
            return false;
        }

        private static string ObjectToJson<T>(T obj)
        {
            if (obj != null)
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            else
                return string.Empty;
        }

        private static T JsonToObject<T>(string json)
        {
            T result = default(T);

            if (!string.IsNullOrEmpty(json))
            {
                var settings = new Newtonsoft.Json.JsonSerializerSettings();

                settings.Error = new EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>((a, eventArgs) =>
                {
                    log.ErrorFormat("Error deserializing json: ", eventArgs.ErrorContext.Error);
                    return;
                });

                result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }

            return result;
        }

        public override List<string> GetKeys()
        {
            return new List<string>();
        }

        public GetOperationStatus Get<T>(string key, ref T result, JsonSerializerSettings jsonSerializerSettings)
        {
            result = jsonSerializerSettings == null 
                ? new CouchbaseManager.CouchbaseManager(bucket).Get<T>(key, out var status)
                : new CouchbaseManager.CouchbaseManager(bucket).Get<T>(key, out status, jsonSerializerSettings);
            return status.ToGetOperationStatus();
        }

        public override bool RemoveKey(string key)
        {
            return new CouchbaseManager.CouchbaseManager(bucket).Remove(key);
        }

        public override bool Add<T>(string key, T value, uint expirationInSeconds)
        {
            return new CouchbaseManager.CouchbaseManager(bucket).Add<T>(key, value);
        }

        public override bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, bool shouldAllowPartialQuery = false)
        {
            return new CouchbaseManager.CouchbaseManager(bucket).GetValues<T>(keys, ref results, shouldAllowPartialQuery);
        }

        public override bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = null, bool shouldAllowPartialQuery = false)
        {
            return new CouchbaseManager.CouchbaseManager(bucket).GetValues<T>(keys, ref results, jsonSerializerSettings, shouldAllowPartialQuery);
        }

        public bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, bool shouldAllowPartialQuery = false, bool asJson = false)
        {
            results = new CouchbaseManager.CouchbaseManager(bucket).GetValues<T>(keys, shouldAllowPartialQuery, asJson);

            return true;
        }

        public new bool Set<T>(string key, T value, uint ttlInSeconds, JsonSerializerSettings jsonSerializerSettings = null)
        {
            if (jsonSerializerSettings != null)
            {
                return new CouchbaseManager.CouchbaseManager(bucket).Set<T>(key, value, ttlInSeconds, jsonSerializerSettings);
            }
            else
            {
                return new CouchbaseManager.CouchbaseManager(bucket).Set<T>(key, value, ttlInSeconds);
            }
        }
    }
    
    public static class EResultStatusExtensions
    {
        public static GetOperationStatus ToGetOperationStatus(this eResultStatus s)
        {
            switch (s)
            {
                case eResultStatus.ERROR: return GetOperationStatus.Error;
                case eResultStatus.SUCCESS: return GetOperationStatus.Success;
                case eResultStatus.KEY_NOT_EXIST: return GetOperationStatus.NotFound;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }
        }
    }
}
