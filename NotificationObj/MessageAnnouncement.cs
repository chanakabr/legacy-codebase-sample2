using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotificationObj
{
    public class MessageAnnouncement
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTime StartTime { get; set; }
        public eAnnouncementStatus Status { get; set; }
        public eAnnouncementRecipients Recipients { get; set; }

        public MessageAnnouncement(string name, string msg, DateTime begin, eAnnouncementRecipients recipients, eAnnouncementStatus status = eAnnouncementStatus.NotSent)
        {
            Name = name;
            Message = msg;
            StartTime = begin;
            Recipients = recipients;
            Status = status;
        }

        public string ToString()
        {
            return string.Format("MessageAnnouncement: Name: {0}, Messagel {1}, StartTime: {2}, Status: {3}, Recipients {4}", Name, Message, StartTime, Status, Recipients);
        }
    }
}
