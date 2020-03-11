using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Currency details
    /// </summary>
    public partial class KalturaCurrency : KalturaOTTObject
    {
        /// <summary>
        /// Identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Currency name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Currency Sign
        /// </summary>
        [DataMember(Name = "sign")]
        [JsonProperty("sign")]
        [XmlElement(ElementName = "sign")]
        public string Sign { get; set; }

        /// <summary>
        /// Is the default Currency of the account
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        public bool IsDefault { get; set; }

    }
}