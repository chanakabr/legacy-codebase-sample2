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
                DefaultValue = "fK0bd1pQxeAQTZECa657LdAxF",
                OriginalKey = "TWITTER_CONSUMER_KEY"
            };
            ConsumerSecret = new StringConfigurationValue("consumer_secret", this)
            {
                DefaultValue = "1Ei0ouAmvUgys8nIFRZgYqQ8K6LiO201WnUcDdEziERfvmOmHa",
                OriginalKey = "TWITTER_CONSUMER_SECRET"
            };
        }
    }
}