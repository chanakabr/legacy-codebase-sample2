using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Kaltura bookmark data
    /// </summary>
    [Serializable]
    public class KalturaPlayerAssetData : KalturaOTTObject
    {
        /// <summary>
        /// Action
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlArrayItem(ElementName = "action")]
        public string action;

        /// <summary>
        /// Location
        /// </summary>
        [DataMember(Name = "location")]
        [JsonProperty(PropertyName = "location")]
        [XmlArrayItem(ElementName = "location")]
        public int location;

        /// <summary>
        /// Average Bitrate
        /// </summary>
        [DataMember(Name = "average_bitrate")]
        [JsonProperty(PropertyName = "average_bitrate")]
        [XmlArrayItem(ElementName = "average_bitrate")]
        public int averageBitRate;

        /// <summary>
        /// Total Bitrate
        /// </summary>
        [DataMember(Name = "total_bitrate")]
        [JsonProperty(PropertyName = "total_bitrate")]
        [XmlArrayItem(ElementName = "total_bitrate")]
        public int totalBitRate;

        /// <summary>
        /// Current Bitrate
        /// </summary>
        [DataMember(Name = "current_bitrate")]
        [JsonProperty(PropertyName = "current_bitrate")]
        [XmlArrayItem(ElementName = "current_bitrate")]
        public int currentBitRate;
    }
}