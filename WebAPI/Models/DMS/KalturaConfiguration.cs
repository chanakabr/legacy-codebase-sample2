using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Application name
        /// </summary>
        [DataMember(Name = "appName")]
        [XmlElement(ElementName = "appName")]
        [JsonProperty("appName", Order = 1)]
        public string AppName { get; set; }

        /// <summary>
        /// Client version
        /// </summary>
        [DataMember(Name = "clientVersion")]
        [XmlElement(ElementName = "clientVersion")]
        [JsonProperty("clientVersion", Order = 2)]
        public string ClientVersion { get; set; }

        /// <summary>
        /// Is force update
        /// </summary>
        [DataMember(Name = "isForceUpdate")]
        [XmlElement(ElementName = "isForceUpdate")]
        [JsonProperty("isForceUpdate", Order = 3)]
        public bool IsForceUpdate { get; set; }

        /// <summary>
        /// Platform: Android/iOS/WindowsPhone/Blackberry/STB/CTV/Other
        /// </summary>
        [DataMember(Name = "platform")]
        [XmlElement(ElementName = "platform")]
        [JsonProperty("platform", Order = 4)]
        public KalturaPlatform Platform { get; set; }

        /// <summary>
        /// Partner id
        /// </summary>
        [DataMember(Name = "partnerId")]
        [XmlElement(ElementName = "partnerId")]
        [JsonProperty("partnerId", Order = 5)]
        [SchemeProperty(ReadOnly = true)]
        public int PartnerId { get; set; }  // TODO: check for ignore

        /// <summary>
        /// External push id
        /// </summary>
        [DataMember(Name = "externalPushId")]
        [XmlElement(ElementName = "externalPushId")]
        [JsonProperty("externalPushId", Order = 6)]
        public string ExternalPushId { get; set; }

        /// <summary>
        /// Content
        /// </summary>
        [DataMember(Name = "content")]
        [XmlElement(ElementName = "content")]
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// Configuration group id
        /// </summary>
        [DataMember(Name = "configurationGroupId")]
        [XmlElement(ElementName = "configurationGroupId")]
        [JsonProperty("configurationGroupId")]
        public string ConfigurationGroupId { get; set; }

        /// <summary>
        /// Configuration id
        /// </summary>
        [DataMember(Name = "id")]
        [XmlElement(ElementName = "id")]
        [JsonProperty("id")]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; set; }

        /// <summary>
        /// Is default 
        /// </summary>
        [DataMember(Name = "isDefault")]
        [XmlElement(ElementName = "isDefault")]
        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }
    }

    public enum KalturaStatus
    {
        Unknown = -1,
        Registered = 0,
        Unregistered = 1,
        Forbidden = 2,
        Error = 3,
        IllegalParams = 4,
        IllegalPostData = 5,
        Success = 6,
        VersionNotFound = 7
    }
}