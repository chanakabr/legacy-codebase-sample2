using ApiObjects.Response;

namespace ApiObjects.Notification
{
    public class EngagementResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public Engagement Engagement { get; set; }

        public EngagementResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            Engagement = new Engagement();
        }
    }
}