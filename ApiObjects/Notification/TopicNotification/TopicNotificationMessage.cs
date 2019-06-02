using System.Collections.Generic;

namespace ApiObjects.Notification
{
    public class TopicNotificationMessage
    {
        public long Id { get; set; }
                
        public int GroupId { get; set; }

        public string Message { get; set; }

        public string ImageUrl { get; set; }

        public long TopicNotificationId { get; set; }

        public TopicNotificationTrigger Trigger { get; set; }

        public List<TopicNotificationDispatcher> Dispatchers { get; set; }

        public TopicNotificationMessageStatus Status { get; set; }
    }
}
