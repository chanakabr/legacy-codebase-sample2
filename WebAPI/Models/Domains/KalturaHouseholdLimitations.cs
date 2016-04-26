using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Household limitations details 
    /// </summary>
    public class KalturaHouseholdLimitations : KalturaOTTObject
    {
        /// <summary>
        /// Household limitation module identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int? Id { get; set; }

        /// <summary>
        /// Household limitation module name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Max number of streams allowed for the household
        /// </summary>
        [DataMember(Name = "concurrent_limit")]
        [JsonProperty("concurrent_limit")]
        [XmlElement(ElementName = "concurrent_limit")]
        public int? ConcurrentLimit { get; set; }

        /// <summary>
        ///  Max number of devices allowed for the household
        /// </summary>
        [DataMember(Name = "device_limit")]
        [JsonProperty("device_limit")]
        [XmlElement(ElementName = "device_limit")]
        public int? DeviceLimit { get; set; }

        /// <summary>
        /// Allowed device change frequency code
        /// </summary>
        [DataMember(Name = "device_frequency")]
        [JsonProperty("device_frequency")]
        [XmlElement(ElementName = "device_frequency")]
        public int? DeviceFrequency { get; set; }

        /// <summary>
        /// Allowed device change frequency description
        /// </summary>
        [DataMember(Name = "device_frequency_description")]
        [JsonProperty("device_frequency_description")]
        [XmlElement(ElementName = "device_frequency_description")]
        public string DeviceFrequencyDescription { get; set; }

        /// <summary>
        /// Allowed user change frequency code
        /// </summary>
        [DataMember(Name = "user_frequency")]
        [JsonProperty("user_frequency")]
        [XmlElement(ElementName = "user_frequency")]
        public int? UserFrequency { get; set; }

        /// <summary>
        /// Allowed user change frequency description
        /// </summary>
        [DataMember(Name = "user_frequency_description")]
        [JsonProperty("user_frequency_description")]
        [XmlElement(ElementName = "user_frequency_description")]
        public string UserFrequencyDescription { get; set; }

        /// <summary>
        /// Allowed NPVR Quota in Seconds
        /// </summary>
        [DataMember(Name = "npvr_quota_in_seconds")]
        [JsonProperty("npvr_quota_in_seconds")]
        [XmlElement(ElementName = "npvr_quota_in_seconds")]
        public int? NpvrQuotaInSeconds { get; set; }

        /// <summary>
        /// Max number of users allowed for the household
        /// </summary>
        [DataMember(Name = "users_limit")]
        [JsonProperty("users_limit")]
        [XmlElement(ElementName = "users_limit")]
        public int? UsersLimit { get; set; }

        /// <summary>
        /// Device families limitations
        /// </summary>
        [DataMember(Name = "device_families_limitations")]
        [JsonProperty("device_families_limitations")]
        [XmlArray(ElementName = "device_families_limitations", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaHouseholdDeviceFamilyLimitations> DeviceFamiliesLimitations { get; set; }

    }
}