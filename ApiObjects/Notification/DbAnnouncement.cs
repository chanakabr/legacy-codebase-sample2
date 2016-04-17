
namespace ApiObjects.Notification
{
    public class DbAnnouncement
    {
        public int ID { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public eAnnouncementRecipientsType  RecipientsType{ get; set; }
        public string FollowPhrase{ get; set; }
    }
}
