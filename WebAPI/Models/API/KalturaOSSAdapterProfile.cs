using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// OSS Adapter
    /// </summary>
    public class KalturaOSSAdapterProfile : KalturaOSSAdapterBaseProfile
    {
        /// <summary>
        /// OSS adapter active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        [OldStandardProperty("is_active")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// OSS adapter adapter URL
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        [OldStandardProperty("adapter_url")]
        public string AdapterUrl { get; set; }
               
        /// <summary>
        /// OSS adapter extra parameters
        /// </summary>
        [DataMember(Name = "ossAdapterSettings")]
        [JsonProperty("ossAdapterSettings")]
        [XmlElement("ossAdapterSettings", IsNullable = true)]
        [OldStandardProperty("oss_adapter_settings")]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }

        /// <summary>
        /// OSS adapter external identifier
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
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("shared_secret")]
        public string SharedSecret { get; set; }
    }
}
