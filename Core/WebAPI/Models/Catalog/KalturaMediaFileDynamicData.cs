using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [SchemeClass(Required = new[] {"mediaFileTypeId", "mediaFileTypeKeyName", "value"})]
    public partial class KalturaMediaFileDynamicData : KalturaOTTObject
    {
        /// <summary>
        /// An integer representing the identifier of the value.
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// An integer representing the the mediaFileType holding the keys for which the values should be stored.
        /// </summary>
        [DataMember(Name = "mediaFileTypeId")]
        [JsonProperty(PropertyName = "mediaFileTypeId")]
        [XmlElement(ElementName = "mediaFileTypeId")]
        public long MediaFileTypeId { get; set; }

        /// <summary>
        /// A string representing the key name within the mediaFileType that identifies the list corresponding
        /// to that key name.
        /// </summary>
        [DataMember(Name = "mediaFileTypeKeyName")]
        [JsonProperty(PropertyName = "mediaFileTypeKeyName")]
        [XmlElement(ElementName = "mediaFileTypeKeyName")]
        [SchemeProperty(IsNullable = false, MinLength = 1, MaxLength = 50, Pattern = SchemeInputAttribute.NO_COMMAS_PATTERN)]
        public string MediaFileTypeKeyName { get; set; }

        /// <summary>
        /// Dynamic data value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        [SchemeProperty(IsNullable = false, MinLength = 1, MaxLength = 50, Pattern = SchemeInputAttribute.NO_COMMAS_PATTERN)]
        public string Value { get; set; }
    }
}
