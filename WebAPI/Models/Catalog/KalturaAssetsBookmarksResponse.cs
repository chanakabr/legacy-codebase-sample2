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
    /// List Of Assets And Their Positions
    /// </summary>
    [DataContract(Name = "KalturaAssetsPositionsResponse", Namespace = "")]
    [XmlRoot("KalturaAssetsPositionsResponse")]
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
    /// The Slim Asset Details And His Positions
    /// </summary>
    [Serializable]
    public class KalturaAssetBookmarks : KalturaSlimAsset
    {

        /// <summary>
        /// Positions
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaAssetBookmark> Bookmarks { get; set; }
    }
}