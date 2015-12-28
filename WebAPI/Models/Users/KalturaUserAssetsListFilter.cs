using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// A user list of assets
    /// </summary>
    [Serializable]
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
        [DataMember(Name = "list_type")]
        [JsonProperty("list_type")]
        [XmlElement(ElementName = "list_type")]
        public KalturaUserAssetsListType ListType { get; set; }

        /// <summary>
        /// The requested asset type
        /// </summary>
        [DataMember(Name = "asset_type")]
        [JsonProperty("asset_type")]
        [XmlElement(ElementName = "asset_type")]
        public KalturaUserAssetsListItemType AssetType { get; set; }
    }
}