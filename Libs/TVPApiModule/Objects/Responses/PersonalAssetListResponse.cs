using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace TVPApiModule.Objects.Responses
{
    /// <summary>
    /// List of personal assets
    /// </summary>
    public class PersonalAssetListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<PersonalAssetInfo> Objects
        {
            get;
            set;
        }

        [DataMember(Name = "total_items")]
        [JsonProperty(PropertyName = "total_items")]
        [XmlElement(ElementName = "total_items")]
        public int TotalItems
        {
            get;
            set;
        }

        [DataMember(Name = "status")]
        [JsonProperty(PropertyName = "status")]
        [XmlElement(ElementName = "status")]
        public Status Status
        {
            get;
            set;
        }
    }

    public class PersonalAssetInfo
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public long Id
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
        public TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.eAssetTypes Type
        {
            get;
            set;
        }

        [DataMember(Name = "bookmarks")]
        [JsonProperty(PropertyName = "bookmarks")]
        [XmlArray(ElementName = "bookmarks", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<AssetBookmarkResponse> Bookmarks
        {
            get;
            set;
        }

        [DataMember(Name = "files")]
        [JsonProperty(PropertyName = "files")]
        [XmlArray(ElementName = "files", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<MediaFileItemPricesContainer> Files
        {
            get;
            set;
        }
    }
}
