using ApiObjects.Pricing;
using ApiObjects.Response;
using System;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Domains;
using WebAPI.Utils;

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
    [ListAction]
    public class HouseholdCouponController : KalturaCrudController<KalturaHouseholdCoupon, CouponWallet, string, CouponWalletFilter>
    {
        /// <summary>
        /// Gets all HouseholdCoupon items for a household
        /// </summary>
        /// <param name="filter">Request filter</param>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaHouseholdCouponListResponse List(KalturaHouseholdCouponFilter filter = null)
        {
            if (filter == null)
            {
                filter = new KalturaHouseholdCouponFilter();
            }
            else
            {
                filter.Validate();
            }

            var responseProfile = Utils.Utils.GetResponseProfileFromRequest();
            var response = filter.Execute<KalturaHouseholdCouponListResponse, KalturaHouseholdCoupon>();
            if (response.Objects != null && response.Objects.Count > 0 && responseProfile != null)
            {
                int groupId = KS.GetFromRequest().GroupId;
                PricingUtils.SetCouopnData(groupId, HouseholdUtils.GetHouseholdIDByKS(groupId), responseProfile, response.Objects);
            }
            return response;
        }
    }
}