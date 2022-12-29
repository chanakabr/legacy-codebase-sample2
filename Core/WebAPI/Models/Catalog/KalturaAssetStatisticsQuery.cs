using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaAssetStatisticsQuery : KalturaOTTObject
    {
        /// <summary>
        /// Comma separated list of asset identifiers.
        /// </summary>
        [DataMember(Name = "assetIdIn")]
        [JsonProperty(PropertyName = "assetIdIn")]
        [XmlElement(ElementName = "assetIdIn", IsNullable = true)]
        public string AssetIdIn { get; set; }

        /// <summary>
        /// Asset type
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty(PropertyName = "assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual")]
        public KalturaAssetType AssetTypeEqual { get; set; }

        /// <summary>
        /// The beginning of the time window to get the statistics for (in epoch). 
        /// </summary>
        [DataMember(Name = "startDateGreaterThanOrEqual")]
        [JsonProperty(PropertyName = "startDateGreaterThanOrEqual")]
        [XmlElement(ElementName = "startDateGreaterThanOrEqual")]
        public long StartDateGreaterThanOrEqual { get; set; }

        /// <summary>
        /// /// The end of the time window to get the statistics for (in epoch).
        /// </summary>
        [DataMember(Name = "endDateGreaterThanOrEqual")]
        [JsonProperty(PropertyName = "endDateGreaterThanOrEqual")]
        [XmlElement(ElementName = "endDateGreaterThanOrEqual")]
        public long EndDateGreaterThanOrEqual { get; set; }
    }
}