using ApiObjects.Response;

namespace ApiObjects.Notification
{
    public class EngagementAdapterResponse
    {
        public Status Status { get; private set; }
        public EngagementAdapter EngagementAdapter { get; private set; }

        private EngagementAdapterResponse()
        {
            Status = new Status((int)eResponseStatus.Error, string.Empty);
            EngagementAdapter = new EngagementAdapter();
        }

        public static EngagementAdapterResponse Ok(EngagementAdapter adapter, string message)
        {
            return new EngagementAdapterResponse
            {
                Status = new Status(eResponseStatus.OK, message),
                EngagementAdapter = adapter
            };
        }

        public static EngagementAdapterResponse Error(Status status)
        {
            return new EngagementAdapterResponse { Status = status };
        }
    }
}