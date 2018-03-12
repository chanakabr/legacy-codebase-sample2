using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class SocialFeedQueueConfiguration : ConfigurationValue
    {
        public StringConfigurationValue Task;
        public StringConfigurationValue RoutingKey;
        
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
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= Task.Validate();
            result &= RoutingKey.Validate();

            return result;
        }
    }
}