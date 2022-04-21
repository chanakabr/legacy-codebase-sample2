using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiObjects.Rules.Converters
{
    public class AssetConditionBaseConverter : JsonConverter<AssetConditionBase>
    {
        private const string TYPE_PROPERTY = "$type";
        private const string ASSET_CONDITION_TYPE = "ApiObjects.Rules.AssetCondition, ApiObjects";
        private const string ASSET_SHOP_CONDITION_TYPE = "ApiObjects.Rules.AssetShopCondition, ApiObjects";

        public override void WriteJson(JsonWriter writer, AssetConditionBase value, JsonSerializer serializer)
        {
            serializer.TypeNameHandling = TypeNameHandling.All;
            serializer.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
            serializer.Serialize(writer, value, value?.GetType());
        }

        public override AssetConditionBase ReadJson(JsonReader reader, Type objectType, AssetConditionBase existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            AssetConditionBase conditionBase;

            var jObject = JObject.Load(reader);
            var typeName = jObject[TYPE_PROPERTY]?.Value<string>();
            if (string.IsNullOrEmpty(typeName) || typeName == ASSET_CONDITION_TYPE)
            {
                conditionBase = new AssetCondition();
            }
            else if (typeName == ASSET_SHOP_CONDITION_TYPE)
            {
                conditionBase = new AssetShopCondition();
            }
            else
            {
                throw new Exception($"{nameof(AssetConditionBase)} can't be deserialized: {jObject}.");
            }

            serializer.Populate(jObject.CreateReader(), conditionBase);

            return conditionBase;
        }
    }
}