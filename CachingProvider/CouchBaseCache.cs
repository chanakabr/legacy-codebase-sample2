using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Couchbase;
using CouchbaseManager;
using Enyim.Caching.Memcached;
using KLogMonitor;


namespace CachingProvider
{
    public class CouchBaseCache<T> : OutOfProcessCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        CouchbaseClient m_Client;

        private CouchBaseCache(eCouchbaseBucket eCacheName)
        {
            m_Client = CouchbaseManager.CouchbaseManager.GetInstance(eCacheName);

            if (m_Client == null)
                throw new Exception("Unable to create out of process cache instance");
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
            return m_Client.Store(Enyim.Caching.Memcached.StoreMode.Add, sKey, oValue.result, DateTime.UtcNow.AddMinutes(nMinuteOffset));
        }

        public override bool Set(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            return m_Client.Store(Enyim.Caching.Memcached.StoreMode.Set, sKey, oValue.result, DateTime.UtcNow.AddMinutes(nMinuteOffset));
        }

        public override bool Add(string sKey, BaseModuleCache oValue)
        {
            return m_Client.Store(Enyim.Caching.Memcached.StoreMode.Add, sKey, oValue.result);
        }

        public override bool Set(string sKey, BaseModuleCache oValue)
        {
            return m_Client.Store(Enyim.Caching.Memcached.StoreMode.Set, sKey, oValue.result);
        }

        public override BaseModuleCache Get(string sKey)
        {
            BaseModuleCache baseModule = new BaseModuleCache();

            try
            {
                baseModule.result = m_Client.Get(sKey);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Get with key = {0}, error = {1}, ST = {2}", sKey, ex.Message, ex.StackTrace), ex);
            }

            return baseModule;
        }

        public override T Get<T>(string sKey)
        {
            return m_Client.Get<T>(sKey);
        }

        public override BaseModuleCache Remove(string sKey)
        {
            BaseModuleCache baseModule = new BaseModuleCache();
            baseModule.result = m_Client.Remove(sKey);
            return baseModule;
        }

        public override BaseModuleCache GetWithVersion<T>(string sKey)
        {
            VersionModuleCache baseModule = new VersionModuleCache();
            CasResult<T> oRes = default(CasResult<T>);

            try
            {
                oRes = m_Client.GetWithCas<T>(sKey);

                if (oRes.StatusCode == 0)
                {
                    baseModule.result = oRes.Result;
                    baseModule.version = oRes.Cas.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error("CasResult", ex);
            }

            return baseModule;
        }

        // Add - insert the value + key to cache only if the key doesn't exists already
        //(does nothing (returns false) if there is already a value for that key )
        public override bool AddWithVersion<T>(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool bRes = false;
            try
            {
                VersionModuleCache baseModule = (VersionModuleCache)oValue;
                ulong cas = 0;
                bool bCas = ulong.TryParse(baseModule.version, out cas);

                DateTime dtExpiresAt = DateTime.UtcNow.AddMinutes(nMinuteOffset);

                CasResult<bool> casRes = m_Client.Cas(Enyim.Caching.Memcached.StoreMode.Add, sKey, baseModule.result, dtExpiresAt, cas);

                if (casRes.StatusCode == 0)
                {
                    bRes = casRes.Result;
                }
                return bRes;
            }
            catch (Exception ex)
            {
                log.Error("AddWithVersion", ex);
                return false;
            }
        }
        public override bool AddWithVersion<T>(string sKey, BaseModuleCache oValue)
        {
            bool bRes = false;
            try
            {
                VersionModuleCache baseModule = (VersionModuleCache)oValue;
                ulong cas = 0;
                bool bCas = ulong.TryParse(baseModule.version, out cas);

                CasResult<bool> casRes = m_Client.Cas(Enyim.Caching.Memcached.StoreMode.Add, sKey, baseModule.result, cas);

                if (casRes.StatusCode == 0)
                {
                    bRes = casRes.Result;
                }
                return bRes;
            }
            catch (Exception ex)
            {
                log.Error("AddWithVersion", ex);
                return false;
            }
        }

        public override bool SetWithVersion<T>(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool bRes = false;
            try
            {
                VersionModuleCache baseModule = (VersionModuleCache)oValue;
                ulong cas = 0;
                bool bCas = ulong.TryParse(baseModule.version, out cas);

                DateTime dtExpiresAt = DateTime.UtcNow.AddMinutes(nMinuteOffset);

                CasResult<bool> casRes = m_Client.Cas(Enyim.Caching.Memcached.StoreMode.Set, sKey, baseModule.result, dtExpiresAt, cas);

                if (casRes.StatusCode == 0)
                {
                    bRes = casRes.Result;
                }
                return bRes;
            }
            catch (Exception ex)
            {
                log.Error("AddWithVersion", ex);
                return false;
            }
        }
        public override bool SetWithVersion<T>(string sKey, BaseModuleCache oValue)
        {
            bool bRes = false;
            try
            {
                VersionModuleCache baseModule = (VersionModuleCache)oValue;
                ulong cas = 0;
                bool bCas = ulong.TryParse(baseModule.version, out cas);

                CasResult<bool> casRes = m_Client.Cas(Enyim.Caching.Memcached.StoreMode.Set, sKey, baseModule.result, cas);

                if (casRes.StatusCode == 0)
                {
                    bRes = casRes.Result;
                }
                return bRes;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }

        public override IDictionary<string, object> GetValues(List<string> keys)
        {
            try
            {
                IDictionary<string, object> dRes = m_Client.Get(keys);
                return dRes;
            }
            catch (Exception ex)
            {
                if (keys != null && keys.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in keys)
                        sb.Append(item + " ");

                    log.ErrorFormat("Error while getting the following keys from CB: {0}. Exception: {1}", sb.ToString(), ex);
                }
                else
                    log.Error("Error while getting keys from CB", ex);
            }
            return null;
        }

        public override bool SetJson<T>(string sKey, T obj, double dCacheTT)
        {
            var json = ObjectToJson<T>(obj);
            return m_Client.Store(Enyim.Caching.Memcached.StoreMode.Set, sKey, json, DateTime.UtcNow.AddMinutes(dCacheTT));
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
