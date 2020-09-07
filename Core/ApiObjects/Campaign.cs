using ApiObjects.Base;
using System.Collections.Generic;
using Newtonsoft.Json;
using ApiObjects.Rules;

namespace ApiObjects
{
    public abstract class Campaign : ICrudHandeledObject
    {
        [DBFieldMapping("ID")]
        public long Id { get; set; }
        
        [DBFieldMapping("group_id")]
        public long GroupId { get; set; }
        
        [DBFieldMapping("create_date")]
        public long CreateDate { get; set; }
        
        public long UpdateDate { get; set; }
        
        public string Name { get; set; }
        
        public string SystemName { get; set; }
        
        public string Description { get; set; }

        [DBFieldMapping("state")]
        public ObjectState State { get; set; }

        [DBFieldMapping("status")]
        public int Status { get; set; }

        [DBFieldMapping("updater_id")]
        public long UpdaterId { get; set; }

        public long? DiscountModuleId { get; set; }
        
        public string Message { get; set; }
        
        public List<KeyValuePair<string, string>> DaynamicData { get; set; }

        [DBFieldMapping("start_date")]
        public long StartDate { get; set; }

        [DBFieldMapping("end_date")]
        public long EndDate { get; set; }

        public Campaign()
        {
        }

        [JsonProperty(PropertyName = "discountConditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Rules.RuleCondition> DiscountConditions { get; set; }

        public bool EvaluateDiscountConditions(Rules.IConditionScope scope)
        {
            if (DiscountConditions != null && DiscountConditions.Count > 0)
            {
                foreach (var condition in DiscountConditions)
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
        [JsonProperty(PropertyName = "triggerConditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Rules.RuleCondition> TriggerConditions { get; set; }

        public ApiService Service { get; set; }
        public ApiAction Action { get; set; }
            
        [DBFieldMapping("campaign_json")]
        public string CampaignJson { get; set; }
        
        public bool EvaluateTriggerConditions(ICampaignObject campaignObject, ContextData contextData)
        {
            if (TriggerConditions != null && TriggerConditions.Count > 0)
            {
                var scope = campaignObject.ConvertToConditionScope(contextData);
                foreach (var condition in TriggerConditions)
                {
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

    public enum ApiAction
    {
        INSERT = 0,
        UPDATE = 1,
    }

    public enum ApiService
    {
        DomainDevice = 0
    }

    public enum ObjectState
    {
        INACTIVE = 0,
        ACTIVE = 1,
        ARCHIVE = 2
    }

    public class BatchCampaign : Campaign
    {
        [JsonProperty(PropertyName = "PopulationConditions",
              TypeNameHandling = TypeNameHandling.Auto,
              ItemTypeNameHandling = TypeNameHandling.Auto,
              ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Rules.RuleCondition> PopulationConditions { get; set; }

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

    public interface ICampaignObject
    {
        IConditionScope ConvertToConditionScope(ContextData contextData);
    } 
}
