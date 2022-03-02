using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    /// <summary>
    /// A Kaltura error message
    /// </summary>
    public partial class KalturaEpgIngestErrorMessage : KalturaOTTObject
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

        // Used for compatibility with KalturaAPIException
        /// <summary>
        /// Message args
        /// </summary>
        [DataMember(Name = "args")]
        [JsonProperty("args")]
        [XmlElement(ElementName = "args", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> Args { get; set; }
    }
}