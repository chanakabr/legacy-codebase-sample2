using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.EventNotifications
{
    public class HttpNotificationHandler : NotificationEventHandler
    {
        internal override void Handle(EventManager.KalturaEvent kalturaEvent, object t)
        {
            throw new NotImplementedException();
        }
    }
}