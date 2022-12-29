using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// The slim asset details and it's bookmarks
    /// </summary>
    [Serializable]
    [Obsolete]
    public partial class KalturaAssetBookmarks : KalturaSlimAsset
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