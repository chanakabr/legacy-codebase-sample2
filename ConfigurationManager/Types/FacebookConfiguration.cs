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
                DefaultValue = "posts?fields=from,message,link,picture,created_time,likes.limit(1).summary(true),comments.limit(25).fields(message,from,like_count,created_time),picture",
                OriginalKey = "FB_GRAPH_SOCIALFEED_FIELDS"
            };
            GraphURI = new StringConfigurationValue("graph_uri", this)
            {
                DefaultValue = "https://graph.facebook.com",
                OriginalKey = "FB_GRAPH_URI"
            };
            TokenKey = new StringConfigurationValue("token_key", this)
            {
                DefaultValue = "tvinci",
                OriginalKey = "FB_TOKEN_KEY"
            };
            ListName = new StringConfigurationValue("list_name", this)
            {
                DefaultValue = "TvinciAppFriends",
                OriginalKey = "FB_LIST_NAME"
            };
            SecureSiteGuidKey = new StringConfigurationValue("secure_site_guid_key", this)
            {
                DefaultValue = "L3CDpYFfCrGnx5ACoO/Az3oIIt/XeC2dhSmFcB6ckxA=",
                OriginalKey = "SecureSiteGuidKey"
            };
            SecureSiteGuidIV = new StringConfigurationValue("secure_site_guid_iv", this)
            {
                DefaultValue = "Yn5/n0s8B0yLRvGuhSLRrA==",
                OriginalKey = "SecureSiteGuidIV"
            };
        }
    }
}