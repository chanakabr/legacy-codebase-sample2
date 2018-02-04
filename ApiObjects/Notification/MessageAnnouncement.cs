using System.Runtime.Serialization;

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

        [DataMember]
        public string ImageUrl { get; set; }

        [DataMember]
        public bool IncludeMail { get; set; }

        [DataMember]
        public string MailSubject { get; set; }

        [DataMember]
        public string MailTemplate { get; set; }

        public MessageAnnouncement() { }

        public override string ToString()
        {
            return string.Format("MessageAnnouncement: Name: {0}, Message {1}, StartTime: {2}, TimeZone: {3} Status: {4}, Recipients {5}, Enabled {6}, AnnouncementId: {7}, IncludeMail: {8}, , MailTemplate: {9}, , MailSubject: {10}",
                Name,
                Message,
                StartTime,
                Timezone,
                Status,
                Recipients,
                Enabled,
                AnnouncementId,
                IncludeMail,
                MailTemplate,
                MailTemplate);
        }
    }
}
