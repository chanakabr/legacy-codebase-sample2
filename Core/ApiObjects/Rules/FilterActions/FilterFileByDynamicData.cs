using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.Rules.FilterActions
{
    public abstract class FilterFileByDynamicData : AssetRuleFilterAction
    {
        [JsonProperty("Key")]
        public string Key { get; set; }

        [JsonProperty("Values")]
        public IReadOnlyCollection<string> Values { get; set; }
    }
}