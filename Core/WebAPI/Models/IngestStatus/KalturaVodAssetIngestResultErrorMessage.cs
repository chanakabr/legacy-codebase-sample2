using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    /// <summary>
    /// A Kaltura error message
    /// </summary>
    public partial class KalturaVodIngestAssetResultErrorMessage : KalturaOTTObject
    {
        /// <summary>
        /// The message description with arguments place holders
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// The message code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }
    }
}