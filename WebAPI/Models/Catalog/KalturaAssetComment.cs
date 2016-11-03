using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Social;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset Comment
    /// </summary>
    [Serializable]
    public class KalturaAssetComment : KalturaSocialComment
    {
        /// <summary>
        /// Comment ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Asset identifier
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        [SchemeProperty(MinInteger = 1)]
        public int AssetId { get; set; }

        /// <summary>
        /// Asset Type
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty(PropertyName = "assetType")]
        [XmlElement(ElementName = "assetType")]
        public KalturaAssetType AssetType { get; set; }

        /// <summary>
        /// Sub Header
        /// </summary>
        [DataMember(Name = "subHeader")]
        [JsonProperty(PropertyName = "subHeader")]
        [XmlElement(ElementName = "subHeader")]
        public string SubHeader { get; set; }
    }
}