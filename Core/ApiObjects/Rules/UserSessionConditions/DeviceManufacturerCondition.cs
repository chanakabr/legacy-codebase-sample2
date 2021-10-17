using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.Rules
{
    public interface IDeviceManufacturerConditionScope : IConditionScope
    {
        long? ManufacturerId { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class DeviceManufacturerCondition : RuleCondition
    {
        public List<long> IdIn { get; set; }

        public DeviceManufacturerCondition()
        {
            Type = RuleConditionType.DeviceManufacturer;
        }
    }
}