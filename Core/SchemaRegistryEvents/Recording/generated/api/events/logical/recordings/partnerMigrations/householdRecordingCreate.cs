namespace Phoenix.Generated.Api.Events.Logical.Recordings.Partnermigrations.HouseholdRecordingCreate
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// A migration event for creation of Household Recordings per partner
    ///
    /// A base event for all Logical events, contains common properties
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class HouseholdRecordingCreate
    {
        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        [JsonProperty("absoluteEndDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? AbsoluteEndDateTime { get; set; }

        [JsonProperty("absoluteStartDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? AbsoluteStartDateTime { get; set; }

        [JsonProperty("endPaddingIsPersonal", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EndPaddingIsPersonal { get; set; }

        [JsonProperty("liveAssetExternalId", NullValueHandling = NullValueHandling.Ignore)]
        public string LiveAssetExternalId { get; set; }

        [JsonProperty("ottUserExternalId")]
        public string OttUserExternalId { get; set; }

        [JsonProperty("paddingEndSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public long? PaddingEndSeconds { get; set; }

        [JsonProperty("paddingStartSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public long? PaddingStartSeconds { get; set; }

        [JsonProperty("programAssetCrid", NullValueHandling = NullValueHandling.Ignore)]
        public string ProgramAssetCrid { get; set; }

        [JsonProperty("programAssetEndDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? ProgramAssetEndDateTime { get; set; }

        [JsonProperty("programAssetExternalId")]
        public string ProgramAssetExternalId { get; set; }

        [JsonProperty("programAssetImages", NullValueHandling = NullValueHandling.Ignore)]
        public ProgramAssetImage[] ProgramAssetImages { get; set; }

        [JsonProperty("programAssetMetas", NullValueHandling = NullValueHandling.Ignore)]
        public AssetMeta[] ProgramAssetMetas { get; set; }

        [JsonProperty("programAssetmultilingualDescription", NullValueHandling = NullValueHandling.Ignore)]
        public TranslationValue[] ProgramAssetmultilingualDescription { get; set; }

        [JsonProperty("programAssetmultilingualName", NullValueHandling = NullValueHandling.Ignore)]
        public TranslationValue[] ProgramAssetmultilingualName { get; set; }

        [JsonProperty("programAssetStartDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? ProgramAssetStartDateTime { get; set; }

        [JsonProperty("programAssetTags", NullValueHandling = NullValueHandling.Ignore)]
        public AssetTag[] ProgramAssetTags { get; set; }

        [JsonProperty("startPaddingIsPersonal", NullValueHandling = NullValueHandling.Ignore)]
        public bool? StartPaddingIsPersonal { get; set; }
    }

    public partial class ProgramAssetImage
    {
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("imageTypeId")]
        public long ImageTypeId { get; set; }

        [JsonProperty("imageTypeName", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageTypeName { get; set; }

        [JsonProperty("ratio", NullValueHandling = NullValueHandling.Ignore)]
        public string Ratio { get; set; }

        [JsonProperty("sourceUrl")]
        public string SourceUrl { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public long? Version { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }
    }

    /// <summary>
    /// Asset meta definition
    /// </summary>
    public partial class AssetMeta
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("translations", NullValueHandling = NullValueHandling.Ignore)]
        public TranslationValue[] Translations { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    /// <summary>
    /// Translation value definition
    /// </summary>
    public partial class TranslationValue
    {
        [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
        public string Language { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    /// <summary>
    /// Asset tag definition
    /// </summary>
    public partial class AssetTag
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
        public AssetTagValue[] Values { get; set; }
    }

    /// <summary>
    /// Asset tag value definition
    /// </summary>
    public partial class AssetTagValue
    {
        [JsonProperty("translations", NullValueHandling = NullValueHandling.Ignore)]
        public TranslationValue[] Translations { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
