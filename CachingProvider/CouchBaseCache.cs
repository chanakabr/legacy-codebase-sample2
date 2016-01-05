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
        eCouchbaseBucket bucket = eCouchbaseBucket.DEFAULT;

        private CouchBaseCache(eCouchbaseBucket eCacheName)
        {
            m_Client = CouchbaseManager.CouchbaseManager.GetInstance(eCacheName);
            bucket = eCacheName;

            if (m_Client == null)
                throw new Exception("Unable to create out of process cache instance");

            m_Client.NodeFailed += m_Client_NodeFailed;
        }

        void m_Client_NodeFailed(IMemcachedNode obj)
        {
            m_Client = CouchbaseManager.CouchbaseManager.RefreshInstance(bucket);
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

        /// <summary>
        /// See status codes at: http://docs.couchbase.com/couchbase-sdk-net-1.3/#checking-error-codes
        /// </summary>
        /// <param name="statusCode"></param>
        private void HandleStatusCode(int? statusCode, string key = "")
        {
            if (statusCode != null)
            {
                if (statusCode.Value != 0)
                {
                    // 1 - not found
                    if (statusCode.Value == 1)
                    {
                        log.DebugFormat("Could not find key on couchbase: {0}", key);
                    }
                    else
                    {
                        log.ErrorFormat("Error while executing action on CB. Status code = {0}", statusCode.Value);
                    }
                }

                // Cases of retry
                switch (statusCode)
                {
                    // Busy
                    case 133:
                    // SocketPoolTimeout
                    case 145:
                    // UnableToLocateNode
                    case 146:
                    // NodeShutdown
                    case 147:
                    // OperationTimeout
                    case 148:
                    {
                        m_Client = CouchbaseManager.CouchbaseManager.RefreshInstance(bucket);

                        break;
                    }
                    default:
                    break;
                }
            }
        }

        public override bool Add(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool result = false;

            var executeStore = m_Client.ExecuteStore(Enyim.Caching.Memcached.StoreMode.Add, sKey, oValue.result, DateTime.UtcNow.AddMinutes(nMinuteOffset));

            if (executeStore != null)
            {
                if (executeStore.Exception != null)
                {
                    throw executeStore.Exception;
                }

                if (executeStore.StatusCode == 0)
                {
                    result = executeStore.Success;
                }
                else
                {
                    HandleStatusCode(executeStore.StatusCode);

                    result = m_Client.Store(Enyim.Caching.Memcached.StoreMode.Add, sKey, oValue.result, DateTime.UtcNow.AddMinutes(nMinuteOffset));
                }
            }

            return result;
        }

        public override bool Set(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool result = false;

            var executeStore = m_Client.ExecuteStore(Enyim.Caching.Memcached.StoreMode.Set, sKey, oValue.result, DateTime.UtcNow.AddMinutes(nMinuteOffset));

            if (executeStore != null)
            {
                if (executeStore.Exception != null)
                {
                    throw executeStore.Exception;
                }

                if (executeStore.StatusCode == 0)
                {
                    result = executeStore.Success;
                }
                else
                {
                    HandleStatusCode(executeStore.StatusCode);

                    result = m_Client.Store(Enyim.Caching.Memcached.StoreMode.Set, sKey, oValue.result, DateTime.UtcNow.AddMinutes(nMinuteOffset));
                }
            }

            return result;
        }

        public override bool Add(string key, BaseModuleCache oValue)
        {
            bool result = false;

            var executeStore = m_Client.ExecuteStore(Enyim.Caching.Memcached.StoreMode.Add, key, oValue.result);

            if (executeStore != null)
            {
                if (executeStore.Exception != null)
                {
                    throw executeStore.Exception;
                }

                if (executeStore.StatusCode == 0)
                {
                    result = executeStore.Success;
                }
                else
                {
                    HandleStatusCode(executeStore.StatusCode);

                    result = m_Client.Store(Enyim.Caching.Memcached.StoreMode.Add, key, oValue.result);
                }
            }

            return result;
        }

        public override bool Set(string sKey, BaseModuleCache oValue)
        {
            bool result = false;

            var executeStore = m_Client.ExecuteStore(Enyim.Caching.Memcached.StoreMode.Set, sKey, oValue.result);

            if (executeStore != null)
            {
                if (executeStore.Exception != null)
                {
                    throw executeStore.Exception;
                }

                if (executeStore.StatusCode == 0)
                {
                    result = executeStore.Success;
                }
                else
                {
                    HandleStatusCode(executeStore.StatusCode);

                    result = m_Client.Store(Enyim.Caching.Memcached.StoreMode.Set, sKey, oValue.result);
                }
            }

            return result;
        }

        public override BaseModuleCache Get(string key)
        {
            BaseModuleCache baseModule = new BaseModuleCache();

            try
            {
                var executeGet = m_Client.ExecuteGet(key);
                baseModule.result = executeGet.Value;

                if (executeGet != null)
                {
                    if (executeGet.Exception != null)
                    {
                        throw executeGet.Exception;
                    }

                    if (executeGet.StatusCode == 0)
                    {
                        baseModule.result = executeGet.Value;
                    }
                    else
                    {
                        int? statusCode = executeGet.StatusCode;
                        HandleStatusCode(statusCode, key);

                        baseModule.result = m_Client.Get(key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Get with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);
            }

            return baseModule;
        }

        public override T Get<T>(string key)
        {
            T result = default(T);

            var executeGet = m_Client.ExecuteGet<T>(key);

            if (executeGet != null)
            {
                if (executeGet.Exception != null)
                {
                    throw executeGet.Exception;
                }

                if (executeGet.StatusCode == 0)
                {
                    result = executeGet.Value;
                }
                else
                {
                    int? statusCode = executeGet.StatusCode;
                    HandleStatusCode(statusCode, key);

                    result = m_Client.Get<T>(key);
                }
            }

            return result;
        }

        public override BaseModuleCache Remove(string key)
        {
            BaseModuleCache baseModule = new BaseModuleCache();

            var executeRemove = m_Client.ExecuteRemove(key);
            baseModule.result = executeRemove.Success;

            if (executeRemove != null)
            {
                if (executeRemove.Exception != null)
                {
                    throw executeRemove.Exception;
                }

                if (executeRemove.StatusCode == 0)
                {
                    baseModule.result = executeRemove.Success;
                }
                else
                {
                    int? statusCode = executeRemove.StatusCode;
                    HandleStatusCode(statusCode);

                    baseModule.result = m_Client.Remove(key);
                }
            }

            return baseModule;
        }

        public override BaseModuleCache GetWithVersion<T>(string key)
        {
            VersionModuleCache baseModule = new VersionModuleCache();
            CasResult<T> oRes = default(CasResult<T>);

            try
            {
                var executeGet = m_Client.ExecuteGet<T>(key);

                if (executeGet != null)
                {
                    if (executeGet.Exception != null)
                    {
                        throw executeGet.Exception;
                    }

                    if (executeGet.StatusCode == 0)
                    {
                        baseModule.result = executeGet.Value;
                        baseModule.version = executeGet.Cas.ToString();
                    }
                    else
                    {
                        int? statusCode = executeGet.StatusCode;
                        HandleStatusCode(statusCode);

                        oRes = m_Client.GetWithCas<T>(key);

                        if (oRes.StatusCode == 0)
                        {
                            baseModule.result = oRes.Result;
                            baseModule.version = oRes.Cas.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("CasResult", ex);
            }

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

            try
            {
                VersionModuleCache baseModule = (VersionModuleCache)oValue;
                ulong cas = 0;
                bool parse = ulong.TryParse(baseModule.version, out cas);

                DateTime dtExpiresAt = DateTime.UtcNow.AddMinutes(nMinuteOffset);

                var executeCas = m_Client.ExecuteCas(Enyim.Caching.Memcached.StoreMode.Add, sKey, baseModule.result, dtExpiresAt, cas);

                if (executeCas != null)
                {
                    if (executeCas.Exception != null)
                    {
                        throw executeCas.Exception;
                    }

                    if (executeCas.StatusCode == 0)
                    {
                        result = executeCas.Success;
                    }
                    else
                    {
                        HandleStatusCode(executeCas.StatusCode);

                        CasResult<bool> casRes = m_Client.Cas(Enyim.Caching.Memcached.StoreMode.Add, sKey, baseModule.result, dtExpiresAt, cas);

                        if (casRes.StatusCode == 0)
                        {
                            result = casRes.Result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("AddWithVersion", ex);
                result = false;
            }

            return result;
        }

        public override bool AddWithVersion<T>(string key, BaseModuleCache oValue)
        {
            bool result = false;
            try
            {
                VersionModuleCache baseModule = (VersionModuleCache)oValue;
                ulong cas = 0;
                bool bCas = ulong.TryParse(baseModule.version, out cas);

                var executeCas = m_Client.ExecuteCas(Enyim.Caching.Memcached.StoreMode.Add, key, baseModule.result, cas);

                if (executeCas != null)
                {
                    if (executeCas.Exception != null)
                    {
                        throw executeCas.Exception;
                    }

                    if (executeCas.StatusCode == 0)
                    {
                        result = executeCas.Success;
                    }
                    else
                    {
                        HandleStatusCode(executeCas.StatusCode);

                        CasResult<bool> casRes = m_Client.Cas(Enyim.Caching.Memcached.StoreMode.Add, key, baseModule.result, cas);

                        if (casRes.StatusCode == 0)
                        {
                            result = casRes.Result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("AddWithVersion", ex);
                result = false;
            }

            return result;
        }

        public override bool SetWithVersion<T>(string key, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool result = false;

            try
            {
                VersionModuleCache baseModule = (VersionModuleCache)oValue;
                ulong cas = 0;
                bool parse = ulong.TryParse(baseModule.version, out cas);

                DateTime dtExpiresAt = DateTime.UtcNow.AddMinutes(nMinuteOffset);

                var executeCas = m_Client.ExecuteCas(Enyim.Caching.Memcached.StoreMode.Set, key, baseModule.result, dtExpiresAt, cas);

                if (executeCas != null)
                {
                    if (executeCas.Exception != null)
                    {
                        throw executeCas.Exception;
                    }

                    if (executeCas.StatusCode == 0)
                    {
                        result = executeCas.Success;
                    }
                    else
                    {
                        HandleStatusCode(executeCas.StatusCode);

                        CasResult<bool> casRes = m_Client.Cas(Enyim.Caching.Memcached.StoreMode.Set, key, baseModule.result, dtExpiresAt, cas);

                        if (casRes.StatusCode == 0)
                        {
                            result = casRes.Result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("SetWithVersion", ex);
                result = false;
            }

            return result;

        }

        public override bool SetWithVersion<T>(string key, BaseModuleCache oValue)
        {
            bool result = false;
            try
            {
                VersionModuleCache baseModule = (VersionModuleCache)oValue;
                ulong cas = 0;
                bool bCas = ulong.TryParse(baseModule.version, out cas);

                var executeCas = m_Client.ExecuteCas(Enyim.Caching.Memcached.StoreMode.Set, key, baseModule.result, cas);

                if (executeCas != null)
                {
                    if (executeCas.Exception != null)
                    {
                        throw executeCas.Exception;
                    }

                    if (executeCas.StatusCode == 0)
                    {
                        result = executeCas.Success;
                    }
                    else
                    {
                        HandleStatusCode(executeCas.StatusCode);

                        CasResult<bool> casRes = m_Client.Cas(Enyim.Caching.Memcached.StoreMode.Set, key, baseModule.result, cas);

                        if (casRes.StatusCode == 0)
                        {
                            result = casRes.Result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("AddWithVersion", ex);
                result = false;
            }

            return result;
        }

        public override IDictionary<string, object> GetValues(List<string> keys)
        {
            IDictionary<string, object> result = null;
            try
            {
                result = m_Client.Get(keys);

                var executeGet = m_Client.ExecuteGet(keys);

                if (executeGet != null)
                {
                    int? statusCode = 0;
                    foreach (var item in executeGet)
                    {
                        if (item.Value.Exception != null)
                        {
                            throw item.Value.Exception;
                        }

                        if (item.Value.StatusCode != 0)
                        {
                            statusCode = item.Value.StatusCode;
                            break;
                        }
                    }

                    if (statusCode == 0)
                    {
                        // if successfull - build dictionary based on execution result
                        result = new Dictionary<string, object>();

                        foreach (var item in executeGet)
                        {
                            result.Add(item.Key, item.Value.Value);
                        }
                    }
                    else
                    {
                        // Otherwise, recreate connection and try again
                        HandleStatusCode(statusCode);

                        result = m_Client.Get(keys);
                    }
                }
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

            return result;
        }

        public override bool SetJson<T>(string sKey, T obj, double dCacheTT)
        {
            bool result = false;

            var json = ObjectToJson<T>(obj);

            var executeStore = m_Client.ExecuteStore(Enyim.Caching.Memcached.StoreMode.Set, sKey, json, DateTime.UtcNow.AddMinutes(dCacheTT));

            if (executeStore != null)
            {
                if (executeStore.Exception != null)
                {
                    throw executeStore.Exception;
                }

                if (executeStore.StatusCode == 0)
                {
                    result = executeStore.Success;
                }
                else
                {
                    HandleStatusCode(executeStore.StatusCode);

                    result = m_Client.Store(Enyim.Caching.Memcached.StoreMode.Set, sKey, json, DateTime.UtcNow.AddMinutes(dCacheTT));
                }
            }

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
