using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/transaction/action")]
    public class TransactionController : ApiController
    {
        /// <summary>
        /// Performs PPV/Subscription/Collection purchase.
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="user_id">User to charge </param>
        /// <param name="household_id">Household to charge </param>
        /// <param name="price">Net sum to charge – as a one-time transaction. Price must match the previously provided price for the specified content. </param>
        /// <param name="currency">Identifier for paying currency, according to ISO 4217</param>
        /// <param name="content_id">Identifier for the content to purchase. Relevant only if Product type = PPV</param>
        /// <param name="product_id">Identifier for the package from which this content is offered</param>        
        /// <param name="product_type">Package type. Possible values: PPV, Subscription, Collection</param>
        /// <param name="coupon">Coupon code</param> 
        /// <remarks>Possible status codes: 
        /// User not in domain = 1005, Invalid user = 1026, User does not exist = 2000, User suspended = 2001, Coupon not valid = 3020, PPV purchased = 3021, Free = 3022, For purchase subscription only = 3023,
        /// Subscription purchased = 3024, Not for purchase = 3025, CollectionPurchased = 3027, Incorrect price = 6000, UnKnown PPV module = 6001, Payment gateway not set for household = 6007, Payment gateway does not exist = 6008, 
        /// Payment gateway charge ID required = 6009, No configuration found = 6011, Signature mismatch = 6013, Unknown transaction state = 6042
        /// Credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007,
        /// Partner is invalid = 500008</remarks>
        [Route("purchase"), HttpPost]
        public KalturaTransaction Purchase(string partner_id, string user_id, int household_id, double price, string currency,
                                                   int content_id, int product_id, KalturaTransactionType product_type, string coupon)
        {
            KalturaTransaction response = new KalturaTransaction();

            int groupId = int.Parse(partner_id);

            // validate household id
            if (household_id < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "illegal household id");

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
        /// <param name="adapter_transaction_state">Payment gateway adapter application state for the transaction to update. 
        /// Possible values: 0 = OK, 1 = Pending, 2 = Failed </param>
        /// <param name="external_transaction_id">external transaction identifier</param>
        /// <param name="external_status">Payment gateway transaction status</param>
        /// <param name="external_message">Payment gateway message</param>
        /// <param name="fail_reason">The reason the transaction failed. 
        /// Possible values: 20 = Insufficient funds, 21 = Invalid account, 22 = User unknown, 23 = Reason unknown, 24 = Unknown payment gateway response, 25 = No response from payment gateway</param>
        /// <param name="signature">Security signature to validate the caller is a payment gateway adapter application</param>
        /// <remarks>Possible status codes: payment gateway not exist = 6008, signature does not match = 6036, error while updating pending transaction = 6037, 
        /// Payment gateway transaction was not found = 6038, Payment gateway transaction is not pending = 6039, Unknown transaction state = 6042, 
        /// credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("updateState"), HttpPost]
        public void UpdateState(string partner_id, string payment_gateway_id, int adapter_transaction_state, string external_transaction_id, string external_status,
            string external_message, int fail_reason, string signature)
        {
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                ClientsManager.ConditionalAccessClient().UpdatePendingTransaction(groupId, payment_gateway_id, adapter_transaction_state, external_transaction_id, external_status,
                    external_message, fail_reason, signature);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Verifies PPV/Subscription/Collection purchase and entitles the user.
        /// </summary>
        /// <param name="household_id">Household to charge </param>
        /// <param name="content_id">Identifier for the content to purchase. Relevant only if Product type = PPV</param>
        /// <param name="product_id">Identifier for the package from which this content is offered</param>        
        /// <param name="product_type">Package type. Possible values: PPV, Subscription, Collection</param>
        /// <param name="purchase_token">The token received after the purchase</param>
        /// <param name="payment_gateway_type_id">The payment gateway type identifier.  Possible values: 1 - Google, 2 - Apple</param>
        /// <remarks>Possible status codes: 
        /// User not in domain = 1005, Invalid user = 1026, User does not exist = 2000, User suspended = 2001, Coupon not valid = 3020, PPV purchased = 3021, Free = 3022, For purchase subscription only = 3023,
        /// Subscription purchased = 3024, Not for purchase = 3025, CollectionPurchased = 3027, Incorrect price = 6000, UnKnown PPV module = 6001, Payment gateway not set for household = 6007, Payment gateway does not exist = 6008, 
        /// Payment gateway charge ID required = 6009, No configuration found = 6011, Signature mismatch = 6013, Unknown transaction state = 6042
        /// Credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007,
        /// Partner is invalid = 500008</remarks>
        [Route("ProcessReceipt"), HttpPost]
        [ApiAuthorize]
        public KalturaTransaction ProcessReceipt(int household_id, int content_id, int product_id, KalturaTransactionType product_type,
                                                         string purchase_token, int payment_gateway_type_id)
        {
            KalturaTransaction response = null;
            KS ks = KS.GetFromRequest();

            // validate household id
            if (household_id < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "illegal household id");

            // validate purchase token
            if (string.IsNullOrEmpty(purchase_token))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "purchase token cannot be empty");

            // validate payment gateway id
            if (payment_gateway_type_id < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "illegal payment gateway ID");

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().VerifyPurchase(ks.GroupId, ks.UserId.ToString(), household_id, content_id, product_id, product_type, string.Empty, purchase_token, payment_gateway_type_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}