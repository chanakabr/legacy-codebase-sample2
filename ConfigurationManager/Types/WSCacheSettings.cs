using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class WSCacheSettings : ConfigurationValue
    {
        public StringConfigurationValue Name;
        public StringConfigurationValue Type;
        public NumericConfigurationValue TimeInMinutes;

        public WSCacheSettings(string key) : base(key)
        {
            Name = new StringConfigurationValue("name", this)
            {
                DefaultValue = "Cache"
            };
            Type = new StringConfigurationValue("cache_type", this)
            {
                DefaultValue = "InnerCache"
            };
            TimeInMinutes = new NumericConfigurationValue("time_in_minutes", this)
            {
                DefaultValue = 120
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= Name.Validate();
            result &= Type.Validate();
            result &= TimeInMinutes.Validate();

            return result;
        }
    }
}