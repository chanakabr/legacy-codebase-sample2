using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset Comment
    /// </summary>
    [Serializable]
    public class KalturaAssetComment : KalturaOTTObject
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
        public string AssetId { get; set; }

        /// <summary>
        /// Asset Type
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty(PropertyName = "assetType")]
        [XmlElement(ElementName = "assetType")]
        public KalturaAssetType AssetType { get; set; }

        /// <summary>
        /// Header
        /// </summary>
        [DataMember(Name = "header")]
        [JsonProperty(PropertyName = "header")]
        [XmlElement(ElementName = "header")]
        public string Header { get; set; }

        /// <summary>
        /// Sub Header
        /// </summary>
        [DataMember(Name = "subHeader")]
        [JsonProperty(PropertyName = "subHeader")]
        [XmlElement(ElementName = "subHeader")]
        public string SubHeader { get; set; }

        /// <summary>
        /// Text
        /// </summary>
        [DataMember(Name = "text")]
        [JsonProperty(PropertyName = "text")]
        [XmlElement(ElementName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// CreateDate
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        public long CreateDate { get; set; }

        /// <summary>
        /// Writer
        /// </summary>
        [DataMember(Name = "writer")]
        [JsonProperty(PropertyName = "writer")]
        [XmlElement(ElementName = "writer")]
        public string Writer { get; set; }
    }
}