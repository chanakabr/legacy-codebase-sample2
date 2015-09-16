using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class OSSAdapterResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<OSSAdapterBase> OSSAdapters  { get; set; }

        public OSSAdapterResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            OSSAdapters = new List<OSSAdapterBase>();
        }

        public OSSAdapterResponse(ApiObjects.Response.Status status, List<OSSAdapterBase> ossAdapters)
        {
            this.Status = status;
            this.OSSAdapters = ossAdapters;
        }
    }
}
