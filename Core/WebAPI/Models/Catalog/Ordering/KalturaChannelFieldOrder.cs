using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.Catalog.Ordering
{
    public partial class KalturaChannelFieldOrder : KalturaBaseChannelOrder
    {
        /// <summary>
        /// Order By
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty(PropertyName = "orderBy")]
        [XmlElement(ElementName = "orderBy")]
        public KalturaChannelFieldOrderByType OrderBy { get; set; }
    }
}