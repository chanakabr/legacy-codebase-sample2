using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class AuthorizationManagerConfiguration : ConfigurationValue
    {
        public StringConfigurationValue UsersSessionsKeyFormat;
        public StringConfigurationValue RevokedKSKeyFormat;
        public NumericConfigurationValue RevokedKSMaxTTLSeconds;
        public StringConfigurationValue RevokedSessionKeyFormat;

        public AuthorizationManagerConfiguration(string key) : base(key)
        {
            UsersSessionsKeyFormat = new ConfigurationManager.StringConfigurationValue("users_sessions_key_format", this)
            {
                DefaultValue = "sessions_{0}"
            };
            RevokedKSKeyFormat = new ConfigurationManager.StringConfigurationValue("revoked_ks_key_format", this)
            {
                DefaultValue = "r_ks_{0}"
            };
            RevokedKSMaxTTLSeconds = new ConfigurationManager.NumericConfigurationValue("revoked_ks_max_ttl_seconds", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = 604800,
                Description = "Revoked KS TTL in seconds. This key is global for all partners. Specific partner data overrides it, if partner has it."
            };
            RevokedSessionKeyFormat = new StringConfigurationValue("revoked_sessions_key_format", this)
            {
                DefaultValue = "r_session_{0}"
            };
        }
    }
}
