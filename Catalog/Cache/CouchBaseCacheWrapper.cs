using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Cache;
using Couchbase;
using CouchbaseManager;
using Couchbase.Extensions;
using Enyim.Caching;
using Logger;
using Enyim.Caching.Memcached;

namespace Catalog
{
    public class CouchBaseCacheWrapper<T> : ICache<T>
    {
        private static readonly double EXPIRY_DATE = (Utils.GetDoubleValFromConfig("cache_doc_expiry") > 0) ? Utils.GetDoubleValFromConfig("cache_doc_expiry") : 7;

        CouchbaseClient m_oClient;

        public CouchBaseCacheWrapper()
        {
        }

        public void Init()
        {
            this.m_oClient = CouchbaseManager.CouchbaseManager.GetInstance(eCouchbaseBucket.CACHE);
            if (this.m_oClient == null)
                throw new NullReferenceException("can't initialize couchBase cache");
        }

        #region implamentation CouchBase
        public CasResult<T> GetWithCas(string sID)
        {
            CasResult<T> oRes = default(CasResult<T>);

            try
            {
                oRes = m_oClient.GetWithCas<T>(sID);
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("Get with CasResult: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return oRes;
        }
        #endregion

        #region implamentation Interface
        public T Get(string sID)
        {
            T oRes = default(T);
            try
            {
                oRes = m_oClient.Get<T>(sID);               
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("Get: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return oRes;
        }
                
        public bool Insert(string sID, T oCache, DateTime? dtExpiresAt)
        {
            bool bRes = false;

            if (oCache != null)
            {
                try
                {
                    bRes = (dtExpiresAt.HasValue) ? m_oClient.Store(Enyim.Caching.Memcached.StoreMode.Add, sID, oCache, dtExpiresAt.Value) :
                                                   m_oClient.Store(Enyim.Caching.Memcached.StoreMode.Add, sID, oCache);
                }
                catch (Exception ex)
                {
                    Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                    log.Message = string.Format("Insert: ex={0} in {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                }
            }

            return bRes;
        }
        public bool Insert(string sID, T oCache, DateTime? dtExpiresAt, ulong cas)
        {
            bool bRes = false;

            if (oCache != null)
            {
                try
                {
                    CasResult<bool> casRes = (dtExpiresAt.HasValue) ? m_oClient.Cas(Enyim.Caching.Memcached.StoreMode.Add, sID, oCache, dtExpiresAt.Value, cas) :
                                                   m_oClient.Cas(Enyim.Caching.Memcached.StoreMode.Add, sID, oCache, cas);
                    if (casRes.StatusCode == 0 && casRes.Result != null)
                    {
                        bRes = casRes.Result;
                    }

                }
                catch (Exception ex)
                {
                    Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                    log.Message = string.Format("Insert with cas: ex={0} in {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                }
            }

            return bRes;
        }

        public bool Update(string sID, T oCache, DateTime? dtExpiresAt)
        {
            bool bRes = false;

            if (oCache != null)
            {
                try
                {
                    bRes = (dtExpiresAt.HasValue) ? m_oClient.Store(Enyim.Caching.Memcached.StoreMode.Set, sID, oCache, dtExpiresAt.Value) :
                                                    m_oClient.Store(Enyim.Caching.Memcached.StoreMode.Set, sID, oCache);
                }
                catch (Exception ex)
                {
                    Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                    log.Message = string.Format("Update: ex={0} in {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                }
            }

            return bRes;
        }
        public bool Update(string sID, T oCache, DateTime? dtExpiresAt, ulong cas)
        {
            bool bRes = false;

            if (oCache != null)
            {
                try
                {
                    CasResult<bool> casRes = (dtExpiresAt.HasValue) ? m_oClient.Cas(Enyim.Caching.Memcached.StoreMode.Set, sID, oCache, dtExpiresAt.Value, cas) :
                                                    m_oClient.Cas(Enyim.Caching.Memcached.StoreMode.Set, sID, oCache, cas);
                    if (casRes.StatusCode == 0 && casRes.Result != null)
                    {
                        bRes = casRes.Result;
                    }
                }
                catch (Exception ex)
                {
                    Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                    log.Message = string.Format("Update with cas: ex={0} in {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                }
            }

            return bRes;
        }

        public bool Delete(string sID)
        {
            bool bRes = false;
            try
            {
                bRes = m_oClient.Remove(sID);
            }
            catch (Exception ex)
            {
                Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true);
                log.Message = string.Format("Delete: ex={0} in {1}", ex.Message, ex.StackTrace);
                log.Error(log.Message, false);
            }

            return bRes;
        }
        #endregion      
    }
}
