using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class LayeredCacheConfigurationValidation : StringConfigurationValue
    {
        private static JsonSerializerSettings layeredCacheConfigSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None
        };

        public LayeredCacheConfigurationValidation(string key) : base(key)
        {
        }

        internal override bool Validate()
        {
            bool result = base.Validate();

            try
            {
                LayeredCacheTcmConfig layeredCacheTcmConfig = null;
                
                if (this.ObjectValue != null)
                {
                    layeredCacheTcmConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<LayeredCacheTcmConfig>(this.ObjectValue.ToString(), layeredCacheConfigSerializerSettings);
                }
            }

            catch (Exception ex)
            {
                LogError(string.Format("Could not parse layered cache configuration. Error = {0}", ex), ConfigurationValidationErrorLevel.Failure);
                result = false;
            }
            
            return result;
        }
    }

    internal class LayeredCacheTcmConfig
    {
        [JsonProperty("Version")]
        public string Version { get; set; }

        [JsonProperty("GroupCacheSettings")]
        public List<LayeredCacheConfig> GroupCacheSettings { get; set; }

        [JsonProperty("InvalidationKeySettings")]
        public LayeredCacheConfig InvalidationKeySettings { get; set; }

        [JsonProperty("BucketSettings")]
        public List<LayeredCacheBucketSettings> BucketSettings { get; set; }

        [JsonProperty("DefaultSettings")]
        public List<LayeredCacheConfig> DefaultSettings { get; set; }

        [JsonProperty("LayeredCacheSettings")]
        public Dictionary<string, List<LayeredCacheConfig>> LayeredCacheSettings { get; set; }

    }

    internal class LayeredCacheBucketSettings
    {
        [JsonProperty("CacheType")]
        public LayeredCacheType CacheType { get; set; }

        [JsonProperty("Bucket")]
        public string Bucket { get; set; }
    }

    internal class LayeredCacheConfig
    {
        [JsonProperty("Type")]
        public LayeredCacheType Type { get; set; }

        [JsonProperty("TTL")]
        public uint TTL { get; set; }

    }

    internal enum LayeredCacheType
    {
        None = 0,
        InMemoryCache = 1,
        CbCache = 2,
        CbMemCache = 3
    }
}
