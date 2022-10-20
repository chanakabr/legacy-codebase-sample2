using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Segmentation
{
    [SchemeClass(Required = new[] { "idIn" })]
    public partial class KalturaSegmentValueFilter : KalturaBaseSegmentationTypeFilter
    {
        /// <summary>
        /// Comma separated segmentation identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(MinLength = 1, DynamicMinInt = 1)]
        public string IdIn { get; set; }
    }
}