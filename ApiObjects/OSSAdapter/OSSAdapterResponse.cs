using ApiObjects.Response;

namespace ApiObjects
{

    public class OSSAdapterResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public OSSAdapter OSSAdapter { get; set; }

        public OSSAdapterResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            OSSAdapter = new OSSAdapter();
        }
    }
}
