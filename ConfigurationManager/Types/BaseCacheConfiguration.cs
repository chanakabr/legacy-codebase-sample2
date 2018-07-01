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

        internal override bool Validate()
        {
            bool result = true;

            if (this.Type != null)
            {
                result &= this.Type.Validate();
            }

            if (this.TTLSeconds != null)
            {
                result &= this.TTLSeconds.Validate();
            }

            return result;
        }
    }
}