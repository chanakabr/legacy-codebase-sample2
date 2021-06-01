using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    public partial class KalturaDynamicData : KalturaOTTObject
    {
        public KalturaDynamicData(string key, KalturaValue value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>Key
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public KalturaValue Value { get; set; }
    }
}