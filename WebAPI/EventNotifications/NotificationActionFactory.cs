using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.EventNotifications;

namespace WebAPI
{
    public class NotificationActionFactory
    {
        internal static NotificationEventHandler CreateEventHandler(Managers.Models.eNotificationActionTypes type, string body)
        {
            switch (type)
            {
                case WebAPI.Managers.Models.eNotificationActionTypes.Http:
                break;
                case WebAPI.Managers.Models.eNotificationActionTypes.Email:
                break;
                case WebAPI.Managers.Models.eNotificationActionTypes.Rabbit:
                break;
                default:
                break;
            }
            return null;
        }
    }
}