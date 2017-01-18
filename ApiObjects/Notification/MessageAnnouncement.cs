using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.Notification
{
    [DataContract]
    public class MessageAnnouncement
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        public long StartTime { get; set; }
        [DataMember]
        public string Timezone { get; set; }
        [DataMember]
        public eAnnouncementStatus Status { get; set; }
        [DataMember]
        public eAnnouncementRecipientsType Recipients { get; set; }
        [DataMember]
        public int MessageAnnouncementId { get; set; }
        [DataMember]
        public int AnnouncementId { get; set; }
        [DataMember]
        public string MessageReference { get; set; }

        public MessageAnnouncement() { }

        public MessageAnnouncement(string name, string msg, bool enabled, long startTime, string timezone, eAnnouncementRecipientsType recipients, eAnnouncementStatus status = eAnnouncementStatus.NotSent, string messageReference = null, int announcementId = 0)
        {
            Name = name;
            Message = msg;
            Enabled = enabled;
            StartTime = startTime;
            Timezone = timezone;
            Recipients = recipients;
            Status = status;
            AnnouncementId = announcementId;
            MessageReference = messageReference;
        }

        public override string ToString()
        {
            return string.Format("MessageAnnouncement: Name: {0}, Message {1}, StartTime: {2}, TimeZone: {3} Status: {4}, Recipients {5}, Enabled {6}, AnnouncementId: {7}",
                Name,
                Message,
                StartTime,
                Timezone,
                Status,
                Recipients,
                Enabled,
                AnnouncementId);
        }
    }
}
