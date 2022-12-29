using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner concurrency configuration
    /// </summary>
    public partial class KalturaConcurrencyPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Comma separated list of device Family Ids order by their priority.
        /// </summary>
        [DataMember(Name = "deviceFamilyIds")]
        [JsonProperty("deviceFamilyIds")]
        [XmlElement(ElementName = "deviceFamilyIds")]
        public string DeviceFamilyIds { get; set; }

        /// <summary>
        /// Policy of eviction devices
        /// </summary>
        [DataMember(Name = "evictionPolicy")]
        [JsonProperty("evictionPolicy")]
        [XmlElement(ElementName = "evictionPolicy")]
        public KalturaEvictionPolicyType? EvictionPolicy { get; set; }

        /// <summary>
        /// Concurrency threshold in seconds
        /// </summary>
        [DataMember(Name = "concurrencyThresholdInSeconds")]
        [JsonProperty("concurrencyThresholdInSeconds")]
        [XmlElement(ElementName = "concurrencyThresholdInSeconds")]
        [SchemeProperty(MinLong = 30, MaxLong = 1200)]
        public long? ConcurrencyThresholdInSeconds { get; set; }

        /// <summary>
        /// Revoke on device delete
        /// </summary>
        [DataMember(Name = "revokeOnDeviceDelete")]
        [JsonProperty("revokeOnDeviceDelete")]
        [XmlElement(ElementName = "revokeOnDeviceDelete")]
        public bool? RevokeOnDeviceDelete { get; set; }

        /// <summary>
        /// If set to true then for all concurrency checks in all APIs, system shall exclude free content from counting towards the use of a concurrency slot
        /// </summary>
        [DataMember(Name = "excludeFreeContentFromConcurrency")]
        [JsonProperty("excludeFreeContentFromConcurrency")]
        [XmlElement(ElementName = "excludeFreeContentFromConcurrency")]
        public bool? ExcludeFreeContentFromConcurrency { get; set; }
    }
}