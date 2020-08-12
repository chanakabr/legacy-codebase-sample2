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
    }

    public class TriggerCampaign : Campaign
    {
        [JsonProperty(PropertyName = "Conditions",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Rules.RuleCondition> TriggerConditions { get; set; }

        internal bool Evaluate(CoreObject coreObject)
        {
            return true;
        }
    }

    public enum CampaignEventStatus
    {
        Queued, Failed, InProgress
    }
}
