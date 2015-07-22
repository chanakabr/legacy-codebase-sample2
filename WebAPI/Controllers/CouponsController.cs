using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("coupons")]
    public class CouponsController : ApiController
    {
        /// <summary>
        /// Returns the details and status of the received coupon code
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, 
        /// Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="code">Coupon code</param>
        [Route("{code}"), HttpGet]
        public CouponDetails GetCouponStatus([FromUri] string partner_id, [FromUri] string code)
        {
            CouponDetails coupon = null;

            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(code))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "code cannot be empty");
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

        [Route("{code}"), HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public CouponDetails _GetCouponStatus([FromUri] string partner_id, [FromUri] string code)
        {
            return GetCouponStatus(partner_id, code);
        }
    }
}