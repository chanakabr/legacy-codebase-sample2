using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;

namespace CachingProvider
{
    public class InMemoryCache : ICachingService
    {
        private static readonly Dictionary<string, InMemoryCache> m_CacheInstances = new Dictionary<string, InMemoryCache>();
        private static ReaderWriterLockSlim m_CacheLocker = new ReaderWriterLockSlim();

        private ObjectCache m_Cache;

        #region C'tor
        private InMemoryCache(string sCacheName)
        {
            m_Cache = new MemoryCache(sCacheName);
        }
        #endregion


        public bool Add(string sKey, object oValue, double nMinuteOffset)
        {
            return m_Cache.Add(sKey, oValue, DateTimeOffset.Now.AddMinutes(nMinuteOffset));
        }

        public bool Add(string sKey, object oValue)
        {
            return m_Cache.Add(sKey, oValue, ObjectCache.InfiniteAbsoluteExpiration);
        }

        public object Remove(string sKey)
        {
            object oRes = null;

            if (m_Cache.Contains(sKey))
            {
                oRes = m_Cache.Remove(sKey);
            }

            return oRes;
        }

        public bool Set(string sKey, object oValue, double nMinuteOffset)
        {
            bool bRes = true;
            try
            {
                m_Cache.Set(sKey, oValue, DateTimeOffset.Now.AddMinutes(nMinuteOffset));
            }
            catch (Exception ex)
            {
                bRes = false;
            }

            return bRes;
        }

        public bool Set(string sKey, object oValue)
        {
            bool bRes = true;
            try
            {
                m_Cache.Set(sKey, oValue, ObjectCache.InfiniteAbsoluteExpiration);
            }
            catch (Exception ex)
            {
                bRes = false;
            }

            return bRes;
        }

        public object Get(string sKey)
        {
            return m_Cache.Get(sKey);
        }

        public T Get<T>(string sKey) where T : class
        {
            return m_Cache.Get(sKey) as T;
        }

        public static InMemoryCache GetInstance(string sCacheName)
        {
            InMemoryCache tempCache = null;

            if (string.IsNullOrEmpty(sCacheName))
                return tempCache;

            if (!m_CacheInstances.ContainsKey(sCacheName))
            {
                if (m_CacheLocker.TryEnterWriteLock(1000))
                {
                    try
                    {
                        if (!m_CacheInstances.ContainsKey(sCacheName))
                        {
                            InMemoryCache cache = new InMemoryCache(sCacheName);
                            if (cache != null)
                            {
                                m_CacheInstances.Add(sCacheName, cache);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        m_CacheLocker.ExitWriteLock();
                    }
                }
            }

            // If item already exist
            if (m_CacheLocker.TryEnterReadLock(1000))
            {
                try
                {
                    m_CacheInstances.TryGetValue(sCacheName, out tempCache);
                }
                catch (Exception ex)
                {
                    //logger.Error("GetSiteMapInstance->", ex);
                }
                finally
                {
                    m_CacheLocker.ExitReadLock();
                }
            }

            return tempCache;
        }

    }
}
