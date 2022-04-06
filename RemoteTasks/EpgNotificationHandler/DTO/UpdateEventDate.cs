using Newtonsoft.Json;
using System;

namespace EpgNotificationHandler.DTO
{
    [Serializable]
    public class UpdateEventDate
    {
        [JsonProperty("type")]
        public string Type{ get; set; }

        [JsonProperty("description")]
        public string Description { get; } = "The event time in epoch seconds";

        [JsonProperty("eventDate")]
        public long EventDate { get; set; }
    }
}
