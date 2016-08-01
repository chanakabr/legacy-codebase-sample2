using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// A user list of assets
    /// </summary>
    [Serializable]
    [OldStandard("listTypeEqual", "list_type")]
    [OldStandard("assetTypeEqual", "asset_type")]
    [Obsolete]
    public class KalturaUserAssetsListFilter : KalturaOTTObject
    {
        /// <summary>
        /// Users identifiers
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaEntityReferenceBy By { get; set; }

        /// <summary>
        /// The requested list type
        /// </summary>
        [DataMember(Name = "listTypeEqual")]
        [JsonProperty("listTypeEqual")]
        [XmlElement(ElementName = "listTypeEqual")]
        public KalturaUserAssetsListType ListTypeEqual { get; set; }

        /// <summary>
        /// The requested asset type
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty("assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual")]
        public KalturaUserAssetsListItemType AssetTypeEqual { get; set; }
    }
}