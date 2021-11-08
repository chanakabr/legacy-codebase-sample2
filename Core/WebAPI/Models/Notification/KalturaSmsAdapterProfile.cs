using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// Sms adapter profile
    /// </summary>
    public partial class KalturaSmsAdapterProfile : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// adapter url
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        [SchemeProperty(MinLength = 1, MaxLength = 256)]
        public string AdapterUrl { get; set; }

        /// <summary>
        /// Shared Secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        [SchemeProperty(MinLength = 1, MaxLength = 256)]
        public string SharedSecret { get; set; }

        /// <summary>
        /// SSO Adapter is active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        public int? IsActive { get; set; }

        /// <summary>
        /// SSO Adapter extra parameters
        /// </summary>
        [DataMember(Name = "settings")]
        [JsonProperty("settings")]
        [XmlElement(ElementName = "settings", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }

        /// <summary>
        /// SSO Adapter external identifier
        /// </summary>
        [DataMember(Name = "externalIdentifier")]
        [JsonProperty("externalIdentifier")]
        [XmlElement(ElementName = "externalIdentifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
    }
}