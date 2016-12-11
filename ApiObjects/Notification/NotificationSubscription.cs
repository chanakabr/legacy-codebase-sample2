using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class NotificationSubscription
    {
        public long Id { get; set; }
        public string ExternalId { get; set; }
        public long SubscribedAtSec { get; set; }
    }
}
