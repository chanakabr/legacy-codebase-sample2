using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;

namespace Core.Users
{
    [JsonObject(Id = "DeviceFamilyLimitations")]
    public class DeviceFamilyLimitations : IDeepCloneable<DeviceFamilyLimitations>
    {
        public int deviceFamily { get; set; }
        public string deviceFamilyName { get; set; }

        public int concurrency { get; set; }
        public int quantity { get; set; }

        public int Frequency { get; set; }
        public bool? isDefaultConcurrency { get; set; }
        public bool? isDefaultQuantity { get; set; }

        public DeviceFamilyLimitations()
        {
        }

        public DeviceFamilyLimitations(int nDeviceFamily, int nConcurrency , int nQuantity, string sDeviceFamilyName, int frequency)
        {
            deviceFamily = nDeviceFamily;
            concurrency = nConcurrency;
            deviceFamily = nDeviceFamily;
            deviceFamilyName = sDeviceFamilyName;
            Frequency = frequency;
        }
        
        public DeviceFamilyLimitations(DeviceFamilyLimitations other) {
            deviceFamily = other.deviceFamily;
            deviceFamilyName = other.deviceFamilyName;
            concurrency = other.concurrency;
            quantity = other.quantity;
            Frequency = other.Frequency;
            isDefaultConcurrency = other.isDefaultConcurrency;
            isDefaultQuantity = other.isDefaultQuantity;
        }

        public DeviceFamilyLimitations Clone()
        {
            return new DeviceFamilyLimitations(this);
        }
    }
}
