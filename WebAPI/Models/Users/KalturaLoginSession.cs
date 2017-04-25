using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Login response
    /// </summary>
    /// 
    public class KalturaLoginSession : KalturaOTTObject
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
        [DataMember(Name = "refreshToken")]
        [JsonProperty("refreshToken")]
        [XmlElement(ElementName = "refreshToken")]
        [OldStandardProperty("refresh_token")]
        [Deprecated("3.6.287.27312")]
        public string RefreshToken { get; set; }

        
    }
}