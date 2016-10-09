using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaConfigurationGroupDevice : KalturaOTTObject
    {
        [DataMember(Name = "groupId")]
        [JsonProperty("groupId")]
        [XmlElement(ElementName = "groupId")]
        public string GroupId { get; set; }

        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int PartnerId { get; set; }

        [DataMember(Name = "udid")]
        [JsonProperty("udid")]
        [XmlElement(ElementName = "udid")]
        public string Udid { get; set; }
    }
}