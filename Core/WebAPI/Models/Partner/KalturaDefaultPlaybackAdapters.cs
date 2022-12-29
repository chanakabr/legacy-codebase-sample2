using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public partial class KalturaDefaultPlaybackAdapters : KalturaOTTObject
    {
        /// <summary>
        /// Default adapter identifier for media
        /// </summary>
        [DataMember(Name = "mediaAdapterId")]
        [JsonProperty("mediaAdapterId")]
        [XmlElement(ElementName = "mediaAdapterId")]
        [SchemeProperty(MinLong = 1)]
        public long MediaAdapterId { get; set; }

        /// <summary>
        /// Default adapter identifier for epg
        /// </summary>
        [DataMember(Name = "epgAdapterId")]
        [JsonProperty("epgAdapterId")]
        [XmlElement(ElementName = "epgAdapterId")]
        [SchemeProperty(MinLong = 1)]
        public long EpgAdapterId { get; set; }

        /// <summary>
        /// Default adapter identifier for recording
        /// </summary>
        [DataMember(Name = "recordingAdapterId")]
        [JsonProperty("recordingAdapterId")]
        [XmlElement(ElementName = "recordingAdapterId")]
        [SchemeProperty(MinLong = 1)]
        public long RecordingAdapterId { get; set; }
    }
}