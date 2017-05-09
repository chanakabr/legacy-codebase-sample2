using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "KalturaPushMessage", Namespace = "")]
    [XmlRoot("KalturaPushMessage")]
    public class KalturaPushMessage : KalturaOTTObject
    {
        /// <summary>
        /// Push text
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Push sound 
        /// </summary>
        [DataMember(Name = "sound")]
        [JsonProperty("sound")]
        [XmlElement(ElementName = "sound")]
        public string Sound { get; set; }

        /// <summary>
        /// Push action 
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty("action")]
        [XmlElement(ElementName = "action")]
        public string Action { get; set; }

        /// <summary>
        /// Push URL 
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty("url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }
    }
}