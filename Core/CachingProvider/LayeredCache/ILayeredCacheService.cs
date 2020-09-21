using System.Collections.Generic;

namespace CachingProvider.LayeredCache
{
    public interface ILayeredCacheService
    {
        bool Get<T>(string key, ref T result, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = null);

        bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = null, bool shouldAllowPartialQuery = false);

        bool Set<T>(string key, T value, uint ttlInSeconds, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = null);

    }
}
