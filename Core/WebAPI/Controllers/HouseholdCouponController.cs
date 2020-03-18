using ApiObjects.Pricing;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Domains;

namespace WebAPI.Controllers
{
    [Service("householdCoupon")]
    [AddAction(Summary = "householdCoupon add",
               ObjectToAddDescription = "householdCoupon details",
               ClientThrows = new eResponseStatus[]
               {
                   eResponseStatus.CouponCodeIsMissing,
                   eResponseStatus.CouponNotValid,
                   eResponseStatus.HouseholdRequired
               })]
    [DeleteAction(Summary = "Remove coupon from household",
                  IdDescription = "Coupon code",
                  ClientThrows = new eResponseStatus[] { eResponseStatus.CouponCodeNotInHousehold })]
    [ListAction(Summary = "Gets all HouseholdCoupon items for a household", IsFilterOptional = true)]
    public class HouseholdCouponController : KalturaCrudController<KalturaHouseholdCoupon, KalturaHouseholdCouponListResponse, CouponWallet, string, KalturaHouseholdCouponFilter>
    {
    }
}