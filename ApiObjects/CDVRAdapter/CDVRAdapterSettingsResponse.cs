using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class CDVRAdapterSettingsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<CDVRAdapter> Adapters { get; set; }

        public CDVRAdapterSettingsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Adapters = new List<CDVRAdapter>();
        }

        public CDVRAdapterSettingsResponse(ApiObjects.Response.Status status, List<CDVRAdapter> adapters)
        { 
            this.Status = status;
            this.Adapters = adapters; 
        }
    }
}
