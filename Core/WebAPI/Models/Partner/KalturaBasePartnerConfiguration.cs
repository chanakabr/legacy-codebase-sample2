using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Partner
{
    public partial class KalturaBasePartnerConfiguration : KalturaPartnerConfiguration
    {
        /// <summary>
        /// KSExpirationSeconds
        /// </summary>
        [DataMember(Name = "ksExpirationSeconds")]
        [JsonProperty("ksExpirationSeconds")]
        [XmlElement(ElementName = "ksExpirationSeconds")]
        [SchemeProperty(MinLong = 1)]
        public long KsExpirationSeconds { get; set; }

        /// <summary>
        /// AppTokenSessionMaxDurationSeconds
        /// </summary>
        [DataMember(Name = "appTokenSessionMaxDurationSeconds")]
        [JsonProperty("appTokenSessionMaxDurationSeconds")]
        [XmlElement(ElementName = "appTokenSessionMaxDurationSeconds")]
        [SchemeProperty(MinInteger = 1)]
        public int AppTokenSessionMaxDurationSeconds { get; set; }

        /// <summary>
        /// AnonymousKSExpirationSeconds
        /// </summary>
        [DataMember(Name = "anonymousKSExpirationSeconds")]
        [JsonProperty("anonymousKSExpirationSeconds")]
        [XmlElement(ElementName = "anonymousKSExpirationSeconds")]
        [SchemeProperty(MinLong = 1)]
        public long AnonymousKSExpirationSeconds { get; set; }

        /// <summary>
        /// RefreshExpirationForPinLoginSeconds
        /// </summary>
        [DataMember(Name = "refreshExpirationForPinLoginSeconds")]
        [JsonProperty("refreshExpirationForPinLoginSeconds")]
        [XmlElement(ElementName = "refreshExpirationForPinLoginSeconds")]
        [SchemeProperty(MinLong = 1)]
        public long RefreshExpirationForPinLoginSeconds { get; set; }

        /// <summary>
        /// AppTokenMaxExpirySeconds
        /// </summary>
        [DataMember(Name = "appTokenMaxExpirySeconds")]
        [JsonProperty("appTokenMaxExpirySeconds")]
        [XmlElement(ElementName = "appTokenMaxExpirySeconds")]
        [SchemeProperty(MinInteger = 1)]
        public int AppTokenMaxExpirySeconds { get; set; }

        /// <summary>
        /// AutoRefreshAppToken
        /// </summary>
        [DataMember(Name = "autoRefreshAppToken")]
        [JsonProperty("autoRefreshAppToken")]
        [XmlElement(ElementName = "autoRefreshAppToken")]
        public bool AutoRefreshAppToken { get; set; }

        /// <summary>
        /// uploadTokenExpirySeconds
        /// </summary>
        [DataMember(Name = "uploadTokenExpirySeconds")]
        [JsonProperty("uploadTokenExpirySeconds")]
        [XmlElement(ElementName = "uploadTokenExpirySeconds")]
        [SchemeProperty(MinInteger = 1)]
        public int UploadTokenExpirySeconds { get; set; }

        /// <summary>
        /// apptokenUserValidationDisabled
        /// </summary>
        [DataMember(Name = "apptokenUserValidationDisabled")]
        [JsonProperty("apptokenUserValidationDisabled")]
        [XmlElement(ElementName = "apptokenUserValidationDisabled")]
        public bool ApptokenUserValidationDisabled { get; set; }
        
        /// <summary>
        /// epgFeatureVersion
        /// defines the epg feature version from version 1 to version 3
        /// if not provided v2 will be used
        /// </summary>
        [DataMember(Name = "epgFeatureVersion")]
        [JsonProperty("epgFeatureVersion")]
        [XmlElement(ElementName = "epgFeatureVersion")]
        [SchemeProperty(MinInteger = 1, MaxInteger = 3, Default = 2, IsNullable = true)]
        public int? EpgFeatureVersion { get; set; }

        protected override void Init()
        {
            base.Init();
            EpgFeatureVersion = 2;
        }
    }
}