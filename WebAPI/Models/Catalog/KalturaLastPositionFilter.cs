using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Filtering last position requests
    /// </summary>
    [Serializable]
    public class KalturaLastPositionFilter : KalturaOTTObject
    {
        /// <summary>
        /// Device UDID
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty(PropertyName = "udid")]
        [XmlElement(ElementName = "udid")]
        public string UDID { get; set; }

        /// <summary>
        /// Assets identifier
        /// </summary>
        [DataMember(Name = "ids")]
        [JsonProperty(PropertyName = "ids")]
        [XmlArray(ElementName = "objects")]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaStringValue> Ids { get; set; }

        /// <summary>
        /// Assets type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public KalturaLastPositionAssetType Type { get; set; }

        /// <summary>
        /// Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaEntityReferenceBy By { get; set; }
    }
}