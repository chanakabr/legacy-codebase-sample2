using Newtonsoft.Json;

namespace ApiObjects.EventBus.EpgIngest
{
    public class EpgIngestStarted : BaseEpgIngestEvent
    {
        public EpgIngestStarted() : base("events.logical.ingest.epg.IngestStarted")
        {
        }
        
        [JsonProperty("createdDate")]
        public long CreatedDate { get; set; }
        
        [JsonProperty("ingestProfileId")]
        public int? IngestProfileId { get; set; }
        
        [JsonProperty("ingestFileName")]
        public string IngestFileName { get; set; }
        
        [JsonProperty("ingestedByUserId")]
        public long IngestedByUserId
        {
            get => UserId;
            set => UserId = value;
        }
    }
}