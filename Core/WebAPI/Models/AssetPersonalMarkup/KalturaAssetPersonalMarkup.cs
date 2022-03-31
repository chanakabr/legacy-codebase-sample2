using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.AssetPersonalMarkup
{
    /// <summary>
    /// Asset Personal Markup
    /// </summary>
    [Serializable]
    public partial class KalturaAssetPersonalMarkup : KalturaOTTObject
    {
        /// <summary>
        /// Asset Id
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty("assetId")]
        [XmlElement("assetId")]
        [SchemeProperty(MinLong = 1, ReadOnly = true)]
        public long AssetId { get; set; }

        /// <summary>
        /// Asset Type
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty("assetType")]
        [XmlElement("assetType")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaAssetType AssetType { get; set; }

        /// <summary>
        /// all related asset's Product Markups
        /// </summary>
        [DataMember(Name = "products")]
        [JsonProperty("products")]
        [XmlArray(ElementName = "products", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaProductMarkup> Products { get; set; }
    }
}