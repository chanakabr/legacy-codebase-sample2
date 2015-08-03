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
    [RoutePrefix("service/transaction/action")]
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
        /// <remarks>Possible status codes: 
        /// Credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008
        /// Payment gateway not set for household = 6007, Payment gateway does not exist = 6008, Payment gateway charge ID required = 6009, No configuration found = 6011, Adapter app failure = 6012,
        /// Signature mismatch = 6013, No response from payment gateway = 6030,  Invalid account = 6031, Insufficient funds = 6032, Unknown payment gateway response = 6033,
        /// Payment gateway adapter user known = 6034, Payment gateway adapter reason unknown = 6035, Unknown transaction state = 6042 </remarks>
        [Route("purchase"), HttpPost]
        public KalturaTransactionResponse Purchase([FromUri] string partner_id, [FromUri] string user_id, [FromUri] int household_id, [FromUri] double price, [FromUri] string currency,
                                                   [FromUri] int content_id, [FromUri] int product_id, [FromUri] KalturaTransactionType product_type, [FromUri] string coupon)
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
                response = ClientsManager.ConditionalAccessClient().Purchase(groupId, user_id, household_id, price, currency, content_id, product_id, product_type, coupon, string.Empty, 0);
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
        /// <param name="adapter_transaction_state">Payment gateway adapter application state for the transaction to update</param>
        /// <param name="adapter_message">Payment gateway adapter application message to update</param> 
        /// <param name="external_transaction_id">external transaction identifier</param>
        /// <param name="external_status">Payment gateway transaction status</param>
        /// <param name="external_message">Payment gateway message</param>
        /// <param name="signature">Security signature to validate the caller is a payment gateway adapter application</param>
        /// <remarks>Possible status codes: signature does not match = 6036, error while updating pending transaction = 6037, payment gateway not exist = 6008,
        /// Payment gateway transaction was not found = 6038, Unknown transaction state = 6042, Payment gateway transaction is not pending = 6039,
        /// credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("update"), HttpPost]
        public void UpdateState(string partner_id, string payment_gateway_id, int adapter_transaction_state, string adapter_message, string external_transaction_id, string external_status,
            string external_message, string signature)
        {
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                ClientsManager.ConditionalAccessClient().UpdatePendingTransaction(groupId, payment_gateway_id, adapter_transaction_state, adapter_message, external_transaction_id, external_status,
                    external_message, signature);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
    }
}