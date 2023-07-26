using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.Rules.Converters;

namespace ApiObjects.Rules
{
    [Serializable]
    public class AssetUserRule
    {
        [JsonProperty("Id")]
        public long Id { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Conditions",
                      ItemConverterType = typeof(AssetConditionBaseConverter),
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<AssetConditionBase> Conditions { get; set; }

        [JsonProperty(PropertyName = "Actions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<AssetUserRuleAction> Actions { get; set; }

        [JsonProperty("GroupId")]
        public int GroupId { get; set; }

        /// <summary>
        /// Fill current AssetUserRule data members with givven assetUserRule only if they are empty\null
        /// </summary>
        /// <param name="assetUserRule">givven assetUserRule to fill with</param>
        public void FillEmpty(AssetUserRule assetUserRule)
        {
            if (assetUserRule != null)
            {
                if (string.IsNullOrEmpty(this.Name) || string.IsNullOrWhiteSpace(this.Name))
                {
                    this.Name = assetUserRule.Name;
                }

                if (string.IsNullOrEmpty(this.Description) || string.IsNullOrWhiteSpace(this.Description))
                {
                    this.Description = assetUserRule.Description;
                }

                if (this.Actions == null || this.Actions.Count == 0)
                {
                    this.Actions = assetUserRule.Actions;
                }

                if (this.Conditions == null || this.Conditions.Count == 0)
                {
                    this.Conditions = assetUserRule.Conditions;
                }
            }
        }
    }

    public static class AssetUserRuleExtensions
    {
        public static bool Contains(this AssetUserRule rule, RuleActionType ruleActionType) =>
            rule.Actions.Any(_ => _.Type == ruleActionType);
        public static bool Contains(this AssetUserRule rule, RuleConditionType ruleConditionType) =>
            rule.Conditions.Any(_ => _.Type == ruleConditionType);
    }
}