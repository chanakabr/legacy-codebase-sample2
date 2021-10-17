using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Rules;
using System;
using System.Collections.Generic;

namespace APILogic.ConditionalAccess
{
    public interface ITriggerCampaignConditionScope :
        ISegmentsConditionScope,
        IDeviceBrandConditionScope,
        IDeviceFamilyConditionScope,
        IUdidDynamicListConditionScope,
        IDeviceManufacturerConditionScope,
        IDeviceModelConditionScope
    {
        int GroupId { get; set; }
        string UserId { get; set; }
    }

    public class TriggerCampaignConditionScope : ITriggerCampaignConditionScope
    {
        public int? BrandId { get; set; }
        public int? FamilyId { get; set; }
        public long? ManufacturerId { get; set; }
        public string Model { get; set; }
        public string Udid { get; set; }
        public long RuleId { get; set; }
        public int GroupId { get; set; }
        public string UserId { get; set; }
        public List<long> SegmentIds { get; set; }
        public bool FilterBySegments { get; set; }

        public bool Evaluate(RuleCondition condition)
        {
            switch (condition)
            {
                case SegmentsCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case DeviceBrandCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case DeviceFamilyCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case UdidDynamicListCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case DeviceManufacturerCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case DeviceModelCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case OrCondition c: return ConditionsEvaluator.Evaluate(this, c);
                default: throw new NotImplementedException($"Evaluation for condition type {condition.Type} was not implemented in TriggerCampaignConditionScope");
            }
        }

        public bool CheckDynamicList(long id)
        {
            var contextData = new ContextData(this.GroupId);
            var filter = new DynamicListSearchFilter()
            {
                TypeEqual = DynamicListType.UDID,
                IdEqual = id,
                ValueEqual = this.Udid
            };

            var dynamicListResponse = DynamicListManager.Instance.SearchDynamicLists(contextData, filter);
            return dynamicListResponse.HasObjects();
        }
    }
}