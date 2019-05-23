using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Notification
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class TopicNotificationMessage
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("GroupId")]
        public int GroupId { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("ImageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("TopicNotificationId")]
        public long TopicNotificationId { get; set; }

        [JsonProperty("Trigger")]
        public TopicNotificationTrigger Trigger { get; set; }

        [JsonProperty("Dispatchers")]
        public List<TopicNotificationDispatcher> Dispatchers { get; set; }

    }
}
