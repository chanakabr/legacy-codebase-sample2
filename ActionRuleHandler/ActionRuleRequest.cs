using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ApiObjects.Json.Converters;
using ApiObjects;

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
        public List<long> RuleIds
        {
            get;
            set;
        }

        [JsonProperty("action_type")]
        public RuleActioType ActionType
        {
            get;
            set;
        }
    }
}
