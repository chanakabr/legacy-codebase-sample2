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
        [SchemeProperty(DynamicMinInt = 0)]
        public int? PaddingBefore { get; set; }
        
        /// <summary>
        /// Household specific end padding of the recording
        /// </summary>
        [DataMember(Name = "endPadding")]
        [JsonProperty("endPadding")]
        [XmlElement(ElementName = "endPadding", IsNullable = true)]
        [SchemeProperty(DynamicMinInt = 0)]
        public int? PaddingAfter { get; set; }
    }
}