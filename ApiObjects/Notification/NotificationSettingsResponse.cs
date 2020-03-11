using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class NotificationSettingsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public UserNotificationSettings settings { get; set; }

        public NotificationSettingsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            settings = new UserNotificationSettings();
        }

        public NotificationSettingsResponse(ApiObjects.Response.Status resp, UserNotificationSettings settings)
        {
            this.Status = resp;
            this.settings = settings;
        }
    }
}
