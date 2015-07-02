using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class GenericRule
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("rule_type")]
        public eGroupRuleType RuleType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        public GenericRule(TVPPro.SiteManager.TvinciPlatform.api.GenericRule rule)
        {
            Id = rule.Id;
            Name = rule.Name;
            Description = rule.Description;
        }
    }
}
