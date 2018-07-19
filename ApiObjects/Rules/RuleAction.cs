using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
