using ApiObjects;
using EventBus.Abstraction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class MessageAnnouncementRequest : DelayedServiceEvent
    {
        [JsonProperty("start_time")]
        public long StartTime { get; set; }

        [JsonProperty("message_announcement_id")]
        public int MessageAnnouncementId { get; set; }

        [JsonProperty("type")]
        public MessageAnnouncementRequestType? Type { get; set; }
    }
}
