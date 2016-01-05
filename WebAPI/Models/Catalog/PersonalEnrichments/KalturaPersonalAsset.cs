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

    public class KalturaPersonalAsset : KalturaOTTObject
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
        /// Identifies the asset type (EPG, Movie, TV Series, etc). 
        /// Possible values: 0 – EPG linear programs, or any asset type ID according to the asset types IDs defined in the system.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public int Type
        {
            get;
            set;
        }

        [DataMember(Name = "bookmark")]
        [JsonProperty(PropertyName = "bookmark")]
        [XmlElement(ElementName = "bookmark")]
        public int Bookmark
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
    }
}