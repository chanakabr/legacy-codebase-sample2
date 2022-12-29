using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Playback adapter partner configuration
    /// </summary>
    public partial class KalturaPlaybackPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// default adapter configuration for: media, epg,recording.
        /// </summary>
        [DataMember(Name = "defaultAdapters")]
        [JsonProperty("defaultAdapters")]
        [XmlElement(ElementName = "defaultAdapters", IsNullable = true)] 
        public KalturaDefaultPlaybackAdapters DefaultAdapters { get; set; }
    }
}