using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;


namespace WebAPI.Models.Partner
{
    public partial class KalturaResetPasswordPartnerConfigTemplate : KalturaOTTObject
    {
        /// <summary>
        /// id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(MinLength = 1, MaxLength = 255)]
        public string Id { get; set; }

        /// <summary>
        /// label
        /// </summary>
        [DataMember(Name = "label")]
        [JsonProperty("label")]
        [XmlElement(ElementName = "label")]
        [SchemeProperty(MinLength = 1, MaxLength = 255)]
        public string Label { get; set; }

        /// <summary>
        /// is Default
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        public bool IsDefault { get; set; }
    }
}