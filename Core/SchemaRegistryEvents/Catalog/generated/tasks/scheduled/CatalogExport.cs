namespace Phoenix.Generated.Tasks.Scheduled.CatalogExport
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
    public partial class CatalogExport
    {
        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }
        
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
