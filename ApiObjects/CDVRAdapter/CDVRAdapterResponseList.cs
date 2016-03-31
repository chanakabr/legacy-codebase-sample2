using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects
{
    public class CDVRAdapterResponseList
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<CDVRAdapterBase> Adapters { get; set; }

        public CDVRAdapterResponseList() 
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Adapters = new List<CDVRAdapterBase>();
        } 

        public CDVRAdapterResponseList(ApiObjects.Response.Status status, List<CDVRAdapterBase> adapters)
        {
            this.Status = status; 
            this.Adapters = adapters;
        }
    }

}
