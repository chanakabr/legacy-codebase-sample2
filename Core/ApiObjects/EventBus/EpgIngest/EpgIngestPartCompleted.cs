using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.EventBus.EpgIngest
{
    public class EpgIngestPartCompleted : BaseEpgIngestEvent
    {
        public EpgIngestPartCompleted() : base("events.logical.ingest.epg.IngestPartCompleted")
        {
        }

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("results")]
        public IEnumerable<EpgIngestResult> Results { get; set; }
    }
}