using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Rules
{
    [Serializable]
    public abstract class RuleAction
    {
        [JsonProperty("Type")]
        public RuleActionType Type { get; protected set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
    }

    [Serializable]
    public abstract class AssetRuleAction : RuleAction
    {
    }

    [Serializable]
    public abstract class AssetUserRuleAction : RuleAction
    {
    }

    [Serializable]
    public class AssetBlockAction : AssetRuleAction
    {
        public AssetBlockAction()
        {
            this.Type = RuleActionType.Block;
        }
    }

    [Serializable]
    public abstract class TimeOffsetRuleAction : AssetRuleAction
    {
        [JsonProperty("Offset")]
        public int Offset { get; set; }

        [JsonProperty("TimeZone")]
        public bool TimeZone { get; set; }
    }

    [Serializable]
    public class EndDateOffsetRuleAction : TimeOffsetRuleAction
    {
        public EndDateOffsetRuleAction()
        {
            Type = RuleActionType.EndDateOffset;
        }
    }

    [Serializable]
    public class StartDateOffsetRuleAction : TimeOffsetRuleAction
    {
        public StartDateOffsetRuleAction()
        {
            Type = RuleActionType.StartDateOffset;
        }
    }

    [Serializable]
    public class AssetUserRuleBlockAction : AssetUserRuleAction
    {
        public AssetUserRuleBlockAction()
        {
            this.Type = RuleActionType.UserBlock;
        }
    }

    [Serializable]
    public class AllowPlaybackAction : AssetRuleAction
    {
        public AllowPlaybackAction()
        {
            this.Type = RuleActionType.AllowPlayback;
        }
    }

    [Serializable]
    public class BlockPlaybackAction : AssetRuleAction
    {
        public BlockPlaybackAction()
        {
            this.Type = RuleActionType.BlockPlayback;
        }
    }
}
