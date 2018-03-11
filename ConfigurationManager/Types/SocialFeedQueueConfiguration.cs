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
                DefaultValue = "distributed_tasks.process_update_social_feed"
            };
            RoutingKey = new StringConfigurationValue("routing_key", this)
            {
                DefaultValue = "PROCESS_UPDATE_SOCIAL_FEED"
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