using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ApiObjects.Json.Converters;

namespace ActionRuleHandler
{
    [Serializable]
    public class ActionRuleRequest
    {
        [JsonProperty("group_id")]
        public int GroupId
        {
            get;
            set;
        }

        [JsonProperty("rule_ids")]
        public List<int> RuleIds
        {
            get;
            set;
        }
    }
}
