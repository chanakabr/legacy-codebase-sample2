using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading;
using Core.Catalog.Response;
using Core.Catalog;

namespace Tvinci.Data.Loaders
{
    public class Cache
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        public List<BaseObject> GetObjects(List<CacheKey> cacheKeys, string keyPrefix, out List<long> missingIds)
        {
            List<BaseObject> lObj = new List<BaseObject>();
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
                        cacheObj = CachingManager.CachingManager.GetCachedData(string.Format("{0}_{1}", keyPrefix, cacheKey.ID));
                    }
                    finally
                    {
                        cacheLock.ExitReadLock();
                    }
                    if (cacheObj != null && cacheObj is BaseObject)
                    {
                        ///
                        /// cacheObj Ticks > cacheKey Ticks when request fails '(cacheKey.UpdateDate).Ticks' = 0
                        /// (cacheObj as BaseObject).m_dUpdateDate.Ticks == (cacheKey.UpdateDate).Ticks when media didn't change
                        ///
                        if ((cacheObj as BaseObject).m_dUpdateDate.Ticks >= (cacheKey.UpdateDate).Ticks)
                        {
                            BaseObject baseObj = cacheObj as BaseObject;
                            lObj.Add(baseObj);
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
            return lObj;
        }
        public void StoreObjects(List<BaseObject> objects, string keyPrefix, int duration)
        {
            DateTime experationTime = duration > 0 ? DateTime.Now.AddMinutes(duration) : DateTime.MaxValue;
            foreach (BaseObject obj in objects)
            {
                if (obj != null)
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        CachingManager.CachingManager.SetCachedData(string.Format("{0}_{1}", keyPrefix, obj.AssetId), obj, duration * 60, System.Runtime.Caching.CacheItemPriority.Default, 0, false);
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }
            }
        }
    }
}