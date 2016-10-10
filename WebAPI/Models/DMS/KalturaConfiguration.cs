using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Renderers;

namespace WebAPI.Models.DMS
{
    public class KalturaConfiguration : KalturaOTTObject
    {
        [DataMember(Name = "appName")]
        [XmlElement(ElementName = "appName")]
        [JsonProperty("appName", Order = 1)]
        public string AppName { get; set; }

        [DataMember(Name = "clientVersion")]
        [XmlElement(ElementName = "clientVersion")]
        [JsonProperty("clientVersion", Order = 2)]
        public string ClientVersion { get; set; }

        [DataMember(Name = "isForceUpdate")]
        [XmlElement(ElementName = "isForceUpdate")]
        [JsonProperty("isForceUpdate", Order = 3)]
        public bool IsForceUpdate { get; set; }

        [DataMember(Name = "platform")]
        [XmlElement(ElementName = "platform")]
        [JsonProperty("platform", Order = 4)]
        public KalturaPlatform Platform { get; set; }

        [DataMember(Name = "partnerId")]
        [XmlElement(ElementName = "partnerId")]
        [JsonProperty("partnerId", Order = 5)]
        [SchemeProperty(ReadOnly = true)]
        public int PartnerId { get; set; }  // TODO: check for ignore

        [DataMember(Name = "externalPushId")]
        [XmlElement(ElementName = "externalPushId")]
        [JsonProperty("externalPushId", Order = 6)]
        public string ExternalPushId { get; set; }

        [DataMember(Name = "content")]
        [XmlElement(ElementName = "content")]
        [JsonProperty("content")]
        public string Content{ get; set; }

        [DataMember(Name = "groupConfigurationId")]
        [XmlElement(ElementName = "groupConfigurationId")]
        [JsonProperty("groupConfigurationId")]
        public string GroupConfigurationId { get; set; }      

        [DataMember(Name = "id")]
        [XmlElement(ElementName = "id")]
        [JsonProperty("id")]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; set; }

        [DataMember(Name = "isDefault")]
        [XmlElement(ElementName = "isDefault")]
        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }
    }

    public enum KalturaeStatus
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