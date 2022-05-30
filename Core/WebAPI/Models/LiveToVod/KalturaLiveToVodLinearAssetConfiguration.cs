using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.LiveToVod
{
    /// <summary>
    /// Configuration of isL2vEnabled and retentionPeriodDays per each channel, overriding the defaults set in the account's configuration.
    /// </summary>
    public partial class KalturaLiveToVodLinearAssetConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Linear asset's identifier.
        /// </summary>
        [DataMember(Name = "linearAssetId")]
        [JsonProperty("linearAssetId")]
        [XmlElement(ElementName = "linearAssetId")]
        public long? LinearAssetId { get; set; }

        /// <summary>
        /// Enable/disable the feature per linear channel. Considered only if the flag is enabled on the account level.
        /// </summary>
        [DataMember(Name = "isL2vEnabled")]
        [JsonProperty("isL2vEnabled")]
        [XmlElement(ElementName = "isL2vEnabled")]
        public bool? IsLiveToVodEnabled { get; set; }

        /// <summary>
        /// Number of days the L2V asset is retained in the system.
        /// Optional - if configured, overriding the account level value.
        /// </summary>
        [DataMember(Name = "retentionPeriodDays")]
        [JsonProperty("retentionPeriodDays")]
        [XmlElement(ElementName = "retentionPeriodDays")]
        public int? RetentionPeriodDays { get; set; }
    }
}