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
    public abstract class LayeredCacheConfig
    {

        [JsonProperty("Type")]
        public LayeredCacheType Type { get; set; }

        [JsonProperty("TTL")]
        public uint TTL { get; set; }

        public LayeredCacheConfig()
        {
            this.Type = LayeredCacheType.None;
            this.TTL = 0;                       
        }

        public LayeredCacheConfig(LayeredCacheType type, uint ttl)
        {
            this.Type = type;
            this.TTL = ttl;
        }

    }
}
