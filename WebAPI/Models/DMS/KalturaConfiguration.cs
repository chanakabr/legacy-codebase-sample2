using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaConfiguration : KalturaOTTObject
    {
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public KalturaeStatus Status { get; set; }

        [DataMember(Name = "udid")]
        [JsonProperty("udid")]
        [XmlElement(ElementName = "udid")]
        public string UDID { get; set; }

        [DataMember(Name = "version")]
        [JsonProperty("version")]
        [XmlElement(ElementName = "version")]
        public KalturaAppVersion Version { get; set; }

        [DataMember(Name = "params")]
        [JsonProperty("params")]
        [XmlElement(ElementName = "params")]
        public SerializableDictionary<string, object> Params { get; set; }

        [DataMember(Name = "token")]
        [JsonProperty("token")]
        [XmlElement(ElementName = "token")]
        public KalturaDeviceToken Token;
    }    

    [DataContract]
    public class KalturaDeviceToken : KalturaOTTObject
    {
        [DataMember(Name = "key")]
        [XmlElement(ElementName = "key")]
        [JsonProperty("key", Order = 0)]
        public Guid Key { get; set; }

        [DataMember(Name = "valid")]
        [XmlElement(ElementName = "valid")]
        [JsonProperty("valid", Order = 1)]
        public long? ValidUntil { get; set; }
    }

    [DataContract]
    public class KalturaAppVersion : KalturaOTTObject
    {
        [DataMember(Name = "appname")]
        [XmlElement(ElementName = "appname")]
        [JsonProperty("appname", Order = 1)]
        public string AppName { get; set; }

        [DataMember(Name = "clientversion")]
        [XmlElement(ElementName = "clientversion")]
        [JsonProperty("clientversion", Order = 2)]
        public string ClientVersion { get; set; }

        [DataMember(Name = "isforceupdate")]
        [XmlElement(ElementName = "isforceupdate")]
        [JsonProperty("isforceupdate", Order = 3)]
        public bool IsForceUpdate { get; set; }

        [DataMember(Name = "platform")]
        [XmlElement(ElementName = "platform")]
        [JsonProperty("platform", Order = 4)]
        public DMSePlatform Platform { get; set; }

        [DataMember(Name = "partnerid")]
        [XmlElement(ElementName = "partnerid")]
        [JsonProperty("partnerid", Order = 5)]
        public int GroupId { get; set; }

        [DataMember(Name = "external_push_id")]
        [XmlElement(ElementName = "external_push_id")]
        [JsonProperty("external_push_id", Order = 6)]
        public string ExternalPushId { get; set; }

        [DataMember(Name = "params")]
        [XmlElement(ElementName = "params")]
        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public SerializableDictionary<string, object> Params { get; set; }

        [DataMember(Name = "group_configuration_id")]
        [XmlElement(ElementName = "group_configuration_id")]
        [JsonProperty("group_configuration_id")]
        public string GroupConfigurationId { get; set; }

        [DataMember(Name = "type")]
        [XmlElement(ElementName = "type")]
        [JsonProperty("type")]
        private string docType { get; set; }

        public KalturaAppVersion()
        {
            this.docType = "configuration";
        }

        [DataMember(Name = "id")]
        [XmlElement(ElementName = "id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        [DataMember(Name = "is_default")]
        [XmlElement(ElementName = "is_default")]
        [JsonProperty("is_default")]
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

    public enum KalturaePlatform
    {
        Android = 0,
        iOS = 1,
        WindowsPhone = 2,
        Blackberry = 3,
        STB = 4,
        CTV = 5,
        Other = 6
    }
}