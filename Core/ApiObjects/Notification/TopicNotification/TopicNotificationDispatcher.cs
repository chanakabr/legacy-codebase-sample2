using System.Runtime.Serialization;

namespace ApiObjects.Notification
{
    [DataContract]
    [KnownType(typeof(TopicNotificationSmsDispatcher))]
    [KnownType(typeof(TopicNotificationMailDispatcher))]
    public abstract class TopicNotificationDispatcher
    {
        [DataMember]
        public TopicNotificationDispatcherType Type { get; set; }
    }

    [DataContract]
    public class TopicNotificationSmsDispatcher : TopicNotificationDispatcher
    {
        public TopicNotificationSmsDispatcher()
        {
            Type = TopicNotificationDispatcherType.Sms;
        }
    }

    [DataContract]
    public class TopicNotificationMailDispatcher : TopicNotificationDispatcher
    {
        [DataMember]
        public string BodyTemplate { get; set; }

        [DataMember]
        public string SubjectTemplate { get; set; }


        public TopicNotificationMailDispatcher()
        {
            Type = TopicNotificationDispatcherType.Mail;
        }
    }

    public enum TopicNotificationDispatcherType
    {
        Mail = 0,
        Sms = 1

    }
}
