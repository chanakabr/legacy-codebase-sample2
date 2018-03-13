namespace ConfigurationManager
{
    public class SocialFeedQueueConfiguration : ConfigurationValue
    {
        public StringConfigurationValue Task;
        public StringConfigurationValue RoutingKey;
        public StringConfigurationValue TaskSocialMerge;
        public StringConfigurationValue RoutingKeyMerge;

        public SocialFeedQueueConfiguration(string key) : base(key)
        {
            Task = new StringConfigurationValue("task", this)
            {
                DefaultValue = "distributed_tasks.process_update_social_feed",
                Description = "Original key is taskSocialFeed"
            };
            RoutingKey = new StringConfigurationValue("routing_key", this)
            {
                DefaultValue = "PROCESS_UPDATE_SOCIAL_FEED",
                Description = "Original key is routingKeySocialFeedUpdate"
            };
            RoutingKeyMerge = new StringConfigurationValue("routingKeySocialFeedMerge", this)
            {
                ShouldAllowEmpty = true              
            };
            TaskSocialMerge = new StringConfigurationValue("taskSocialMerge", this)
            {
                ShouldAllowEmpty = true
            };
        }
    }
}