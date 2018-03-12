using ApiObjects.Social;

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
                DefaultValue = 100,
                Description = "Original key is SocialFeed_FB_item_count"
            };
            InAppItemCount = new NumericConfigurationValue("in_app_item_count", this)
            {
                DefaultValue = 100,
                Description = "Original key is SocialFeed_InApp_item_count"
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

        public NumericConfigurationValue GetTTLByPlatform(eSocialPlatform platform)
        {
            NumericConfigurationValue ttl = null;
            switch (platform)
            {
                case eSocialPlatform.InApp:
                    ttl = InAppTTL;
                    break;
                case eSocialPlatform.Facebook:
                    ttl = FacebookTTL;
                    break;
                case eSocialPlatform.Twitter:
                    ttl = TwitterTTL;
                    break;
                case eSocialPlatform.Unknown:
                default:
                    ttl = new NumericConfigurationValue("unknown_ttl") { DefaultValue = 10 };
                    break;
            }

            return ttl;
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