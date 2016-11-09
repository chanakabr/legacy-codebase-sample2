using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects
{
    public class OSSAdapterResponseList
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<OSSAdapter> OSSAdapters { get; set; }

        public OSSAdapterResponseList()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            OSSAdapters = new List<OSSAdapter>();
        }

        public OSSAdapterResponseList(ApiObjects.Response.Status status, List<OSSAdapter> ossAdapters)
        {
            this.Status = status;
            this.OSSAdapters = ossAdapters;
        }
    }

}
