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
        [SchemeProperty(ReadOnly = true)]
        public string Name { get; set; }

        /// <summary>
        /// Max number of streams allowed for the household
        /// </summary>
        [DataMember(Name = "concurrentLimit")]
        [JsonProperty("concurrentLimit")]
        [XmlElement(ElementName = "concurrentLimit")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("concurrent_limit")]
        public int? ConcurrentLimit { get; set; }

        /// <summary>
        ///  Max number of devices allowed for the household
        /// </summary>
        [DataMember(Name = "deviceLimit")]
        [JsonProperty("deviceLimit")]
        [XmlElement(ElementName = "deviceLimit")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("device_limit")]
        public int? DeviceLimit { get; set; }

        /// <summary>
        /// Allowed device change frequency code
        /// </summary>
        [DataMember(Name = "deviceFrequency")]
        [JsonProperty("deviceFrequency")]
        [XmlElement(ElementName = "deviceFrequency")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("device_frequency")]
        public int? DeviceFrequency { get; set; }

        /// <summary>
        /// Allowed device change frequency description
        /// </summary>
        [DataMember(Name = "deviceFrequencyDescription")]
        [JsonProperty("deviceFrequencyDescription")]
        [XmlElement(ElementName = "deviceFrequencyDescription")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("device_frequency_description")]
        public string DeviceFrequencyDescription { get; set; }

        /// <summary>
        /// Allowed user change frequency code
        /// </summary>
        [DataMember(Name = "userFrequency")]
        [JsonProperty("userFrequency")]
        [XmlElement(ElementName = "userFrequency")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("user_frequency")]
        public int? UserFrequency { get; set; }

        /// <summary>
        /// Allowed user change frequency description
        /// </summary>
        [DataMember(Name = "userFrequencyDescription")]
        [JsonProperty("userFrequencyDescription")]
        [XmlElement(ElementName = "userFrequencyDescription")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("user_frequency_description")]
        public string UserFrequencyDescription { get; set; }

        /// <summary>
        /// Allowed NPVR Quota in Seconds
        /// </summary>
        [DataMember(Name = "npvrQuotaInSeconds")]
        [JsonProperty("npvrQuotaInSeconds")]
        [XmlElement(ElementName = "npvrQuotaInSeconds")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("npvr_quota_in_seconds")]
        public int? NpvrQuotaInSeconds { get; set; }

        /// <summary>
        /// Max number of users allowed for the household
        /// </summary>
        [DataMember(Name = "usersLimit")]
        [JsonProperty("usersLimit")]
        [XmlElement(ElementName = "usersLimit")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("users_limit")]
        public int? UsersLimit { get; set; }

        /// <summary>
        /// Device families limitations
        /// </summary>
        [DataMember(Name = "deviceFamiliesLimitations")]
        [JsonProperty("deviceFamiliesLimitations")]
        [XmlArray(ElementName = "deviceFamiliesLimitations", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("device_families_limitations")]
        public List<KalturaHouseholdDeviceFamilyLimitations> DeviceFamiliesLimitations { get; set; }

    }
}