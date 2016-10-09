using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        [JsonProperty("appname", Order = 1)]
        public string AppName { get; set; }

        [JsonProperty("clientversion", Order = 2)]
        public string ClientVersion { get; set; }

        [JsonProperty("isforceupdate", Order = 3)]
        public bool IsForceUpdate { get; set; }

        [JsonProperty("platform", Order = 4)]
        public DMSePlatform Platform { get; set; }

        [JsonProperty("partnerid", Order = 5)]
        public int GroupId { get; set; }

        [JsonProperty("external_push_id", Order = 6)]
        public string ExternalPushId { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Params { get; set; }

        [JsonProperty("group_configuration_id")]
        public string GroupConfigurationId { get; set; }

        [JsonProperty("type")]
        private string docType { get; set; }

        public DMSAppVersion()
        {
            this.docType = "configuration";
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("is_default")]
        public bool IsDefault { get; set; }
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