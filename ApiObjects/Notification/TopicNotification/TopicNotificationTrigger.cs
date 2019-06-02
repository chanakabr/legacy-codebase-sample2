using System;
using System.Runtime.Serialization;

namespace ApiObjects.Notification
{
    [DataContract]
    [KnownType(typeof(TopicNotificationDateTrigger))]
    [KnownType(typeof(TopicNotificationSubscriptionTrigger))]    
    public abstract class TopicNotificationTrigger
    {
        [DataMember]
        public TopicNotificationTriggerType Type { get; set; }       
    }

    [DataContract]
    public class TopicNotificationDateTrigger : TopicNotificationTrigger
    {
        [DataMember]
        public DateTime Date { get; set; }

        public TopicNotificationDateTrigger()
        {
            Type = TopicNotificationTriggerType.Date;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TopicNotificationDateTrigger;
            if (other == null)
            {
                return false;
            }

            return this.Type == other.Type && this.Date == other.Date;
        }
    }

    [DataContract]
    public class TopicNotificationSubscriptionTrigger : TopicNotificationTrigger
    {
        [DataMember]
        public long Offset { get; set; }

        [DataMember]
        public TopicNotificationSubscriptionTriggerType TriggerType { get; set; }


        public TopicNotificationSubscriptionTrigger()
        {
            Type = TopicNotificationTriggerType.Subscription;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TopicNotificationSubscriptionTrigger;
            if (other == null)
            {
                return false;
            }

            return this.Type == other.Type  && this.Offset == other.Offset && this.TriggerType == other.TriggerType;
        }        
    }

    public enum TopicNotificationTriggerType
    {
        Date = 0,
        Subscription = 1

    }
}