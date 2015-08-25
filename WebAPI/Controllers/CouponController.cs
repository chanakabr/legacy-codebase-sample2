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
        /// Returns the details and status of the received coupon code
        /// </summary>
        /// <remarks>Possible status codes:      
        ///    </remarks>
        /// <param name="code">Coupon code</param>
        [Route("get"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaCoupon Get(string code)
        {
            KalturaCoupon coupon = null;

            int groupId = KS.GetFromRequest().GroupId;

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
    }
}