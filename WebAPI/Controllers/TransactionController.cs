using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/transaction/action")]
    [OldStandardAction("purchaseOldStandard", "purchase")]
    [OldStandardAction("setWaiver", "waiver")]
    [OldStandardAction("purchaseSessionIdOldStandard", "purchaseSessionId")]
    public class TransactionController : ApiController
    {
        /// <summary>
        /// Purchase specific product or subscription for a household. Upon successful charge entitlements to use the requested product or subscription are granted. 
        /// </summary>
        /// <param name="purchase">Purchase properties</param>
        /// <remarks>Possible status codes: 
        /// User not in domain = 1005, Invalid user = 1026, User does not exist = 2000, User suspended = 2001, Coupon not valid = 3020, Unable to purchase - PPV purchased = 3021,  Unable to purchase - Free = 3022,  Unable to purchase - For purchase subscription only = 3023,
        ///  Unable to purchase - Subscription purchased = 3024, Not for purchase = 3025, Unable to purchase - Collection purchased = 3027, 
        ///  Adapter Url required = 5013, Incorrect price = 6000, UnKnown PPV module = 6001, Payment gateway not set for household = 6007, 
        ///  Payment gateway does not exist = 6008, Payment gateway charge ID required = 6009, No configuration found = 6011, 
        ///  Signature mismatch = 6013, Unknown transaction state = 6042, Payment method not set for household = 6048,
        ///  Payment method not exist = 6049       
        /// </remarks>
        [Route("purchase"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public KalturaTransaction Purchase(KalturaPurchase purchase)
        {
            KalturaTransaction response = new KalturaTransaction();

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            purchase.Validate();
            
            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().Purchase(groupId, KS.GetFromRequest().UserId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), purchase.Price, purchase.Currency, purchase.getContentId(), purchase.ProductId, purchase.ProductType, purchase.getCoupon(), udid, purchase.getPaymentGatewayId(), purchase.getPaymentMethodId());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Purchase specific product or subscription for a household. Upon successful charge entitlements to use the requested product or subscription are granted. 
        /// </summary>
        /// <param name="price">Net sum to charge – as a one-time transaction. Price must match the previously provided price for the specified content. </param>
        /// <param name="currency">Identifier for paying currency, according to ISO 4217</param>
        /// <param name="content_id">Identifier for the content to purchase. Relevant only if Product type = PPV</param>
        /// <param name="product_id">Identifier for the package from which this content is offered</param>        
        /// <param name="product_type">Package type. Possible values: PPV, Subscription, Collection</param>
        /// <param name="coupon">Coupon code</param> 
        /// <param name="payment_gateway_id">Identifier for a pre-associated payment gateway. If not provided – the account’s default payment gateway is used</param>
        /// <param name="payment_method_id">Identifier for a pre-entered payment method. If not provided – the household’s default payment method is used</param>
        /// <remarks>Possible status codes: 
        /// User not in domain = 1005, Invalid user = 1026, User does not exist = 2000, User suspended = 2001, Coupon not valid = 3020, Unable to purchase - PPV purchased = 3021,  Unable to purchase - Free = 3022,  Unable to purchase - For purchase subscription only = 3023,
        ///  Unable to purchase - Subscription purchased = 3024, Not for purchase = 3025, Unable to purchase - Collection purchased = 3027, 
        ///  Adapter Url required = 5013, Incorrect price = 6000, UnKnown PPV module = 6001, Payment gateway not set for household = 6007, 
        ///  Payment gateway does not exist = 6008, Payment gateway charge ID required = 6009, No configuration found = 6011, 
        ///  Signature mismatch = 6013, Unknown transaction state = 6042, Payment method not set for household = 6048,
        ///  Payment method not exist = 6049       
        /// </remarks>
        [Route("purchaseOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaTransaction PurchaseOldStandard(double price, string currency, int product_id, KalturaTransactionType product_type, int content_id = 0, string coupon = null, int payment_gateway_id = 0,
            int? payment_method_id = null)
        {
            KalturaTransaction response = new KalturaTransaction();

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            // validate purchase token
            if (string.IsNullOrEmpty(currency))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "currency cannot be empty");

            //// validate price
            if (price < 0)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "price is illegal");

            //// validate product_id
            if (product_id <= 0)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "product_id is illegal");

            //// validate payment_method_id
            if (!payment_method_id.HasValue)
            {
                payment_method_id = 0;
            }
            else if (payment_method_id.HasValue && payment_method_id.Value == 0)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment_method_id cannot be 0");


            if (coupon == null)
            {
                coupon = string.Empty;
            }

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().Purchase(groupId, KS.GetFromRequest().UserId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), price, currency, content_id, product_id, product_type, coupon, udid, payment_gateway_id, payment_method_id.Value);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Updates a pending purchase transaction state.
        /// </summary>
        /// <param name="paymentGatewayId">Payment gateway identifier</param>
        /// <param name="externalTransactionId">external transaction identifier</param>
        /// <param name="signature">Security signature to validate the caller is a payment gateway adapter application</param>
        /// <param name="status">Status properties</param>
        /// <remarks>Possible status codes: payment gateway not exist = 6008, signature does not match = 6036, error while updating pending transaction = 6037, 
        /// Payment gateway transaction was not found = 6038, Payment gateway transaction is not pending = 6039, Unknown transaction state = 6042, 
        ///,         </remarks>
        [Route("updateStatus"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public void UpdateStatus(string paymentGatewayId, string externalTransactionId, string signature, KalturaTransactionStatus status)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                ClientsManager.ConditionalAccessClient().UpdatePendingTransaction(groupId, paymentGatewayId, (int)status.AdapterStatus, status.ExternalId, status.ExternalStatus,
                    status.ExternalMessage, status.FailReason, signature);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Updates a pending purchase transaction state.
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
        [Obsolete]
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
        /// <param name="externalReceipt">Receipt properties</param>
        /// <remarks>Possible status codes: 
        /// User not in domain = 1005, Invalid user = 1026, User does not exist = 2000, User suspended = 2001, PPV purchased = 3021, Free = 3022, For purchase subscription only = 3023,
        /// Subscription purchased = 3024, Not for purchase = 3025, CollectionPurchased = 3027, UnKnown PPV module = 6001, Payment gateway does not exist = 6008, No configuration found = 6011,
        /// Signature mismatch = 6013, Unknown transaction state = 6042   
        ///    </remarks>
        [Route("validateReceipt"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public KalturaTransaction ValidateReceipt(KalturaExternalReceipt externalReceipt)
        {
            KalturaTransaction response = null;
            KS ks = KS.GetFromRequest();
            string udid = KSUtils.ExtractKSPayload(ks).UDID;

            externalReceipt.Validate();

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().ProcessReceipt(ks.GroupId, ks.UserId.ToString(), 0, externalReceipt.getContentId(), externalReceipt.ProductId, externalReceipt.ProductType, udid, externalReceipt.ReceiptId, externalReceipt.PaymentGatewayName);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Verifies PPV/Subscription/Collection client purchase (such as InApp) and entitles the user.
        /// </summary>
        /// <param name="content_id">Identifier for the content. Relevent only if Product type = PPV. Verified to match the purchase details represented by the purchase_token</param>
        /// <param name="product_id">Identifier for the product package from which this content is offered. Verified to match the purchase details represented by the purchase_token </param>        
        /// <param name="product_type">Product package type. Possible values: PPV, Subscription, Collection. Verified to match the purchase details represented by the purchase_token </param>
        /// <param name="purchase_receipt">A unique identifier that was provided by the In-App billing service to validate the purchase</param>
        /// <param name="payment_gateway_name">The payment gateway name for the In-App billing service to be used. Possible values: Google/Apple</param>
        /// <remarks>Possible status codes: 
        /// User not in domain = 1005, Invalid user = 1026, User does not exist = 2000, User suspended = 2001, PPV purchased = 3021, Free = 3022, For purchase subscription only = 3023,
        /// Subscription purchased = 3024, Not for purchase = 3025, CollectionPurchased = 3027, UnKnown PPV module = 6001, Payment gateway does not exist = 6008, No configuration found = 6011,
        /// Signature mismatch = 6013, Unknown transaction state = 6042   
        ///    </remarks>
        [Route("processReceipt"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaTransaction ProcessReceipt(int product_id, KalturaTransactionType product_type, string purchase_receipt, string payment_gateway_name, int content_id = 0)
        {
            KalturaTransaction response = null;
            KS ks = KS.GetFromRequest();
            string udid = KSUtils.ExtractKSPayload(ks).UDID;

            // validate purchase token
            if (string.IsNullOrEmpty(purchase_receipt))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "purchase receipt cannot be empty");

            // validate payment gateway id
            if (string.IsNullOrEmpty(payment_gateway_name))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment gateway type cannot be empty");

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().ProcessReceipt(ks.GroupId, ks.UserId.ToString(), 0, content_id, product_id, product_type, udid, purchase_receipt, payment_gateway_name);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// This method shall set the waiver flag on the user entitlement table and the waiver date field to the current date.
        /// </summary>
        /// <param name="assetId">Asset identifier</param>        
        /// <param name="transactionType">The transaction type</param>
        /// <remarks>Possible status codes: 
        ///  User suspended = 2001
        /// </remarks>
        [Route("setWaiver"), HttpPost]
        [ApiAuthorize]
        [OldStandard("assetId", "asset_id")]
        [OldStandard("transactionType", "transaction_type")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool SetWaiver(int assetId, KalturaTransactionType transactionType)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            try
            {

                // get domain       
                var domain = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // check if the user performing the action is domain master
                if (domain == 0)
                {
                    throw new ForbiddenException();
                }

                if (assetId == 0)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id not valid");
                }

                // call client
                response = ClientsManager.ConditionalAccessClient().WaiverTransaction(groupId, (int)domain, KS.GetFromRequest().UserId, assetId, transactionType);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == false)
            {
                throw new InternalServerErrorException();
            }
            return response;
        }

        /// <summary>
        /// Retrieve the purchase session identifier
        /// </summary>
        /// <param name="purchaseSession">Purchase properties</param>
        /// <remarks>
        /// </remarks>
        [Route("getPurchaseSessionId"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public long getPurchaseSessionId(KalturaPurchaseSession purchaseSession)
        {
            long response = 0;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GetCustomDataId(groupId, KS.GetFromRequest().UserId, KSUtils.ExtractKSPayload().UDID, purchaseSession.Price, purchaseSession.Currency, purchaseSession.ProductId, purchaseSession.getContentId(),
                    purchaseSession.Coupon, purchaseSession.ProductType, purchaseSession.getPreviewModuleId());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Retrieve the purchase session identifier
        /// </summary>
        /// <param name="price">Net sum to charge – as a one-time transaction. Price must match the previously provided price for the specified content. </param>
        /// <param name="currency">Identifier for paying currency, according to ISO 4217</param>
        /// <param name="content_id">Identifier for the content to purchase. Relevant only if Product type = PPV</param>
        /// <param name="product_id">Identifier for the package from which this content is offered</param>        
        /// <param name="product_type">Package type. Possible values: PPV, Subscription, Collection</param>
        /// <param name="coupon">Coupon code</param> 
        /// <param name="preview_module_id">Preview module identifier (relevant only for subscription)</param> 
        /// <remarks>
        /// </remarks>
        [Route("purchaseSessionIdOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public long PurchaseSessionIdOldStandard(double price, string currency, int product_id, KalturaTransactionType product_type, int content_id = 0, string coupon = null, int preview_module_id = 0)
        {
            long response = 0;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GetCustomDataId(groupId, KS.GetFromRequest().UserId, KSUtils.ExtractKSPayload().UDID, price, currency, product_id, content_id,
                    coupon, product_type, preview_module_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}