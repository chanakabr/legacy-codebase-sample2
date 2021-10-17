using ApiLogic.Api.Managers;
using ApiObjects.Rules;
using ApiObjects.User.SessionProfile;
using System;
using System.Collections.Generic;

namespace ApiLogic.Users
{
    public class UserSessionConditionScope : IUserSessionConditionScope
    {
        public long RuleId { get; set; }
        public int? BrandId { get; set; }
        public int? FamilyId { get; set; }
        public List<long> SegmentIds { get; set; }
        public bool FilterBySegments { get; set; }
        public long? ManufacturerId { get; set; }
        public string Model { get; set; }
        public Dictionary<string, List<string>> SessionCharacteristics { get; set; }
        public List<ApiObjects.KeyValuePair> DeviceDynamicData { get; set;  }

        public bool Evaluate(RuleCondition condition)
        {
            switch (condition)
            {
                case DeviceBrandCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case DeviceFamilyCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case SegmentsCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case DeviceManufacturerCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case DeviceModelCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case DynamicKeysCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case DeviceDynamicDataCondition c: return ConditionsEvaluator.Evaluate(this, c);
                default: throw new NotImplementedException($"Evaluation for condition type {condition.Type} was not implemented in  UserSessionConditionScope");
            }
        }
    }
}
