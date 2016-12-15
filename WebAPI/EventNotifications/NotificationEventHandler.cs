using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebAPI.EventNotifications
{
    [Serializable]
    public abstract class NotificationEventHandler
    {
        public NotificationEventHandler()
        {

        }

        public NotificationEventHandler(JObject definitions)
        {

        }

        public NotificationEventHandler(string definitions)
        {

        }

        internal abstract void Handle(EventManager.KalturaEvent kalturaEvent, object t);

        // props
    }
}
