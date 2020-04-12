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
    }
}
