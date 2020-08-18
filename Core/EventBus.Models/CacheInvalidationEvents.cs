using EventBus.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;
using Caching.InvalidationKeys;

namespace EventBus.Models
{
    public abstract class CacheInvalidationEvent : ServiceEvent
    {
        public abstract string InvalidaionKey { get; }

        // cache invalidation events send the invalidation key using the kafka message key
        // here we set the event key that will be published as the cache invalidation key
        public override string EventKey => InvalidaionKey;

        // cache invalidation events have no body
        public override string Serialize() => "";
    }

    [ServiceEventRoutingKeyOverride("OTTUser")]
    public class OTTUserInvalidationEvent : CacheInvalidationEvent
    {
        public new long UserId { get; set; }
        public override string InvalidaionKey => InvalidationKeys.GetOTTUserInvalidationKey(UserId);
        public OTTUserInvalidationEvent(long userId)
        {
            UserId = userId;
        }
    }

    [ServiceEventRoutingKeyOverride("PartnerConfiguration")]
    public class PartnerConfigurationInvalidationEvent : CacheInvalidationEvent
    {
        public long PartnerId { get; set; }
        public override string InvalidaionKey => InvalidationKeys.GetPartnerConfigurationInvalidationKey(PartnerId);

        public PartnerConfigurationInvalidationEvent(long partnerId)
        {
            PartnerId = partnerId;
        }
    }


    [ServiceEventRoutingKeyOverride("Roles")]
    public class RoleInvalidationEvent : CacheInvalidationEvent
    {
        public long PartnerId { get; set; }
        public override string InvalidaionKey => InvalidationKeys.GetPartnerRolesInvalidationKey(PartnerId);

        public RoleInvalidationEvent(long partnerId)
        {
            PartnerId = partnerId;
        }

    }

    [ServiceEventRoutingKeyOverride("Roles")]
    public class UserRoleInvalidationEvent : CacheInvalidationEvent
    {
        public new long UserId { get; set; }
        public override string InvalidaionKey => InvalidationKeys.GetUserRolesInvalidationKey(UserId);
        public UserRoleInvalidationEvent(long userId)
        {
            UserId = userId;
        }

    }

}
