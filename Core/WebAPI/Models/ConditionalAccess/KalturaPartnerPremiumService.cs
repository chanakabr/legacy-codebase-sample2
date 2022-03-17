using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Premium service
    /// </summary>
    public partial class KalturaPartnerPremiumService : KalturaOTTObject
    {
        /// <summary>
        /// Service identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Service name / description
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(ReadOnly = true)]
        public string Name { get; set; }

        /// <summary>
        /// Service name / description
        /// </summary>
        [DataMember(Name = "isApplied")]
        [JsonProperty("isApplied")]
        [XmlElement(ElementName = "isApplied")]
        public bool IsApplied { get; set; }
    }
}