using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/coupon/action")]
    public class CouponController : ApiController
    {
        /// <summary>
        /// Returns information about a coupon
        /// </summary>
        /// <remarks>Possible status codes: Coupon not valid = 3020
        ///    </remarks>
        /// <param name="code">Coupon code</param>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaCoupon Get(string code)
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
                coupon = ClientsManager.PricingClient().GetCouponStatus(groupId, code);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return coupon;
        }
    }
}