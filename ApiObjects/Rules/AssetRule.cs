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

        [JsonProperty("Conditions")]
        public List<AssetRuleCondition> Conditions { get; set; }

        [JsonProperty("Actions")]
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
    }
}
