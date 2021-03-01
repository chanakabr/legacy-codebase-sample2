using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System.Collections.Generic;

namespace ConfigurationManager.Types
{
    public class MicroservicesClientConfiguration : BaseConfig<MicroservicesClientConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.MicroservicesClientConfiguration;
        public override string[] TcmPath => new string[] { TcmKey };
        public AuthenticationServiceConfiguration Authentication = new AuthenticationServiceConfiguration();
        public MicroServicesLayeredCacheConfiguration LayeredCacheConfiguration = new MicroServicesLayeredCacheConfiguration();
        public BaseValue<bool> ShouldAllowCanaryDeploymentConfiguration = new BaseValue<bool>("should_allow_canary_deployment_configuration", false, false, "configures if canary deployment configuration per group is looked at");
    }

    public class AuthenticationServiceConfiguration : BaseConfig<AuthenticationServiceConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.AuthenticationServiceConfiguration;
        public override string[] TcmPath => new string[] { TcmObjectKeys.MicroservicesClientConfiguration, TcmKey };
        public BaseValue<string> Address = new BaseValue<string>("address", "");
        public BaseValue<string> CertFilePath = new BaseValue<string>("cert_file_path", "");
    }

    public class MicroServicesLayeredCacheConfiguration : BaseConfig<MicroServicesLayeredCacheConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.MicroserviceLayeredCacheConfiguration;
        public override string[] TcmPath => new string[] { TcmObjectKeys.MicroservicesClientConfiguration, TcmKey };

        private static readonly List<string> defaultInvalidationEventsMatchRules = new List<string>()
        {
            "(.*)(_InvalidateOTTUser_)(.*)",
            "(.*)(_InvalidateUserRoles_)(.*)",
            "(.*)(_InvalidateUserAndHouseholdSegments_)(.*)",
            "(.*)(_InvalidatePartnerSecurityConfiguration)",
            "(.*)(_InvalidatePartnerRoles)",
            "(.*)(_InvalidateHouseholdDevice_)(.*)"
        };

        public BaseValue<bool> ShouldProduceInvalidationEventsToKafka = new BaseValue<bool>("should_produce_invalidation_events_to_kafka", false);
        public BaseValue<List<string>> InvalidationEventsMatchRules = new BaseValue<List<string>>("invalidation_events_match_rules", defaultInvalidationEventsMatchRules);
        public BaseValue<string> InvalidationEventsTopic = new BaseValue<string>("invalidation_events_topic", "PHOENIX_CACHE_INVALIDATIONS");
    }
}