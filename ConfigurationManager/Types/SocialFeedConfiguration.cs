
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
        public NumericConfigurationValue TagsTTL;

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
            TagsTTL = new NumericConfigurationValue("tags_ttl", this)
            {
                DefaultValue = 30
            };
        }

        public NumericConfigurationValue GetTTLByPlatform(string platform)
        {
            NumericConfigurationValue ttl = null;
            switch (platform.ToLower())
            {
                case "inapp":
                    ttl = InAppTTL;
                    break;
                case "facebook":
                    ttl = FacebookTTL;
                    break;
                case "twitter":
                    ttl = TwitterTTL;
                    break;
                case "unknown":
                default:
                    ttl = new NumericConfigurationValue("unknown_ttl") { DefaultValue = 10 };
                    break;
            }

            return ttl;
        }
    }
}