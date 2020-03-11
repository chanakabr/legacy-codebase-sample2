using ApiLogic.Base;
using ApiLogic.Users.Managers;
using ApiObjects.Segmentation;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    public partial class KalturaHouseholdSegmentFilter : KalturaCrudFilter<KalturaHouseholdSegmentOrderBy, HouseholdSegment, long, HouseholdSegmentFilter>
    {
        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        public override ICrudHandler<HouseholdSegment, long, HouseholdSegmentFilter> Handler
        {
            get
            {
                return HouseholdSegmentManager.Instance;
            }
        }

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
    }

    public enum KalturaHouseholdSegmentOrderBy
    {
        NONE
    }
}