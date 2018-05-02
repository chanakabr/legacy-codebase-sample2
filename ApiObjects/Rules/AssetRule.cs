using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects.Rules
{
    public class AssetRule
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AssetRuleCondition> Conditions { get; set; }
        public List<AssetRuleAction> Actions { get; set; }
        public int GroupId { get; set; }
    }

    public class AssetRuleAction
    {
        public long Id { get; set; }
        public RuleActionType Type { get; set; }
        public string Description { get; set; }
    }

    public class AccessControlBlockAction : AssetRuleAction
    {
    }

    public class TimeOffsetRuleAction : AssetRuleAction
    {
        public int Offset { get; set; }
        public bool TimeZone { get; set; }
    }

    public class EndDateOffsetRuleAction : TimeOffsetRuleAction
    {
    }

    public class StartDateOffsetRuleAction : TimeOffsetRuleAction
    {
    }

    public class AssetRuleCondition
    {
        public int Id { get; set; }
        public AssetRuleConditionType Type { get; set; }
        public string Description { get; set; }
    }

    public class AssetCondition : AssetRuleCondition
    {
        public string Ksql { get; set; }
    }

    public class CountryCondition : AssetRuleCondition
    {
        public bool Not { get; set; }

        public List<int> Countries { get; set; }
    }
}
