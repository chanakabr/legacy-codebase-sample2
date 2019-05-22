using ApiObjects.Response;

namespace ApiObjects.Notification
{
    public class EngagementAdapterResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public EngagementAdapter EngagementAdapter { get; set; }

        public EngagementAdapterResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            EngagementAdapter = new EngagementAdapter();
        }
    }
}