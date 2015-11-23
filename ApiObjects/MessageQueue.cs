using Newtonsoft.Json;
using System;

namespace ApiObjects
{
    [Serializable]
    public class MessageQueue
    {
        [JsonProperty("Id")]
        public Guid Id { get; set; }
        [JsonProperty("MessageData")]
        public string MessageData { get; set; }
        [JsonProperty("RoutingKey")]
        public string RoutingKey { get; set; }
        [JsonProperty("ExecutionDate")]
        public long ExecutionDate { get; set; }
        [JsonProperty("Type")]
        public string Type { get; set; }

        public MessageQueue()
        {

        }
    }
}
