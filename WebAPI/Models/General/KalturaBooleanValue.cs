using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// A string representation to return an array of booleans
    /// </summary>
    public partial class KalturaBooleanValue : KalturaValue
    {
        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [XmlElement("value")]
        [JsonProperty("value")]
        [ValidationException(SchemeValidationType.NULLABLE)]
        public bool value { get; set; }
    }
}