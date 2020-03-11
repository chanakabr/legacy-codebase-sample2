using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class DeviceResponse
    {
        public DeviceResponseObject Device { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
