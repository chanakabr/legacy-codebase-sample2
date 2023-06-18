namespace Phoenix.Generated.Api.Events.Logical.PersonalActivityCleanup
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Indicates that its time to cleanup old personal data activity records for partner in
    /// question.
    ///
    /// Indicates that its time to cleanup old personal data activity records for partner in
    /// configuration
    ///
    /// A base event for all Logical events, contains common properties
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class PersonalActivityCleanup
    {
        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        /// <summary>
        /// the key of personaActivityCleanup is its execution time
        /// </summary>
        [JsonProperty("key")]
        public long Key { get; set; }

        /// <summary>
        /// retention period in days as it configured at the time that the message is distributed
        /// </summary>
        [JsonProperty("retentionPeriodDays")]
        public long RetentionPeriodDays { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
