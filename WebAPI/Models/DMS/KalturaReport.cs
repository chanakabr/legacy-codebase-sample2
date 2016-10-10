using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    [JsonObject]
    public class KalturaReport : KalturaOTTObject
    {
        [DataMember(Name = "groupId")]
        [XmlElement(ElementName = "groupId")]
        [JsonProperty("groupId")]
        public int GroupId { get; set; }

        [DataMember(Name = "udid")]
        [XmlElement(ElementName = "udid")]
        [JsonProperty("udid")]
        public string Udid { get; set; }

        [DataMember(Name = "push")]
        [XmlElement(ElementName = "push")]
        [JsonProperty("push")]
        public KalturaPushParams PushParameters { get; set; }

        [DataMember(Name = "versionNumber")]
        [XmlElement(ElementName = "versionNumber")]
        [JsonProperty("versionNumber")]
        public string VersionNumber { get; set; }

        [DataMember(Name = "versionPlatform")]
        [XmlElement(ElementName = "versionPlatform")]
        [JsonProperty("versionPlatform")]
        [JsonConverter(typeof(StringEnumConverter))]
        public KalturaPlatform VersionPlatform { get; set; }

        [DataMember(Name = "versionAppName")]
        [XmlElement(ElementName = "versionAppName")]
        [JsonProperty("versionAppName")]
        public string VersionAppName { get; set; }

        [DataMember(Name = "lastAccessIP")]
        [XmlElement(ElementName = "lastAccessIP")]
        [JsonProperty("lastAccessIP")]
        public string LastAccessIP { get; set; }

        [DataMember(Name = "lastAccessDate")]
        [XmlElement(ElementName = "lastAccessDate")]
        [JsonProperty("lastAccessDate")]
        public long LastAccessDate { get; set; }

        [DataMember(Name = "userAgent")]
        [XmlElement(ElementName = "userAgent")]
        [JsonProperty("userAgent")]
        public string UserAgent { get; set; }

        [DataMember(Name = "operationSystem")]
        [XmlElement(ElementName = "operationSystem")]
        [JsonProperty("operationSystem")]
        public string OperationSystem { get; set; }

        [DataMember(Name = "groupConfigurationId")]
        [XmlElement(ElementName = "groupConfigurationId")]
        [JsonProperty("groupConfigurationId")]
        public string GroupConfigurationId { get; set; }


    }
}
