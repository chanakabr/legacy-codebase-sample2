using System;
using System.Reflection;
using CouchbaseManager;
using Phx.Lib.Log;
using Newtonsoft.Json;

namespace CachingProvider.LayeredCache
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class CompressionCbLayeredCacheConfig : LayeredCacheConfig
    {
        private static readonly KLogger Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [JsonProperty("Bucket")] 
        public string Bucket { get; set; }

        public CompressionCbLayeredCacheConfig()
        {
            this.Bucket = string.Empty;
        }

        public override ILayeredCacheService GetILayeredCachingService()
        {
            ILayeredCacheService cache = null;
            try
            {
                var bucket = string.IsNullOrEmpty(this.Bucket) ? LayeredCache.GetBucketFromLayeredCacheConfig(this.Type) : this.Bucket;
                if (!Enum.TryParse<eCouchbaseBucket>(bucket.ToUpper(), out var eCacheName))
                {
                    Log.Error($"Error - Unable to create OOP cache. Please check that cache of type {bucket} exists.");
                }

                cache = new CompressionCouchbaseCache(eCacheName);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed {nameof(GetILayeredCachingService)} for {nameof(CompressionCbLayeredCacheConfig)}", ex);
            }

            return cache;
        }
    }
}