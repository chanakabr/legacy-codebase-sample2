using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

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
        public long SeriesId { get; set; }
    }    
}