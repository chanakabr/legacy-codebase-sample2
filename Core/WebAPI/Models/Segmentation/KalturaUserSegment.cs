using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;


namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Indicates a segment of a user
    /// </summary>
    public partial class KalturaUserSegment : KalturaOTTObject
    {
        /// <summary>
        /// Segment Id
        /// </summary>
        [DataMember(Name = "segmentId")]
        [JsonProperty(PropertyName = "segmentId")]
        [XmlElement(ElementName = "segmentId")]
        [SchemeProperty()]
        public long SegmentId { get; set; }
        
        /// <summary>
        /// User Id of segment
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty(PropertyName = "userId")]
        [XmlElement(ElementName = "userId")]
        [SchemeProperty(WriteOnly = true)]
        public string UserId { get; set; }

    }

    /// <summary>
    /// List of user segments
    /// </summary>
    [DataContract(Name = "KalturaUserSegmentListResponse", Namespace = "")]
    [XmlRoot("KalturaUserSegmentListResponse")]
    public partial class KalturaUserSegmentListResponse : KalturaListResponse
    {
        /// <summary>
        /// Segmentation Types
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty()]
        public List<KalturaUserSegment> Segments { get; set; }
    }

    /// <summary>
    /// Filter for user segments
    /// </summary>
    [SchemeClass(Required = new[] { "userIdEqual" })]
    public partial class KalturaUserSegmentFilter : KalturaFilter<KalturaUserSegmentOrder>
    {
        /// <summary>
        /// User ID
        /// </summary>
        [DataMember(Name = "userIdEqual", IsRequired = true)]
        [JsonProperty(PropertyName = "userIdEqual")]
        [XmlElement(ElementName = "userIdEqual")]
        [SchemeProperty(MinLength = 1)]
        public string UserIdEqual { get; set; }

        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        public override KalturaUserSegmentOrder GetDefaultOrderByValue()
        {
            return KalturaUserSegmentOrder.NONE;
        }
    }

    /// <summary>
    /// User segments order
    /// </summary>
    public enum KalturaUserSegmentOrder
    {
        NONE
    }
}