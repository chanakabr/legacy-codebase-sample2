using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    [JsonObject(Id = "DeviceFamilyLimitations")]
    public class DeviceFamilyLimitations
    {
        public int deviceFamily { get; set; }
        public string deviceFamilyName { get; set; }

        public int concurrency { get; set; }
        public int quantity { get; set; }

        public int Frequency { get; set; }

        public DeviceFamilyLimitations()
        {
        }

        public DeviceFamilyLimitations(int nDeviceFamily, int nConcurrency , int nQuantity, string sDeviceFamilyName, int frequency)
        {
            this.deviceFamily = nDeviceFamily;
            this.concurrency = nConcurrency;
            this.deviceFamily = nDeviceFamily;
            this.deviceFamilyName = sDeviceFamilyName;
            this.Frequency = frequency;
        }


    }
}
