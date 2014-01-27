using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DeviceInfo
    {
        public string name { get; set; }

        public string udid { get; set; }

        public string type { get; set; }

        public bool active { get; set; }
    }
}
