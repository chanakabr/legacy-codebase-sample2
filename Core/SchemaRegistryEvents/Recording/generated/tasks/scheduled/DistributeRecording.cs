namespace Phoenix.Generated.Tasks.Scheduled.DistributeRecording
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// distribute recording (distribute to users the recording of Program as part of series)
    /// scheduled task definition.
    ///
    /// A base event for all scheduled tasks
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class DistributeRecording
    {
        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        [JsonProperty("epgStartDate", NullValueHandling = NullValueHandling.Ignore)]
        public long? EpgStartDate { get; set; }

        [JsonProperty("maxDomainSeriesId", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxDomainSeriesId { get; set; }

        [JsonProperty("programId", NullValueHandling = NullValueHandling.Ignore)]
        public long? ProgramId { get; set; }

        [JsonProperty("recordingId", NullValueHandling = NullValueHandling.Ignore)]
        public long? RecordingId { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
