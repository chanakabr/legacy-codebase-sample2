using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class NotificationPartnerSettingsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public NotificationPartnerSettings settings { get; set; }

        public NotificationPartnerSettingsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            settings = null;
        }

        public NotificationPartnerSettingsResponse(ApiObjects.Response.Status resp, NotificationPartnerSettings settings)
        {
            this.Status = resp;
            this.settings = settings;
        }
    }
}
