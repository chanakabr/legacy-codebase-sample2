using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaConfigurationGroupTag : KalturaOTTObject
    {
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int PartnerId { get; set; }

        [DataMember(Name = "tag")]
        [JsonProperty("tag")]
        [XmlElement(ElementName = "tag")]
        public string Tag { get; set; }

        [DataMember(Name = "docType")]
        [JsonProperty("docType")]
        [XmlElement(ElementName = "docType")]
        private string DocType { get; set; }

        public KalturaConfigurationGroupTag()
        {
            this.DocType = "tag_map";
        }
    }
}