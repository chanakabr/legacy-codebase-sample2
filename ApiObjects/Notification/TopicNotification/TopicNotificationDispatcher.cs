using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Notification
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class TopicNotificationDispatcher
    {
        [JsonProperty("Type")]
        public TopicNotificationDispatcherType Type { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class TopicNotificationSmsDispatcher : TopicNotificationDispatcher
    {
        public TopicNotificationSmsDispatcher()
        {
            Type = TopicNotificationDispatcherType.Sms;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class TopicNotificationMailDispatcher : TopicNotificationDispatcher
    {
        [JsonProperty("BodyTemplate")]
        public string BodyTemplate { get; set; }

        [JsonProperty("SubjectTemplate")]
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
