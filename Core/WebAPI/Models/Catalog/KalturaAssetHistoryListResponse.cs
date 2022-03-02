using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Watch history asset wrapper
    /// </summary>
    [Serializable]
    public partial class KalturaAssetHistoryListResponse : KalturaListResponse
    {
        /// <summary>
        /// WatchHistoryAssets Models
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetHistory> Objects { get; set; }
    }
}
