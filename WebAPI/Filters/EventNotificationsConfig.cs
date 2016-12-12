using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Filters
{
    public class EventNotificationsConfig
    {
        public static void SubscribeConsumers()
        {
            EventManager.EventManager.Subscribe(new RestNotificationEventConsumer());
        }
    }
}