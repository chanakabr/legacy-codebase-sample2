using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Phx.Lib.Couchbase.Core.Abstractions;

namespace Phoenix.AsyncHandler.Couchbase
{
    public class CouchbaseWorker : ICouchbaseWorker
    {
        private const string KRONOS_FEATUERE_TOGGLE_KEY = "Kronos_Feature_Toggle";
        private readonly ICouchbaseClient<IScheduledTasks> _scheduledTasksCouchbaseClient;
        private readonly ObjectCache _cache;

        public CouchbaseWorker(ICouchbaseClient<IScheduledTasks> scheduledTasksCouchbaseClient)
        {
            _cache = MemoryCache.Default;
            _scheduledTasksCouchbaseClient = scheduledTasksCouchbaseClient;
        }

        public Dictionary<string, string> GetKronosFeatureToggel()
        {
            var cacheToggle = _cache.Get(KRONOS_FEATUERE_TOGGLE_KEY);
            if (cacheToggle != null)
            {
                return (Dictionary<string, string>) cacheToggle;
            }
            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(60),

            };

            Dictionary<string, string> toggle =  _scheduledTasksCouchbaseClient.Get<Dictionary<string, string>>(KRONOS_FEATUERE_TOGGLE_KEY);
            if (toggle != null)
            {
                _cache.Set(KRONOS_FEATUERE_TOGGLE_KEY, toggle, cacheItemPolicy);
            }
            return toggle;
        }
    }
}