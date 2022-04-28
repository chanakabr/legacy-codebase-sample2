namespace ApiLogic.Notification
{
    internal class PublishIotMessageRequest
    {
        public string GroupId { get; }
        public string Message { get; }
        public string Topic { get; }
        public string ExternalAnnouncementId { get; }

        public PublishIotMessageRequest(string groupId, string message, string topic, string externalAnnouncementId)
        {
            GroupId = groupId;
            Message = message;
            Topic = topic;
            ExternalAnnouncementId = externalAnnouncementId;
        }
    }
}