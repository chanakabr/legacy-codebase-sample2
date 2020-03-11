using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class DevicePinResponse
    {
        public string Pin { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
