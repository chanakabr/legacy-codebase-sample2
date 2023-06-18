namespace Phoenix.Generated.Api.Events.Logical.Gdpr.ObjectCleanupComplete
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Indicates that the referenced KTP Service has successfully handled removal of data
    /// related to the referenced Kafka Delete message
    ///
    /// Indicates that the referenced KTP Service has successfully handled removal of data
    /// related to the referenced Kafka Delete message, or expiration of personal data activity
    /// retention period.
    ///
    /// A base event for all Logical events, contains common properties
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class ObjectCleanupComplete
    {
        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        /// <summary>
        /// service which produced the Cleanup Complete event
        /// </summary>
        [JsonProperty("serviceName", NullValueHandling = NullValueHandling.Ignore)]
        public string ServiceName { get; set; }

        /// <summary>
        /// identifier of the Household/User/Device whose delete event was successfully handled
        /// </summary>
        [JsonProperty("cleanedObjectId")]
        public long CleanedObjectId { get; set; }

        /// <summary>
        /// the type of Kafka delete message which has been handled successfully. E.g.
        /// 'events.crud.Household', 'events.logical.gdpr.householdRetentionPeriodExpired'
        /// </summary>
        [JsonProperty("sourceMessageTypeId")]
        public string SourceMessageTypeId { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
