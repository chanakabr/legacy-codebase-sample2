using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.CDNAdapter
{
    public class CDNAdapterResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public CDNAdapter Adapter { get; set; }

        public CDNAdapterResponse()
        { 
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Adapter = new CDNAdapter();
        }
    }
}
