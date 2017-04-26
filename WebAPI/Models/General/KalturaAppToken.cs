using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Application token
    /// </summary>
    public class KalturaAppToken : KalturaOTTObject
    {
        /// <summary>
        /// The id of the application token
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; set; }

        /// <summary>
        /// Expiry time of current token (unix timestamp in seconds)
        /// </summary>
        [DataMember(Name = "expiry")]
        [JsonProperty("expiry")]
        [XmlElement(ElementName = "expiry")]
        public int? Expiry { get; set; }

        /// <summary>
        /// Partner identifier
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        [SchemeProperty(ReadOnly = true)]
        public int? PartnerId { get; set; }

        /// <summary>
        /// Expiry duration of KS (Kaltura Session) that created using the current token (in seconds)
        /// </summary>
        [DataMember(Name = "sessionDuration")]
        [JsonProperty("sessionDuration")]
        [XmlElement(ElementName = "sessionDuration")]
        public int? SessionDuration { get; set; }

        /// <summary>
        /// The hash type of the token
        /// </summary>
        [DataMember(Name = "hashType")]
        [JsonProperty("hashType")]
        [XmlElement(ElementName = "hashType")]
        public KalturaAppTokenHashType? HashType { get; set; }

        /// <summary>
        /// Comma separated privileges to be applied on KS (Kaltura Session) that created using the current token
        /// </summary>
        [DataMember(Name = "sessionPrivileges")]
        [JsonProperty("sessionPrivileges")]
        [XmlElement(ElementName = "sessionPrivileges")]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE)]
        public string SessionPrivileges { get; set; }

        /// <summary>
        /// Type of KS (Kaltura Session) that created using the current token
        /// </summary>
        [DataMember(Name = "sessionType")]
        [JsonProperty("sessionType")]
        [XmlElement(ElementName = "sessionType")]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE)]
        public KalturaSessionType? SessionType { get; set; }

        /// <summary>
        /// Application token status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaAppTokenStatus Status { get; set; }

        /// <summary>
        /// The application token
        /// </summary>
        [DataMember(Name = "token")]
        [JsonProperty("token")]
        [XmlElement(ElementName = "token")]
        [SchemeProperty(ReadOnly = true)]
        public string Token { get; set; }

        /// <summary>
        /// User id of KS (Kaltura Session) that created using the current token
        /// </summary>
        [DataMember(Name = "sessionUserId")]
        [JsonProperty("sessionUserId")]
        [XmlElement(ElementName = "sessionUserId")]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE)]
        public string SessionUserId { get; set; }

        public KalturaAppToken()
        {
        }

        public KalturaAppToken(AppToken appToken)
        {
            Id = appToken.AppTokenId;
            Expiry = appToken.Expiry;
            PartnerId = appToken.PartnerId;
            SessionDuration = appToken.SessionDuration;
            HashType = appToken.HashType;
            SessionPrivileges = appToken.SessionPrivileges;
            SessionType = appToken.SessionType;
            Status = appToken.Status;
            Token = appToken.Token;
            SessionUserId = appToken.SessionUserId;
        }

        internal int getSessionDuration()
        {
            return SessionDuration.HasValue ? (int)SessionDuration : 0;
        }

        internal int getPartnerId()
        {
            return PartnerId.HasValue ? (int)PartnerId : 0;
        }

        internal int getExpiry()
        {
            return Expiry.HasValue ? (int)Expiry : 0;
        }
    }
}