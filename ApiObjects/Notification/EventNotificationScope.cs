using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Notification
{
    public abstract class EventNotificationScope
    {
    }

    public partial class EventNotificationObjectScope : EventNotificationScope
    {
        public CoreObject EventObject { get; set; }
    }
}
