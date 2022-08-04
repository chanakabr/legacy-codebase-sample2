using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaUnifiedChannel : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// Channel identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(MinLong = 1)]
        public long Id { get; set; }

        /// <summary>
        /// Channel Type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type", IsNullable = false)]
        public KalturaChannelType Type { get; set; }
    }
}