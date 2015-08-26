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
        /// Entities IDs
        /// </summary>
        [DataMember(Name = "ids")]
        [JsonProperty(PropertyName = "ids")]
        [XmlArray(ElementName = "ids")]
        [XmlArrayItem(ElementName = "item")]        
        public List<KalturaIntegerValue> IDs { get; set; }

        /// <summary>
        /// Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaCatalogReferenceBy By { get; set; }
    }
}