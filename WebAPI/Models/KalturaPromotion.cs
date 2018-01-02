using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models
{
    public class KalturaPromotion : KalturaOTTObject
    {
        /// <summary>
        /// Link
        /// </summary>
        [DataMember(Name = "link")]
        [JsonProperty("link")]
        [XmlElement(ElementName = "link")]
        public string Link { get; set; }

        /// <summary>
        /// Text
        /// </summary>
        [DataMember(Name = "text")]
        [JsonProperty("text")]
        [XmlElement(ElementName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// StartTime 
        /// </summary>
        [DataMember(Name = "startTime")]
        [JsonProperty("startTime")]
        [XmlElement(ElementName = "startTime")]
        public long StartTime { get; set; }

        /// <summary>
        /// EndTime  
        /// </summary>
        [DataMember(Name = "endTime")]
        [JsonProperty("endTime")]
        [XmlElement(ElementName = "endTime")]
        public long EndTime { get; set; }
    }
}