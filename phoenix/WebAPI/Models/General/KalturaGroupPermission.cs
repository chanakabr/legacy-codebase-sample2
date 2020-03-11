using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;

namespace WebAPI.Models.General
{
    public partial class KalturaGroupPermission : KalturaPermission
    {
        /// <summary>
        /// Permission identifier
        /// </summary>
        [DataMember(Name = "group")]
        [JsonProperty("group")]
        [XmlElement(ElementName = "group")]
        [SchemeProperty(ReadOnly = true)]
        public string Group { get; set; }
    }
}