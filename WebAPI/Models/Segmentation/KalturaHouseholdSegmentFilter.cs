using ApiLogic.Base;
using ApiLogic.Users.Managers;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Segmentation;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    public partial class KalturaHouseholdSegmentFilter : KalturaCrudFilter<KalturaHouseholdSegmentOrderBy, HouseholdSegment>
    {
        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        public override KalturaHouseholdSegmentOrderBy GetDefaultOrderByValue()
        {
            return KalturaHouseholdSegmentOrderBy.NONE;
        }

        public override void Validate()
        {
        }

        public KalturaHouseholdSegmentFilter() : base()
        {
        }

        public override GenericListResponse<HouseholdSegment> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<HouseholdSegmentFilter>(this);
            return HouseholdSegmentManager.Instance.List(contextData, coreFilter);
        }
    }

    public enum KalturaHouseholdSegmentOrderBy
    {
        NONE
    }
}