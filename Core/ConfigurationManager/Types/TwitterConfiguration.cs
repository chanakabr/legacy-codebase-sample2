using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class TwitterConfiguration : BaseConfig<TwitterConfiguration>
    {
        public BaseValue<string> ConsumerKey = new BaseValue<string>("consumer_key", "fK0bd1pQxeAQTZECa657LdAxF");
        public BaseValue<string> ConsumerSecret = new BaseValue<string>("consumer_secret", "1Ei0ouAmvUgys8nIFRZgYqQ8K6LiO201WnUcDdEziERfvmOmHa");

        public override string TcmKey => TcmObjectKeys.TwitterConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}