using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaConfigurationGroupTag : KalturaOTTObject
    {
        /// <summary>
        /// Configuration group identifier
        /// </summary>
        [DataMember(Name = "configurationGroupId")]
        [JsonProperty("configurationGroupId")]
        [XmlElement(ElementName = "configurationGroupId")]
        public string ConfigurationGroupId { get; set; }

        /// <summary>
        /// Partner identifier
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        [SchemeProperty(ReadOnly = true)]
        public int PartnerId { get; set; }

        /// <summary>
        /// Tag
        /// </summary>
        [DataMember(Name = "tag")]
        [JsonProperty("tag")]
        [XmlElement(ElementName = "tag")]
        public string Tag { get; set; }       
    }
}