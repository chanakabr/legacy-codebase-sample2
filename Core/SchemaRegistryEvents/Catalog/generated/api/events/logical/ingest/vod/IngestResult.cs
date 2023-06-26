namespace Phoenix.Generated.Api.Events.Logical.Ingest.Vod.IngestResult
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Describes ingest result for VOD asset
    ///
    /// A base event for all Logical events, contains common properties
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class IngestResult
    {
        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        /// <summary>
        /// Optional KTP asset ID (BE internal asset ID) returned from the BE in case of a successful
        /// asset ingest
        /// </summary>
        [JsonProperty("assetId", NullValueHandling = NullValueHandling.Ignore)]
        public long? AssetId { get; set; }

        /// <summary>
        /// The asset name as written in media.basic.name element of the TvinciXML. It is a mandatory
        /// TvinciXML element and will not be sent only in case of a NameRequired error
        /// </summary>
        [JsonProperty("assetName", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }

        /// <summary>
        /// The unique assetStruct system name
        /// </summary>
        [JsonProperty("assetStructSystemName", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetStructSystemName { get; set; }

        /// <summary>
        /// Errors for a single ingested asset
        /// </summary>
        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
        public Error[] Errors { get; set; }

        /// <summary>
        /// Optional external asset ID / co-guid. It is a mandatory TvinciXML element and will not
        /// exist only in case of a MissingExternalIdentifier error.
        /// </summary>
        [JsonProperty("externalAssetId", NullValueHandling = NullValueHandling.Ignore)]
        public string ExternalAssetId { get; set; }

        /// <summary>
        /// The VOD ingest file name conveyed in the SOAP request sent to the WS_ingest endpoint
        /// </summary>
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("ingestDate")]
        public long IngestDate { get; set; }

        /// <summary>
        /// The shop ID to which this asset is associated. If the asset is not assigned to any shop
        /// then this parameter is omitted
        /// </summary>
        [JsonProperty("shopAssetUserRuleId", NullValueHandling = NullValueHandling.Ignore)]
        public long? ShopAssetUserRuleId { get; set; }

        /// <summary>
        /// Possible status values are FAILED, SUCCESS, SUCCESS_WARNING
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Minor errors (=warnings) which do not prevent the asset from being ingested
        /// </summary>
        [JsonProperty("warnings", NullValueHandling = NullValueHandling.Ignore)]
        public Error[] Warnings { get; set; }
    }

    /// <summary>
    /// Detailed description of an error
    /// </summary>
    public partial class Error
    {
        /// <summary>
        /// The message code
        /// </summary>
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public long? Code { get; set; }

        /// <summary>
        /// The message description with arguments placeholders
        /// </summary>
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
