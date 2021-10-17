using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.Rules
{
    public interface IDeviceDynamicDataConditionScope : IConditionScope
    {
        List<KeyValuePair> DeviceDynamicData { get; }
    }
    
    [Serializable]
    public class DeviceDynamicDataCondition : RuleCondition
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        
        [JsonProperty("value")]
        public string Value { get; set; }

        public DeviceDynamicDataCondition()
        {
            Type = RuleConditionType.DeviceDynamicData;
        }
    }
}