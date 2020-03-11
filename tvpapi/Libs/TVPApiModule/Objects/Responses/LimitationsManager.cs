using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class LimitationsManager
    {
        [JsonProperty(PropertyName = "concurrency")]
        public int Concurrency { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        [JsonProperty(PropertyName = "device_frequency")]
        public int DeviceFrequency { get; set; }
        
        [JsonProperty(PropertyName = "device_family_limitations")]
        public List<DeviceFamilyLimitations> DeviceFamilyLimitations { get; set; }

        [JsonProperty(PropertyName = "domian_limit_id")]
        public int DomianLimitID { get; set; }

        [JsonProperty(PropertyName = "domain_limit_name")]
        public string DomainLimitName { get; set; }

        [JsonProperty(PropertyName = "npvr_quota_in_seconds")]        
        public int NpvrQuotaInSecs { get; set; }

        [JsonProperty(PropertyName = "user_limit")]        
        public int UserLimit { get; set; }

        [JsonProperty(PropertyName = "user_frequency")] 
        public int UserFrequency { get; set; }

        [JsonProperty(PropertyName = "user_frequency_description")] 
        public string UserFrequencyDescrition { get; set; }

        [JsonProperty(PropertyName = "device_frequency_description")] 
        public string DeviceFrequencyDescrition { get; set; }
       
        public LimitationsManager()
        {
            DeviceFamilyLimitations = new List<DeviceFamilyLimitations>();
        }

        public LimitationsManager(TVPPro.SiteManager.TvinciPlatform.Domains.LimitationsManager limitationManager)
        {
            if (limitationManager != null)
            {
                Concurrency = limitationManager.Concurrency;
                Quantity = limitationManager.Quantity;
                DeviceFrequency = limitationManager.Frequency;
                if (limitationManager.lDeviceFamilyLimitations != null)
                {
                    DeviceFamilyLimitations = new List<DeviceFamilyLimitations>();
                    foreach (TVPPro.SiteManager.TvinciPlatform.Domains.DeviceFamilyLimitations dl in limitationManager.lDeviceFamilyLimitations)
                    {
                        DeviceFamilyLimitations.Add(new DeviceFamilyLimitations(dl));
                    }
                }
                DomianLimitID = limitationManager.domianLimitID;
                DomainLimitName = limitationManager.DomainLimitName;
                NpvrQuotaInSecs = limitationManager.npvrQuotaInSecs;
                UserLimit = limitationManager.nUserLimit;
                UserFrequency = limitationManager.UserFrequency;
                UserFrequencyDescrition = limitationManager.UserFrequencyDescrition;
                DeviceFrequencyDescrition = limitationManager.FrequencyDescription;
            }
        }
    }
}
