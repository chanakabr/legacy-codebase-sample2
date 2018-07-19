using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects.Rules
{
    [Serializable]
    public class AssetRule
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Conditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<AssetRuleCondition> Conditions { get; set; }

        [JsonProperty(PropertyName = "Actions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<AssetRuleAction> Actions { get; set; }

        [JsonProperty("GroupId")]
        public int GroupId { get; set; }
    }

    [Serializable]
    public class AssetRuleTypeMapping
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("TypeIdIn")]
        public List<int> TypeIdIn { get; set; }
    }
}
