using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
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
        /// <param name="request">Charge request parameters</param>
        [Route("ppvs/{ppv_id}/buy"), HttpPost]
        public KalturaBillingResponse ChargeUserForMediaFile([FromUri] string partner_id, [FromUri] string ppv_id, [FromBody] KalturaChargePPV request, [FromUri]string udid = null)
        {
            KalturaBillingResponse response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().ChargeUserForMediaFile(groupId, request.UserId, request.Price, request.Currency, request.FileId, ppv_id, request.CouponCode,
                    request.ExtraParams, udid, request.EncryptedCvv);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }        
    }
}