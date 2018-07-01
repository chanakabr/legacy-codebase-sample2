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
                OriginalKey = "taskSocialFeed"
            };
            RoutingKey = new StringConfigurationValue("routing_key", this)
            {
                DefaultValue = "PROCESS_UPDATE_SOCIAL_FEED",
                OriginalKey = "routingKeySocialFeedUpdate"
            };
            RoutingKeyMerge = new StringConfigurationValue("routingKeySocialFeedMerge", this)
            {
                ShouldAllowEmpty = true,
                OriginalKey = "routingKeySocialFeedMerge"
            };
            TaskSocialMerge = new StringConfigurationValue("taskSocialMerge", this)
            {
                ShouldAllowEmpty = true,
                OriginalKey = "taskSocialMerge"
            };
        }
    }
}