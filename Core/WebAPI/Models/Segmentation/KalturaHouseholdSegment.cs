using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Indicates a segment of a household
    /// </summary>
    public partial class KalturaHouseholdSegment : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// Segment Id
        /// </summary>
        [DataMember(Name = "segmentId")]
        [JsonProperty(PropertyName = "segmentId")]
        [XmlElement(ElementName = "segmentId")]
        [SchemeProperty(MinLong = 1)]        
        public long SegmentId { get; set; }

        /// <summary>
        /// Segment Id
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty(PropertyName = "householdId")]
        [XmlElement(ElementName = "householdId")]
        public long HouseholdId { get; set; }
    }
}