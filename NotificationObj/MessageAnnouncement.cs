using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NotificationObj
{
    [DataContract]
    public class MessageAnnouncement
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public DateTime StartTime { get; set; }
        [DataMember]
        public eAnnouncementStatus Status { get; set; }
        [DataMember]
        public eAnnouncementRecipients Recipients { get; set; }
        [DataMember]
        public int MessageAnnouncementId { get; set; }

        public MessageAnnouncement(string name, string msg, DateTime begin, eAnnouncementRecipients recipients, eAnnouncementStatus status = eAnnouncementStatus.NotSent)
        {
            Name = name;
            Message = msg;
            StartTime = begin;
            Recipients = recipients;
            Status = status;
        }

        public override string ToString()
        {
            return string.Format("MessageAnnouncement: Name: {0}, Messagel {1}, StartTime: {2}, Status: {3}, Recipients {4}", Name, Message, StartTime, Status, Recipients);
        }
    }
}
