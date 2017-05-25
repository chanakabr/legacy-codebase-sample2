using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class InterestNotification
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ExternalId { get; set; }

        public MessageTemplateType TemplateType { get; set; }

        public long LastMessageSentDateSec { get; set; }

        public string QueueName { get; set; }

        public string TopicNameValue { get; set; }

        public int TopicInterestId { get; set; }
    }
}
