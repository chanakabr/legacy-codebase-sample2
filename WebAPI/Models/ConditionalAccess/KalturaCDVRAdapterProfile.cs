using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// C-DVR adapter profiles
    /// </summary>
    [Serializable]
    public class KalturaCDVRAdapterProfileListResponse : KalturaListResponse
    {
        /// <summary>
        /// C-DVR adapter profiles
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaCDVRAdapterProfile> Objects { get; set; }
    }

    /// <summary>
    /// C-DVR Adapter
    /// </summary>
    public class KalturaCDVRAdapterProfile : KalturaOTTObject
    {
        /// <summary>
        /// C-DVR adapter identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// C-DVR adapter name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// C-DVR adapter active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        [OldStandardProperty("is_active")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// C-DVR adapter adapter URL
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        [OldStandardProperty("adapter_url")]
        public string AdapterUrl { get; set; }
               
        /// <summary>
        /// C-DVR adapter extra parameters
        /// </summary>
        [DataMember(Name = "settings")]
        [JsonProperty("settings")]        
        [XmlElement("settings", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }

        /// <summary>
        /// C-DVR adapter external identifier
        /// </summary>
        [DataMember(Name = "externalIdentifier")]
        [JsonProperty("externalIdentifier")]
        [XmlElement(ElementName = "externalIdentifier")]
        [OldStandardProperty("external_identifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// C-DVR shared secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("shared_secret")]
        public string SharedSecret { get; set; }

        /// <summary>
        /// Indicates whether the C-DVR adapter supports dynamic URLs
        /// </summary>
        [DataMember(Name = "dynamicLinksSupport")]
        [JsonProperty("dynamicLinksSupport")]
        [XmlElement(ElementName = "dynamicLinksSupport")]
        [OldStandardProperty("dynamic_links_support")]
        public bool? DynamicLinksSupport { get; set; }
    }
}
