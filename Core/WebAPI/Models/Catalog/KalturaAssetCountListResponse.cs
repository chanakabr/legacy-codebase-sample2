using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset counts wrapper - represents a group
    /// </summary>
    [Serializable]
    public partial class KalturaAssetCountListResponse : KalturaListResponse
    {
        /// <summary>
        /// Count of assets that match filter result, regardless of group by result
        /// </summary>
        [DataMember(Name = "assetsCount")]
        [JsonProperty(PropertyName = "assetsCount")]
        [XmlElement(ElementName = "assetsCount")]
        public int AssetsCount { get; set; }

        /// <summary>
        /// List of groupings (field name and sub-list of values and their counts)
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetsCount> Objects { get; set; }
    }
}
