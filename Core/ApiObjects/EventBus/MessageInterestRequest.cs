using EventBus.Abstraction;
using Newtonsoft.Json;
using System;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class MessageInterestRequest : DelayedServiceEvent
    {
        [JsonProperty("start_time")]
        public long StartTime { get; set; }

        [JsonProperty("message_interest_id")]
        public int MessageInterestId { get; set; }

        public override string ToString()
        {
            return $"StartTime:{StartTime}, MessageInterestId:{MessageInterestId}";
        }
    }
}