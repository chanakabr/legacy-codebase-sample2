using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaPaddedRecording: KalturaRecording
    {
        /// <summary>
        /// Household specific start padding of the recording
        /// </summary>
        [DataMember(Name = "startPadding")]
        [JsonProperty("startPadding")]
        [XmlElement(ElementName = "startPadding", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, IsNullable = true)]
        public int? StartPadding { get; set; }
        
        /// <summary>
        /// Household specific end padding of the recording
        /// </summary>
        [DataMember(Name = "endPadding")]
        [JsonProperty("endPadding")]
        [XmlElement(ElementName = "endPadding", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, IsNullable = true)]
        public int? EndPadding { get; set; }
    }
}