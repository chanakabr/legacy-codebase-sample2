using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    [JsonObject]
    public class KalturaConfigurationGroupDeviceReport : KalturaReport
    {
        /// <summary>
        /// Partner id
        /// </summary>
        [DataMember(Name = "partnerId")]
        [XmlElement(ElementName = "partnerId")]
        [JsonProperty("partnerId")]
        public int PartnerId { get; set; }

        /// <summary>
        /// Configuration group id
        /// </summary>
        [DataMember(Name = "configurationGroupId")]
        [XmlElement(ElementName = "configurationGroupId")]
        [JsonProperty("configurationGroupId")]
        public string ConfigurationGroupId { get; set; }

        /// <summary>
        /// Device UDID
        /// </summary>
        [DataMember(Name = "udid")]
        [XmlElement(ElementName = "udid")]
        [JsonProperty("udid")]
        public string Udid { get; set; }

        /// <summary>
        /// Push parameters
        /// </summary>
        [DataMember(Name = "pushParameters")]
        [XmlElement(ElementName = "pushParameters")]
        [JsonProperty("pushParameters")]
        public KalturaPushParams PushParameters { get; set; }

        /// <summary>
        /// Version number
        /// </summary>
        [DataMember(Name = "versionNumber")]
        [XmlElement(ElementName = "versionNumber")]
        [JsonProperty("versionNumber")]
        public string VersionNumber { get; set; }

        /// <summary>
        /// Version platform
        /// </summary>
        [DataMember(Name = "versionPlatform")]
        [XmlElement(ElementName = "versionPlatform")]
        [JsonProperty("versionPlatform")]
        [JsonConverter(typeof(StringEnumConverter))]
        public KalturaPlatform VersionPlatform { get; set; }

        /// <summary>
        /// Version application name
        /// </summary>
        [DataMember(Name = "versionAppName")]
        [XmlElement(ElementName = "versionAppName")]
        [JsonProperty("versionAppName")]
        public string VersionAppName { get; set; }

        /// <summary>
        /// Last access IP
        /// </summary>
        [DataMember(Name = "lastAccessIP")]
        [XmlElement(ElementName = "lastAccessIP")]
        [JsonProperty("lastAccessIP")]
        public string LastAccessIP { get; set; }

        /// <summary>
        /// Last access date
        /// </summary>
        [DataMember(Name = "lastAccessDate")]
        [XmlElement(ElementName = "lastAccessDate")]
        [JsonProperty("lastAccessDate")]
        public long LastAccessDate { get; set; }

        /// <summary>
        /// User agent
        /// </summary>
        [DataMember(Name = "userAgent")]
        [XmlElement(ElementName = "userAgent")]
        [JsonProperty("userAgent")]
        public string UserAgent { get; set; }

        /// <summary>
        /// Operation system
        /// </summary>
        [DataMember(Name = "operationSystem")]
        [XmlElement(ElementName = "operationSystem")]
        [JsonProperty("operationSystem")]
        public string OperationSystem { get; set; }
    }
}
