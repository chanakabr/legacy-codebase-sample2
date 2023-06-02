using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CachingProvider.LayeredCache
{
    public class RequestLayeredCache 
    {
        public ConcurrentDictionary<string, object> cachedObjects;
        public ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> invalidationKeysToKeys;
        public ConcurrentDictionary<string, long> invalidationKeysValues;

        public RequestLayeredCache()
        {
            cachedObjects = new ConcurrentDictionary<string, object>();
            invalidationKeysToKeys = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>();
            invalidationKeysValues = new ConcurrentDictionary<string, long>();
        }
    }
}
