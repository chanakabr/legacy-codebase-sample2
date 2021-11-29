using ApiObjects.SearchPriorityGroups;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DAL.SearchPriorityGroups
{
    public class SearchPriorityCriteriaCb
    {
        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public SearchPriorityCriteriaType Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public SearchPriorityCriteriaCb(SearchPriorityCriteriaType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}