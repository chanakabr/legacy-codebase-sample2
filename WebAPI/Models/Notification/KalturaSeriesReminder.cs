using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Notifications
{
    [Serializable]
    public class KalturaSeriesReminder : KalturaReminder
    {
        /// <summary>
        /// Series identifier
        /// </summary>
        [DataMember(Name = "seriesId")]
        [JsonProperty(PropertyName = "seriesId")]
        [XmlElement(ElementName = "seriesId")]
        [SchemeProperty(MinLength = 1)]
        public string SeriesId { get; set; }

        /// <summary>
        /// Season number
        /// </summary>
        [DataMember(Name = "seasonNumber")]
        [JsonProperty(PropertyName = "seasonNumber")]
        [XmlElement(ElementName = "seasonNumber")]
        public long SeasonNumber { get; set; }

        /// <summary>
        /// EPG channel identifier 
        /// </summary>
        [DataMember(Name = "epgChannelId")]
        [JsonProperty(PropertyName = "epgChannelId")]
        [XmlElement(ElementName = "epgChannelId")]
        [SchemeProperty(MinLong = 1)]
        public long EpgChannelId { get; set; }
    }    
}