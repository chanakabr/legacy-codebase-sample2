using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Notifications
{
    [Serializable]
    public class KalturaSeasonReminder : KalturaReminder
    {
        /// <summary>
        /// Series identifier
        /// </summary>
        [DataMember(Name = "seriesId")]
        [JsonProperty(PropertyName = "seriesId")]
        [XmlElement(ElementName = "seriesId")]
        public long SeriesId { get; set; }

        /// <summary>
        /// Season identifier
        /// </summary>
        [DataMember(Name = "seasonId")]
        [JsonProperty(PropertyName = "seasonId")]
        [XmlElement(ElementName = "seasonId")]
        public long SeasonId { get; set; }
    }    
}