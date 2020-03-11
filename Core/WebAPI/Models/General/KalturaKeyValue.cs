using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    public partial class KalturaKeyValue : KalturaOTTObject
    {
        /// <summary>
        /// Key
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key", IsNullable = true)]
        public string key { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value", IsNullable = true)]
        public string value { get; set; }
    }
}
