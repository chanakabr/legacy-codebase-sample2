using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaVodIngestAssetResultFilter: KalturaFilter<KalturaVodIngestAssetResultOrderBy>
    {
        public override KalturaVodIngestAssetResultOrderBy GetDefaultOrderByValue()
        {
            return KalturaVodIngestAssetResultOrderBy.INGEST_DATE_DESC;
        }

        /// <summary>
        /// Filter KalturaVodIngestAssetResult elements based on the ingest XML file name or partial name.
        /// </summary>
        [DataMember(Name = "fileNameContains")]
        [JsonProperty("fileNameContains")]
        [XmlElement(ElementName = "fileNameContains", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLength = 1, MaxLength = 50)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string FileNameContains { get; set; }

        /// <summary>
        /// Filter KalturaVodIngestAssetResult elements based on the asset name or partial name.
        /// </summary>
        [DataMember(Name = "assetNameContains")]
        [JsonProperty("assetNameContains")]
        [XmlElement(ElementName = "assetNameContains", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLength = 1, MaxLength = 50)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string AssetNameContains { get; set; }

        /// <summary>
        /// Comma separated values, representing multiple selection of ingest status state (\"SUCCESS\",\"FAIL\",\"SUCCESS_WARNING\"EXTERNAL_FAIL\").
        /// </summary>
        [DataMember(Name = "ingestStatusIn")]
        [JsonProperty("ingestStatusIn")]
        [XmlElement(ElementName = "ingestStatusIn", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLength = 1, MaxLength = 50)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string IngestStatusIn { get; set; }

        /// <summary>
        /// Filter assets ingested after the greater than value. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "ingestDateGreaterThan")]
        [JsonProperty("ingestDateGreaterThan")]
        [XmlElement(ElementName = "ingestDateGreaterThan", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLong = 1)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public long? IngestDateGreaterThan { get; set; }

        /// <summary>
        /// Filter assets ingested before the smaller than value. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "ingestDateSmallerThan")]
        [JsonProperty("ingestDateSmallerThan")]
        [XmlElement(ElementName = "ingestDateSmallerThan", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLong = 1)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public long? IngestDateSmallerThan { get; set; }

        /// <summary>
        /// Comma separated asset types, representing multiple selection of VOD asset types (e.g. \"MOVIE\",\"SERIES\",\"SEASON\",\"EPISODE\"...).
        /// </summary>
        [DataMember(Name = "vodTypeSystemNameIn")]
        [JsonProperty("vodTypeSystemNameIn")]
        [XmlElement(ElementName = "vodTypeSystemNameIn", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLength = 1, MaxLength = 50)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string VodTypeSystemNameIn { get; set; }

        /// <summary>
        /// Comma separated Ids, pointing to AssetUserRules which hold the shop markers (shop provider values)
        /// </summary>
        [DataMember(Name = "shopAssetUserRuleIdIn")]
        [JsonProperty("shopAssetUserRuleIdIn")]
        [XmlElement(ElementName = "shopAssetUserRuleIdIn", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLength = 1, MaxLength = 50)]
        public string ShopAssetUserRuleIdIn { get; set; }
    }
}