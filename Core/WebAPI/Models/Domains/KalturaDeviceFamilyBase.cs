using Newtonsoft.Json;
using System;
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

        /// <summary>
        /// Type of device family.
        /// <see cref="KalturaDeviceFamilyType.Custom"/> if this device family belongs only to this group,
        /// <see cref="KalturaDeviceFamilyType.System"/> otherwise.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement("type")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaDeviceFamilyType Type { get; set; }
    }

}