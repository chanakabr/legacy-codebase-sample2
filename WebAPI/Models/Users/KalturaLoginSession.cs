using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Login response
    /// </summary>
    /// 
    public partial class KalturaLoginSession : KalturaOTTObject
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
        [Obsolete]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Expiration
        /// </summary>
        [DataMember(Name = "expiry")]
        [JsonProperty("expiry")]
        [XmlElement(ElementName = "expiry")]
        public long Expiry { get; set; }
    }
}