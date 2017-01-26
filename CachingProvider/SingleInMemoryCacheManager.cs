using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingProvider
{
    public class SingleInMemoryCacheManager
    {
        private static ConcurrentDictionary<string, SingleInMemoryCache> caches;
        private static object locker = new object();
        private static object cacheLocker = new object();

        private SingleInMemoryCacheManager()
        {
        }

        public static SingleInMemoryCache Instance(string name, uint expirationInSeconds)
        {
            SingleInMemoryCache inMemoryCache;
            if (caches == null)
            {
                lock (locker)
                {
                    if (caches == null)
                    {
                        caches = new ConcurrentDictionary<string, SingleInMemoryCache>();
                    }
                }
            }

            if (!caches.TryGetValue(name, out inMemoryCache))
            {
                lock (cacheLocker)
                {
                    if (!caches.TryGetValue(name, out inMemoryCache))
                    {
                        inMemoryCache = new SingleInMemoryCache(name, Math.Ceiling((double)expirationInSeconds / 60));
                        caches.TryAdd(name, inMemoryCache);
                    }
                }
            }

            return inMemoryCache;
        }
    }
}
