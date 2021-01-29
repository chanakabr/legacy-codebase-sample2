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
    }

    public class AuthenticationServiceConfiguration : BaseConfig<AuthenticationServiceConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.AuthenticationServiceConfiguration;
        public override string[] TcmPath => new string[] { TcmObjectKeys.MicroservicesClientConfiguration, TcmKey };
        public BaseValue<string> Address = new BaseValue<string>("address", "");
        public BaseValue<string> CertFilePath = new BaseValue<string>("cert_file_path", "");
        public AuthenticationServiceDataOwnershipConfiguration DataOwnershipConfiguration = new AuthenticationServiceDataOwnershipConfiguration();
    }

    public class AuthenticationServiceDataOwnershipConfiguration : BaseConfig<AuthenticationServiceConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.MicroserviceDataOwnershipConfiguration;
        public override string[] TcmPath => new string[] { TcmObjectKeys.MicroservicesClientConfiguration, TcmObjectKeys.AuthenticationServiceConfiguration, TcmKey };
        public BaseValue<bool> UserLoginHistory = new BaseValue<bool>("user_login_history", false);
        public BaseValue<bool> DeviceLoginHistory = new BaseValue<bool>("device_login_history", false);
        public BaseValue<bool> SSOAdapterProfiles = new BaseValue<bool>("sso_adapter_profiles", false);
        public BaseValue<bool> RefreshToken = new BaseValue<bool>("refresh_token", false);
        public BaseValue<bool> KSStatusCheck = new BaseValue<bool>("ks_status_check", false,description:"when set to true will call ks validation in auth ms");
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
            "(.*)(_invalidationKeySecurityPartnerConfig_groupId_)(.*)",
            "(.*)(_InvalidatePartnerRoles)"
        };

        public BaseValue<bool> ShouldProduceInvalidationEventsToKafka = new BaseValue<bool>("should_produce_invalidation_events_to_kafka", false);
        public BaseValue<List<string>> InvalidationEventsMatchRules = new BaseValue<List<string>>("invalidation_events_match_rules", defaultInvalidationEventsMatchRules);
        public BaseValue<string> InvalidationEventsTopic = new BaseValue<string>("invalidation_events_topic", "PHOENIX_CACHE_INVALIDATIONS");
    }
}