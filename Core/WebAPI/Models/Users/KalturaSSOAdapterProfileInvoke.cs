using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    public partial class KalturaSSOAdapterProfileInvoke : KalturaOTTObject
    {
        /// <summary>
        /// key/value map field for adapter data
        /// </summary>
        [DataMember(Name = "adapterData")]
        [JsonProperty("adapterData")]
        [XmlElement(ElementName = "adapterData")]
        public SerializableDictionary<string, KalturaStringValue> AdapterData { get; set; }

        /// <summary>
        /// code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// message
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }
    }
}
