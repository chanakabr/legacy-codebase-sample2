using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Indicates a segment of a household
    /// </summary>
    public partial class KalturaHouseholdSegment : KalturaOTTObject
    {
        /// <summary>
        /// Household SegmentId
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty(PropertyName = "householdId")]
        [XmlElement(ElementName = "householdId")]
        [SchemeProperty()]
        public long HouseholdSegmentId { get; internal set; }

        /// <summary>
        /// Segment Id
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty(PropertyName = "householdId")]
        [XmlElement(ElementName = "householdId")]
        [SchemeProperty()]
        public long HouseholdId { get; set; }

        /// <summary>
        /// Blocking segment Ids
        /// </summary>
        [DataMember(Name = "blockingSegmentIds")]
        [JsonProperty(PropertyName = "blockingSegmentIds")]
        [XmlElement(ElementName = "blockingSegmentIds")]
        [SchemeProperty(WriteOnly = true)]
        public string BlockingSegmentIds { get; set; }

        internal List<long> GetBlockingSegmentIds()
        {
            return BlockingSegmentIds.Split(',').Select(x => long.Parse(x.Trim())).ToList();
        }
    }

    /// <summary>
    /// List of user segments
    /// </summary>
    [DataContract(Name = "KalturaHouseholdSegmentListResponse", Namespace = "")]
    [XmlRoot("KalturaHouseholdSegmentListResponse")]
    public partial class KalturaHouseholdSegmentListResponse : KalturaListResponse
    {
        /// <summary>
        /// Segmentation Types
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty()]
        public List<KalturaHouseholdSegment> HouseholdSegments { get; set; }
    }
}