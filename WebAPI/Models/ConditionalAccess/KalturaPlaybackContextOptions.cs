using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaPlaybackContextOptions : KalturaOTTObject
    {
        /// <summary>
        /// Protocol of the specific media object.
        /// </summary>
        [DataMember(Name = "mediaProtocol")]
        [JsonProperty("mediaProtocol")]
        [XmlElement(ElementName = "mediaProtocol")]
        public string MediaProtocol { get; set; }

        /// <summary>
        /// Playback streamer type: RTMP, HTTP, appleHttps, rtsp, sl.
        /// </summary>
        [DataMember(Name = "streamerType")]
        [JsonProperty("streamerType")]
        [XmlElement(ElementName = "streamerType")]
        public string StreamerType { get; set; }

        /// <summary>
        /// Media file ID
        /// </summary>
        [DataMember(Name = "mediaFileId")]
        [JsonProperty("mediaFileId")]
        [XmlElement(ElementName = "mediaFileId")]
        public int MediaFileId { get; set; }
    }
}