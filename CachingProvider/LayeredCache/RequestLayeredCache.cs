using System;
using System.Collections.Generic;
using System.Text;

namespace CachingProvider.LayeredCache
{
    public class RequestLayeredCache 
    {
        public Dictionary<string, object> cachedObjects;
        public Dictionary<string, HashSet<string>> invalidationKeysToKeys;

        public RequestLayeredCache()
        {
            cachedObjects = new Dictionary<string, object>();
            invalidationKeysToKeys = new Dictionary<string, HashSet<string>>();
        }
    }
}
