using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class DeviceFamilyResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<DeviceFamily> DeviceFamilies { get; set; }

        public int TotalItems { get; set; }

        public DeviceFamilyResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            DeviceFamilies = new List<DeviceFamily>();
            TotalItems = 0;
        }

    }
}