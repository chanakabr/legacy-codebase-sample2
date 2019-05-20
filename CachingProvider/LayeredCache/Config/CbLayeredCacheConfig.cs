using CouchbaseManager;
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
    public class CbLayeredCacheConfig : LayeredCacheConfig
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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

        public override ICachingService GetICachingService()
        {
            ICachingService cache = null;
            try
            {
                cache =  CouchBaseCache<object>.GetInstance(string.IsNullOrEmpty(this.Bucket) ? LayeredCache.GetBucketFromLayeredCacheConfig(this.Type) : this.Bucket);
            }
            catch (Exception ex)
            {
                log.Error("Failed GetICachingService for CbLayeredCacheConfig", ex);
            }

            return cache;
        }

    }
}
