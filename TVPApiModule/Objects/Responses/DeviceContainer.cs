using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DeviceContainer
    {
        public string deviceFamilyName { get; set; }

        public int deviceFamilyID { get; set; }

        public int deviceLimit { get; set; }

        public int deviceConcurrentLimit { get; set; }

        public Device[] deviceInstances { get; set; }
    }
}
