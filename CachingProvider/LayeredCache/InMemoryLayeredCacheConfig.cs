using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingProvider.LayeredCache
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class InMemoryLayeredCacheConfig : LayeredCacheConfig
    {

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

    }
}
