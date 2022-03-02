using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Segmentation
{
    public partial class KalturaSegmentValueFilter : KalturaBaseSegmentationTypeFilter
    {
        /// <summary>
        /// Comma separated segmentation identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        public string IdIn { get; set; }
    }
}
