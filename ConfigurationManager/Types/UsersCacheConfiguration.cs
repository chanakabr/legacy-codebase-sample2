using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class UsersCacheConfiguration : ConfigurationValue
    {
        public BooleanConfigurationValue ShouldUseCache;
        public NumericConfigurationValue TTLSeconds;

        public UsersCacheConfiguration(string key) : base(key)
        {
            ShouldUseCache = new BooleanConfigurationValue("should_use_cache")
            {
                DefaultValue = true
            };
            TTLSeconds = new NumericConfigurationValue("ttl_seconds", this)
            {
                DefaultValue = 1440.0
            };
        }
    }
}
