using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class OTTUserControllerConfiguration : ConfigurationValue
    {
        public StringConfigurationValue UserIdEncryptionKey;
        public StringConfigurationValue UserIdEncryptionIV;

        public OTTUserControllerConfiguration(string key) : base(key)
        {
            UserIdEncryptionKey = new StringConfigurationValue("user_id_encryption_key", this)
            {
                DefaultValue = "L3CDpYFfCrGnx5ACoO/Az3oIIt/XeC2dhSmFcB6ckxA=",
                OriginalKey = "user_id_encryption_key"
            };
            UserIdEncryptionIV = new StringConfigurationValue("user_id_encryption_iv", this)
            {
                DefaultValue = "Yn5/n0s8B0yLRvGuhSLRrA==",
                OriginalKey = "user_id_encryption_iv"
            };
        }
    }
}