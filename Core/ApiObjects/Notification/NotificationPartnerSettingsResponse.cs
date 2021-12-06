using ApiObjects.Response;

namespace ApiObjects.Notification
{
    public class NotificationPartnerSettingsResponse
    {
        public Status Status { get; set; }
        public NotificationPartnerSettings settings { get; set; }

        public NotificationPartnerSettingsResponse()
        {
            Status = new Status((int)eResponseStatus.Error, string.Empty);
            settings = null;
        }

        public NotificationPartnerSettingsResponse(Status resp, NotificationPartnerSettings settings)
        {
            this.Status = resp;
            this.settings = settings;
        }
    }
}
