using Newtonsoft.Json;
using System;

namespace ApiObjects.Notification
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class TopicNotification
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("GroupId")]
        public int GroupId { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("SubscribeReference")]
        public SubscribeReference SubscribeReference { get; set; }        

        [JsonProperty("PushExternalId")]
        public string PushExternalId { get; set; }

        [JsonProperty("MailExternalId")]
        public string MailExternalId { get; set; }

        public TopicNotification() { }
    }
}