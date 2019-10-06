using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Notification
{
    public abstract class EventNotificationScope
    {
        public string ScopeType { get; set; }
    }

    public partial class EventNotificationEventObjectType : EventNotificationScope
    {
        public CoreObject EventObject { get; set; }
    }
}
