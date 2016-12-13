using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebAPI.EventNotifications
{
    public abstract class NotificationEventHandler
    {
        public NotificationEventHandler(string definitions)
        {

        }

        internal abstract void HandleEvent(EventManager.KalturaEvent kalturaEvent, object t);
    }
}
