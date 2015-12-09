using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Kaltura Session
    /// </summary>
    public class KalturaSessionInfo : KalturaOTTObject
    {
        /// <summary>
        /// KS
        /// </summary>
        [DataMember(Name = "ks")]
        [JsonProperty(PropertyName = "ks")]
        [XmlElement("ks")]
        public string ks { get; set; }

        /// <summary>
        /// Session type
        /// </summary>
        [DataMember(Name = "sessionType")]
        [JsonProperty(PropertyName = "sessionType")]
        [XmlElement("sessionType")]
        public KalturaSessionType sessionType { get; set; }

        /// <summary>
        /// Partner identifier
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty(PropertyName = "partnerId")]
        [XmlElement("partnerId")]
        public int partnerId { get; set; }

        /// <summary>
        /// User identifier
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty(PropertyName = "userId")]
        [XmlElement("userId")]
        public string userId { get; set; }

        /// <summary>
        /// Expiry
        /// </summary>
        [DataMember(Name = "expiry")]
        [JsonProperty(PropertyName = "expiry")]
        [XmlElement("expiry")]
        public int expiry { get; set; }

        /// <summary>
        /// Privileges
        /// </summary>
        [DataMember(Name = "privileges")]
        [JsonProperty(PropertyName = "privileges")]
        [XmlElement("privileges")]
        public string privileges { get; set; }

        /// <summary>
        /// udid
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty(PropertyName = "udid")]
        [XmlElement("udid")]
        public string udid { get; set; }
    }
}