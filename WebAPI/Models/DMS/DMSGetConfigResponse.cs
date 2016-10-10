using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebAPI.Models.DMS
{
    public class DMSGetConfigResponse
    {
        public DMSeStatus Status { get; set; }

        public string UDID { get; set; }

        public DMSAppVersion Version { get; set; }

        public Dictionary<string, object> Params { get; set; }

        public DMSDeviceToken Token;
    }    

    [DataContract]
    public class DMSDeviceToken
    {
        [JsonProperty("key", Order = 0)]
        public Guid Key { get; set; }

        [JsonProperty("valid", Order = 1)]
        public long? ValidUntil { get; set; }
    }

    [DataContract]
    public class DMSAppVersion
    {
         [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("is_default")]
        public bool IsDefault { get; set; }

        [JsonProperty("group_configuration_id")]
        public string GroupConfigurationId { get; set; }

        [JsonProperty("appname")]
        public string AppName { get; set; }

        [JsonProperty("clientversion")]
        public string ClientVersion { get; set; }

        [JsonProperty("isforceupdate")]
        public bool IsForceUpdate { get; set; }

        [JsonProperty("platform")]
        public DMSePlatform Platform { get; set; }

        [JsonProperty("partnerid")]
        public int GroupId { get; set; }

        [JsonProperty("external_push_id")]
        public string ExternalPushId { get; set; }      

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Params { get; set; }
    }

    public enum DMSeStatus
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

    [DataContract]
    public enum DMSePlatform
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