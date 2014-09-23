using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase;
using CouchbaseManager;
using Enyim.Caching.Memcached;
using Logger;


namespace CachingProvider
{
    public class CouchBaseCache<T> : OutOfProcessCache
    {
         #region C'tor
        CouchbaseClient m_Client;
            
        private CouchBaseCache(eCouchbaseBucket eCacheName)
        {
            m_Client = CouchbaseManager.CouchbaseManager.GetInstance(eCacheName);

            if (m_Client == null)
                throw new Exception("Unable to create out of process cache instance");
        }
        #endregion

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
                    Logger.Logger.Log("Error", string.Format("Unable to create OOP cache. Please check that cache of type {0} exists.", sCacheName), "CachingProvider");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("Unable to create OOP cache. Ex={0};\nCall stack={1}", ex.Message, ex.StackTrace), "CachingProvider");
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
            baseModule.result = m_Client.Get(sKey);
            return baseModule;
        }
        
        public override T Get<T>(string sKey)
        {
            return m_Client.Get<T>(sKey);         
        }
        
        public override BaseModuleCache Remove(string sKey)
        {
            BaseModuleCache baseModule = new BaseModuleCache();
            baseModule.result =  m_Client.Remove(sKey);
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
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("Get with CasResult: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return baseModule; 
        }

        public override bool AddWithVersion(string sKey, BaseModuleCache oValue, double nMinuteOffset)
        {
            bool bRes = false;
            try
            {
                VersionModuleCache baseModule = (VersionModuleCache)oValue;
                ulong cas = 0;
                bool bCas = ulong.TryParse(baseModule.version, out cas);
                
                if (!bCas)
                {
                    return false;
                }
                
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
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("AddWithVersion: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
                return false;
            }
        }
        public override bool AddWithVersion(string sKey, BaseModuleCache oValue)
        {
            bool bRes = false;
            try
            {
                VersionModuleCache baseModule = (VersionModuleCache)oValue;
                ulong cas = 0;
                bool bCas = ulong.TryParse(baseModule.version, out cas);
                if (!bCas)
                {
                    return false;
                }

                CasResult<bool> casRes = m_Client.Cas(Enyim.Caching.Memcached.StoreMode.Add, sKey, baseModule.result, cas);

                if (casRes.StatusCode == 0)
                {
                    bRes = casRes.Result;
                }
                return bRes;
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("AddWithVersion: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
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

                if (!bCas)
                {
                    return false;
                }

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
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("AddWithVersion: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
                return false;
            }
        }
        public override bool SetWithVersion(string sKey, BaseModuleCache oValue)
        {
            bool bRes = false;
            try
            {
                VersionModuleCache baseModule = (VersionModuleCache)oValue;
                ulong cas = 0;
                bool bCas = ulong.TryParse(baseModule.version, out cas);
                if (!bCas)
                {
                    return false;
                }

                CasResult<bool> casRes = m_Client.Cas(Enyim.Caching.Memcached.StoreMode.Set, sKey, baseModule.result, cas);

                if (casRes.StatusCode == 0)
                {
                    bRes = casRes.Result;
                }
                return bRes;
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("AddWithVersion: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
                return false;
            }
        }

      
      
    }
}
