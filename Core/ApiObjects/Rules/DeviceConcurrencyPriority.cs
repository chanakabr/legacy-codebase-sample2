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

        [JsonProperty("ConcurrencyThresholdInSeconds")]
        public long? ConcurrencyThresholdInSeconds { get; set; }

        public bool SetUnchangedProperties(DeviceConcurrencyPriority oldConfig)
        {
            var needToUpdate = false;

            if (this.ConcurrencyThresholdInSeconds.HasValue)
            {
                needToUpdate = true;
            }
            else
            {
                this.ConcurrencyThresholdInSeconds = oldConfig.ConcurrencyThresholdInSeconds;
            }

            return needToUpdate;
        }
    }
}