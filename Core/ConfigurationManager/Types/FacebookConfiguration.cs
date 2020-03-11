using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class FacebookConfiguration : BaseConfig<FacebookConfiguration>
    {
        public BaseValue<string> GraphSocialFeedFields = new BaseValue<string>("graph_socialfeed_fields", "posts?fields=from,message,link,picture,created_time,likes.limit(1).summary(true),comments.limit(25).fields(message,from,like_count,created_time),picture");
        public BaseValue<string> GraphURI = new BaseValue<string>("graph_uri", "https://graph.facebook.com");
        public BaseValue<string> TokenKey = new BaseValue<string>("token_key", "tvinci");
        public BaseValue<string> ListName = new BaseValue<string>("list_name", "TvinciAppFriends");
        public BaseValue<string> SecureSiteGuidKey = new BaseValue<string>("secure_site_guid_key", "L3CDpYFfCrGnx5ACoO/Az3oIIt/XeC2dhSmFcB6ckxA=");
        public BaseValue<string> SecureSiteGuidIV = new BaseValue<string>("secure_site_guid_iv", "Yn5/n0s8B0yLRvGuhSLRrA==");



        public override string TcmKey => TcmObjectKeys.FacebookConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}