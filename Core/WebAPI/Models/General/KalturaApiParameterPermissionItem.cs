using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.API;

namespace WebAPI.Models.General
{
    public partial class KalturaApiParameterPermissionItem : KalturaPermissionItem
    {
        /// <summary>
        /// API object name
        /// </summary>
        [DataMember(Name = "object")]
        [JsonProperty("object")]
        [XmlElement(ElementName = "object")]
        public string Object { get; set; }

        /// <summary>
        /// API parameter name
        /// </summary>
        [DataMember(Name = "parameter")]
        [JsonProperty("parameter")]
        [XmlElement(ElementName = "parameter")]
        public string Parameter { get; set; }

        /// <summary>
        /// API action type
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty("action")]
        [XmlElement(ElementName = "action")]
        public KalturaApiParameterPermissionItemAction Action { get; set; }
    }
}