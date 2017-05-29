using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    [DataContract]
    public class InterestNotificationMessage
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime SendTime { get; set; }

        [DataMember]
        public string TopicInterestsNotificationsId { get; set; }
    }
}
