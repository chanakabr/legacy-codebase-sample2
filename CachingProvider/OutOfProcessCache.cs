using Couchbase;
using CouchbaseManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachingProvider
{
    public class OutOfProcessCache : ICachingService
    {
        #region C'tor
        CouchbaseClient m_Client;

        private OutOfProcessCache(eCouchbaseBucket eCacheName)
        {
            m_Client = CouchbaseManager.CouchbaseManager.GetInstance(eCacheName);

            if (m_Client == null)
                throw new Exception("Unable to create out of process cache instance");
        }
        #endregion

        public bool Add(string sKey, object oValue, double nMinuteOffset)
        {
            return m_Client.Store(Enyim.Caching.Memcached.StoreMode.Add, sKey, oValue, DateTime.UtcNow.AddMinutes(nMinuteOffset));
        }
        public bool Set(string sKey, object oValue, double nMinuteOffset)
        {
            return m_Client.Store(Enyim.Caching.Memcached.StoreMode.Set, sKey, oValue, DateTime.UtcNow.AddMinutes(nMinuteOffset));
        }

        public bool Add(string sKey, object oValue)
        {
            return m_Client.Store(Enyim.Caching.Memcached.StoreMode.Add, sKey, oValue);
        }
        public bool Set(string sKey, object oValue)
        {
            return m_Client.Store(Enyim.Caching.Memcached.StoreMode.Set, sKey, oValue);
        }

        public object Get(string sKey)
        {
            return m_Client.Get(sKey);
        }
        public T Get<T>(string sKey) where T : class
        {
            return m_Client.Get<T>(sKey);
        }
        public object Remove(string sKey)
        {
            return m_Client.Remove(sKey);
        }

        public static OutOfProcessCache GetInstance(string sCacheName)
        {
            OutOfProcessCache cache = null;
            try
            {
                eCouchbaseBucket eCacheName;
                if (Enum.TryParse<eCouchbaseBucket>(sCacheName.ToUpper(), out eCacheName))
                {
                    cache = new OutOfProcessCache(eCacheName);
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

    }
}
