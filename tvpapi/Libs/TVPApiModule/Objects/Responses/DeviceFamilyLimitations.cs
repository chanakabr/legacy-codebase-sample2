using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DeviceFamilyLimitations
    {
        [JsonProperty(PropertyName = "device_family")]
        public int DeviceFamily { get; set; }

        [JsonProperty(PropertyName = "device_family_name")]
        public string DeviceFamilyName { get; set; }

        [JsonProperty(PropertyName = "concurrency")]
        public int Concurrency { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        [JsonProperty(PropertyName = "frequency")]
        public int Frequency { get; set; }

        public DeviceFamilyLimitations()
        {
        }

        public DeviceFamilyLimitations(Core.Users.DeviceFamilyLimitations deviceFamilyLimitations)
        {
            if (deviceFamilyLimitations != null)
            {
                DeviceFamily = deviceFamilyLimitations.deviceFamily;
                DeviceFamilyName = deviceFamilyLimitations.deviceFamilyName;
                Concurrency = deviceFamilyLimitations.concurrency;
                Quantity = deviceFamilyLimitations.quantity;
                Frequency = deviceFamilyLimitations.Frequency;
            }
        }
    }
}
