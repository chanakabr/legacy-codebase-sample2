using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Notification
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class TopicNotificationTrigger
    {
        [JsonProperty("Type")]
        public TopicNotificationTriggerType Type { get; set; }       
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class TopicNotificationDateTrigger : TopicNotificationTrigger
    {
        [JsonProperty("Date")]
        public DateTime Date { get; set; }

        public TopicNotificationDateTrigger()
        {
            Type = TopicNotificationTriggerType.Date;
        }

        public bool Equals(TopicNotificationDateTrigger other)
        {
            return this.Type == other.Type && this.Date == other.Date;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class TopicNotificationSubscriptionTrigger : TopicNotificationTrigger
    {
        [JsonProperty("Offset")]
        public long Offset { get; set; }

        [JsonProperty("TriggerType")]
        public TopicNotificationSubscriptionTriggerType TriggerType { get; set; }


        public TopicNotificationSubscriptionTrigger()
        {
            Type = TopicNotificationTriggerType.Subscription;
        }

        public bool Equals(TopicNotificationSubscriptionTrigger other)
        {
            return this.Type == other.Type 
                && this.Offset == other.Offset
                && this.TriggerType == other.TriggerType;
        }
    }

    public enum TopicNotificationTriggerType
    {
        Date = 0,
        Subscription = 1

    }
}
