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
    /// Device family details
    /// </summary>
    [OldStandard("deviceLimit", "device_limit")]
    [OldStandard("concurrentLimit", "concurrent_limit")]
    public class KalturaDeviceFamilyBase : KalturaOTTObject
    {
        /// <summary>
        /// Device family identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

        /// <summary>
        /// Device family name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Max number of devices allowed for this family
        /// </summary>
        [DataMember(Name = "deviceLimit")]
        [JsonProperty("deviceLimit")]
        [XmlElement(ElementName = "deviceLimit")]
        public int? DeviceLimit { get; set; }

        /// <summary>
        /// Max number of streams allowed for this family
        /// </summary>
        [DataMember(Name = "concurrentLimit")]
        [JsonProperty("concurrentLimit")]
        [XmlElement(ElementName = "concurrentLimit")]
        public int? ConcurrentLimit { get; set; }
    }


    /// <summary>
    /// Device family details
    /// </summary>
    [Obsolete]
    public class KalturaDeviceFamily : KalturaDeviceFamilyBase
    {
        /// <summary>
        /// List of all the devices in this family
        /// </summary>
        [DataMember(Name = "devices")]
        [JsonProperty("devices")]
        [XmlArray(ElementName = "devices", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaDevice> Devices { get; set; }
    }

    /// <summary>
    /// Device family limitations details
    /// </summary>
    public class KalturaHouseholdDeviceFamilyLimitations : KalturaDeviceFamilyBase
    {
        /// <summary>
        /// Allowed device change frequency code
        /// </summary>
        [DataMember(Name = "frequency")]
        [JsonProperty("frequency")]
        [XmlElement(ElementName = "frequency")]
        public int? Frequency { get; set; }
    }
}