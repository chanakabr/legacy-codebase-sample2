using System;
using ApiObjects.EventBus;
using Newtonsoft.Json;

namespace NotificationHandlers.Common.DTO
{
    [Serializable]
    public class UpdateHeader
    {
        [JsonProperty("event_type")]
        public EventType EventType { get; set; }

        [JsonProperty("event_date")]
        public long EventDate { get; set; }
    }
}