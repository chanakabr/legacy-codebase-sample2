namespace Phoenix.Generated.Api.Events.Crud.ProgramAsset
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An event used to reflect a crud operation in a program asset
    ///
    /// temp event used to reflect a crud operation in programAsset
    ///
    /// A base event for all CRUD events, contains common properties
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class ProgramAsset
    {
        [JsonProperty("operation", NullValueHandling = NullValueHandling.Ignore)]
        public long? Operation { get; set; }

        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        [JsonProperty("expirationDate", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExpirationDate { get; set; }

        [JsonProperty("externalOfferIds", NullValueHandling = NullValueHandling.Ignore)]
        public string[] ExternalOfferIds { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
