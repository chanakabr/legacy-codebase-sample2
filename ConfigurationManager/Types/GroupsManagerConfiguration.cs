using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class GroupsManagerConfiguration : ConfigurationValue
    {
        public StringConfigurationValue KeyFormat;
        public NumericConfigurationValue CacheTTLSeconds;

        public GroupsManagerConfiguration(string key) : base(key)
        {
            KeyFormat = new StringConfigurationValue("key_format", this)
            {
                DefaultValue = "group_{0}"
            };
            CacheTTLSeconds = new NumericConfigurationValue("cache_ttl_seconds", this)
            {
                DefaultValue =  60
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= KeyFormat.Validate();
            result &= CacheTTLSeconds.Validate();

            return result;
        }
    }
}