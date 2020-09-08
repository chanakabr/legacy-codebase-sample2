using ApiObjects.Base;
using System.Collections.Generic;
using Newtonsoft.Json;
using ApiObjects.Rules;

namespace ApiObjects
{
    public enum eCampaignType
    {
        Trigger = 1,
        Batch = 2
    }

    public enum ObjectState
    {
        INACTIVE = 0,
        ACTIVE = 1,
        ARCHIVE = 2
    }

    public interface ICampaignObject
    {
        IConditionScope ConvertToConditionScope(ContextData contextData);
    }

    public class CampaignDB
    {
        [DBFieldMapping("ID")]
        public long Id { get; set; }

        [DBFieldMapping("state")]
        public ObjectState State { get; set; }

        [DBFieldMapping("start_date")]
        public long StartDate { get; set; }

        [DBFieldMapping("end_date")]
        public long EndDate { get; set; }

        [DBFieldMapping("discount_module_id")]
        public long? DiscountModuleId { get; set; }
    }

    public abstract class Campaign : CampaignDB, ICrudHandeledObject
    {
        #region Data members

        public long CreateDate { get; set; }
        
        public long UpdateDate { get; set; }
        
        public string Name { get; set; }
        
        public string SystemName { get; set; }
        
        public string Description { get; set; }

        public long UpdaterId { get; set; }
        
        public string Message { get; set; }
        
        public List<KeyValuePair<string, string>> DaynamicData { get; set; }

        // TODO SHIR - WE NEED THAT?? campaign_json
        [DBFieldMapping("campaign_json")]
        public string CampaignJson { get; set; }

        [JsonProperty(PropertyName = "discountConditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Rules.RuleCondition> DiscountConditions { get; set; }

        #endregion

        public abstract eCampaignType CampaignType { get; }

        public Campaign() { }

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

        /// <summary>
        /// Fill current Campaign data members with given Campaign only if they are empty\null
        /// </summary>
        /// <param name="oldRule">given assetRule to fill with</param>
        public virtual void FillEmpty(Campaign oldCampaign)
        {
            if (this.DaynamicData == null)
            {
                this.DaynamicData = oldCampaign.DaynamicData;
            }
            if (string.IsNullOrEmpty(this.Description))
            {
                this.Description = oldCampaign.Description;
            }
            if (this.DiscountConditions == null)
            {
                this.DiscountConditions = oldCampaign.DiscountConditions;
            }
            if (this.DiscountModuleId == null)
            {
                this.DiscountModuleId = oldCampaign.DiscountModuleId;
            }
            if (this.EndDate == default)
            {
                this.EndDate = oldCampaign.EndDate;
            }
            //if (campaignToUpdate.IsActive == default)
            //{
            //    campaignToUpdate.IsActive = campaign.IsActive;
            //}
            if (string.IsNullOrEmpty(this.Message))
            {
                this.Message = oldCampaign.Message;
            }
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = oldCampaign.Name;
            }
            if (string.IsNullOrEmpty(this.SystemName))
            {
                this.SystemName = oldCampaign.SystemName;
            }
            if (this.StartDate == default)
            {
                this.StartDate = oldCampaign.StartDate;
            }
            
            // TODO SHIR / MATAN FILL EMPTY IN BASE
        }
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

        public override eCampaignType CampaignType { get { return eCampaignType.Trigger; } }

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

        /// <summary>
        /// Fill current TriggerCampaign data members with given TriggerCampaign only if they are empty\null
        /// </summary>
        /// <param name="oldCampaign">given TriggerCampaign to fill with</param>
        public void FillEmpty(TriggerCampaign oldCampaign)
        {
            base.FillEmpty(oldCampaign);

            // TODO MATAN / shir - FILL EMPTY TriggerCampaign
            //if (string.IsNullOrEmpty(campaignToUpdate.Action))
            //{
            //    campaignToUpdate.Action = campaign.Action;
            //}

            //if (string.IsNullOrEmpty(campaignToUpdate.Service))
            //{
            //    campaignToUpdate.Service = campaign.Service;
            //}

            if (this.TriggerConditions == null)
            {
                this.TriggerConditions = oldCampaign.TriggerConditions;
            }
        }
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

        public override eCampaignType CampaignType { get { return eCampaignType.Batch; } }

        // TODO SHIR / MATAN BatchCampaign

        ///// <summary>
        ///// Fill current AssetRule data members with given assetRule only if they are empty\null
        ///// </summary>
        ///// <param name="oldRule">given assetRule to fill with</param>
        //internal void FillEmpty(KalturaBusinessModuleRule oldRule)
        //{
        //    // TODO shir / MATAN- FillEmpty BatchCampaign
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
