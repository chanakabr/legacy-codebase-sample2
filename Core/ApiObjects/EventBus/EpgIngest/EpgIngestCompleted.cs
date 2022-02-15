using System.Collections.Generic;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApiObjects.EventBus.EpgIngest
{
    public class EpgIngestCompleted : BaseEpgIngestEvent
    {
        public EpgIngestCompleted() : base("events.logical.ingest.epg.IngestCompleted")
        {
        }
        
        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EpgIngestCompletionStatus Status { get; set; }
        
        [JsonProperty("completedDate")]
        public long CompletedDate { get; set; }
        
        [JsonProperty("errors")]
        public IEnumerable<Status> Errors { get; set; }
    }

    public enum EpgIngestCompletionStatus
    {
        SUCCESS,
        PARTIAL_FAILURE,
        TOTAL_FAILURE
    }
}