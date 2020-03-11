
namespace ApiObjects.Notification
{
    public class DbAnnouncement
    {
        public int ID { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public eAnnouncementRecipientsType RecipientsType { get; set; }
        public string FollowPhrase { get; set; }
        public string FollowReference { get; set; }
        public bool? AutomaticIssueFollowNotification { get; set; }
        public long LastMessageSentDateSec { get; set; }
        public int SubscribersAmount{ get; set; }
        public string QueueName { get; set; }
        public string MailExternalId { get; set; }

        public override string ToString()
        {
            return string.Format("ID:{0}, Name: {1}, FollowPhrase:{2}, FollowReference:{3}.", ID, Name, FollowPhrase, FollowReference);
        }
    }
}
