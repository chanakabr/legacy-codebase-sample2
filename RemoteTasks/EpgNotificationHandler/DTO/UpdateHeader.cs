using Newtonsoft.Json;
using System;

namespace EpgNotificationHandler.DTO
{
    [Serializable]
    public class UpdateHeader
    {
        [JsonProperty("type")]
        public string Type { get; } = typeof(UpdateHeader).Name;

        [JsonProperty("description")]
        public string Description { get; } = "Header of IoT notification";

        [JsonProperty("properties")]
        public UpdateEventProperties Properties { get; set; }
    }
}
