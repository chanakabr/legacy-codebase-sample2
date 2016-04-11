using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
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
        public int Id { get; set; }

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
        [DataMember(Name = "is_active")]
        [JsonProperty("is_active")]
        [XmlElement(ElementName = "is_active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// C-DVR adapter adapter URL
        /// </summary>
        [DataMember(Name = "adapter_url")]
        [JsonProperty("adapter_url")]
        [XmlElement(ElementName = "adapter_url")]
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
        [DataMember(Name = "external_identifier")]
        [JsonProperty("external_identifier")]
        [XmlElement(ElementName = "external_identifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// C-DVR shared secret
        /// </summary>
        [DataMember(Name = "shared_secret")]
        [JsonProperty("shared_secret")]
        [XmlElement(ElementName = "shared_secret")]
        public string SharedSecret { get; set; }

        /// <summary>
        /// Indicates whether the C-DVR adapter supports dynamic URLs
        /// </summary>
        [DataMember(Name = "dynamic_links_support")]
        [JsonProperty("dynamic_links_support")]
        [XmlElement(ElementName = "dynamic_links_support")]
        public bool DynamicLinksSupport { get; set; }
    }
}
