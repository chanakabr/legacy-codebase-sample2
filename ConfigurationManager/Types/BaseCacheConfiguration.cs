using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class BaseCacheConfiguration : ConfigurationValue
    {
        public StringConfigurationValue Type;
        public NumericConfigurationValue TTLSeconds;

        public BaseCacheConfiguration(string key) : base(key)
        {
            Type = new StringConfigurationValue("type", this)
            {
                DefaultValue = "CouchBase"
            };
            TTLSeconds = new NumericConfigurationValue("ttl_seconds", this)
            {
                DefaultValue = 86400
            };
        }
    }
}