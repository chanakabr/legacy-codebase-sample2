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
    /// Asset base definition.
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class ProgramAsset
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

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("endDate")]
        public long EndDate { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("images", NullValueHandling = NullValueHandling.Ignore)]
        public Image[] Images { get; set; }

        [JsonProperty("metas", NullValueHandling = NullValueHandling.Ignore)]
        public AssetMeta[] Metas { get; set; }

        [JsonProperty("multilingualDescription", NullValueHandling = NullValueHandling.Ignore)]
        public TranslationValue[] MultilingualDescription { get; set; }

        [JsonProperty("multilingualName", NullValueHandling = NullValueHandling.Ignore)]
        public TranslationValue[] MultilingualName { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("startDate")]
        public long StartDate { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public AssetTag[] Tags { get; set; }

        [JsonProperty("updaterId")]
        public long UpdaterId { get; set; }

        [JsonProperty("crid", NullValueHandling = NullValueHandling.Ignore)]
        public string Crid { get; set; }

        [JsonProperty("epgChannelId")]
        public long EpgChannelId { get; set; }

        [JsonProperty("epgId", NullValueHandling = NullValueHandling.Ignore)]
        public string EpgId { get; set; }

        [JsonProperty("expirationDate", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExpirationDate { get; set; }

        [JsonProperty("externalOfferIds", NullValueHandling = NullValueHandling.Ignore)]
        public string[] ExternalOfferIds { get; set; }

        [JsonProperty("linearAssetId")]
        public long LinearAssetId { get; set; }
    }

    /// <summary>
    /// Asset image definition
    /// </summary>
    public partial class Image
    {
        [JsonProperty("contentId", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentId { get; set; }

        [JsonProperty("imageObjectId")]
        public long ImageObjectId { get; set; }

        [JsonProperty("imageTypeId")]
        public long ImageTypeId { get; set; }

        [JsonProperty("sourceUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string SourceUrl { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }
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

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
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

    public enum Status { Failed, Pending, Ready };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                StatusConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class StatusConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Status) || t == typeof(Status?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "failed":
                    return Status.Failed;
                case "pending":
                    return Status.Pending;
                case "ready":
                    return Status.Ready;
            }
            throw new Exception("Cannot unmarshal type Status");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Status)untypedValue;
            switch (value)
            {
                case Status.Failed:
                    serializer.Serialize(writer, "failed");
                    return;
                case Status.Pending:
                    serializer.Serialize(writer, "pending");
                    return;
                case Status.Ready:
                    serializer.Serialize(writer, "ready");
                    return;
            }
            throw new Exception("Cannot marshal type Status");
        }

        public static readonly StatusConverter Singleton = new StatusConverter();
    }
}
