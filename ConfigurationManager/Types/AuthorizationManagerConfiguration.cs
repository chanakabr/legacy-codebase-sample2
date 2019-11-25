using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ConfigurationManager
{
    public class AuthorizationManagerConfiguration : BaseConfig<AuthorizationManagerConfiguration>
    {

        public override string TcmKey => "authorization_manager_configuration";

        public BaseValue<string> UsersSessionsKeyFormat = new BaseValue<string>("users_sessions_key_format", "sessions_{0}", true, "description");
        public BaseValue<string> RevokedKSKeyFormat = new BaseValue<string>("revoked_ks_key_format", "r_ks_{0}", true, "description");
        public BaseValue<int> RevokedKSMaxTTLSeconds = new BaseValue<int>("revoked_ks_max_ttl_seconds", 604800, true, "Revoked KS TTL in seconds. This key is global for all partners. Specific partner data overrides it, if partner has it.");
        public BaseValue<string> RevokedSessionKeyFormat = new BaseValue<string>("revoked_sessions_key_format", "r_session_{0}", true, "description");

        

        public override void SetActualValues(JToken token)
        {
            if (token != null)
            {
                SetActualValue(token, UsersSessionsKeyFormat);
                SetActualValue(token, RevokedKSKeyFormat);
                SetActualValue(token, RevokedKSMaxTTLSeconds);
                SetActualValue(token, RevokedSessionKeyFormat);
            }
        }
    }
}
