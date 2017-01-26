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
    public class LayeredCacheBucketSettings
    {
        [JsonProperty("CacheType")]
        public LayeredCacheType CacheType { get; set; }

        [JsonProperty("Bucket")]
        public eCouchbaseBucket Bucket { get; set; }

        public LayeredCacheBucketSettings()
        {
            CacheType = LayeredCacheType.None;
            Bucket = eCouchbaseBucket.DEFAULT;
        }
    }
}
