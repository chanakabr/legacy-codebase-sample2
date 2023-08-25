using System.Collections.Generic;
using System.Reflection;
using CachingProvider.LayeredCache;
using CouchbaseManager;
using Phx.Lib.Log;
using Newtonsoft.Json;

namespace CachingProvider
{
    public class CompressionCouchbaseCache : ILayeredCacheService
    {
        private static readonly KLogger Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly eCouchbaseBucket _bucket;
        private readonly ICompressionCouchbaseManager _compressionCouchbaseManager;

        public CompressionCouchbaseCache(eCouchbaseBucket eCacheName)
        {
            _bucket = eCacheName;
            _compressionCouchbaseManager = new CompressionCouchbaseManager(new CouchbaseManager.CouchbaseManager(_bucket));
        }

        public GetOperationStatus Get<T>(string key, ref T result, JsonSerializerSettings jsonSerializerSettings)
        {
            result = _compressionCouchbaseManager.Get<T>(key, out var status, jsonSerializerSettings);
            return status.ToGetOperationStatus();
        }

        public bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, JsonSerializerSettings jsonSerializerSettings = null, bool shouldAllowPartialQuery = false)
        {
            var values = _compressionCouchbaseManager.GetValues<T>(keys, jsonSerializerSettings, shouldAllowPartialQuery);
            if (values == null)
            {
                return false;
            }
            
            results = values;
            return true;
        }

        public bool Set<T>(string key, T value, uint ttlInSeconds, JsonSerializerSettings jsonSerializerSettings = null)
        {
            return _compressionCouchbaseManager.Set(key, value, ttlInSeconds);
        }
    }
}