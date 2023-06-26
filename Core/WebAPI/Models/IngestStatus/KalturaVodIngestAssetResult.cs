using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaVodIngestAssetResult : KalturaOTTObject
    {
        /// <summary>
        /// Ingested asset name. Absent only in case of NameRequired error
        /// </summary>
        [DataMember(Name = "assetName")]
        [JsonProperty("assetName")]
        [XmlElement(ElementName = "assetName")]
        public string AssetName { get; set; }

        /// <summary>
        /// The shop ID the asset is assigned to. Omitted if the asset is not associated to any shop.
        /// </summary>
        [DataMember(Name = "shopAssetUserRuleId")]
        [JsonProperty("shopAssetUserRuleId")]
        [XmlElement(ElementName = "shopAssetUserRuleId")]
        public long? ShopAssetUserRuleId { get; set; }

        /// <summary>
        /// The XML file name used at the ingest gateway. Referred to as process name
        /// </summary>
        [DataMember(Name = "fileName")]
        [JsonProperty("fileName")]
        [XmlElement(ElementName = "fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// Date and time the asset was ingested. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "ingestDate")]
        [JsonProperty("ingestDate")]
        [XmlElement(ElementName = "ingestDate")]
        public long IngestDate { get; set; }

        /// <summary>
        /// The status result for the asset ingest.
        /// FAILURE - the asset ingest was failed after the ingest process started, specify the error for it.
        /// SUCCESS - the asset was succeeded to be ingested.
        /// SUCCESS_WARNING - the asset was succeeded to be ingested with warnings that do not prevent the ingest.
        /// EXTERNAL_FAILURE - the asset ingest was failed before the ingest process started, specify the error for it.
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public KalturaVodIngestAssetResultStatus Status { get; set; }

        /// <summary>
        /// VOD asset type (assetStruct.systemName).
        /// </summary>
        [DataMember(Name = "vodTypeSystemName")]
        [JsonProperty("vodTypeSystemName")]
        [XmlElement(ElementName = "vodTypeSystemName")]
        public string VodTypeSystemName { get; set; }

        /// <summary>
        /// Errors which prevent the asset from being ingested
        /// </summary>
        [DataMember(Name = "errors")]
        [JsonProperty("errors")]
        [XmlElement(ElementName = "errors")]
        public List<KalturaVodIngestAssetResultErrorMessage> Errors { get; set; }

        /// <summary>
        /// Errors which do not prevent the asset from being ingested
        /// </summary>
        [DataMember(Name = "warnings")]
        [JsonProperty("warnings")]
        [XmlElement(ElementName = "warnings")]
        public List<KalturaVodIngestAssetResultErrorMessage> Warnings { get; set; }
    }
}