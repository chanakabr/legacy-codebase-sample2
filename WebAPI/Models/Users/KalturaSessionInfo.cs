using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Kaltura Session
    /// </summary>
    public class KalturaSession : KalturaOTTObject
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
        public int? partnerId { get; set; }

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
        public int? expiry { get; set; }

        /// <summary>
        /// Privileges
        /// </summary>
        [DataMember(Name = "privileges")]
        [JsonProperty(PropertyName = "privileges")]
        [XmlElement("privileges")]
        public string privileges { get; set; }

        /// <summary>
        /// UDID
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty(PropertyName = "udid")]
        [XmlElement("udid")]
        public string udid { get; set; }

        /// <summary>
        /// Create date
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement("createDate")]
        public int createDate { get; set; }

        public KalturaSession()
        {
        }

        public KalturaSession(KS ks)
        {
            var payload = KSUtils.ExtractKSPayload(ks);
            this.ks = ks.ToString();
            this.expiry = (int)SerializationUtils.ConvertToUnixTimestamp(ks.Expiration);
            this.partnerId = ks.GroupId;
            this.privileges = ks.Privileges != null && ks.Privileges.Count > 0 ? string.Join(",", ks.Privileges.Select(p => string.Join(":", p.key, p.value))) : string.Empty;
            this.sessionType = ks.SessionType;
            this.userId = ks.UserId;
            this.udid = payload.UDID;
            this.createDate = payload.CreateDate;
        }
    }

    /// <summary>
    /// Kaltura Session
    /// </summary>
    public class KalturaSessionInfo : KalturaSession
    {
        public KalturaSessionInfo()
        {
        }

        public KalturaSessionInfo(KS ks) : base(ks)
        {
        }
    }
}