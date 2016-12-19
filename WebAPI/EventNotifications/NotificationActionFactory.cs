using Newtonsoft.Json.Linq;
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
            NotificationEventHandler handler = null;

            JObject json = JObject.Parse(body);

            return CreateEventHandler(type, json);
        }

        internal static NotificationEventHandler CreateEventHandler(Managers.Models.eNotificationActionTypes type, JObject jsonHandler)
        {
            NotificationEventHandler handler = null;

            switch (type)
            {
                case WebAPI.Managers.Models.eNotificationActionTypes.Http:
                {
                    handler = jsonHandler.ToObject<HttpNotificationHandler>();
                    break;
                }
                case WebAPI.Managers.Models.eNotificationActionTypes.Email:
                {
                    handler = jsonHandler.ToObject<EmailNotificationHandler>();
                    break;
                }
                case WebAPI.Managers.Models.eNotificationActionTypes.RabbitQueue:
                {
                    handler = jsonHandler.ToObject<RabbitQueueHandler>();
                    break;
                }
                default:
                break;
            }

            return handler;
        }
    }
}