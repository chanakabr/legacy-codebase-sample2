using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.Rules
{
    public interface IDynamicKeysConditionScope : IConditionScope
    {
        Dictionary<string, List<string>> SessionCharacteristics { get; }
    }

    [Serializable]
    public class DynamicKeysCondition : RuleCondition
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        
        [JsonProperty("values")]
        public List<string> Values { get; set; }

        public DynamicKeysCondition()
        {
            Type = RuleConditionType.DynamicKeys;
        }
    }
}