using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Core.Catalog;
using Core.Catalog.Response;

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
                          cacheObj = HttpContext.Current.Cache.Get(string.Format("{0}_{1}", keyPrefix, cacheKey.ID));
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
            DateTime experationTime = duration > 0 ? DateTime.UtcNow.AddSeconds(duration) : DateTime.UtcNow.AddSeconds(DEFAULT_DURATION);
            foreach (BaseObject obj in objects)
            {
                if (obj != null)
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        HttpContext.Current.Cache.Insert(string.Format("{0}_{1}", keyPrefix, obj.AssetId), obj, null, experationTime, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }
            }
        }

        public BaseResponse GetFailOverResponse(string key)
        {
            BaseResponse response = null;

            object cacheObj;
            cacheLock.EnterReadLock();
            try
            {
                cacheObj = HttpContext.Current.Cache.Get(key);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }

            if (cacheObj != null)
            {
                response = cacheObj as BaseResponse;
            }
            return response;
        }

        public void InsertFailOverResponse(BaseResponse response, string key)
        {
            cacheLock.EnterWriteLock();
            try
            {
                HttpContext.Current.Cache.Insert(key, response);
            }
            finally
            {
                cacheLock.ExitWriteLock();
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