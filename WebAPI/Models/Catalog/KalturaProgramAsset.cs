using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Program-asset info
    /// </summary>
    [Serializable]
    public class KalturaProgramAsset : KalturaAsset
    {
        /// <summary>
        /// EPG channel identifier
        /// </summary>
        [DataMember(Name = "epgChannelId")]
        [JsonProperty(PropertyName = "epgChannelId")]
        [XmlElement(ElementName = "epgChannelId")]
        public long? EpgChannelId { get; set; }

        /// <summary>
        /// EPG identifier
        /// </summary>
        [DataMember(Name = "epgId")]
        [JsonProperty(PropertyName = "epgId")]
        [XmlElement(ElementName = "epgId")]
        public string EpgId { get; set; }

        /// <summary>
        /// Ralated media identifier
        /// </summary>
        [DataMember(Name = "relatedMediaId")]
        [JsonProperty(PropertyName = "relatedMediaId")]
        [XmlElement(ElementName = "relatedMediaId")]
        public long? RelatedMediaId { get; set; }

        /// <summary>
        /// Unique identifier for the program
        /// </summary>
        [DataMember(Name = "crid")]
        [JsonProperty(PropertyName = "crid")]
        [XmlElement(ElementName = "crid")]
        public string Crid { get; set; }

        /// <summary>
        /// Id of linear media asset
        /// </summary>
        [DataMember(Name = "linearAssetId")]
        [JsonProperty(PropertyName = "linearAssetId")]
        [XmlElement(ElementName = "linearAssetId")]
        public long? LinearAssetId { get; set; }
    }
}