using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.CDNAdapter
{
    public class CDNAdapterListResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<CDNAdapter> Adapters { get; set; }

        public CDNAdapterListResponse() 
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Adapters = new List<CDNAdapter>();
        }

        public CDNAdapterListResponse(ApiObjects.Response.Status status, List<CDNAdapter> adapters)
        {
            this.Status = status; 
            this.Adapters = adapters;
        }
    }
}
