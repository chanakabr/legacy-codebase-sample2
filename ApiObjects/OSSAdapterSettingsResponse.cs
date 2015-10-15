using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class OSSAdapterSettingsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<OSSAdapter> OSSAdapters { get; set; }

        public OSSAdapterSettingsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            OSSAdapters = new List<OSSAdapter>();
        }

        public OSSAdapterSettingsResponse(ApiObjects.Response.Status status, List<OSSAdapter> ossAdapters)
        {
            this.Status = status;
            this.OSSAdapters = ossAdapters;
        }
    }
}
