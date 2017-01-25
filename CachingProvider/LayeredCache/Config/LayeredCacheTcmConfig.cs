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
    public class LayeredCacheTcmConfig
    {

        [JsonProperty("Version")]
        public string Version { get; set; }

        [JsonProperty("InvalidationKeySettings")]
        public LayeredCacheConfig InvalidationKeySettings { get; set; }

        [JsonProperty("BucketSettings")]
        public List<LayeredCacheBucketSettings> BucketSettings { get; set; }

        [JsonProperty("DefaultSettings")]
        public List<LayeredCacheConfig> DefaultSettings { get; set; }

        [JsonProperty("LayeredCacheSettings")]
        public Dictionary<string, List<LayeredCacheConfig>> LayeredCacheSettings { get; set; }

        public LayeredCacheTcmConfig()
        {
            Version = string.Empty;
            InvalidationKeySettings = null;
            BucketSettings = new List<LayeredCacheBucketSettings>();
            DefaultSettings = new List<LayeredCacheConfig>();
            LayeredCacheSettings = new Dictionary<string,List<LayeredCacheConfig>>();
        }

    }
}
