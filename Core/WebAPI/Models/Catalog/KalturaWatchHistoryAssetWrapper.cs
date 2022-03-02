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
    [Obsolete]
    public partial class KalturaWatchHistoryAssetWrapper : KalturaListResponse, KalturaIAssetable
    {
        /// <summary>
        /// WatchHistoryAssets Models
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaWatchHistoryAsset> Objects { get; set; }

        protected override void Init()
        {
            base.Init();
            Objects = new List<KalturaWatchHistoryAsset>();
        }
    }
}
