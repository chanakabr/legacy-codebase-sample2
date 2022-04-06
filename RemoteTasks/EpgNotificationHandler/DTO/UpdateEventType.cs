using ApiObjects.EventBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EpgNotificationHandler.DTO
{
    [Serializable]
    public class UpdateEventType
    {
        [JsonProperty("enum")]
        public IList<string> Enum { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
