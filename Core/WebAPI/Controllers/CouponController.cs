using ApiObjects.Response;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("coupon")]
    public class CouponController : IKalturaController
    {
        /// <summary>
        /// Returns information about a coupon
        /// </summary>
        /// <remarks>Possible status codes: Coupon not valid = 3020
        ///    </remarks>
        /// <param name="code">Coupon code</param>
        [Action("get")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CouponNotValid)]
        static public KalturaCoupon Get(string code)
        {
            KalturaCoupon coupon = null;

            int groupId = KS.GetFromRequest().GroupId;
            

            if (string.IsNullOrEmpty(code))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "code");
            }
        
            try
            {
                // call client
                coupon = ClientsManager.PricingClient().GetCouponStatus(groupId, code, HouseholdUtils.GetHouseholdIDByKS());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return coupon;
        }      
    }
}