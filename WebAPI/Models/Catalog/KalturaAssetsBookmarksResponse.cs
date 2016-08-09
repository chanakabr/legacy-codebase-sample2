using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// List of assets and their bookmarks
    /// </summary>
    [DataContract(Name = "KalturaAssetsPositionsResponse", Namespace = "")]
    [XmlRoot("KalturaAssetsPositionsResponse")]
    [Obsolete]
    public class KalturaAssetsBookmarksResponse : KalturaListResponse
    {

        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaAssetBookmarks> AssetsBookmarks { get; set; }
    }

    /// <summary>
    /// The slim asset details and it's bookmarks
    /// </summary>
    [Serializable]
    [Obsolete]
    public class KalturaAssetBookmarks : KalturaSlimAsset
    {

        /// <summary>
        /// List of bookmarks
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaAssetBookmark> Bookmarks { get; set; }
    }
}