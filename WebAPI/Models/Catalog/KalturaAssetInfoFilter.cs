using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Filtering assets
    /// </summary>
    [Serializable]
    [OldStandard("referenceType", "reference_type")]
    [Obsolete]
    public class KalturaAssetInfoFilter : KalturaOTTObject
    {
        /// <summary>
        /// Filtering condition
        /// </summary>
        public enum KalturaCutWith
        {
            or = 0,
            and = 1
        }

        /// <summary>
        /// Entities IDs
        /// </summary>
        [DataMember(Name = "ids")]
        [JsonProperty(PropertyName = "ids")]
        [XmlArray(ElementName = "ids", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaStringValue> IDs { get; set; }

        /// <summary>
        /// Reference type of the given IDs
        /// </summary>
        [DataMember(Name = "referenceType")]
        [JsonProperty("referenceType")]
        [XmlElement(ElementName = "referenceType")]
        public KalturaCatalogReferenceBy ReferenceType { get; set; }


        /// <summary>
        /// Filter by Tags when filtering by channel ID
        /// </summary>
        [DataMember(Name = "filter_tags")]
        [JsonProperty(PropertyName = "filter_tags")]
        [XmlElement("filter_tags", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> FilterTags { get; set; }

        /// <summary>
        /// Condition between filter_tags
        /// </summary>
        [DataMember(Name = "cut_with")]
        [JsonProperty(PropertyName = "cut_with")]
        [XmlElement("cut_with")]
        public KalturaCutWith cutWith { get; set; }
    }
}