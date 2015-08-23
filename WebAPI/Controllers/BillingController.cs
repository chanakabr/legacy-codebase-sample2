using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Billing;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/billing/action")]
    public class BillingController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Charges a user for a media file with the given PPV module entitlements         
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Price not correct = 6000, Unknown PPV module = 6001, Expired credit card = 6002, Cellular permissions error (for cellular charge) = 6003, Unknown billing provider = 6004
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="ppv_id">PPV module identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="price">Price</param>
        /// <param name="currency">Currency</param>
        /// <param name="coupon_code">Coupon code</param>
        /// <param name="extra_params">Custom extra parameters (changes between different billing providers)</param>
        /// <param name="encrypted_cvv">Encrypted credit card CVV</param>
        /// <param name="file_id">Media file identifier</param>
        //TODO: change name and route or remove
        [Route("ppvs/{ppv_id}/buy"), HttpPost]
        [ApiAuthorize]
        public KalturaBillingResponse ChargeUserForMediaFile(string partner_id, string ppv_id, string user_id, double price, string currency, string coupon_code, string extra_params,
            string encrypted_cvv, int file_id, [FromUri]string udid = null)
        {
            KalturaBillingResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().ChargeUserForMediaFile(groupId, user_id, price, currency, file_id, ppv_id, coupon_code,
                    extra_params, udid, encrypted_cvv);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }        
    }
}