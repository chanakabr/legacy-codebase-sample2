using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Kaltura bookmark data
    /// </summary>
    [Serializable]
    public partial class KalturaPlayerAssetData : KalturaOTTObject
    {
        /// <summary>
        /// Action: HIT/PLAY/STOP/PAUSE/FIRST_PLAY/SWOOSH/FULL_SCREEN/SEND_TO_FRIEND/LOAD/FULL_SCREEN_EXIT/FINISH/BITRATE_CHANGE/ERROR/NONE
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlArrayItem(ElementName = "action")]
        public string action { get; set; }

        /// <summary>
        /// Location
        /// </summary>
        [DataMember(Name = "location")]
        [JsonProperty(PropertyName = "location")]
        [XmlArrayItem(ElementName = "location")]
        public int? location { get; set; }

        /// <summary>
        /// Average Bitrate
        /// </summary>
        [DataMember(Name = "averageBitrate")]
        [JsonProperty(PropertyName = "averageBitrate")]
        [XmlArrayItem(ElementName = "averageBitrate")]
        [OldStandardProperty("average_bitrate")]
        public int? averageBitRate { get; set; }

        /// <summary>
        /// Total Bitrate
        /// </summary>
        [DataMember(Name = "totalBitrate")]
        [JsonProperty(PropertyName = "totalBitrate")]
        [XmlArrayItem(ElementName = "totalBitrate")]
        [OldStandardProperty("total_bitrate")]
        public int? totalBitRate { get; set; }

        /// <summary>
        /// Current Bitrate
        /// </summary>
        [DataMember(Name = "currentBitrate")]
        [JsonProperty(PropertyName = "currentBitrate")]
        [XmlArrayItem(ElementName = "currentBitrate")]
        [OldStandardProperty("current_bitrate")]
        public int? currentBitRate { get; set; }
    }
}