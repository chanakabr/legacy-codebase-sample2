namespace Phoenix.Generated.Api.Events.Crud.Entitlement
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An event used to reflect a crud operation in a entitlement
    ///
    /// event used to reflect a crud operation in Entitlement
    ///
    /// A base event for all CRUD events, contains common properties
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class Entitlement
    {
        /// <summary>
        /// enum values - Created=0, Updated=1,Deleted=2
        /// </summary>
        [JsonProperty("operation", NullValueHandling = NullValueHandling.Ignore)]
        public long? Operation { get; set; }

        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        [JsonProperty("endDate", NullValueHandling = NullValueHandling.Ignore)]
        public long? EndDate { get; set; }

        [JsonProperty("householdId", NullValueHandling = NullValueHandling.Ignore)]
        public long? HouseholdId { get; set; }

        [JsonProperty("productId", NullValueHandling = NullValueHandling.Ignore)]
        public long? ProductId { get; set; }

        [JsonProperty("productTypeId", NullValueHandling = NullValueHandling.Ignore)]
        public long? ProductTypeId { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
