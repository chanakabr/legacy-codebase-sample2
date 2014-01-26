using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DeviceResponseObject
    {
        public Device device { get; set; }

        public DeviceResponseStatus deviceResponseStatus { get; set; }
    }
}
