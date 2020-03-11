using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class ImageUtilsConfiguration : BaseConfig<ImageUtilsConfiguration>
    {
        public BaseValue<string> Task = new BaseValue<string>("picture_queue_task", "distributed_tasks.process_image", false, "");
        public BaseValue<string> RoutingKey = new BaseValue<string>("picture_queue_routing_key", "PROCESS_IMAGE", false, "");

        public override string TcmKey => "image_utils_configuration";

        public override string[] TcmPath => new[] { TcmKey };
    }
}
