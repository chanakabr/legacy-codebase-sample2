using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class DrmAdapterResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public DrmAdapter Adapter { get; set; }

        public DrmAdapterResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Adapter = new DrmAdapter();
        }
    }
}
