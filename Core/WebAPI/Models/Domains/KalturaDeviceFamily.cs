using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Device family details
    /// </summary>
    public partial class KalturaDeviceFamilyBase : KalturaOTTObject
    {
        /// <summary>
        /// Device family identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(IsNullable = true)]
        public long? Id { get; set; }

        /// <summary>
        /// Device family name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(ReadOnly = true)]
        public string Name { get; set; }

        /// <summary>
        /// Max number of devices allowed for this family
        /// </summary>
        [DataMember(Name = "deviceLimit")]
        [JsonProperty("deviceLimit")]
        [XmlElement(ElementName = "deviceLimit")]
        [OldStandardProperty("device_limit")]
        [Obsolete]
        public int? DeviceLimit { get; set; }

        /// <summary>
        /// Max number of streams allowed for this family
        /// </summary>
        [DataMember(Name = "concurrentLimit")]
        [JsonProperty("concurrentLimit")]
        [XmlElement(ElementName = "concurrentLimit")]
        [OldStandardProperty("concurrent_limit")]
        [Obsolete]
        public int? ConcurrentLimit { get; set; }
    }

    /// <summary>
    /// Device family details
    /// </summary>
    public partial class KalturaDeviceFamily : KalturaDeviceFamilyBase
    {
        /// <summary>
        /// List of all the devices in this family
        /// </summary>
        [DataMember(Name = "devices")]
        [JsonProperty("devices")]
        [XmlArray(ElementName = "devices", IsNullable = true)]
        [XmlArrayItem("item")]
        [Obsolete]
        public List<KalturaDevice> Devices { get; set; }
    }

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
    }
}