using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class DeviceBrandResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<DeviceBrand> DeviceBrands { get; set; }

        public int TotalItems { get; set; }

        public DeviceBrandResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            DeviceBrands = new List<DeviceBrand>();
            TotalItems = 0;
        }

    }
}