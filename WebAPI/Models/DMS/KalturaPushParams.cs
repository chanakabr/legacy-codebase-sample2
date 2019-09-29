using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public partial class KalturaPushParams : KalturaOTTObject
    {
        /// <summary>
        /// Device-Application push token
        /// </summary>
        [DataMember(Name = "token")]
        [XmlElement(ElementName = "token")]       
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// External device token as received from external push provider in exchange for the device token
        /// </summary>
        [DataMember(Name = "externalToken")]
        [XmlElement(ElementName = "externalToken")]
        [JsonProperty("externalToken")]
        public string ExternalToken { get; set; }
    }
}
