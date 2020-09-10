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

        [DBFieldMapping("start_date")]
        public long StartDate { get; set; }

        [DBFieldMapping("end_date")]
        public long EndDate { get; set; }

        [DBFieldMapping("has_promotion")]
        public bool HasPromotion { get; set; }

        [DBFieldMapping("state")]
        public ObjectState State { get; set; }

        [DBFieldMapping("type")]
        public int type { get; set; }
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
        
        public List<long> CollectionIds { get; set; }

        [DBFieldMapping("campaign_json")]
        public string CampaignJson { get; set; }

        public Promotion Promotion { get; set; }

        #endregion

        public abstract eCampaignType CampaignType { get; }

        public Campaign() { }

        /// <summary>
        /// Fill current Campaign data members with given Campaign only if they are empty\null
        /// </summary>
        /// <param name="oldRule">given assetRule to fill with</param>
        public virtual void FillEmpty(Campaign oldCampaign)
        {
            if (this.CollectionIds == null)
            {
                this.CollectionIds = oldCampaign.CollectionIds;
            }
            if (string.IsNullOrEmpty(this.Description))
            {
                this.Description = oldCampaign.Description;
            }
            if (this.Promotion == null)
            {
                this.Promotion = oldCampaign.Promotion;
            }
            if (this.EndDate == default)
            {
                this.EndDate = oldCampaign.EndDate;
            }
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

        public bool EvaluatePopulationConditions(IConditionScope scope)
        {
            if (PopulationConditions != null && PopulationConditions.Count > 0)
            {
                foreach (var condition in PopulationConditions)
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
        /// Fill current BatchCampaign data members with given BatchCampaign only if they are empty\null
        /// </summary>
        /// <param name="oldCampaign">given BatchCampaign to fill with</param>
        public void FillEmpty(BatchCampaign oldCampaign)
        {
            base.FillEmpty(oldCampaign);

            if (this.PopulationConditions == null)
            {
                this.PopulationConditions = oldCampaign.PopulationConditions;
            }
        }
    }
}