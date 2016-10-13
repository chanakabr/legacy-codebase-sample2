using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaConfigurationGroupDevice : KalturaOTTObject
    {
        /// <summary>
        /// Configuration group id
        /// </summary>
        [DataMember(Name = "configurationGroupId")]
        [JsonProperty("configurationGroupId")]
        [XmlElement(ElementName = "configurationGroupId")]
        public string ConfigurationGroupId { get; set; }

        /// <summary>
        /// Partner id
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        [SchemeProperty(ReadOnly = true)]
        public int PartnerId { get; set; }

        /// <summary>
        /// Device UDID
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty("udid")]
        [XmlElement(ElementName = "udid")]
        public string Udid { get; set; }
    }
}