using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CouchbaseManager;
using Enyim.Caching.Memcached;
using KLogMonitor;

namespace CachingProvider
{
    public class CouchBaseCache<T> : OutOfProcessCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        eCouchbaseBucket bucket = eCouchbaseBucket.DEFAULT;

        private CouchBaseCache(eCouchbaseBucket eCacheName)
        {
            bucket = eCacheName;
        }

        public static CouchBaseCache<T> GetInstance(string sCacheName)
        {
            CouchBaseCache<T> cache = null;
            try
            {
                eCouchbaseBucket eCacheName;
                if (Enum.TryParse<eCouchbaseBucket>(sCacheName.ToUpper(), out eCacheName))
                {
                    cache = new CouchBaseCache<T>(eCacheName);
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

            T result = new CouchbaseManager.CouchbaseManager(bucket).Get<T>(key);

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

            bool result = new CouchbaseManager.CouchbaseManager(bucket).Remove<T>(key);
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

        public override IDictionary<string, object> GetValues(List<string> keys)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();

            var innerResult = new CouchbaseManager.CouchbaseManager(bucket).GetValues<T>(keys, false);

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
            var json = Get<string>(sKey);
            if (!string.IsNullOrEmpty(json))
            {
                res = JsonToObject<T>(json);
                return true;
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
            if (!string.IsNullOrEmpty(json))
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            else
                return default(T);
        }
    }
}
