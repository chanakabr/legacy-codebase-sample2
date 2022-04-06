using Newtonsoft.Json;
using System;

namespace EpgNotificationHandler.DTO
{
    [Serializable]
    public class UpdateEventProperties
    {
        [JsonProperty("event_type")]
        public UpdateEventType EventType { get; set; }

        [JsonProperty("event_date")]
        public UpdateEventDate EventDate { get; set; }
    }
}
