using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;


namespace WebAPI.Models.Notification
{
    public partial class KalturaSeriesReminderFilter : KalturaReminderFilter<KalturaSeriesReminderOrderBy>
    {
        /// <summary>
        /// Comma separated series IDs
        /// </summary>
        [DataMember(Name = "seriesIdIn")]
        [JsonProperty("seriesIdIn")]
        [XmlElement(ElementName = "seriesIdIn", IsNullable = true)]
        public string SeriesIdIn { get; set; }

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