using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaBookmarkPlayerData : KalturaOTTObject
    {
        /// <summary>
        /// Action
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlElement(ElementName = "action")]
        public KalturaBookmarkActionType action { get; set; }

        /// <summary>
        /// Average Bitrate
        /// </summary>
        [DataMember(Name = "averageBitrate")]
        [JsonProperty(PropertyName = "averageBitrate")]
        [XmlElement(ElementName = "averageBitrate")]
        public int? averageBitRate { get; set; }

        /// <summary>
        /// Total Bitrate
        /// </summary>
        [DataMember(Name = "totalBitrate")]
        [JsonProperty(PropertyName = "totalBitrate")]
        [XmlElement(ElementName = "totalBitrate")]
        public int? totalBitRate { get; set; }

        /// <summary>
        /// Current Bitrate
        /// </summary>
        [DataMember(Name = "currentBitrate")]
        [JsonProperty(PropertyName = "currentBitrate")]
        [XmlElement(ElementName = "currentBitrate")]
        public int? currentBitRate { get; set; }

        /// <summary>
        /// Identifier of the file
        /// </summary>
        [DataMember(Name = "fileId")]
        [JsonProperty(PropertyName = "fileId")]
        [XmlElement(ElementName = "fileId")]
        public long? FileId { get; set; }
    }
}