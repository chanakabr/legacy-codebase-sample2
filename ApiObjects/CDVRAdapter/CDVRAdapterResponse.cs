using ApiObjects.Response;

namespace ApiObjects
{

    public class CDVRAdapterResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public CDVRAdapter Adapter { get; set; }

        public CDVRAdapterResponse()
        { 
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Adapter = new CDVRAdapter();
        }
    } 
}
