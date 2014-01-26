using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DeviceContainer
    {
        public string device_family_name { get; set; }

        public int device_family_id { get; set; }

        public int device_limit { get; set; }

        public int device_concurrent_limit { get; set; }

        public Device[] device_instances { get; set; }
    }
}
