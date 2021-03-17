using EventBus.Abstraction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CachingProvider.LayeredCache
{
    public class CacheInvalidationEvent : ServiceEvent
    {
        public CacheInvalidationEvent()
        {
        }

        public CacheInvalidationEvent(string invalidationKey, string routingKey)
        {
            if (string.IsNullOrEmpty(routingKey))
            {
                throw new ArgumentException("cannot be null and has to be configured using TCM", nameof(routingKey));
            }
            InvalidationKey = invalidationKey;
            EventNameOverride = routingKey;
        }

        [JsonIgnore]
        public string InvalidationKey { get; set; }

        // cache invalidation events send the invalidation key using the kafka message key
        // here we set the event key that will be published as the cache invalidation key
        public override string EventKey => InvalidationKey;
    }
}
