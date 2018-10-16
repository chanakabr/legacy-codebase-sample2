using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Filters;
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
        public long? SegmentId { get; set; }

        /// <summary>
        /// Segmentation type Id
        /// </summary>
        [DataMember(Name = "segmentationTypeId")]
        [JsonProperty(PropertyName = "segmentationTypeId")]
        [XmlElement(ElementName = "segmentationTypeId")]
        [SchemeProperty()]
        public long SegmentationTypeId { get; set; }
        
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
    public partial class KalturaUserSegmentFilter : KalturaFilter<KalturaUserSegmentOrder>
    {
        /// <summary>
        /// User ID
        /// </summary>
        [DataMember(Name = "userIdEqual")]
        [JsonProperty(PropertyName = "userIdEqual")]
        [XmlElement(ElementName = "userIdEqual")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public string UserIdEqual { get; set; }

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