using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Household limitations details 
    /// </summary>
    [OldStandard("concurrentLimit", "concurrent_limit")]
    [OldStandard("deviceLimit", "device_limit")]
    [OldStandard("deviceFrequency", "device_frequency")]
    [OldStandard("deviceFrequencyDescription", "device_frequency_description")]
    [OldStandard("userFrequency", "user_frequency")]
    [OldStandard("userFrequencyDescription", "user_frequency_description")]
    [OldStandard("npvrQuotaInSeconds", "npvr_quota_in_seconds")]
    [OldStandard("usersLimit", "users_limit")]
    [OldStandard("deviceFamiliesLimitations", "device_families_limitations")]
    public class KalturaHouseholdLimitations : KalturaOTTObject
    {
        /// <summary>
        /// Household limitation module identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
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
        [DataMember(Name = "concurrentLimit")]
        [JsonProperty("concurrentLimit")]
        [XmlElement(ElementName = "concurrentLimit")]
        public int? ConcurrentLimit { get; set; }

        /// <summary>
        ///  Max number of devices allowed for the household
        /// </summary>
        [DataMember(Name = "deviceLimit")]
        [JsonProperty("deviceLimit")]
        [XmlElement(ElementName = "deviceLimit")]
        public int? DeviceLimit { get; set; }

        /// <summary>
        /// Allowed device change frequency code
        /// </summary>
        [DataMember(Name = "deviceFrequency")]
        [JsonProperty("deviceFrequency")]
        [XmlElement(ElementName = "deviceFrequency")]
        public int? DeviceFrequency { get; set; }

        /// <summary>
        /// Allowed device change frequency description
        /// </summary>
        [DataMember(Name = "deviceFrequencyDescription")]
        [JsonProperty("deviceFrequencyDescription")]
        [XmlElement(ElementName = "deviceFrequencyDescription")]
        public string DeviceFrequencyDescription { get; set; }

        /// <summary>
        /// Allowed user change frequency code
        /// </summary>
        [DataMember(Name = "userFrequency")]
        [JsonProperty("userFrequency")]
        [XmlElement(ElementName = "userFrequency")]
        public int? UserFrequency { get; set; }

        /// <summary>
        /// Allowed user change frequency description
        /// </summary>
        [DataMember(Name = "userFrequencyDescription")]
        [JsonProperty("userFrequencyDescription")]
        [XmlElement(ElementName = "userFrequencyDescription")]
        public string UserFrequencyDescription { get; set; }

        /// <summary>
        /// Allowed NPVR Quota in Seconds
        /// </summary>
        [DataMember(Name = "npvrQuotaInSeconds")]
        [JsonProperty("npvrQuotaInSeconds")]
        [XmlElement(ElementName = "npvrQuotaInSeconds")]
        public int? NpvrQuotaInSeconds { get; set; }

        /// <summary>
        /// Max number of users allowed for the household
        /// </summary>
        [DataMember(Name = "usersLimit")]
        [JsonProperty("usersLimit")]
        [XmlElement(ElementName = "usersLimit")]
        public int? UsersLimit { get; set; }

        /// <summary>
        /// Device families limitations
        /// </summary>
        [DataMember(Name = "deviceFamiliesLimitations")]
        [JsonProperty("deviceFamiliesLimitations")]
        [XmlArray(ElementName = "deviceFamiliesLimitations", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaHouseholdDeviceFamilyLimitations> DeviceFamiliesLimitations { get; set; }

    }
}