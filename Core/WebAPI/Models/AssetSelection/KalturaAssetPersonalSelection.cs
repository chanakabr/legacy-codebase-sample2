using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.AssetSelection
{
    /// <summary>
    /// Asset personal selection
    /// </summary>
    [Serializable]
    public partial class KalturaAssetPersonalSelection : KalturaOTTObject
    {
        /// <summary>
        /// Asset Id
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty("assetId")]
        [XmlElement("assetId")]
        [SchemeProperty(MinLong = 1, ReadOnly = true)]
        public long AssetId { get; set; }

        /// <summary>
        /// Asset Type
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty("assetType")]
        [XmlElement("assetType")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaAssetType AssetType { get; set; }
        
        /// <summary>
        /// Update Date
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement("updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }
        
    }
}