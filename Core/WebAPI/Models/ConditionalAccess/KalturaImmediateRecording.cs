using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaImmediateRecording: KalturaRecording
    {
        /// <summary>
        /// Household specific end padding of the recording
        /// </summary>
        [DataMember(Name = "endPadding")]
        [JsonProperty("endPadding")]
        [XmlElement(ElementName = "endPadding", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public int? EndPadding { get; set; }
        
        /// <summary>
        /// Household absolute start time of the immediate recording
        /// </summary>
        [DataMember(Name = "absoluteStart")]
        [JsonProperty("absoluteStart")]
        [XmlElement(ElementName = "absoluteStart", IsNullable = true)]
        [SchemeProperty(ReadOnly = true, IsNullable = true)]
        public long AbsoluteStartTime { get; set; }
        
        /// <summary>
        /// Household absolute end time of the immediate recording, empty if till end of program
        /// </summary>
        [DataMember(Name = "absoluteEnd")]
        [JsonProperty("absoluteEnd")]
        [XmlElement(ElementName = "absoluteEnd", IsNullable = true)]
        [SchemeProperty(ReadOnly = true, IsNullable = true)]
        public long? AbsoluteEndTime { get; set; }
    }
}