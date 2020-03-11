using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;
using System;


namespace ConfigurationManager.Types
{
    public class AuthorizationManagerConfiguration : BaseConfig<AuthorizationManagerConfiguration>
    {

        public override string TcmKey => TcmObjectKeys.AuthorizationManagerConfiguration;

        public override string[] TcmPath => new string[] {  TcmKey };

        public BaseValue<string> UsersSessionsKeyFormat = new BaseValue<string>("users_sessions_key_format", "sessions_{0}", false, "description");
        public BaseValue<string> RevokedKSKeyFormat = new BaseValue<string>("revoked_ks_key_format", "r_ks_{0}", false, "description");
        public BaseValue<int> RevokedKSMaxTTLSeconds = new BaseValue<int>("revoked_ks_max_ttl_seconds", 604800, false, "Revoked KS TTL in seconds. This key is global for all partners. Specific partner data overrides it, if partner has it.");
        public BaseValue<string> RevokedSessionKeyFormat = new BaseValue<string>("revoked_sessions_key_format", "r_session_{0}", false, "description");

        

    }
}
