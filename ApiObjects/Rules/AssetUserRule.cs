using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects.Rules
{
    public class AssetUserRule
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AssetCondition> Conditions { get; set; }
        public List<AssetUserRuleAction> Actions { get; set; }
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
}
