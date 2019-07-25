using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Kaltura Houseold CouponCode Filter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaHouseholdCouponCodeFilter : KalturaFilter<KalturaHouseholdCouponCodeFilterOrderBy>, KalturaRelatedObjectFilter
    {
        internal virtual void Validate()
        {
        }

        public override KalturaHouseholdCouponCodeFilterOrderBy GetDefaultOrderByValue()
        {
            return KalturaHouseholdCouponCodeFilterOrderBy.NONE;
        }
    }

    public enum KalturaHouseholdCouponCodeFilterOrderBy
    {
        NONE
    }
}
