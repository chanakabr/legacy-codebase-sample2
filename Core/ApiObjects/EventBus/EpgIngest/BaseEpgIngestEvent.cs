using EventBus.Abstraction;
using Newtonsoft.Json;

namespace ApiObjects.EventBus.EpgIngest
{
    public class BaseEpgIngestEvent : ServiceEvent
    {
        public override string EventKey => IngestId.ToString();

        [JsonProperty("partnerId")]
        public long PartnerId
        {
            get => GroupId;
            set => GroupId = (int) value;
        }

        [JsonProperty("ingestId")]
        public long IngestId { get; set; }

        private BaseEpgIngestEvent() {}

        public BaseEpgIngestEvent(string topic)
        {
            EventNameOverride = topic;
        }
    }
}