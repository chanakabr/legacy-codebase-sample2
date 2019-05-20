using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CachingProvider.LayeredCache
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class InMemoryLayeredCacheConfig : LayeredCacheConfig
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());        
        private static object locker = new object();
        private static SingleInMemoryCache cache;
        private const string IN_MEMORY_CACHE_NAME = "LayeredInMemoryCache";

        public InMemoryLayeredCacheConfig()
            : base() { }

        public InMemoryLayeredCacheConfig(LayeredCacheType type, uint ttl)
            : base(type, ttl) { }        

        public override ICachingService GetICachingService()
        {      
            try
            {
                if (cache == null)
                {
                    lock (locker)
                    {
                        if (cache == null)
                        {
                            cache = new SingleInMemoryCache(this.TTL);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed GetICachingService for InMemoryLayeredCacheConfig", ex);
            }

            return cache;
        }

    }
}
