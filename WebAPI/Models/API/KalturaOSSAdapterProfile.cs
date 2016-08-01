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
    [OldStandard("isActive", "is_active")]
    [OldStandard("adapterUrl", "adapter_url")]
    [OldStandard("ossAdapterSettings", "oss_adapter_settings")]
    [OldStandard("externalIdentifier", "external_identifier")]
    [OldStandard("sharedSecret", "shared_secret")]
    public class KalturaOSSAdapterProfile : KalturaOSSAdapterBaseProfile
    {
        /// <summary>
        /// OSS adapter active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// OSS adapter adapter URL
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        public string AdapterUrl { get; set; }
               
        /// <summary>
        /// OSS adapter extra parameters
        /// </summary>
        [DataMember(Name = "ossAdapterSettings")]
        [JsonProperty("ossAdapterSettings")]
        [XmlElement("ossAdapterSettings", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }

        /// <summary>
        /// OSS adapter external identifier
        /// </summary>
        [DataMember(Name = "externalIdentifier")]
        [JsonProperty("externalIdentifier")]
        [XmlElement(ElementName = "externalIdentifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Shared Secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        public string SharedSecret { get; set; }
    }
}
