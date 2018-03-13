namespace ConfigurationManager
{
    public class FacebookConfiguration : ConfigurationValue
    {
        public StringConfigurationValue GraphSocialFeedFields;
        public StringConfigurationValue GraphURI;
        public StringConfigurationValue TokenKey;
        public StringConfigurationValue ListName;
        public StringConfigurationValue SecureSiteGuidKey;
        public StringConfigurationValue SecureSiteGuidIV;
        
        public FacebookConfiguration(string key) : base(key)
        {
            GraphSocialFeedFields = new StringConfigurationValue("graph_socialfeed_fields", this)
            {
                DefaultValue = "posts?fields=from,message,link,picture,created_time,likes.limit(1).summary(true),comments.limit(25).fields(message,from,like_count,created_time),picture"
            };
            GraphURI = new StringConfigurationValue("graph_uri", this)
            {
                DefaultValue = "https://graph.facebook.com",
                Description = "Original key is FB_GRAPH_URI"
            };
            TokenKey = new StringConfigurationValue("token_key", this)
            {
                DefaultValue = "tvinci",
                Description = "Original key is FB_TOKEN_KEY"
            };
            ListName = new StringConfigurationValue("list_name", this)
            {
                DefaultValue = "TvinciAppFriends",
                Description = "Original key is FB_LIST_NAME"
            };
            SecureSiteGuidKey = new StringConfigurationValue("secure_site_guid_key", this)
            {
                DefaultValue = "L3CDpYFfCrGnx5ACoO/Az3oIIt/XeC2dhSmFcB6ckxA="
            };
            SecureSiteGuidIV = new StringConfigurationValue("secure_site_guid_iv", this)
            {
                DefaultValue = "Yn5/n0s8B0yLRvGuhSLRrA=="
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= GraphSocialFeedFields.Validate();
            result &= GraphURI.Validate();
            result &= TokenKey.Validate();
            result &= ListName.Validate();
            result &= SecureSiteGuidKey.Validate();
            result &= SecureSiteGuidIV.Validate();

            return result;
        }
    }
}