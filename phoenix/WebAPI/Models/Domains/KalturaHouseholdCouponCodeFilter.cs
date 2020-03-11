using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Kaltura Houseold CouponCode Filter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaHouseholdCouponCodeFilter : KalturaFilter<KalturaHouseoldCouponCodeOrderBy>, KalturaRelatedObjectFilter
    {
        public override KalturaHouseoldCouponCodeOrderBy GetDefaultOrderByValue()
        {
            return KalturaHouseoldCouponCodeOrderBy.NONE;
        }
    }
    
    public enum KalturaHouseoldCouponCodeOrderBy
    {
        NONE
    }
}
