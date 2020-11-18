using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager.Types
{
    public class MicroservicesClientConfiguration : BaseConfig<MicroservicesClientConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.MicroservicesClientConfiguration;
        public override string[] TcmPath => new string[] { TcmKey };
        public AuthenticationServiceConfiguration Authentication = new AuthenticationServiceConfiguration();
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
    }
}