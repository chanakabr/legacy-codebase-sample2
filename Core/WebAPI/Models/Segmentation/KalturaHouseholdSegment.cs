using ApiLogic.Base;
using ApiLogic.Users.Managers;
using ApiObjects.Base;
using ApiObjects.Response;
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
    public partial class KalturaHouseholdSegment : KalturaCrudObject<HouseholdSegment, long>
    {
        /// <summary>
        /// Segment Id
        /// </summary>
        [DataMember(Name = "segmentId")]
        [JsonProperty(PropertyName = "segmentId")]
        [XmlElement(ElementName = "segmentId")]
        [SchemeProperty(MinInteger = 1)]        
        public long SegmentId { get; set; }

        /// <summary>
        /// Segment Id
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty(PropertyName = "householdId")]
        [XmlElement(ElementName = "householdId")]
        public long HouseholdId { get; set; }
        
        internal override ICrudHandler<HouseholdSegment, long> Handler
        {
            get
            {
                return HouseholdSegmentManager.Instance;
            }
        }

        internal override void SetId(long id)
        {
            throw new NotImplementedException();
        }

        public KalturaHouseholdSegment() : base() { }

        internal override GenericResponse<HouseholdSegment> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<HouseholdSegment>(this);
            return HouseholdSegmentManager.Instance.Add(contextData, coreObject);
        }
    }

    public partial class KalturaHouseholdSegmentListResponse : KalturaListResponse<KalturaHouseholdSegment>
    {
        public KalturaHouseholdSegmentListResponse() : base() { }
    }
}