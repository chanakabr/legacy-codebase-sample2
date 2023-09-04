using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Caching;
using CouchbaseManager;
using Phx.Lib.Log;

// using Phx.Lib.Couchbase.Core.Abstractions;

namespace Phoenix.AsyncHandler.Couchbase
{
    public class CouchbaseWorker : ICouchbaseWorker
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string KRONOS_FEATUERE_TOGGLE_KEY = "Kronos_Feature_Toggle";
        private const string KRONOS_KEY = "kronos";

        // private readonly ICouchbaseClient<IScheduledTasks> _scheduledTasksCouchbaseClient;
        private readonly ObjectCache _cache = MemoryCache.Default;
        private readonly CouchbaseManager.CouchbaseManager _couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.SCHEDULED_TASKS);
        
        //ICouchbaseClient<IScheduledTasks> scheduledTasksCouchbaseClient
        // _scheduledTasksCouchbaseClient = scheduledTasksCouchbaseClient;

        public Dictionary<string, string> GetKronosFeatureToggel()
        {
            var cacheToggle = _cache.Get(KRONOS_FEATUERE_TOGGLE_KEY);
            if (cacheToggle != null)
            {
                return (Dictionary<string, string>)cacheToggle;
            }

            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(60),
            };

            // Dictionary<string, string> toggle = null;

            //pending CB library fix
            //_scheduledTasksCouchbaseClient.Get<Dictionary<string, string>>(KRONOS_FEATUERE_TOGGLE_KEY);

            // if (toggle == null)
            // {
            var toggle = _couchbaseManager.Get<Dictionary<string, string>>(KRONOS_FEATUERE_TOGGLE_KEY);
            // }

            if (toggle != null)
                _cache.Set(KRONOS_FEATUERE_TOGGLE_KEY, toggle, cacheItemPolicy);
            else
                log.Debug($"Toggle document: {KRONOS_FEATUERE_TOGGLE_KEY} wasn't found in CB, will use RT");

            return toggle;
        }

        public string GetKronosKeyName()
        {
            return KRONOS_KEY;
        }
    }
}