using KLogMonitor;
using Newtonsoft.Json;
using RedisManager;
using System;
using System.Reflection;

namespace CachingProvider.LayeredCache
{

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class RedisLayeredCacheConfig : LayeredCacheConfig
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();
        private static RedisCache redisCache;

        [JsonProperty("Database")]
        public string Database { get; set; }

        public RedisLayeredCacheConfig()
            : base() { }

        public RedisLayeredCacheConfig(LayeredCacheType type, uint ttl, string database)
            : base(type, ttl)
        {
            this.Database = database;
        }

        public override ILayeredCacheService GetILayeredCachingService()
        {
            try
            {
                if (redisCache == null)
                {
                    lock (locker)
                    {
                        if (redisCache == null)
                        {
                            redisCache = new RedisCache();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed GetICachingService for RedisLayeredCacheConfig", ex);
            }

            return redisCache;
        }

    }
}
