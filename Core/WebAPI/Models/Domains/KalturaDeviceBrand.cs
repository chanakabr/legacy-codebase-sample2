using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Device brand details
    /// </summary>
    public partial class KalturaDeviceBrand : KalturaOTTObject
    {
        /// <summary>
        /// Device brand identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long? Id { get; set; }

        /// <summary>
        /// Device brand name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Device family identifier
        /// </summary>
        [DataMember(Name = "deviceFamilyid")]
        [JsonProperty("deviceFamilyid")]
        [XmlElement(ElementName = "deviceFamilyid")]
        public long? DeviceFamilyId { get; set; }

        /// <summary>
        /// Type of device family.
        /// <see cref="KalturaDeviceBrandType.Custom"/> if this device family belongs only to this group,
        /// <see cref="KalturaDeviceBrandType.System"/> otherwise.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement("type")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaDeviceBrandType Type { get; set; }
    }
}