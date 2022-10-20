using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Filter for segmentation types
    /// </summary>
    [SchemeClass(MaxProperties = 1)]
    public partial class KalturaSegmentationTypeFilter : KalturaBaseSegmentationTypeFilter
    {
        /// <summary>
        /// Comma separated segmentation types identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(IsNullable = true, MinLength = 1, DynamicMinInt = 1)]
        public string IdIn { get; set; }

        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(IsNullable = true, MinLength = 1)]
        public string Ksql { get; set; }

        /// <summary>
        /// Name of segment contains specific string value
        /// </summary>
        [DataMember(Name = "nameContain")]
        [JsonProperty("nameContain")]
        [XmlElement(ElementName = "nameContain", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(IsNullable = true, MinLength = 1, MaxLength = 50)]
        public string NameContain { get; set; }
    }
}