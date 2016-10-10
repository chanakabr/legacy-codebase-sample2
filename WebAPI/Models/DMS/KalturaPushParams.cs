using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaPushParams : KalturaOTTObject
    {
        /// <summary>
        /// Token
        /// </summary>
        [DataMember(Name = "token")]
        [XmlElement(ElementName = "token")]       
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// External token
        /// </summary>
        [DataMember(Name = "externalToken")]
        [XmlElement(ElementName = "externalToken")]
        [JsonProperty("externalToken")]
        public string ExternalToken { get; set; }
    }
}
