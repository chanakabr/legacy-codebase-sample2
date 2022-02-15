using System.Collections.Generic;
using ApiObjects.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApiObjects.EventBus.EpgIngest
{
    public class EpgIngestResult
    {
        [JsonProperty("startDate")]
        public long StartDate { get; set; }

        [JsonProperty("endDate")]
        public long EndDate { get; set; }

        [JsonProperty("externalProgramId")]
        public string ExternalProgramId { get; set; }

        [JsonProperty("indexInFile")]
        public int IndexInFile { get; set; }

        [JsonProperty("programId")]
        public long? ProgramId { get; set; }

        [JsonProperty("linearChannelId")]
        public long LinearChannelId { get; set; }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProgramIngestResultStatus Status { get; set; }

        [JsonProperty("errors")]
        public IEnumerable<Status> Errors { get; set; }

        [JsonProperty("warnings")]
        public IEnumerable<Status> Warnings { get; set; }
    }
}