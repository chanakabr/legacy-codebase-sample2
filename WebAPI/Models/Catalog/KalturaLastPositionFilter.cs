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
        /// Asset identifier
        /// </summary>
        [DataMember(Name = "asset_id")]
        [JsonProperty(PropertyName = "asset_id")]
        [XmlElement(ElementName = "asset_id")]
        public string AssetID { get; set; }

        /// <summary>
        /// Asset type
        /// </summary>
        [DataMember(Name = "asset_type")]
        [JsonProperty(PropertyName = "asset_type")]
        [XmlElement(ElementName = "asset_type")]
        public KalturaAssetType AssetType { get; set; }

        /// <summary>
        /// Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaEntityReferenceBy By { get; set; }
    }
}