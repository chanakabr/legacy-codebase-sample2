using ApiLogic.Base;
using ApiLogic.Users.Managers;
using ApiObjects.Segmentation;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Indicates a segment of a household
    /// </summary>
    public partial class KalturaHouseholdSegment : KalturaCrudObject<HouseholdSegment, long, HouseholdSegmentFilter>
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
        /// Segment Id
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty(PropertyName = "householdId")]
        [XmlElement(ElementName = "householdId")]
        [SchemeProperty()]
        public long HouseholdId { get; set; }
        
        internal override ICrudHandler<HouseholdSegment, long, HouseholdSegmentFilter> Handler
        {
            get
            {
                return HouseholdSegmentManager.Instance;
            }
        }

        internal override void ValidateForAdd()
        {
            throw new NotImplementedException();
        }

        internal override void ValidateForUpdate()
        {
            throw new NotImplementedException();
        }

        internal override void SetId(long id)
        {
            SegmentId = id;
            
        }

        public KalturaHouseholdSegment() : base() { }
    }

    public partial class KalturaHouseholdSegmentListResponse : KalturaListResponse<KalturaHouseholdSegment>
    {
        public KalturaHouseholdSegmentListResponse() : base() { }
    }
}