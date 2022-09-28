namespace Ingesthandler.common.Generated.Api.Events.UpdateBulkUpload
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// an event used to indicate that the data for a given EPG channel has been updated with
    /// relevant updates
    /// </summary>
    public partial class UpdateBulkUpload
    {
        /// <summary>
        /// the id of the bulk upload containing the file that was uploaded for the ingest process
        /// </summary>
        [JsonProperty("bulkUploadId", NullValueHandling = NullValueHandling.Ignore)]
        public long? BulkUploadId { get; set; }

        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        /// <summary>
        /// if true then bulkUpload is in fatal failure and should override any other status, in case
        /// there are no other successes, in which case it will be set to partial
        /// </summary>
        [JsonProperty("setStatusToFatal", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SetStatusToFatal { get; set; }
    }
}
