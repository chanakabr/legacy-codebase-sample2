using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class DrmAdapterListResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<DrmAdapter> Adapters { get; set; }

        public DrmAdapterListResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Adapters = new List<DrmAdapter>();
        }
    }
}
