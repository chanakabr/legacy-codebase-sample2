using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner catalog configuration
    /// </summary>
    public partial class KalturaCatalogPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Single multilingual mode
        /// </summary>
        [DataMember(Name = "singleMultilingualMode")]
        [JsonProperty("singleMultilingualMode")]
        [XmlElement(ElementName = "singleMultilingualMode")]
        [SchemeProperty(IsNullable = true)]
        public bool? SingleMultilingualMode { get; set; }

        /// <summary>
        /// Category management
        /// </summary>
        [DataMember(Name = "categoryManagement")]
        [JsonProperty("categoryManagement")]
        [XmlElement(ElementName = "categoryManagement", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaCategoryManagement CategoryManagement { get; set; }

        /// <summary>
        /// EPG Multilingual Fallback Support
        /// </summary>
        [DataMember(Name = "epgMultilingualFallbackSupport")]
        [JsonProperty("epgMultilingualFallbackSupport")]
        [XmlElement(ElementName = "epgMultilingualFallbackSupport")]
        [SchemeProperty(IsNullable = true)]
        public bool? EpgMultilingualFallbackSupport { get; set; }

        /// <summary>
        /// Upload Export Datalake
        /// </summary>
        [DataMember(Name = "uploadExportDatalake")]
        [JsonProperty("uploadExportDatalake")]
        [XmlElement(ElementName = "uploadExportDatalake")]
        [SchemeProperty(IsNullable = true)]
        public bool? UploadExportDatalake { get; set; }
        /// <summary>
        /// Shop Marker's identifier
        /// </summary>
        [DataMember(Name = "shopMarkerMetaId")]
        [JsonProperty("shopMarkerMetaId")]
        [XmlElement(ElementName = "shopMarkerMetaId")]
        [SchemeProperty(IsNullable = true)]
        public long? ShopMarkerMetaId { get; set; }
    }
}