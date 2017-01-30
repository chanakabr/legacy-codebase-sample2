using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.Threading;
using System.Security.Principal;
using System.Security.AccessControl;
using KLogMonitor;
using System.Reflection;

namespace CachingProvider
{
    public class SingleInMemoryCache : ICachingService, IDisposable
    {
        /*
         * Pay attention !
         * 1. MemoryCache is threadsafe, however the references it holds are not necessarily thread safe.
         * 2. MemoryCache should be properly disposed.
         */

        private static readonly string DEFAULT_CACHE_NAME = "Cache";
        private static readonly uint DEFAULT_CACHE_TTL_IN_SECONDS = 7200;
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private MemoryCache cache = null;

        public string CacheName
        {
            get;
            private set;
        }
        public double DefaultMinOffset
        {
            get;
            private set;
        }

        #region Ctors

        public SingleInMemoryCache(uint defaultExpirationInSeconds)
        {
            CacheName = GetCacheName();
            DefaultMinOffset = (double)defaultExpirationInSeconds / 60;
            cache = new MemoryCache(CacheName);
        }

        #endregion

        private string GetCacheName()
        {
            string res = Utils.GetTcmConfigValue("CACHE_NAME");
            if (res.Length > 0)
                return res;
            return DEFAULT_CACHE_NAME;
        }

        public bool Add<T>(string sKey, T value, uint expirationInSeconds)
        {
            if (string.IsNullOrEmpty(sKey))
                return false;
            return cache.Add(sKey, value, DateTime.UtcNow.AddMinutes((double)expirationInSeconds / 60));
        }

        public bool Add(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            if (string.IsNullOrEmpty(sKey))
                return false;
            return cache.Add(sKey, oValue.result, DateTime.UtcNow.AddMinutes(nMinuteOffset));
        }

        public bool Add(string sKey, BaseModuleCache oValue)
        {
            return Add(sKey, oValue, DefaultMinOffset);
        }

        public bool Set(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool res = false;
            if (string.IsNullOrEmpty(sKey))
                return false;
            try
            {
                cache.Set(sKey, oValue.result, DateTime.UtcNow.AddMinutes(nMinuteOffset));
                res = true;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at Set. ");
                sb.Append(String.Concat(" Key: ", sKey));
                sb.Append(String.Concat(" Val: ", oValue != null ? oValue.ToString() : "null"));
                sb.Append(String.Concat(" Min Offset: ", nMinuteOffset));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
            }

            return res;
        }

        public bool Set(string sKey, BaseModuleCache oValue)
        {
            return Set(sKey, oValue, DefaultMinOffset);
        }

        public BaseModuleCache Get(string sKey)
        {
            BaseModuleCache baseModule = new BaseModuleCache();
            if (string.IsNullOrEmpty(sKey))
                return null;
            baseModule.result = cache.Get(sKey);
            return baseModule;
        }

        public BaseModuleCache Remove(string sKey)
        {
            BaseModuleCache baseModule = new BaseModuleCache();
            baseModule.result = cache.Remove(sKey);
            return baseModule;
        }        

        public T Get<T>(string sKey) where T : class
        {
            if (string.IsNullOrEmpty(sKey))
                return default(T);
            return cache.Get(sKey) as T;
        }

        public BaseModuleCache GetWithVersion<T>(string sKey)
        {         
            try
            {
                if (string.IsNullOrEmpty(sKey))
                    return null;
                VersionModuleCache baseModule = (VersionModuleCache)cache.Get(sKey);

                if (baseModule == null)
                {
                    baseModule = new VersionModuleCache();
                }
                return baseModule;
            }
            catch
            {
                return null;
            }
        }

        public bool AddWithVersion<T>(string sKey, BaseModuleCache oValue)
        {
            return AddWithVersion<T>(sKey, oValue, DefaultMinOffset);
        }

        public bool AddWithVersion<T>(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            if (string.IsNullOrEmpty(sKey))
                return false;

            return cache.Add(sKey, oValue, DateTime.UtcNow.AddMinutes(nMinuteOffset));
        }

        public bool SetWithVersion<T>(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool bRes = false;
            try
            {
                DateTime dtExpiresAt = DateTime.UtcNow.AddMinutes(nMinuteOffset);

                //lock 
                bool createdNew = false;
                var mutexSecurity = CreateMutex();
                using (Mutex mutex = new Mutex(false, string.Concat("Lock", sKey), out createdNew, mutexSecurity))
                {
                    try
                    {
                        mutex.WaitOne(-1);
                        BaseModuleCache vModule = GetWithVersion<T>(sKey);
                        // memory is empty for this key OR the object must have the same version 
                        if (vModule == null || vModule.result == null || (vModule != null && vModule.result != null))
                        {                            
                            Guid versionGuid = Guid.NewGuid();
                            cache.Set(sKey, oValue, DateTime.UtcNow.AddMinutes(nMinuteOffset));
                            return true;
                        }                        
                    }
                    catch
                    {

                    }
                    finally
                    {
                        //unlock
                        mutex.ReleaseMutex();
                    }
                }
                return bRes;
            }
            catch (Exception ex)
            {
                //Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                //log.Message = string.Format("AddWithVersion: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error("AddWithVersion", ex);
                return false;
            }
        }

        public bool SetWithVersion<T>(string sKey, BaseModuleCache oValue)
        {
            return SetWithVersion<T>(sKey, oValue, DefaultMinOffset);
        }

        private MutexSecurity CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            MutexSecurity mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.Delete, AccessControlType.Deny));

            return mutexSecurity;
        }

        public void Dispose()
        {
            if (cache != null)
            {
                cache.Dispose();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("SingleInMemoryCache. ");
            sb.Append(String.Concat(" Cache Name: ", CacheName));
            sb.Append(String.Concat(" DefaultMinOffset: ", DefaultMinOffset));
            sb.Append(String.Concat(" Items in cache: ", cache.GetCount()));
            sb.Append(String.Concat(" Total amt of bytes on machine the cache can use: ", cache.CacheMemoryLimit));
            sb.Append(String.Concat(" Total percentage of physical memory the cache can use: ", cache.PhysicalMemoryLimit));
            sb.Append(String.Concat(" Polling Interval: ", cache.PollingInterval.ToString()));

            return sb.ToString();
        }

        public IDictionary<string, object> GetValues(List<string> keys, bool asJson = false)
        {
            IDictionary<string, object> iDict = null;
            try
            {
                if (keys == null || keys.Count == 0)
                    return null;

                iDict = cache.GetValues(keys);

                return iDict;
            }
            catch
            {
                return null;
            }

        }

        public bool SetJson<T>(string sKey, T obj, double dCacheTT)
        {
            return false;
        }

        public bool GetJsonAsT<T>(string sKey, out T res) where T : class
        {
            res = null;
            return false;
        }

        public List<string> GetKeys()
        {                        
            List<string> keys = new List<string>();
            foreach (var item in cache)
            {                
                keys.Add(item.Key);
            }
            return keys;
        }

        public bool Get<T>(string key, ref T result)
        {
            result = default(T);
            bool res = false;
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    object cacheResult = cache.Get(key);
                    if (cacheResult != null)
                    {
                        result = (T) cacheResult;
                        res = result != null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Get<T> with key: {0}", key), ex);
            }

            return res;
        }

        public bool GetWithVersion<T>(string key, out ulong version, ref T result)
        {            
            bool res = false;
            version = 0;
            try
            {
                res = Get<T>(key, ref result);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetWithVersion<T> with key: {0}", key), ex);
            }

            return res;
        }

        public bool RemoveKey(string sKey)
        {
            bool? result = false;
            result = cache.Remove(sKey) as bool?;
            return result.HasValue && result.Value;
        }

        public bool SetWithVersion<T>(string key, T value, ulong version, uint expirationInSeconds)
        {
            bool bRes = false;
            try
            {
                

                //lock 
                bool createdNew = false;
                var mutexSecurity = CreateMutex();
                using (Mutex mutex = new Mutex(false, string.Concat("Lock", key), out createdNew, mutexSecurity))
                {
                    try
                    {
                        mutex.WaitOne(-1);
                        T getResult = default(T);
                        if (!GetWithVersion<T>(key, out version, ref getResult) || getResult == null)
                        {
                            DateTime dtExpiresAt = DateTime.UtcNow.AddMinutes((double)expirationInSeconds / 60);
                            cache.Set(key, value, dtExpiresAt);
                            return true;                        
                        }
                    }
                    catch
                    {

                    }
                    finally
                    {
                        //unlock
                        mutex.ReleaseMutex();
                    }
                }
                return bRes;
            }
            catch (Exception ex)
            {
                log.Error("SetWithVersion<T>", ex);
                return false;
            }            
        }

        public bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, bool shouldAllowPartialQuery = false)
        {
            bool res = false;
            try
            {
                IDictionary<string, object> getResults = null;
                getResults = GetValues(keys, shouldAllowPartialQuery);
                if (getResults != null && getResults.Count > 0)
                {
                    results = getResults.ToDictionary(x => x.Key, x => (T)x.Value);
                    if (results != null)
                    {
                        if (shouldAllowPartialQuery)
                        {
                            res = results.Count > 0;
                        }
                        else
                        {
                            res = keys.Count == results.Count;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetValues<T> from InMemoryCache while getting the following keys: {0}", string.Join(",", keys)), ex);
            }

            return res;
        }

    }

}
