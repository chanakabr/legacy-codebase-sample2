namespace Phoenix.Generated.Api.Events.Logical.PersonalActivityCleanupComplete
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Indicates that the referenced KTP Service complete cleanup process. The message includes
    /// completion status (fail/success).
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
    public partial class PersonalActivityCleanupComplete
    {
        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        /// <summary>
        /// The cleanup result.
        /// </summary>
        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public Result Result { get; set; }

        /// <summary>
        /// service which produced the Cleanup Complete event
        /// </summary>
        [JsonProperty("serviceName", NullValueHandling = NullValueHandling.Ignore)]
        public string ServiceName { get; set; }

        /// <summary>
        /// Execution time in epoch (seconds)
        /// </summary>
        [JsonProperty("executionTimeEpoch", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExecutionTimeEpoch { get; set; }
    }

    /// <summary>
    /// The cleanup result.
    /// </summary>
    public partial class Result
    {
        /// <summary>
        /// The failure reason description - Shall be embeded in the OpsGenie alert description
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// The operation status (success, or fail)
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
