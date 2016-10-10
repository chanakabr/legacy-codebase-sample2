using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaConfigurationGroupTag : KalturaOTTObject
    {
        [DataMember(Name = "configurationGroupId")]
        [JsonProperty("configurationGroupId")]
        [XmlElement(ElementName = "configurationGroupId")]
        public string ConfigurationGroupId { get; set; }

        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int PartnerId { get; set; }

        [DataMember(Name = "tag")]
        [JsonProperty("tag")]
        [XmlElement(ElementName = "tag")]
        public string Tag { get; set; }       
    }
}