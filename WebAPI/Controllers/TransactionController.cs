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
        /// <summary>
        /// Performs PPV/Subscription/Collection purchase.
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="price">Item price</param>
        /// <param name="currency">Payment currency</param>
        /// <param name="content_id">In case the transaction type is PPV - the content ID represent the relevant file identifier</param>
        /// <param name="product_id">Item identifier: PPV/Subscription/Collection identifier</param>
        /// <param name="coupon">Coupon code</param> 
        /// <param name="product_type">Purchase item type: PPV/Subscription/Collection</param>
        /// <param name="udid">Client UDID</param>
        /// <remarks>Possible status codes: credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("purchase"), HttpPost]
        public KalturaTransactionResponse Purchase([FromUri] string partner_id, [FromUri] string user_id, [FromUri] int household_id, [FromUri] double price, [FromUri] string currency,
                                                   [FromUri] int content_id, [FromUri] int product_id, [FromUri] KalturaTransactionType product_type, [FromUri] string coupon, [FromUri] string udid)
        {
            KalturaTransactionResponse response = new KalturaTransactionResponse();

            int groupId = int.Parse(partner_id);

            // validate user id
            if (string.IsNullOrEmpty(user_id))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user_id cannot be empty");

            // validate currency
            if (string.IsNullOrEmpty(currency))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "currency cannot be empty");

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().Purchase(groupId, user_id, household_id, price, currency, content_id, product_id, product_type, coupon, udid, 0);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Updates a pending transaction state (can be called only from a payment gateway adapter application, makes validation using signature and shared secret).
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="payment_gateway_id">Payment gateway identifier</param>
        /// <param name="external_transaction_id">external transaction identifier</param>
        /// <param name="state">The state of the transaction to update</param>
        /// <param name="signature">Security signature to validate the caller is a payment gateway adapter application</param>
        /// <remarks>Possible status codes: signature does not match = 6023, error while updating pending transaction = 6024, payment gateway does not exist = 6008, no payment gateway was found = 6018,
        /// credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        public void UpdateState(string partner_id, string payment_gateway_id, string external_transaction_id, KalturaTransactionState state, string signature)
        {
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                ClientsManager.ConditionalAccessClient().UpdatePendingTransaction(groupId, payment_gateway_id, external_transaction_id, state, signature);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
    }
}