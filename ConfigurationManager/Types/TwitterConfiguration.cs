using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class TwitterConfiguration : ConfigurationValue
    {
        public StringConfigurationValue ConsumerKey;
        public StringConfigurationValue ConsumerSecret;
        
        public TwitterConfiguration(string key) : base(key)
        {
            ConsumerKey = new StringConfigurationValue("consumer_key", this)
            {
                DefaultValue = "fK0bd1pQxeAQTZECa657LdAxF"
            };
            ConsumerSecret = new StringConfigurationValue("consumer_secret", this)
            {
                DefaultValue = "1Ei0ouAmvUgys8nIFRZgYqQ8K6LiO201WnUcDdEziERfvmOmHa"
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= ConsumerKey.Validate();
            result &= ConsumerSecret.Validate();

            return result;
        }
    }
}