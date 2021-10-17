using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.Rules
{
    public interface IDeviceFamilyConditionScope : IConditionScope
    {
        int? FamilyId { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class DeviceFamilyCondition : RuleCondition
    {
        public List<int> IdIn { get; set; }

        public DeviceFamilyCondition()
        {
            Type = RuleConditionType.DeviceFamily;
        }
    }
}