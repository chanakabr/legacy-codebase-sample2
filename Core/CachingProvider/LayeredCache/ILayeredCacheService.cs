using System.Collections.Generic;

namespace CachingProvider.LayeredCache
{
    public interface ILayeredCacheService
    {
        GetOperationStatus Get<T>(string key, ref T result, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings);

        bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = null, bool shouldAllowPartialQuery = false);

        bool Set<T>(string key, T value, uint ttlInSeconds, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = null);

    }

    public enum GetOperationStatus
    {
        Success = 1,
        NotFound = 2,
        Error = 3
    }
}
