using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
using System.Web;
using CachingProvider;
using Core.Catalog;
using Core.Catalog.Response;
using TVinciShared;

namespace WebAPI.Managers
{
    public class CatalogCacheManager
    {
        private static Cache cache = new Cache();

        private CatalogCacheManager()
        {
        }

        public static Cache Cache { get { return cache; } }
    }

    public class Cache
    {
        private const int DEFAULT_DURATION = 86400;
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private SingleInMemoryCache cache;

        public Cache()
        {
            cache = SingleInMemoryCache.GetInstance(InMemoryCacheType.General, DEFAULT_DURATION);
        }

        public List<BaseObject> GetObjects(List<CacheKey> cacheKeys, string keyPrefix, out List<long> missingIds)
        {
            List<BaseObject> foundObjects = new List<BaseObject>();
            missingIds = new List<long>();

            foreach (CacheKey cacheKey in cacheKeys)
            {
                  long longId = 0;
                  if (!long.TryParse(cacheKey.ID, out longId))
                  {
                      throw new ArgumentException("Id was in unexpected format");
                  }
                  else
                  {
                      object cacheObj;
                      cacheLock.EnterReadLock();
                      try
                      {
                          cacheObj = cache.Get<BaseObject>(string.Format("{0}_{1}", keyPrefix, cacheKey.ID));
                      }
                      finally
                      {
                          cacheLock.ExitReadLock();
                      }
                      if (cacheObj != null)
                      {
                          // cacheObj Ticks > cacheKey Ticks when request fails '(cacheKey.UpdateDate).Ticks' = 0
                          // (cacheObj as BaseObject).m_dUpdateDate.Ticks == (cacheKey.UpdateDate).Ticks when media didn't change
                          if ((cacheObj as BaseObject).m_dUpdateDate.Ticks >= (cacheKey.UpdateDate).Ticks)
                          {
                              BaseObject baseObj = cacheObj as BaseObject;
                              foundObjects.Add(baseObj);
                          }
                          else // cache miss
                          {
                              missingIds.Add(longId);
                          }
                      }
                      else // cache miss
                      {
                          missingIds.Add(longId);
                      }
                  }
            }
            return foundObjects;
        }

        public void StoreObjects(List<BaseObject> objects, string keyPrefix, int duration)
        {
            var expirationTime = duration > 0 ? DateTimeOffset.UtcNow.AddSeconds(duration) : DateTimeOffset.UtcNow.AddSeconds(DEFAULT_DURATION);
            foreach (BaseObject obj in objects)
            {
                if (obj != null)
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        var key = string.Format("{0}_{1}", keyPrefix, obj.AssetId);
                        cache.Set(key, obj, expirationTime);
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }
            }
        }
    }

    public class CacheKey
    {
        public string ID { get; set; }
        public DateTime UpdateDate { get; set; }

        public CacheKey()
        {
        }

        public CacheKey(string id, DateTime updateDate)
        {
            ID = id;
            UpdateDate = updateDate;
        }
    }
}