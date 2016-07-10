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
    [DataContract(Name = "KalturaBookmarkListResponse", Namespace = "")]
    [XmlRoot("KalturaBookmarkListResponse")]
    public class KalturaBookmarkListResponse : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaBookmark> AssetsBookmarks { get; set; }
    }
}