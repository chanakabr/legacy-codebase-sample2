using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Kaltura Session
    /// </summary>
    public partial class KalturaSession : KalturaOTTObject
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
        [Deprecated("4.5.0.0")]
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
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
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
    }
}