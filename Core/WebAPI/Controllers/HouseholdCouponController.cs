using ApiObjects.Pricing;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Domains;

namespace WebAPI.Controllers
{
    [Service("householdCoupon")]
    [AddAction(Summary = "householdCoupon add",
        ObjectToAddDescription = "householdCoupon details",
        ClientThrows = new []
        {
            eResponseStatus.CouponCodeIsMissing,
            eResponseStatus.CouponNotValid,
            eResponseStatus.HouseholdRequired,
            eResponseStatus.CouponCodeAlreadyLoaded,
            eResponseStatus.ExceededHouseholdCouponLimit
        })]
    [DeleteAction(Summary = "Remove coupon from household",
        IdDescription = "Coupon code",
        ClientThrows = new []
        {
            eResponseStatus.CouponCodeNotInHousehold,
            eResponseStatus.HouseholdRequired,
            eResponseStatus.CouponCodeIsMissing
        })]
    [ListAction(Summary = "Gets all HouseholdCoupon items for a household",
        IsFilterOptional = true,
        ClientThrows = new []
        {
            eResponseStatus.HouseholdRequired
        })]
    public class HouseholdCouponController : KalturaCrudController<KalturaHouseholdCoupon, KalturaHouseholdCouponListResponse, CouponWallet, string, KalturaHouseholdCouponFilter>
    {
    }
}