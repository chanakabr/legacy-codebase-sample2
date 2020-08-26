using EventBus.Models;
using NUnit.Framework;
using System;

namespace CachingProvider.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [TestCase("invalidationKey_general_partner_config_1234", typeof(PartnerConfigurationInvalidationEvent))]
        [TestCase("invalidationKey_user_1234", typeof(OTTUserInvalidationEvent))]
        public void ShouldParseInvalidationKey(string legacyKey, Type invalidationEventType)
        {
            var invalidationEvent = LayeredCache.InvalidationKeysLegacyMapping.GetInvalidationEventKeyByLegacyKey(legacyKey);
            Assert.That(invalidationEvent.GetType() == invalidationEventType,
                $"expected type [{invalidationEventType}] for key:[{legacyKey}] but got [{invalidationEvent.GetType()}]");

        }
    }
}