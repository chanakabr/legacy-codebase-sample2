using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class ImageUtilsConfiguration : ConfigurationValue
    {
        public StringConfigurationValue Task;
        public StringConfigurationValue RoutingKey;

        public ImageUtilsConfiguration(string key) : base(key)
        {
            this.Task = new ConfigurationManager.StringConfigurationValue("picture_queue_task")
            {
                ShouldAllowEmpty = true,
                DefaultValue = "distributed_tasks.process_image"
            };

            this.RoutingKey = new StringConfigurationValue("picture_queue_routing_key")
            {
                ShouldAllowEmpty = true,
                DefaultValue = "PROCESS_IMAGE"
            };
        }
    }
}
