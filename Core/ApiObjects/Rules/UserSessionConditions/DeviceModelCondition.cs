using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace ApiObjects.Rules
{
    public interface IDeviceModelConditionScope : IConditionScope
    {
        string Model { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class DeviceModelCondition : RuleCondition
    {
        public string RegexEqual { get; set; }

        public DeviceModelCondition()
        {
            Type = RuleConditionType.DeviceModel;
        }
    }
}