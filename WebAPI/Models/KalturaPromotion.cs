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
        /// Id  
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

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
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// EndTime  
        /// </summary>
        [DataMember(Name = "endTime")]
        [JsonProperty("endTime")]
        [XmlElement(ElementName = "endTime")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Saved  
        /// </summary>
        [DataMember(Name = "saved")]
        [JsonProperty("saved")]
        [XmlElement(ElementName = "saved")]
        public bool Saved { get; set; }

        /// <summary>
        /// Location X  
        /// </summary>
        [DataMember(Name = "locationX")]
        [JsonProperty("locationX")]
        [XmlElement(ElementName = "locationX")]
        public int LocationX { get; set; }

        /// <summary>
        /// Location Y  
        /// </summary>
        [DataMember(Name = "locationY")]
        [JsonProperty("locationY")]
        [XmlElement(ElementName = "locationY")]
        public int LocationY { get; set; }

        /// <summary>
        /// Provider Thumbnail
        /// </summary>
        [DataMember(Name = "providerThumbnail")]
        [JsonProperty("providerThumbnail")]
        [XmlElement(ElementName = "providerThumbnail")]
        public string ProviderThumbnail { get; set; }

        /// <summary>
        /// Image
        /// </summary>
        [DataMember(Name = "image")]
        [JsonProperty("image")]
        [XmlElement(ElementName = "image")]
        public string Image { get; set; }

        /// <summary>
        /// Animation Type
        /// </summary>
        [DataMember(Name = "animationType")]
        [JsonProperty("animationType")]
        [XmlElement(ElementName = "animationType")]
        public KalturaAnimationType AnimationType { get; set; }
    }

    public enum KalturaAnimationType
    {
        resize,
        blink,
        right_to_left,
        left_to_right
    }
}