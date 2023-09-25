using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CachingProvider.LayeredCache
{
    public class RequestLayeredCache 
    {
        public ConcurrentDictionary<string, object> cachedObjects;
        //it is in use to provide invalidation keys for GRPC calls - it keeps the invalidation keys for the request
        public HashSet<string> InvalidationKeysRequested;
        public ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> invalidationKeysToKeys;
        public ConcurrentDictionary<string, long> invalidationKeysValues;

        public RequestLayeredCache()
        {
            cachedObjects = new ConcurrentDictionary<string, object>();
            invalidationKeysToKeys = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>();
            invalidationKeysValues = new ConcurrentDictionary<string, long>();
            InvalidationKeysRequested = new HashSet<string>();
        }
    }
}
