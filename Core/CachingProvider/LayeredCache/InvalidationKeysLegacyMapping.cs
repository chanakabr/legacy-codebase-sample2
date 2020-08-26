using EventBus.Abstraction;
using EventBus.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CachingProvider.LayeredCache
{
    public static class InvalidationKeysLegacyMapping
    {
        private delegate CacheInvalidationEvent InvalidationEventFactory(string keyParams);

        private static readonly Regex _InvalidationKeySplitRegex = new Regex(@"(?<baseKey>.*?)(?<keyParams>_\d.*)", RegexOptions.Compiled);
        private static readonly Dictionary<string, InvalidationEventFactory> _BaseKeyToInvalidationEvent = new Dictionary<string, InvalidationEventFactory>();

        static InvalidationKeysLegacyMapping()
        {
            _BaseKeyToInvalidationEvent["invalidationKey_user"] = CreateOTTUserInvalidationEvent;
            _BaseKeyToInvalidationEvent["invalidationKey_general_partner_config"] = CreatePartnerConfigurationInvalidationEvent;
            _BaseKeyToInvalidationEvent["invalidationKey_permissionRoleIds_groupId"] = CreateRolesInvalidationEvent;
            _BaseKeyToInvalidationEvent["add_role_userId"] = CreateUserRolesInvalidationEvent;

            // todo: add all the others ...
        }


        public static CacheInvalidationEvent GetInvalidationEventKeyByLegacyKey(string legacyKey)
        {
            var match = _InvalidationKeySplitRegex.Match(legacyKey);
            var baseKey = match.Groups["baseKey"].Value;
            var keyParams = match.Groups["keyParams"].Value;

            if (_BaseKeyToInvalidationEvent.TryGetValue(baseKey, out var invalidationEventFactory))
            {
                var invalidationEvent = invalidationEventFactory(keyParams);
                return invalidationEvent;
            }

            return null;
        }

        private static CacheInvalidationEvent CreateUserRolesInvalidationEvent(string keyParams)
        {
            var userIdStr = keyParams.Split('_')[1];
            var userId = long.Parse(userIdStr);
            return new UserRoleInvalidationEvent(userId);
        }

        private static CacheInvalidationEvent CreateRolesInvalidationEvent(string keyParams)
        {
            var partnerIdStr = keyParams.Split('_')[1];
            var partnerId = long.Parse(partnerIdStr);
            return new RoleInvalidationEvent(partnerId);
        }



        private static CacheInvalidationEvent CreatePartnerConfigurationInvalidationEvent(string keyParams)
        {
            var partnerIdStr = keyParams.Split('_')[1];
            var partnerId = long.Parse(partnerIdStr);
            return new PartnerConfigurationInvalidationEvent(partnerId);

        }

        private static CacheInvalidationEvent CreateOTTUserInvalidationEvent(string keyParams)
        {
            var userIdStr = keyParams.Split('_')[1];
            var userId = long.Parse(userIdStr);
            return new OTTUserInvalidationEvent(userId);
        }



    }
}
