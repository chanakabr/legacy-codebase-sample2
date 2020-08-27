using ApiObjects.Base;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        [DBFieldMapping("is_active")]
        public bool IsActive { get; set; }
        
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

        public bool Evaluate(Rules.IConditionScope scope)
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
        public string CoreObject { get; set; }//TODO SHIR, INIT
        public string CoreAction { get; set; }//TODO SHIR
            
        [DBFieldMapping("campaign_json")]
        public string CampaignJson { get; set; }
        
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

    public enum ApiAction
    {
        INSERT = 0,
        UPDATE = 1,
    }

    public enum ApiService
    {
        DomainDevice = 0
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
}
