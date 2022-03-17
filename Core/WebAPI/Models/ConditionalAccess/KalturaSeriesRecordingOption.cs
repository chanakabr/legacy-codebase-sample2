using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaSeriesRecordingOption: KalturaOTTObject
    {
        /// <summary>
        /// min Season Number
        /// </summary>
        [DataMember(Name = "minSeasonNumber")]
        [JsonProperty("minSeasonNumber")]
        [XmlElement(ElementName = "minSeasonNumber", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, IsNullable = true)]
        public int? MinSeasonNumber { get; set; }

        /// <summary>
        /// min Season Number
        /// </summary>
        [DataMember(Name = "minEpisodeNumber")]
        [JsonProperty("minEpisodeNumber")]
        [XmlElement(ElementName = "minEpisodeNumber", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, IsNullable = true)]
        public int? MinEpisodeNumber { get; set; }

        /// <summary>
        /// Record future only from selected value
        /// </summary>
        [DataMember(Name = "chronologicalRecordStartTime")]
        [JsonProperty("chronologicalRecordStartTime")]
        [XmlElement(ElementName = "chronologicalRecordStartTime", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaChronologicalRecordStartTime? ChronologicalRecordStartTime { get; set; }
    }
}