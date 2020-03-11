using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects.Notification;

namespace Core.Notification
{
    [KnownType(typeof(EmailNotification))]
    [KnownType(typeof(ElisaEmailNotification))]


    public class NotificationBase 
    {
        private const string GROUP_APP_NAME_KEY_PREFIX = "NOTIFICATION_BANDLE_APP_NAME_GROUP_ID_"; 

        public NotificationRequest oNotificationRequest;
        public NotificationBase()
        {
        }


        public static string GetAppNameFromConfig(long groupID)
        {
            string groupAppNameKey = GROUP_APP_NAME_KEY_PREFIX + groupID.ToString();
            return TVinciShared.WS_Utils.GetTcmConfigValue(groupAppNameKey);
        }
        
    }
}
