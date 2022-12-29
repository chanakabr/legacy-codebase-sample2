using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;


namespace WebAPI.Models.Notification
{
    public partial class KalturaSeasonsReminderFilter : KalturaReminderFilter<KalturaSeriesReminderOrderBy>
    {
        /// <summary>
        /// Series ID
        /// </summary>
        [DataMember(Name = "seriesIdEqual")]
        [JsonProperty("seriesIdEqual")]
        [XmlElement(ElementName = "seriesIdEqual", IsNullable = true)]
        public string SeriesIdEqual { get; set; }

        /// <summary>
        /// Comma separated season numbers
        /// </summary>
        [DataMember(Name = "seasonNumberIn")]
        [JsonProperty("seasonNumberIn")]
        [XmlElement(ElementName = "seasonNumberIn", IsNullable = true)]
        public string SeasonNumberIn { get; set; }

        /// <summary>
        /// EPG channel ID
        /// </summary>
        [DataMember(Name = "epgChannelIdEqual")]
        [JsonProperty("epgChannelIdEqual")]
        [XmlElement(ElementName = "epgChannelIdEqual", IsNullable = true)]
        public long? EpgChannelIdEqual { get; set; }

        public override KalturaSeriesReminderOrderBy GetDefaultOrderByValue()
        {
            return KalturaSeriesReminderOrderBy.NONE;
        }
    }
}