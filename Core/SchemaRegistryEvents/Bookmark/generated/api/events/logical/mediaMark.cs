namespace Phoenix.Generated.Api.Events.Logical.mediaMark
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// mediaMark
    ///
    /// Message that come from ccproxy for media mark
    ///
    /// A base event for all Logical events, contains common properties
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class MediaMark
    {
        [JsonProperty("partnerId")]
        public long PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; set; }

        [JsonProperty("assetId")]
        public string AssetId { get; set; }

        [JsonProperty("assetType", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetType { get; set; }

        [JsonProperty("domainId")]
        public long DomainId { get; set; }

        [JsonProperty("fileId", NullValueHandling = NullValueHandling.Ignore)]
        public long? FileId { get; set; }

        [JsonProperty("ip", NullValueHandling = NullValueHandling.Ignore)]
        public string Ip { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public long? Position { get; set; }

        [JsonProperty("programId")]
        public long ProgramId { get; set; }

        [JsonProperty("udid", NullValueHandling = NullValueHandling.Ignore)]
        public string Udid { get; set; }

        [JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
