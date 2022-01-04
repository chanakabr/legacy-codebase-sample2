using System;
using System.Linq;
using ApiObjects.SearchObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DAL.SearchObjects.Converters
{
    public class AssetOrderConverter : JsonConverter<AssetOrder>
    {
        private const string TYPE_PROPERTY = "type";
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { ContractResolver = new AssetOrderContractResolver() };

        public override void WriteJson(JsonWriter writer, AssetOrder value, JsonSerializer serializer)
        {
            var type = value.GetType();
            var jObject = new JObject { { TYPE_PROPERTY, type.Name } };
            foreach (var prop in type.GetProperties().Where(x => x.CanRead))
            {
                var propVal = prop.GetValue(value, null);
                if (propVal != null)
                {
                    jObject.Add(prop.Name, JToken.FromObject(propVal, serializer));
                }
            }

            jObject.WriteTo(writer);
        }

        public override AssetOrder ReadJson(JsonReader reader, Type objectType, AssetOrder existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var channelOrderType = jObject[TYPE_PROPERTY]?.Value<string>();
            var channelOrderJson = jObject.ToString();
            switch (channelOrderType)
            {
                case nameof(AssetOrder):
                    return jObject.ToObject<AssetOrder>(JsonSerializer.Create(SerializerSettings));
                case nameof(AssetOrderByMeta):
                    return jObject.ToObject<AssetOrderByMeta>(JsonSerializer.Create(SerializerSettings));
                case nameof(AssetSlidingWindowOrder):
                    return jObject.ToObject<AssetSlidingWindowOrder>(JsonSerializer.Create(SerializerSettings));
                default:
                    throw new Exception($"{nameof(AssetOrder)} can't be deserialized: {channelOrderJson}.");
            }
        }

        private class AssetOrderContractResolver : DefaultContractResolver
        {
            protected override JsonConverter ResolveContractConverter(Type objectType)
            {
                return typeof(AssetOrder).IsAssignableFrom(objectType)
                    ? null
                    : base.ResolveContractConverter(objectType);
            }
        }
    }
}