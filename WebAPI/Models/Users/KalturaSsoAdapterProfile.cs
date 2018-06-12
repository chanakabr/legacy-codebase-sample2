using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// SSO adapter configuration
    /// </summary>
    public class KalturaSsoAdapterProfile : KalturaOTTObject
    {
        /// <summary>
        /// SSO Adapter id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// SSO Adapter name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// SSO Adapter is active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        [OldStandardProperty("is_active")]
        public int? IsActive { get; set; }

        /// <summary>
        /// SSO Adapter URL
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        [OldStandardProperty("adapter_url")]
        public string AdapterUrl { get; set; }

        /// <summary>
        /// SSO Adapter extra parameters
        /// </summary>
        [DataMember(Name = "ssoAdapterSettings")]
        [JsonProperty("ssoAdapterSettings")]
        [XmlElement(ElementName = "ssoAdapterSettings", IsNullable = true)]
        [OldStandardProperty("sso_adapter_settings")]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }

        /// <summary>
        /// SSO Adapter external identifier
        /// </summary>
        [DataMember(Name = "externalIdentifier")]
        [JsonProperty("externalIdentifier")]
        [XmlElement(ElementName = "externalIdentifier")]
        [OldStandardProperty("external_identifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Shared Secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        [OldStandardProperty("shared_secret")]
        public string SharedSecret { get; set; }

    }
}
