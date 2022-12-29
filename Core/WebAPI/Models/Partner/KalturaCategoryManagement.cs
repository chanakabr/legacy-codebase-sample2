using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Category management
    /// </summary>
    public partial class KalturaCategoryManagement : KalturaOTTObject
    {
        /// <summary>
        /// Default CategoryVersion tree id
        /// </summary>
        [DataMember(Name = "defaultTreeId")]
        [JsonProperty("defaultTreeId")]
        [XmlElement(ElementName = "defaultTreeId")]
        [SchemeProperty(IsNullable = true)]
        public long? DefaultCategoryTreeId { get; set; }

        /// <summary>
        /// Device family to Category TreeId mapping
        /// </summary>
        [DataMember(Name = "deviceFamilyToCategoryTree")]
        [JsonProperty("deviceFamilyToCategoryTree")]
        [XmlElement(ElementName = "deviceFamilyToCategoryTree", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public SerializableDictionary<string, KalturaLongValue> DeviceFamilyToCategoryTree { get; set; }
    }
}