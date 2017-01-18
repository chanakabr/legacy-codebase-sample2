using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class DeviceRegistrationStatusResponse
    {
        public DeviceRegistrationStatus DeviceRegistrationStatus { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
