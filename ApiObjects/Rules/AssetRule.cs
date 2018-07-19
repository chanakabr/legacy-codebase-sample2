using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        
        public bool HasCountryConditions()
        {
            if (this.Conditions != null && this.Conditions.Count > 0)
            {
                return this.Conditions.Exists(x => x.Type == AssetRuleConditionType.Country);
            }

            return false;
        }

        public AssetRuleConcurrency GetAssetRuleConcurrency()
        {
            return new AssetRuleConcurrency()
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                GroupId = this.GroupId,
                Conditions = this.Conditions == null? null : new List<ConcurrencyCondition>(this.Conditions.Where(x=> x is ConcurrencyCondition).Select(x => x as ConcurrencyCondition)),
                Actions = this.Actions == null ? null : new List<AssetBlockAction>(this.Actions.Where(x => x is AssetBlockAction).Select(x => x as AssetBlockAction))
            };
        }
    }

    public class AssetRuleConcurrency
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int GroupId { get; set; }
        public List<ConcurrencyCondition> Conditions { get; set; }
        public List<AssetBlockAction> Actions { get; set; }
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
