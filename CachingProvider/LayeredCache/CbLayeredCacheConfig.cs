using CouchbaseManager;
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
    public class CbLayeredCacheConfig : LayeredCacheConfig
    {

        [JsonProperty("Bucket")]
        public string Bucket { get; set; }

        public CbLayeredCacheConfig()
            : base()
        {
            this.Bucket = string.Empty;
        }

        public CbLayeredCacheConfig(LayeredCacheType type, uint ttl, string bucket)
            : base(type, ttl)
        {
            this.Bucket = bucket;
        }

    }
}
