using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class SocialFeedConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue FacebookItemCount;
        public NumericConfigurationValue InAppItemCount;
        public NumericConfigurationValue TwitterItemCount;
        public NumericConfigurationValue FacebookTTL;
        public NumericConfigurationValue InAppTTL;
        public NumericConfigurationValue TwitterTTL;

        public SocialFeedConfiguration(string key) : base(key)
        {
            FacebookItemCount = new NumericConfigurationValue("facebook_item_count", this)
            {
                DefaultValue = 100
            };
            InAppItemCount = new NumericConfigurationValue("in_app_item_count", this)
            {
                DefaultValue = 100
            };
            TwitterItemCount = new NumericConfigurationValue("twitter_item_count", this)
            {
                DefaultValue = 100,
                Description = "Original key is SocialFeed_Twitter_item_count"
            };
            FacebookTTL = new NumericConfigurationValue("facebook_ttl", this)
            {
                DefaultValue = 10
            };
            InAppTTL = new NumericConfigurationValue("in_app_ttl", this)
            {
                DefaultValue = 10
            };
            TwitterTTL = new NumericConfigurationValue("twitter_ttl", this)
            {
                DefaultValue = 10
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= FacebookItemCount.Validate();
            result &= FacebookTTL.Validate();
            result &= InAppItemCount.Validate();
            result &= InAppTTL.Validate();
            result &= TwitterItemCount.Validate();
            result &= TwitterTTL.Validate();

            return result;
        }
    }
}