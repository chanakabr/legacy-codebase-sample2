using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class AnnouncementSubscription
    {
        public long AnnouncementId { get; set; }
        public string SubscriptionExternalId { get; set; }
        public long SubscribedAtSec { get; set; }
    }
}
