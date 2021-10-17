using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.Rules
{
    public interface IDeviceBrandConditionScope : IConditionScope
    {
        int? BrandId { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class DeviceBrandCondition : RuleCondition
    {
        public List<int> IdIn { get; set; }

        public DeviceBrandCondition()
        {
            Type = RuleConditionType.DeviceBrand;
        }
    }
}