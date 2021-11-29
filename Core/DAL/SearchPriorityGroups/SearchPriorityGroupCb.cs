using System.Collections.Generic;
using ApiObjects.SearchPriorityGroups;
using Newtonsoft.Json;

namespace DAL.SearchPriorityGroups
{
    public class SearchPriorityGroupCb
    {
        [JsonProperty("name")]
        public Dictionary<string, string> Name { get; set; }

        [JsonProperty("criteria")]
        public SearchPriorityCriteriaCb Criteria { get; set; }

        public SearchPriorityGroupCb()
        {
        }

        public SearchPriorityGroupCb(Dictionary<string, string> name, SearchPriorityCriteriaType type, string value)
        {
            Name = name;
            Criteria = new SearchPriorityCriteriaCb(type, value);
        }
    }
}