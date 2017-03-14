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
    /// Asset counts wrapper - represents a group
    /// </summary>
    [Serializable]
    public class KalturaAssetCountListResponse : KalturaListResponse
    {
        /// <summary>
        /// Count of assets that match filter result, regardless of group by result
        /// </summary>
        [DataMember(Name = "assetsCount")]
        [JsonProperty(PropertyName = "assetsCount")]
        [XmlElement(ElementName = "assetsCount")]
        public int AssetsCount
        {
            get;
            set;
        }

        /// <summary>
        /// List of groupings (field name and sub-list of values and their counts)
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetsCount> Objects
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Asset count - represents a specific value of the field, its count and its sub groups.
    /// </summary>
    public class KalturaAssetCount : KalturaOTTObject
    {
        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// Count
        /// </summary>
        [DataMember(Name = "count")]
        [JsonProperty(PropertyName = "count")]
        [XmlElement(ElementName = "count")]
        public int Count
        {
            get;
            set;
        }

        /// <summary>
        /// Sub groups
        /// </summary>
        [DataMember(Name = "subs")]
        [JsonProperty(PropertyName = "subs")]
        [XmlArray(ElementName = "subs", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetsCount> SubCounts
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Single aggregation objects
    /// </summary>
    public class KalturaAssetsCount : KalturaOTTObject
    {
        /// <summary>
        /// Field name
        /// </summary>
        [DataMember(Name = "field")]
        [JsonProperty(PropertyName = "field")]
        [XmlElement(ElementName = "field")]
        public string Field
        {
            get;
            set;
        }

        /// <summary>
        /// Values, their count and sub groups
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetCount> Objects
        {
            get;
            set;
        }
    }
}