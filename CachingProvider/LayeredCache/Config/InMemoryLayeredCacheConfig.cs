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
        private const string IN_MEMORY_CACHE_NAME = "LayeredInMemoryCache";

        [JsonProperty("CacheName")]
        public string CacheName { get; set; }

        public InMemoryLayeredCacheConfig()
            : base()
        {
            this.CacheName = string.Empty; ;
        }

        public InMemoryLayeredCacheConfig(LayeredCacheType type, uint ttl, string cacheName)
            : base(type, ttl)
        {
            this.CacheName = cacheName;
        }

        public override ICachingService GetICachingService()
        {
            ICachingService cache = null;
            try
            {
                cache = new SingleInMemoryCache(this.TTL);
            }
            catch (Exception ex)
            {
                log.Error("Failed GetICachingService for InMemoryLayeredCacheConfig", ex);
            }

            return cache;
        }

    }
}
