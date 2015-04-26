using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Clients
{
    public class NotificationsClient : BaseClient
    {
        public NotificationsClient()
        {

        }

        #region Properties

        protected WebAPI.Notifications.NotificationServiceClient Notification
        {
            get
            {
                return (Module as WebAPI.Notifications.NotificationServiceClient);
            }
        }

        #endregion
    }
}