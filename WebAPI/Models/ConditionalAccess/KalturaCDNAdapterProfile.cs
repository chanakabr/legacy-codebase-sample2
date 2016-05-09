using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// CDN Adapter
    /// </summary>
    public class KalturaCDNAdapterProfile
    {
        /// <summary>
        /// CDN adapter identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int? Id { get; set; }

        /// <summary>
        /// CDNR adapter name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// CDN adapter active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// CDN adapter base URL
        /// </summary>
        [DataMember(Name = "baseUrl")]
        [JsonProperty("baseUrl")]
        [XmlElement(ElementName = "baseUrl")]
        public string BaseUrl { get; set; }

        /// <summary>
        /// CDN adapter dynamic data
        /// </summary>
        [DataMember(Name = "dynamicData")]
        [JsonProperty("dynamicData")]
        [XmlElement("dynamicData", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> DynamicData { get; set; }

        /// <summary>
        /// CDN adapter alias
        /// </summary>
        [DataMember(Name = "alias")]
        [JsonProperty("alias")]
        [XmlElement(ElementName = "alias")]
        public string Alias { get; set; }

        /// <summary>
        /// CDN shared secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        public string SharedSecret { get; set; }

    }
}