using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Content based source (meta, tag etc.)
    /// </summary>
    public partial class KalturaContentSource : KalturaSegmentSource
    {
        /// <summary>
        /// Topic (meta or tag) name
        /// </summary>
        [DataMember(Name = "field")]
        [JsonProperty(PropertyName = "field")]
        [XmlElement(ElementName = "field")]
        [SchemeProperty()]
        public string Field { get; set; }
    }
}
