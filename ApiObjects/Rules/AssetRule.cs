using System.Collections.Generic;

namespace ApiObjects.Rules
{
    public class AssetRule
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AssetRuleCondition> Conditions { get; set; }
        public List<AssetRuleAction> Actions { get; set; }
    }

    public abstract class AssetRuleAction
    {
        public RuleActionType Type { get; set; }
        public string Description { get; set; }
    }


    public class AccessControlBlockAction: AssetRuleAction
    {
        public AccessControlBlockAction()
        {
            Type = RuleActionType.Block;
        }

    }

    public class EndDateOffsetRuleAction : AssetRuleAction
    {
        public int Offset { get; set; }
        public bool TimeZone { get; set; }

        public EndDateOffsetRuleAction()
        {
            Type = RuleActionType.EndDateOffset;
        }
    }

    public class StartDateOffsetRuleAction : AssetRuleAction
    {
        public int Offset { get; set; }
        public bool TimeZone { get; set; }

        public StartDateOffsetRuleAction()
        {
            Type = RuleActionType.StartDateOffset;
        }
    }

    public abstract class AssetRuleCondition
    {
        public AssetRuleConditionType Type { get; set; }
        public string Description { get; set; }
    }

    public class  AssetCondition : AssetRuleCondition
    {
        public string Ksql { get; set; }
    }

    public class CountryCondition : AssetRuleCondition
    {
        public bool Not { get; set; }

        public List<int> Countries { get; set; }
    }
}
