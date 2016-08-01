using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Kaltura bookmark data
    /// </summary>
    [Serializable]
    [OldStandard("averageBitrate", "average_bitrate")]
    [OldStandard("totalBitrate", "total_bitrate")]
    [OldStandard("currentBitrate", "current_bitrate")]
    public class KalturaPlayerAssetData : KalturaOTTObject
    {
        /// <summary>
        /// Action
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
        public int? averageBitRate { get; set; }

        /// <summary>
        /// Total Bitrate
        /// </summary>
        [DataMember(Name = "totalBitrate")]
        [JsonProperty(PropertyName = "totalBitrate")]
        [XmlArrayItem(ElementName = "totalBitrate")]
        public int? totalBitRate { get; set; }

        /// <summary>
        /// Current Bitrate
        /// </summary>
        [DataMember(Name = "currentBitrate")]
        [JsonProperty(PropertyName = "currentBitrate")]
        [XmlArrayItem(ElementName = "currentBitrate")]
        public int? currentBitRate { get; set; }

        internal int getLocation()
        {
            return location.HasValue ? (int)location : 0;
        }

        internal int getAverageBitRate()
        {
            return averageBitRate.HasValue ? (int)averageBitRate : 0;
        }

        internal int getCurrentBitRate()
        {
            return currentBitRate.HasValue ? (int)currentBitRate : 0;
        }

        internal int getTotalBitRate()
        {
            return totalBitRate.HasValue ? (int)totalBitRate : 0;
        }
    }
}