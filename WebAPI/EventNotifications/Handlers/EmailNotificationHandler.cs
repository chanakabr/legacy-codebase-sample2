using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.EventNotifications.Handlers
{
    public class EmailNotificationHandler : NotificationEventHandler
    {
        internal override void HandleEvent(EventManager.KalturaEvent kalturaEvent, object t)
        {
            throw new NotImplementedException();
        }
    }
}