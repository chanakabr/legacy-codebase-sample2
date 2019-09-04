using ApiObjects.Base;

namespace ApiObjects
{
    public class EventNotificationAction : ICrudHandeledObject
    {
        public string Id { get; set; }

        public long ObjectId { get; set; }

        public string ObjectType { get; set; }

        public string Message { get; set; }

        public EventNotificationActionStatus Status { get; set; }   

        public string ActionType{ get; set; }

        public long CreateDate { get; set; }

        public long UpdateDate { get; set; }

        public string DocumentType = "EventNotificationAction";
    }

    public enum EventNotificationActionStatus
    {
        Sent = 0,
        Failed = 1,
        Success = 2,
        FailedToSend = 3
    }
}