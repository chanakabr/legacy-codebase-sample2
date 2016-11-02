using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaConfigurations : KalturaOTTObject
    {
        /// <summary>
        /// Configuration id
        /// </summary>
        [DataMember(Name = "id")]
        [XmlElement(ElementName = "id")]
        [JsonProperty("id")]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; set; }

        /// <summary>
        /// Partner id
        /// </summary>
        [DataMember(Name = "partnerId")]
        [XmlElement(ElementName = "partnerId")]
        [JsonProperty("partnerId")]
        [SchemeProperty(ReadOnly = true)]
        public int PartnerId { get; set; }  

        /// <summary>
        /// Configuration group id
        /// </summary>
        [DataMember(Name = "configurationGroupId")]
        [XmlElement(ElementName = "configurationGroupId")]
        [JsonProperty("configurationGroupId")]
        public string ConfigurationGroupId { get; set; }

        /// <summary>
        /// Application name
        /// </summary>
        [DataMember(Name = "appName")]
        [XmlElement(ElementName = "appName")]
        [JsonProperty("appName")]
        public string AppName { get; set; }

        /// <summary>
        /// Client version
        /// </summary>
        [DataMember(Name = "clientVersion")]
        [XmlElement(ElementName = "clientVersion")]
        [JsonProperty("clientVersion")]
        public string ClientVersion { get; set; }
       
        /// <summary>
        /// Platform: Android/iOS/WindowsPhone/Blackberry/STB/CTV/Other
        /// </summary>
        [DataMember(Name = "platform")]
        [XmlElement(ElementName = "platform")]
        [JsonProperty("platform")]
        public KalturaPlatform Platform { get; set; }
     
        /// <summary>
        /// External push id
        /// </summary>
        [DataMember(Name = "externalPushId")]
        [XmlElement(ElementName = "externalPushId")]
        [JsonProperty("externalPushId")]
        public string ExternalPushId { get; set; }

        /// <summary>
        /// Is force update
        /// </summary>
        [DataMember(Name = "isForceUpdate")]
        [XmlElement(ElementName = "isForceUpdate")]
        [JsonProperty("isForceUpdate")]
        public bool IsForceUpdate { get; set; }

        /// <summary>
        /// Content
        /// </summary>
        [DataMember(Name = "content")]
        [XmlElement(ElementName = "content")]
        [JsonProperty("content")]
        public string Content { get; set; }
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