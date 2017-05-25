using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class InterestNotificationMessage
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Message { get; set; }

        public DateTime SendTime { get; set; }

        public string TopicInterestsNotificationsId { get; set; }
    }
}
