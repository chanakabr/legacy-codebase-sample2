using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Device family limitations details
    /// </summary>
    public partial class KalturaHouseholdDeviceFamilyLimitations : KalturaDeviceFamilyBase
    {
        /// <summary>
        /// Allowed device change frequency code
        /// </summary>
        [DataMember(Name = "frequency")]
        [JsonProperty("frequency")]
        [XmlElement(ElementName = "frequency")]
        [SchemeProperty(IsNullable = true)]
        public int? Frequency { get; set; }

        /// <summary>
        /// Max number of devices allowed for this family
        /// </summary>
        [DataMember(Name = "deviceLimit")]
        [JsonProperty("deviceLimit")]
        [XmlElement(ElementName = "deviceLimit")]
        [SchemeProperty(IsNullable = true)]
        [OldStandardProperty("device_limit")]
        public int? DeviceLimit { get; set; }

        /// <summary>
        /// Max number of streams allowed for this family
        /// </summary>
        [DataMember(Name = "concurrentLimit")]
        [JsonProperty("concurrentLimit")]
        [XmlElement(ElementName = "concurrentLimit")]
        [SchemeProperty(IsNullable = true)]
        [OldStandardProperty("concurrent_limit")]
        public int? ConcurrentLimit { get; set; }

        /// <summary>
        /// Is the Max number of devices allowed for this family is default value or not
        /// </summary>
        [DataMember(Name = "isDefaultDeviceLimit")]
        [JsonProperty("isDefaultDeviceLimit")]
        [XmlElement(ElementName = "isDefaultDeviceLimit")]
        [SchemeProperty(ReadOnly = true, IsNullable = true)]
        public bool? IsDefaultDeviceLimit { get; set; }

        /// <summary>
        /// Is the Max number of streams allowed for this family is default value or not
        /// </summary>
        [DataMember(Name = "isDefaultConcurrentLimit")]
        [JsonProperty("isDefaultConcurrentLimit")]
        [XmlElement(ElementName = "isDefaultConcurrentLimit")]
        [SchemeProperty(ReadOnly = true, IsNullable = true)]
        public bool? IsDefaultConcurrentLimit { get; set; }
    }
}