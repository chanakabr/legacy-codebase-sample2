using ApiObjects.Base;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Kaltura Houseold CouponCode Filter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaHouseoldCouponCodeFilter : KalturaFilter<KalturaHouseoldCouponCodeOrderBy>, KalturaRelatedObjectFilter
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