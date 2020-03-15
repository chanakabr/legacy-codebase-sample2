using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.Rules
{
    [Serializable]
    public class DeviceConcurrencyPriority
    {
        [JsonProperty("DeviceFamilyIds")]
        public List<int> DeviceFamilyIds { get; set; }

        [JsonProperty("PriorityOrder")]
        public DowngradePolicy PriorityOrder { get; set; }

        [JsonProperty("DevicePlayDataExpirationTTL")]
        public long? DevicePlayDataExpirationTTL { get; set; }
    }
}