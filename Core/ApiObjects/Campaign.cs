using ApiObjects.Base;
using System.Collections.Generic;
using Newtonsoft.Json;
using ApiObjects.Rules;
using System;

namespace ApiObjects
{
    public enum eCampaignType
    {
        Trigger = 1,
        Batch = 2
    }

    public enum CampaignState
    {
        INACTIVE = 0,
        ACTIVE = 1,
        ARCHIVE = 2
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class CampaignDB: BaseSupportsNullable
    {
        [DBFieldMapping("ID")]
        public long Id { get; set; }

        [DBFieldMapping("start_date")]
        public long StartDate { get; set; }

        [DBFieldMapping("end_date")]
        public long EndDate { get; set; }

        [DBFieldMapping("has_promotion")]
        public bool HasPromotion { get; set; }

        [DBFieldMapping("state")]
        public CampaignState State { get; set; }

        [DBFieldMapping("type")]
        public int type { get; set; }

        [DBFieldMapping("ASSET_USER_RULE_ID")]
        public long? AssetUserRuleId { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class Campaign : CampaignDB, ICrudHandeledObject
    {
        #region Data members

        public long CreateDate { get; set; }
        
        public long UpdateDate { get; set; }
        
        public string Name { get; set; }
        
        public string SystemName { get; set; }
        
        public string Description { get; set; }

        public long UpdaterId { get; set; }
        
        public string Message { get; set; }
        
        public List<long> CollectionIds { get; set; }

        [DBFieldMapping("campaign_json")]
        public string CampaignJson { get; set; }

        public BasePromotion Promotion { get; set; }


        #endregion

        public virtual eCampaignType CampaignType { get; }

        public Campaign() { }

        /// <summary>
        /// Fill current Campaign data members with given Campaign only if they are empty\null
        /// </summary>
        /// <param name="oldRule">given assetRule to fill with</param>
        public virtual void FillEmpty(Campaign oldCampaign)
        {
            this.CreateDate = oldCampaign.CreateDate;
            this.UpdateDate = oldCampaign.UpdateDate;
            this.UpdaterId = oldCampaign.UpdaterId;

            if (this.StartDate == 0)
            {
                this.StartDate = oldCampaign.StartDate;
            }

            if (this.EndDate == 0)
            {
                this.EndDate = oldCampaign.EndDate;
            }

            if (this.Name == null)
            {
                this.Name = oldCampaign.Name;
            }

            if (this.SystemName == null)
            {
                this.SystemName = oldCampaign.SystemName;
            }

            if (this.Description == null)
            {
                this.Description = oldCampaign.Description;
            }

            if (this.Message == null)
            {
                this.Message = oldCampaign.Message;
            }

            if (this.CollectionIds == null)
            {
                this.CollectionIds = oldCampaign.CollectionIds;
            }

            var nullablePromotion = new NullableObj<BasePromotion>(this.Promotion, this.IsNullablePropertyExists("Promotion"));
            if (!nullablePromotion.IsNull && this.Promotion == null)
            {
                this.Promotion = oldCampaign.Promotion;
            }

            var nullableAssetUserRuleId = new NullableObj<long?>(this.AssetUserRuleId, this.IsNullablePropertyExists("AssetUserRuleId"));
            if (!nullableAssetUserRuleId.IsNull && !this.AssetUserRuleId.HasValue)
            {
                this.AssetUserRuleId = oldCampaign.AssetUserRuleId;
            }
        }

        public virtual List<RuleCondition> GetConditions() { return null; }

        public bool EvaluateConditions(IConditionScope scope)
        {
            var conditions = GetConditions();
            if (conditions != null && conditions.Count > 0)
            {
                foreach (var condition in conditions)
                {
                    if (!scope.Evaluate(condition))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class TriggerCampaign : Campaign
    {
        [JsonProperty(PropertyName = "triggerConditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<RuleCondition> TriggerConditions { get; set; }

        public ApiService Service { get; set; }
        public ApiAction Action { get; set; }

        public override eCampaignType CampaignType { get { return eCampaignType.Trigger; } }

        public override List<RuleCondition> GetConditions()
        {
            return this.TriggerConditions;
        }

        /// <summary>
        /// Fill current TriggerCampaign data members with given TriggerCampaign only if they are empty\null
        /// </summary>
        /// <param name="oldCampaign">given TriggerCampaign to fill with</param>
        public void FillEmpty(TriggerCampaign oldCampaign)
        {
            base.FillEmpty(oldCampaign);
            
            this.Service = oldCampaign.Service;
            this.Action = oldCampaign.Action;

            if (this.TriggerConditions == null)
            {
                this.TriggerConditions = oldCampaign.TriggerConditions;
            }
        }
    }

    public enum ApiAction
    {
        Insert = 0
    }

    public enum ApiService
    {
        DomainDevice = 0
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BatchCampaign : Campaign
    {
        [JsonProperty(PropertyName = "PopulationConditions",
              TypeNameHandling = TypeNameHandling.Auto,
              ItemTypeNameHandling = TypeNameHandling.Auto,
              ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<RuleCondition> PopulationConditions { get; set; }

        public override eCampaignType CampaignType { get { return eCampaignType.Batch; } }

        public override List<RuleCondition> GetConditions()
        {
            return this.PopulationConditions;
        }

        /// <summary>
        /// Fill current BatchCampaign data members with given BatchCampaign only if they are empty\null
        /// </summary>
        /// <param name="oldCampaign">given BatchCampaign to fill with</param>
        public void FillEmpty(BatchCampaign oldCampaign)
        {
            base.FillEmpty(oldCampaign);

            var nullablePopulation = new NullableObj<List<RuleCondition>>(this.PopulationConditions, this.IsNullablePropertyExists("PopulationConditions"));

            if (!nullablePopulation.IsNull && this.PopulationConditions == null)
            {
                this.PopulationConditions = oldCampaign.PopulationConditions;
            }
        }
    }
}