using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.LiveToVod
{
    public partial class KalturaLiveToVodFullConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Enable/disable the feature globally. If disabled, then all linear assets are not enabled.
        /// </summary>
        [DataMember(Name = "isL2vEnabled")]
        [JsonProperty("isL2vEnabled")]
        [XmlElement(ElementName = "isL2vEnabled")]
        public bool IsLiveToVodEnabled { get; set; }

        /// <summary>
        /// Number of days the L2V asset is retained in the system.
        /// </summary>
        [DataMember(Name = "retentionPeriodDays")]
        [JsonProperty("retentionPeriodDays")]
        [XmlElement(ElementName = "retentionPeriodDays")]
        public int RetentionPeriodDays { get; set; }

        /// <summary>
        /// The name (label) of the metadata field marking the program asset to be duplicated as a L2V asset.
        /// </summary>
        [DataMember(Name = "metadataClassifier")]
        [JsonProperty("metadataClassifier")]
        [XmlElement(ElementName = "metadataClassifier")]
        public string MetadataClassifier { get; set; }

        /// <summary>
        /// Configuring isL2vEnabled/retentionPeriodDays per each channel, overriding the defaults set in the global isL2vEnabled and retentionPeriodDays parameters.
        /// </summary>
        [DataMember(Name = "linearAssets")]
        [JsonProperty("linearAssets")]
        [XmlElement(ElementName = "linearAssets")]
        public List<KalturaLiveToVodLinearAssetConfiguration> LinearAssets { get; set; }
    }
}