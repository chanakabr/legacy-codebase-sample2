using ApiObjects.AssetLifeCycleRules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ApiObjects.Rules.PreActionCondition;

namespace ApiObjects.Rules
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class RuleAction
    {
        [JsonProperty("Type")]
        public RuleActionType Type { get; protected set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class AssetRuleAction : RuleAction
    {
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class AssetRuleFilterAction : AssetRuleAction
    {
        [JsonProperty("PreActionCondition")]
        public BasePreActionCondition PreActionCondition { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class AssetUserRuleAction : RuleAction
    {
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AssetBlockAction : AssetRuleAction
    {
        public AssetBlockAction()
        {
            this.Type = RuleActionType.Block;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class TimeOffsetRuleAction : AssetRuleAction
    {
        [JsonProperty("Offset")]
        public int Offset { get; set; }

        [JsonProperty("TimeZone")]
        public bool TimeZone { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class EndDateOffsetRuleAction : TimeOffsetRuleAction
    {
        public EndDateOffsetRuleAction()
        {
            Type = RuleActionType.EndDateOffset;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class StartDateOffsetRuleAction : TimeOffsetRuleAction
    {
        public StartDateOffsetRuleAction()
        {
            Type = RuleActionType.StartDateOffset;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AssetUserRuleBlockAction : AssetUserRuleAction
    {
        public AssetUserRuleBlockAction()
        {
            this.Type = RuleActionType.UserBlock;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AssetUserRuleFilterAction : AssetUserRuleAction
    {
        [JsonProperty("ApplyOnChannel")]
        public bool ApplyOnChannel { get; set; }

        public AssetUserRuleFilterAction()
        {
            this.Type = RuleActionType.UserFilter;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AllowPlaybackAction : AssetRuleAction
    {
        public AllowPlaybackAction()
        {
            this.Type = RuleActionType.AllowPlayback;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BlockPlaybackAction : AssetRuleAction
    {
        public BlockPlaybackAction()
        {
            this.Type = RuleActionType.BlockPlayback;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BusinessModuleRuleAction : RuleAction
    {
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ApplyDiscountModuleRuleAction : BusinessModuleRuleAction
    {
        [JsonProperty("DiscountModuleId")]
        public long DiscountModuleId { get; set; }

        public ApplyDiscountModuleRuleAction()
        {
            this.Type = RuleActionType.ApplyDiscountModuleRule;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ApplyPlaybackAdapterRuleAction : AssetRuleAction
    {
        [JsonProperty("AdapterId")]
        public int AdapterId { get; set; }

        public ApplyPlaybackAdapterRuleAction()
        {
            this.Type = RuleActionType.ApplyPlaybackAdapter;
        }
    }
    
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class AssetLifeCycleTransitionAction : AssetRuleAction
    {
        [JsonProperty("ActionType")]
        public AssetLifeCycleRuleAction ActionType { get; set; }

        [JsonProperty("TransitionType")]
        public AssetLifeCycleRuleTransitionType TransitionType { get; protected set; }
    }
    
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AssetLifeCycleTagTransitionAction : AssetLifeCycleTransitionAction
    {
        [JsonProperty("TagIds")]
        public List<int> TagIds { get; set; }

        public AssetLifeCycleTagTransitionAction()
        {
            Type = RuleActionType.AssetLifeCycleTransition;
            TransitionType = AssetLifeCycleRuleTransitionType.Tag;
        }
    }
    
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AssetLifeCycleBuisnessModuleTransitionAction : AssetLifeCycleTransitionAction
    {
        [JsonProperty("Transitions")]
        public LifeCycleFileTypesAndPpvsTransitions Transitions { get; set; }

        public AssetLifeCycleBuisnessModuleTransitionAction()
        {
            Type = RuleActionType.AssetLifeCycleTransition;
            TransitionType = AssetLifeCycleRuleTransitionType.BusinessModel;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ApplyFreePlaybackAction : BusinessModuleRuleAction
    {
        public ApplyFreePlaybackAction()
        {
            Type = RuleActionType.ApplyFreePlayback;
        }
    }
}
