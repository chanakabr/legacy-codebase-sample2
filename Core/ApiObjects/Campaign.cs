using ApiObjects.Base;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects
{
    public abstract class Campaign : ICrudHandeledObject
    {
        public long Id { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }
        public CampaignEventStatus Status { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public long? DiscountModuleId { get; set; }
        public List<KeyValuePair<string, string>> Messages { get; set; }

        public Campaign()
        {
        }

        [JsonProperty(PropertyName = "Conditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Rules.RuleCondition> CampaignConditions { get; set; }

        public bool Evaluate(Rules.IConditionScope scope)
        {
            if (CampaignConditions != null && CampaignConditions.Count > 0)
            {
                foreach (var condition in CampaignConditions)
                {
                    scope.RuleId = this.Id;

                    if (!condition.Evaluate(scope))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        ///// <summary>
        ///// Fill current AssetRule data members with given assetRule only if they are empty\null
        ///// </summary>
        ///// <param name="oldRule">given assetRule to fill with</param>
        //internal void FillEmpty(KalturaBusinessModuleRule oldRule)
        //{
        //    // TODO shir - WWE NEED THIS
        //    if (oldRule != null)
        //    {
        //        this.CreateDate = oldRule.CreateDate;

        //        if (string.IsNullOrEmpty(this.Name) || string.IsNullOrWhiteSpace(this.Name))
        //        {
        //            this.Name = oldRule.Name;
        //        }

        //        if (this.Description == null)
        //        {
        //            this.Description = oldRule.Description;
        //        }

        //        if (this.Conditions == null)
        //        {
        //            this.Conditions = oldRule.Conditions;
        //        }
        //    }
        //}
    }

    public class TriggerCampaign : Campaign
    {
        [JsonProperty(PropertyName = "Conditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Rules.RuleCondition> TriggerConditions { get; set; }

        public bool Evaluate(CoreObject coreObject)
        {
            return true;
        }

        ///// <summary>
        ///// Fill current AssetRule data members with given assetRule only if they are empty\null
        ///// </summary>
        ///// <param name="oldRule">given assetRule to fill with</param>
        //internal void FillEmpty(KalturaBusinessModuleRule oldRule)
        //{
        //    // TODO shir - WWE NEED THIS
        //    if (oldRule != null)
        //    {
        //        this.CreateDate = oldRule.CreateDate;

        //        if (string.IsNullOrEmpty(this.Name) || string.IsNullOrWhiteSpace(this.Name))
        //        {
        //            this.Name = oldRule.Name;
        //        }

        //        if (this.Description == null)
        //        {
        //            this.Description = oldRule.Description;
        //        }

        //        if (this.Conditions == null)
        //        {
        //            this.Conditions = oldRule.Conditions;
        //        }
        //    }
        //}
    }

    public class BatchCampaign : Campaign
    {
        // TODO SHIR BatchCampaign

        ///// <summary>
        ///// Fill current AssetRule data members with given assetRule only if they are empty\null
        ///// </summary>
        ///// <param name="oldRule">given assetRule to fill with</param>
        //internal void FillEmpty(KalturaBusinessModuleRule oldRule)
        //{
        //    // TODO shir - WWE NEED THIS
        //    if (oldRule != null)
        //    {
        //        this.CreateDate = oldRule.CreateDate;

        //        if (string.IsNullOrEmpty(this.Name) || string.IsNullOrWhiteSpace(this.Name))
        //        {
        //            this.Name = oldRule.Name;
        //        }

        //        if (this.Description == null)
        //        {
        //            this.Description = oldRule.Description;
        //        }

        //        if (this.Conditions == null)
        //        {
        //            this.Conditions = oldRule.Conditions;
        //        }
        //    }
        //}
    }

    public enum CampaignEventStatus
    {
        Queued, Failed, InProgress
    }

    public class CampaignFilter : ICrudFilter
    {

    }
}
