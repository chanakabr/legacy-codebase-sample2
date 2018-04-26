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

    public class AssetRuleCondition
    {
        public RuleActionType Type { get; set; }
        public string Description { get; set; }
    }
}
