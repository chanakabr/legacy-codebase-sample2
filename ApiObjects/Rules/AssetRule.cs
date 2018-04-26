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

    public class AssetRuleAction
    {
        public RuleActionType Type { get; set; }
        public string Description { get; set; }
    }

    public abstract class AssetRuleCondition
    {
        public AssetRuleConditionType Type { get; set; }
        public bool Not { get; set; }
        public string Description { get; set; }
    }

    public class  AssetCondition : AssetRuleCondition
    {
        public string Ksql { get; set; }
    }

    public class CountryCondition : AssetRuleCondition
    {
        public List<long> Countries { get; set; }
    }
}
