using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    [XmlInclude(typeof(KalturaBumpersPlaybackPluginData))]
    public partial class KalturaPlaybackPluginData : KalturaOTTObject
    {
    }

    public partial class KalturaBumpersPlaybackPluginData : KalturaPlaybackPluginData
    {
        /// <summary>
        /// url
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty("url")]
        [XmlElement(ElementName = "url")]
        public string URL { get; set; }

        /// <summary>
        /// Streamer type: hls, dash, progressive.
        /// </summary>
        [DataMember(Name = "streamertype")]
        [JsonProperty("streamertype")]
        [XmlElement(ElementName = "streamertype")]
        public string StreamerType { get; set; }
    }
}