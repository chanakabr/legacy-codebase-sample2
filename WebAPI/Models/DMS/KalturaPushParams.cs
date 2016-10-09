using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaPushParams : KalturaOTTObject
    {
        [DataMember(Name = "token")]
        [XmlElement(ElementName = "token")]       
        [JsonProperty("token")]
        public string Token { get; set; }

        [DataMember(Name = "externalToken")]
        [XmlElement(ElementName = "externalToken")]
        [JsonProperty("externalToken")]
        public string ExternalToken { get; set; }
    }
}
