using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CachingProvider.LayeredCache
{
    internal class InternalLayeredCacheSettings
    {
        internal static readonly IReadOnlyDictionary<string, List<LayeredCacheConfig>> CacheSettings = new ReadOnlyDictionary<string, List<LayeredCacheConfig>>(
            new Dictionary<string, List<LayeredCacheConfig>>
            {
                {LayeredCacheConfigNames.GET_BULK_UPLOADS_FROM_CACHE, new List<LayeredCacheConfig>
                {
                    new InMemoryLayeredCacheConfig
                    {
                        Type = LayeredCacheType.InMemoryCache,
                        TTL = 3600
                    },
                    new CompressionCbLayeredCacheConfig
                    {
                        Bucket = "Cache",
                        Type = LayeredCacheType.CbMemCache,
                        TTL = 86400
                    }
                }}
            });
    }
}