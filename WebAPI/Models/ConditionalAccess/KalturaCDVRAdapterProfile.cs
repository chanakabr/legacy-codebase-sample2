using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
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
    [OldStandard("isActive", "is_active")]
    [OldStandard("adapterUrl", "adapter_url")]
    [OldStandard("externalIdentifier", "external_identifier")]
    [OldStandard("sharedSecret", "shared_secret")]
    [OldStandard("dynamicLinksSupport", "dynamic_links_support")]
    public class KalturaCDVRAdapterProfile : KalturaOTTObject
    {
        /// <summary>
        /// C-DVR adapter identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
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
        public bool? IsActive { get; set; }

        /// <summary>
        /// C-DVR adapter adapter URL
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
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
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// C-DVR shared secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        public string SharedSecret { get; set; }

        /// <summary>
        /// Indicates whether the C-DVR adapter supports dynamic URLs
        /// </summary>
        [DataMember(Name = "dynamicLinksSupport")]
        [JsonProperty("dynamicLinksSupport")]
        [XmlElement(ElementName = "dynamicLinksSupport")]
        public bool? DynamicLinksSupport { get; set; }
    }
}
