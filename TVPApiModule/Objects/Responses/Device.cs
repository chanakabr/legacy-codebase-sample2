using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class Device
    {
        public string id { get; set; }

        public string deviceUDID { get; set; }

        public string deviceBrand { get; set; }

        public string deviceFamily { get; set; }

        public int deviceFamilyID { get; set; }

        public int domainID { get; set; }

        public string deviceName { get; set; }

        public int deviceBrandID { get; set; }

        public string pin { get; set; }

        public System.DateTime activationDate { get; set; }

        public DeviceState state { get; set; }
    }
}
