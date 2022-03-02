using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public partial class KalturaCognitoIdentity : KalturaOTTObject
    {
        /// <summary>
        /// Default
        /// </summary>
        [DataMember(Name = "iotDefault")]
        [JsonProperty(PropertyName = "iotDefault")]
        [XmlElement(ElementName = "iotDefault")]
        public KalturaIotDefault IotDefault { get; set; }
    }
}
