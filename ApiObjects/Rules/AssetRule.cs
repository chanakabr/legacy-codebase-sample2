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

    #region Actions

    public abstract class RuleAction
    {
        public RuleActionType Type { get; protected set; }
        public string Description { get; set; }
    }

    public abstract class AssetRuleAction : RuleAction
    {
    }

    public abstract class AssetUserRuleAction : RuleAction
    {
    }

    public class AssetBlockAction : AssetRuleAction
    {
        public AssetBlockAction()
        {
            this.Type = RuleActionType.Block;
        }
    }

    public abstract class TimeOffsetRuleAction : AssetRuleAction
    {
        public int Offset { get; set; }
        public bool TimeZone { get; set; }
    }

    public class EndDateOffsetRuleAction : TimeOffsetRuleAction
    {
        public EndDateOffsetRuleAction()
        {
            Type = RuleActionType.EndDateOffset;
        }
    }

    public class StartDateOffsetRuleAction : TimeOffsetRuleAction
    {
        public StartDateOffsetRuleAction()
        {
            Type = RuleActionType.StartDateOffset;
        }
    }

    public class AssetUserRuleBlockAction : AssetUserRuleAction
    {
        public AssetUserRuleBlockAction()
        {
            this.Type = RuleActionType.UserBlock;
        }
    }

    #endregion

    #region Conditions

    public class AssetRuleCondition
    {
        public AssetRuleConditionType Type { get; protected set; }
        public string Description { get; set; }
    }

    public class AssetCondition : AssetRuleCondition
    {
        public string Ksql { get; set; }

        public AssetCondition()
        {
            Type = AssetRuleConditionType.Asset;
        }
    }

    public class CountryCondition : AssetRuleCondition
    {
        public bool Not { get; set; }
        public List<int> Countries { get; set; }

        public CountryCondition()
        {
            this.Type = AssetRuleConditionType.Country;
        }
    }

    #endregion
}
