using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [SchemeClass(Required = new[] { "seriesId" })]
    public partial class KalturaSeriesIdArguments : KalturaOTTObject
    {
        /// <summary>
        /// Comma separated asset type IDs
        /// </summary>
        [DataMember(Name = "assetTypeIdIn")]
        [JsonProperty("assetTypeIdIn")]
        [XmlElement(ElementName = "assetTypeIdIn", IsNullable = true)]
        [SchemeProperty(DynamicMinInt = 0, MinLength = 1, IsNullable = true)]
        public string AssetTypeIdIn { get; set; }

        /// <summary>
        /// Series ID
        /// </summary>
        [DataMember(Name = "seriesId")]
        [JsonProperty(PropertyName = "seriesId")]
        [XmlElement(ElementName = "seriesId")]
        [SchemeProperty(MinLength = 1)]
        public string SeriesId { get; set; }

        /// <summary>
        /// Series ID meta name.
        /// </summary>
        [DataMember(Name = "seriesIdMetaName")]
        [JsonProperty("seriesIdMetaName")]
        [XmlElement(ElementName = "seriesIdMetaName", IsNullable = true)]
        [SchemeProperty(MinLength = 1, IsNullable = true)]
        public string SeriesIdMetaName { get; set; }

        /// <summary>
        /// Season number meta name
        /// </summary>
        [DataMember(Name = "seasonNumberMetaName")]
        [JsonProperty("seasonNumberMetaName")]
        [XmlElement(ElementName = "seasonNumberMetaName", IsNullable = true)]
        [SchemeProperty(MinLength = 1, IsNullable = true)]
        public string SeasonNumberMetaName { get; set; }

        /// <summary>
        /// Episode number meta name
        /// </summary>
        [DataMember(Name = "episodeNumberMetaName")]
        [JsonProperty("episodeNumberMetaName")]
        [XmlElement(ElementName = "episodeNumberMetaName", IsNullable = true)]
        [SchemeProperty(MinLength = 1, IsNullable = true)]
        public string EpisodeNumberMetaName { get; set; }
    }
}