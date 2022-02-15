using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// User dynamic data source
    /// </summary>
    public partial class KalturaUserDynamicDataSource : KalturaSegmentSource
    {
        /// <summary>
        /// Field name
        /// </summary>
        [DataMember(Name = "field")]
        [JsonProperty(PropertyName = "field")]
        [XmlElement(ElementName = "field")]
        [SchemeProperty()]
        public string Field { get; set; }
    }
}
