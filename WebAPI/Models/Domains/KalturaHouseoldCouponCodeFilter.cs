using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Kaltura Houseold CouponCode Filter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaHouseoldCouponCodeFilter : KalturaFilter<KalturaAggregationCountOrderBy>, KalturaRelatedObjectFilter
    {
        internal virtual void Validate()
        {
        }

        public override KalturaAggregationCountOrderBy GetDefaultOrderByValue()
        {
            return KalturaAggregationCountOrderBy.NONE;
        }
    }
}
