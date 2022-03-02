using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public partial class KalturaIotDefault : KalturaOTTObject
    {
        /// <summary>
        /// PoolId
        /// </summary>
        [DataMember(Name = "poolId")]
        [JsonProperty(PropertyName = "poolId")]
        [XmlElement(ElementName = "poolId")]
        public string PoolId { get; set; }
        /// <summary>
        /// Region
        /// </summary>
        [DataMember(Name = "region")]
        [JsonProperty(PropertyName = "region")]
        [XmlElement(ElementName = "region")]
        public string Region { get; set; }
        /// <summary>
        /// AppClientId
        /// </summary>
        [DataMember(Name = "appClientId")]
        [JsonProperty(PropertyName = "appClientId")]
        [XmlElement(ElementName = "appClientId")]
        public string AppClientId { get; set; }
    }
}
