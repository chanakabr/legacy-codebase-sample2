namespace ApiObjects.Notification
{
    public class TopicNotification
    {
        public long Id { get; set; }

        public int GroupId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public SubscribeReference SubscribeReference { get; set; }

        public string PushExternalId { get; set; }

        public string MailExternalId { get; set; }

        public TopicNotification() { }
    }
}