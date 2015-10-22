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
        /// Charge a user’s household for specific content utilizing the household’s pre-assigned payment gateway. Online, one-time charge only of various content types. Upon successful charge entitlements to use the requested content are granted.
        /// </summary>
        /// <param name="price">Net sum to charge – as a one-time transaction. Price must match the previously provided price for the specified content. </param>
        /// <param name="currency">Identifier for paying currency, according to ISO 4217</param>
        /// <param name="content_id">Identifier for the content to purchase. Relevant only if Product type = PPV</param>
        /// <param name="product_id">Identifier for the package from which this content is offered</param>        
        /// <param name="product_type">Package type. Possible values: PPV, Subscription, Collection</param>
        /// <param name="coupon">Coupon code</param> 
        /// <remarks>Possible status codes: 
        /// User not in domain = 1005, Invalid user = 1026, User does not exist = 2000, User suspended = 2001, Coupon not valid = 3020, Unable to purchase - PPV purchased = 3021,  Unable to purchase - Free = 3022,  Unable to purchase - For purchase subscription only = 3023,
        ///  Unable to purchase - Subscription purchased = 3024, Not for purchase = 3025, Unable to purchase - Collection purchased = 3027, Adapter Url required = 5013, Incorrect price = 6000, UnKnown PPV module = 6001, Payment gateway not set for household = 6007, Payment gateway does not exist = 6008, 
        /// Payment gateway charge ID required = 6009, No configuration found = 6011, Signature mismatch = 6013, Unknown transaction state = 6042
        ///,       
        /// </remarks>
        [Route("purchase"), HttpPost]
        [ApiAuthorize]
        public KalturaTransaction Purchase(double price, string currency, int product_id, KalturaTransactionType product_type, int content_id = 0, string coupon = null)
        {
            KalturaTransaction response = new KalturaTransaction();

            int groupId = KS.GetFromRequest().GroupId;

            // validate purchase token
            if (string.IsNullOrEmpty(currency))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "currency cannot be empty");

            //// validate price
            if (price <= 0)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "price is illegal");

            //// validate product_id
            if (product_id <= 0)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "product_id is illegal");

            if (coupon == null)
            {
                coupon = string.Empty;
            }

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().Purchase(groupId, KS.GetFromRequest().UserId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), price, currency, content_id, product_id, product_type, coupon, string.Empty, 0);
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
        ///,         </remarks>
        [Route("updateState"), HttpPost]
        [ApiAuthorize]
        public void UpdateState(string payment_gateway_id, int adapter_transaction_state, string external_transaction_id, string external_status, string external_message, int fail_reason, string signature)
        {
            int groupId = KS.GetFromRequest().GroupId;

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
        /// Verifies PPV/Subscription/Collection client purchase (such as InApp) and entitles the user.
        /// </summary>
        /// <param name="content_id">Identifier for the content. Relevant only if Product type = PPV. Verified to match the purchase details represented by the purchase_receipt</param>
        /// <param name="product_id">Identifier for the product package from which this content is offered. Verified to match the purchase details represented by the purchase_receipt</param>        
        /// <param name="product_type">Product package type. Possible values: PPV, Subscription, Collection. Verified to match the purchase details represented by the purchase_receipt</param>
        /// <param name="purchase_receipt">A unique identifier that was provided by the In-App billing service to validate the purchase</param>
        /// <param name="payment_gateway_name">The payment gateway name for the In-App billing service to be used. Possible values: Google/Apple</param>
        /// <remarks>Possible status codes: 
        /// User not in domain = 1005, Invalid user = 1026, User does not exist = 2000, User suspended = 2001, PPV purchased = 3021, Free = 3022, For purchase subscription only = 3023,
        /// Subscription purchased = 3024, Not for purchase = 3025, CollectionPurchased = 3027, UnKnown PPV module = 6001, Payment gateway does not exist = 6008, No configuration found = 6011,
        /// Signature mismatch = 6013, Unknown transaction state = 6042   
        ///    </remarks>
        [Route("processReceipt"), HttpPost]
        [ApiAuthorize]
        public KalturaTransaction ProcessReceipt(int product_id, KalturaTransactionType product_type, string purchase_receipt, string payment_gateway_name, int content_id = 0)
        {
            KalturaTransaction response = null;
            KS ks = KS.GetFromRequest();

            // validate purchase token
            if (string.IsNullOrEmpty(purchase_receipt))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "purchase receipt cannot be empty");

            // validate payment gateway id
            if (string.IsNullOrEmpty(payment_gateway_name))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment gateway type cannot be empty");

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().ProcessReceipt(ks.GroupId, ks.UserId.ToString(), 0, content_id, product_id, product_type, string.Empty, purchase_receipt, payment_gateway_name);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}