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
        public RuleType RuleType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        public GenericRule(TVPPro.SiteManager.TvinciPlatform.api.GenericRule rule)
        {
            Id = rule.Id;
            Name = rule.Name;
            Description = rule.Description;
            switch (rule.RuleType)
            {
                case TVPPro.SiteManager.TvinciPlatform.api.RuleType.Parental:
                    RuleType = Responses.RuleType.Parental;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.api.RuleType.Geo:
                    RuleType = Responses.RuleType.Geo;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.api.RuleType.UserType:
                    RuleType = Responses.RuleType.UserType;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.api.RuleType.Device:
                    RuleType = Responses.RuleType.Device;
                    break;
                default:
                    throw new Exception("missing RuleType value");
            }
        }
    }

    public enum RuleType
    {
        Parental = 0,
        Geo = 1,
        UserType = 2,
        Device = 3
    }
}
