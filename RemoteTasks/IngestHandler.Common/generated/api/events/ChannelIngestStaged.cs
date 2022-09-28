namespace Ingesthandler.common.Generated.Api.Events.ChannelIngestStaged
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// an event used to indicate that the data for a given EPG channel has been staged and ready
    /// for ingest
    /// </summary>
    public partial class ChannelIngestStaged
    {
        /// <summary>
        /// the id of the bulk upload containing the file that was uploaded for the ingest process
        /// </summary>
        [JsonProperty("bulkUploadId", NullValueHandling = NullValueHandling.Ignore)]
        public long? BulkUploadId { get; set; }

        /// <summary>
        /// this is the linearMediaId aka: KalturaLiveAsset id representing the channel that contains
        /// the EPG programs
        /// </summary>
        [JsonProperty("linearChannelId", NullValueHandling = NullValueHandling.Ignore)]
        public long? LinearChannelId { get; set; }

        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }
    }
}
