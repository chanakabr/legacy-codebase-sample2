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
    /// Filtering assets
    /// </summary>
    [Serializable]
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
        [DataMember(Name = "reference_type")]
        [JsonProperty("reference_type")]
        [XmlElement(ElementName = "reference_type")]
        public KalturaCatalogReferenceBy ReferenceType { get; set; }
    }
}