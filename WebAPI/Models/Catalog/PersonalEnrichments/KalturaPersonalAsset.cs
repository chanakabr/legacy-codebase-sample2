using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// List of personal assets
    /// </summary>
    [Obsolete]
    public class KalturaPersonalAssetListResponse : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPersonalAsset> Objects
        {
            get;
            set;
        }
    }

    [Obsolete]
    public class KalturaPersonalAsset : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public long? Id
        {
            get;
            set;
        }

        /// <summary>
        /// Identifies the asset type (EPG, Media, etc). 
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public KalturaAssetType Type
        {
            get;
            set;
        }

        [DataMember(Name = "bookmarks")]
        [JsonProperty(PropertyName = "bookmarks")]
        [XmlArray(ElementName = "bookmarks", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetBookmark> Bookmarks
        {
            get;
            set;
        }

        [DataMember(Name = "files")]
        [JsonProperty(PropertyName = "files")]
        [XmlArray(ElementName = "files", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaItemPrice> Files
        {
            get;
            set;
        }

        /// <summary>
        /// Is followed by user. 
        /// </summary>
        [DataMember(Name = "following")]
        [JsonProperty(PropertyName = "following")]
        [XmlElement(ElementName = "following")]
        public bool Following
        {
            get;
            set;
        }
    }
}