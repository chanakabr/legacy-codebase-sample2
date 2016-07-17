using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaSeriesRecording : KalturaOTTObject
    {
        /// <summary>
        /// Kaltura unique ID representing the series recording identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id", IsNullable = true)]
        public long Id { get; set; }

        /// <summary>
        /// Kaltura EpgId
        /// </summary>
        [DataMember(Name = "epgId")]
        [JsonProperty("epgId")]
        [XmlElement(ElementName = "epgId", IsNullable = true)]
        public long EpgId { get; set; }

        /// <summary>
        /// Kaltura ChannelId
        /// </summary>
        [DataMember(Name = "channelId")]
        [JsonProperty("channelId")]
        [XmlElement(ElementName = "channelId", IsNullable = true)]
        public long ChannelId { get; set; }

        /// <summary>
        /// Kaltura SeriesId
        /// </summary>
        [DataMember(Name = "seriesId")]
        [JsonProperty("seriesId")]
        [XmlElement(ElementName = "seriesId", IsNullable = true)]
        public long SeriesId { get; set; }

        /// <summary>
        /// Kaltura SeasonNumber
        /// </summary>
        [DataMember(Name = "seasonNumber")]
        [JsonProperty("seasonNumber")]
        [XmlElement(ElementName = "seasonNumber", IsNullable = true)]
        public long SeasonNumber { get; set; }

        /// <summary>
        /// Recording Type: single/series/season
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public KalturaRecordingType Type { get; set; }

        /// <summary>
        /// Specifies when was the series recording created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the series recording last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        public long UpdateDate { get; set; }
    }
}