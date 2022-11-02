namespace Phoenix.Generated.Api.Events.Crud.SegmentationType
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An event used to reflect a crud operation in a segmentation type
    ///
    /// An event used to reflect a crud operation on a segmentation Type
    ///
    /// A base event for all CRUD events, contains common properties
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class SegmentationType
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

        [JsonProperty("createDate", NullValueHandling = NullValueHandling.Ignore)]
        public long? CreateDate { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("segmentsValues", NullValueHandling = NullValueHandling.Ignore)]
        public SegmentsValue[] SegmentsValues { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }

    public partial class SegmentsValue
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("Name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("Value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }
}
