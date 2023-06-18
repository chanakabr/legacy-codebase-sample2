namespace Phoenix.Generated.Api.Events.Crud.HouseholdDevice
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An event used to reflect a crud operation in a household device
    ///
    /// A base event for all CRUD events, contains common properties
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class HouseholdDevice
    {
        /// <summary>
        /// enum values - Created=0, Updated=1,Deleted=2
        /// </summary>
        [JsonProperty("operation")]
        public long Operation { get; set; }

        [JsonProperty("partnerId")]
        public long PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// unique device identifier
        /// </summary>
        [JsonProperty("udid", NullValueHandling = NullValueHandling.Ignore)]
        public string Udid { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
