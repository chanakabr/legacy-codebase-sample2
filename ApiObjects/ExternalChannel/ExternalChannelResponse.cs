using ApiObjects.Response;

namespace ApiObjects
{

    public class ExternalChannelResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public ExternalChannel ExternalChannel { get; set; }

        public ExternalChannelResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            ExternalChannel = new ExternalChannel();
        }
    }
}
