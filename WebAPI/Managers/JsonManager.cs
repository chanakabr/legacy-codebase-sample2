using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.General;
using WebAPI.Reflection;

namespace WebAPI.Managers
{
    public class JsonManager
    {
        private static JsonManager instance;

        private JsonManager()
        {   
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                Converters = new List<JsonConverter> { new MultiStringJsonConverter(), new EnumConverter(), new DoubleConverter() },
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public static JsonManager GetInstance()
        {
            if (instance == null)
            {
                instance = new JsonManager();
            }

            return instance;
        }

        public class KalturaObjectContractResolver : DefaultContractResolver
        {
            private bool shouldOmitObsolete;

            public KalturaObjectContractResolver(bool omitObsolete)
            {
                shouldOmitObsolete = omitObsolete;
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

                if (typeof(KalturaOTTObject).IsAssignableFrom(type))
                {
                    if (shouldOmitObsolete)
                    {
                        return properties.Where(property => !DataModel.IsObsolete(type, property.UnderlyingName)).ToList();
                    }
                    else
                    {
                        return properties.Where(property => !DataModel.IsDeprecated(type, property.UnderlyingName)).ToList();
                    }
                }
                else
                {
                    return properties;
                }
            }
        }

        public class MultiStringJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JToken jToken = value.ToString();
                jToken.WriteTo(writer);

                string language = Utils.Utils.GetLanguageFromRequest();
                if (language == null || !language.Equals("*"))
                {
                    return;
                }

                string name = writer.Path.Substring(writer.Path.LastIndexOf('.') + 1);
                string multilingualName = KalturaMultilingualString.GetMultilingualName(name);

                KalturaMultilingualString multilingualString = (KalturaMultilingualString)value;
                if (multilingualString.Values != null)
                {
                    writer.WritePropertyName(multilingualName);
                    jToken = JToken.FromObject(multilingualString.Values);
                    jToken.WriteTo(writer);
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(KalturaMultilingualString);
            }
        }

        public class EnumConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Type type = value.GetType();
                JToken jToken;

                if (type.IsEnum)
                {
                    jToken = Enum.GetName(type, value);
                }
                else
                {
                    jToken = Enum.GetName(Nullable.GetUnderlyingType(type), value);
                }
                jToken.WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType.IsEnum || (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>) && Nullable.GetUnderlyingType(objectType).IsEnum);
            }
        }

        public class DoubleConverter : JsonConverter
        {
            private JToken GetToken(double value)
            {
                if ((value % 1) == 0)
                {
                    return (int)value;
                }

                return value;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JToken jToken = null;
                if (value.GetType().IsGenericType)
                {
                    double? nullableValue = (double?)value;
                    if (!nullableValue.HasValue)
                    {
                        jToken = GetToken(nullableValue.Value);
                    }
                }
                else
                {
                    jToken = GetToken((double)value);
                }

                jToken.WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(double) || (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>) && Nullable.GetUnderlyingType(objectType) == typeof(double));
            }
        }

        public string Serialize(object value, bool omitObsolete = false)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings{ContractResolver = new KalturaObjectContractResolver(omitObsolete)});
        }
    }
}