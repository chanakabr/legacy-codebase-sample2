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
    /// Login response
    /// </summary>
    public class KalturaLoginResponse : KalturaOTTObject
    {
        /// <summary>
        /// Access token in a KS format
        /// </summary>
        [DataMember(Name = "ks")]
        [JsonProperty("ks")]
        [XmlElement(ElementName = "ks")]
        public string KS { get; set; }

        /// <summary>
        /// Refresh Token
        /// </summary>
        [DataMember(Name = "refresh_token")]
        [JsonProperty("refresh_token")]
        [XmlElement(ElementName = "refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [DataMember(Name = "user")]
        [JsonProperty("user")]
        [XmlElement(ElementName = "user")]
        public KalturaOTTUser User { get; set; }
    }
}