using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Users
{
    public partial class KalturaDeviceManufacturersReferenceDataFilter : KalturaDeviceReferenceDataFilter
    {
        /// <summary>
        /// name equal
        /// </summary>
        [DataMember(Name = "nameEqual")]
        [JsonProperty("nameEqual")]
        [XmlElement(ElementName = "nameEqual")]
        public string NameEqual { get; set; }
    }
}
