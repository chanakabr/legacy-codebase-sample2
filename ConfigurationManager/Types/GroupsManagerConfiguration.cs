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
                DefaultValue = "group_{0}",
                OriginalKey = "group_key_format"
            };
            CacheTTLSeconds = new NumericConfigurationValue("cache_ttl_seconds", this)
            {
                DefaultValue =  60,
                OriginalKey = "group_cache_ttl_seconds"
            };
        }
    }
}