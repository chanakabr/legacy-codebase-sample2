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
    }

    public enum TopicNotificationTriggerType
    {
        Date = 0,
        Subscription = 1

    }
}
