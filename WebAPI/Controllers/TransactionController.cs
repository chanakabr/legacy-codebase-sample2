using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("transaction")]
    public class TransactionController : ApiController
    {


        // TODO: finish documentation
        /// <summary>
        /// Performs PPV/Subscription/Collection purchase.
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="token">Facebook token</param>
        /// <remarks>Possible status codes: Conflict - 7000, MinFriendsLimitationBad - 7001, credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("transaction/purchase"), HttpGet]
        public KalturaTransactionResponse Purchase([FromUri] string partner_id, [FromUri] string user_id, [FromUri] int household_id, [FromUri] double price, [FromUri] string currency, [FromUri] int content_id, [FromUri] int product_id, [FromUri] string coupon, [FromUri] KalturaTransactionType transaction_type, [FromUri] string device_name, [FromUri] int payment_gateway_id)
        {
            KalturaTransactionResponse response = new KalturaTransactionResponse();
            int groupId = int.Parse(partner_id);


            // TODO: finish variable validation

            // validate user id
            if (string.IsNullOrEmpty(user_id))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user_id cannot be empty");

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().Purchase(groupId, user_id, household_id, price, currency, content_id, product_id, transaction_type, coupon, device_name, payment_gateway_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}