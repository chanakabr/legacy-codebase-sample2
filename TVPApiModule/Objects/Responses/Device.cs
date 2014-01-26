using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class Device
    {
        public string id { get; set; }

        public string device_udid { get; set; }

        public string device_brand { get; set; }

        public string device_family { get; set; }

        public int device_family_id { get; set; }

        public int domain_id { get; set; }

        public string device_name { get; set; }

        public int device_brand_id { get; set; }

        public string pin { get; set; }

        public System.DateTime activation_date { get; set; }

        public DeviceState state { get; set; }
    }
}
