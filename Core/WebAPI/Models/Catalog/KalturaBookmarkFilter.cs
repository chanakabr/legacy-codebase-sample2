using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Filtering Assets requests
    /// </summary>
    [Serializable]
    public partial class KalturaBookmarkFilter : KalturaFilter<KalturaBookmarkOrderBy>
    {
        /// <summary>
        /// List of assets identifier
        /// </summary>
        [Obsolete]
        [DataMember(Name = "assetIn")]
        [JsonProperty(PropertyName = "assetIn")]
        [XmlArray(ElementName = "assetIn", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaSlimAsset> AssetIn { get; set; }

        /// <summary>
        /// Comma separated list of assets identifiers
        /// </summary>
        [DataMember(Name = "assetIdIn")]
        [JsonProperty(PropertyName = "assetIdIn")]
        [XmlElement(ElementName = "assetIdIn", IsNullable = true)]
        public string AssetIdIn { get; set; }

        /// <summary>
        /// Asset type
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty(PropertyName = "assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual", IsNullable = true)]
        public KalturaAssetType? AssetTypeEqual { get; set; }

        public override KalturaBookmarkOrderBy GetDefaultOrderByValue()
        {
            return KalturaBookmarkOrderBy.POSITION_ASC;
        }
    }
}