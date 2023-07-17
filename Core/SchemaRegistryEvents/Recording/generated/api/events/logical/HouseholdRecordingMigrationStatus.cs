namespace Phoenix.Generated.Api.Events.Logical.HouseholdRecordingMigrationStatus
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// contains status information relating to an attempt to import a household recording from
    /// legacy system to KTP via previous partnerMigrationHouseholdRecording event
    ///
    /// A base event for all Logical events, contains common properties
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class HouseholdRecordingMigrationStatus
    {
        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        [JsonProperty("code")]
        public Code Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("ottUserExternalId")]
        public string OttUserExternalId { get; set; }

        [JsonProperty("programAssetExternalId")]
        public string ProgramAssetExternalId { get; set; }

        [JsonProperty("recordingId", NullValueHandling = NullValueHandling.Ignore)]
        public long? RecordingId { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }

    [JsonConverter(typeof(CodeConverter))]
    public enum Code { The0, The1, The1006, The11003, The2000, The2038, The3040, The3043, The3094, The4028, The4033, The4038, The4039, The4040, The4041, The4064, The4078, The4111, The4122, The4123, The4124, The4125, The4126, The5005, The5022, The5033, The5097, The9011 };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                CodeConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class CodeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Code) || t == typeof(Code?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "0":
                    return Code.The0;
                case "1":
                    return Code.The1;
                case "1006":
                    return Code.The1006;
                case "11003":
                    return Code.The11003;
                case "2000":
                    return Code.The2000;
                case "2038":
                    return Code.The2038;
                case "3040":
                    return Code.The3040;
                case "3043":
                    return Code.The3043;
                case "3094":
                    return Code.The3094;
                case "4028":
                    return Code.The4028;
                case "4033":
                    return Code.The4033;
                case "4038":
                    return Code.The4038;
                case "4039":
                    return Code.The4039;
                case "4040":
                    return Code.The4040;
                case "4041":
                    return Code.The4041;
                case "4064":
                    return Code.The4064;
                case "4078":
                    return Code.The4078;
                case "4111":
                    return Code.The4111;
                case "4122":
                    return Code.The4122;
                case "4123":
                    return Code.The4123;
                case "4124":
                    return Code.The4124;
                case "4125":
                    return Code.The4125;
                case "4126":
                    return Code.The4126;
                case "5005":
                    return Code.The5005;
                case "5022":
                    return Code.The5022;
                case "5033":
                    return Code.The5033;
                case "5097":
                    return Code.The5097;
                case "9011":
                    return Code.The9011;
            }
            throw new Exception("Cannot unmarshal type Code");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Code)untypedValue;
            switch (value)
            {
                case Code.The0:
                    serializer.Serialize(writer, "0");
                    return;
                case Code.The1:
                    serializer.Serialize(writer, "1");
                    return;
                case Code.The1006:
                    serializer.Serialize(writer, "1006");
                    return;
                case Code.The11003:
                    serializer.Serialize(writer, "11003");
                    return;
                case Code.The2000:
                    serializer.Serialize(writer, "2000");
                    return;
                case Code.The2038:
                    serializer.Serialize(writer, "2038");
                    return;
                case Code.The3040:
                    serializer.Serialize(writer, "3040");
                    return;
                case Code.The3043:
                    serializer.Serialize(writer, "3043");
                    return;
                case Code.The3094:
                    serializer.Serialize(writer, "3094");
                    return;
                case Code.The4028:
                    serializer.Serialize(writer, "4028");
                    return;
                case Code.The4033:
                    serializer.Serialize(writer, "4033");
                    return;
                case Code.The4038:
                    serializer.Serialize(writer, "4038");
                    return;
                case Code.The4039:
                    serializer.Serialize(writer, "4039");
                    return;
                case Code.The4040:
                    serializer.Serialize(writer, "4040");
                    return;
                case Code.The4041:
                    serializer.Serialize(writer, "4041");
                    return;
                case Code.The4064:
                    serializer.Serialize(writer, "4064");
                    return;
                case Code.The4078:
                    serializer.Serialize(writer, "4078");
                    return;
                case Code.The4111:
                    serializer.Serialize(writer, "4111");
                    return;
                case Code.The4122:
                    serializer.Serialize(writer, "4122");
                    return;
                case Code.The4123:
                    serializer.Serialize(writer, "4123");
                    return;
                case Code.The4124:
                    serializer.Serialize(writer, "4124");
                    return;
                case Code.The4125:
                    serializer.Serialize(writer, "4125");
                    return;
                case Code.The4126:
                    serializer.Serialize(writer, "4126");
                    return;
                case Code.The5005:
                    serializer.Serialize(writer, "5005");
                    return;
                case Code.The5022:
                    serializer.Serialize(writer, "5022");
                    return;
                case Code.The5033:
                    serializer.Serialize(writer, "5033");
                    return;
                case Code.The5097:
                    serializer.Serialize(writer, "5097");
                    return;
                case Code.The9011:
                    serializer.Serialize(writer, "9011");
                    return;
            }
            throw new Exception("Cannot marshal type Code");
        }

        public static readonly CodeConverter Singleton = new CodeConverter();
    }
}
