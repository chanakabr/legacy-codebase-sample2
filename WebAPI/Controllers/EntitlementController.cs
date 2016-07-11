using System;
using System.Collections.Generic;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/entitlement/action")]
    [OldStandardAction("listOldStandard", "list")]
    public class EntitlementController : ApiController
    {
        /// <summary>
        /// Immediately cancel a subscription, PPV or collection. Cancel is possible only if within cancellation window and content not already consumed
        /// </summary>                
        /// <param name="assetId">The mediaFileID to cancel</param>        
        /// <param name="transactionType">The transaction type for the cancelation</param>
        /// <remarks>Possible status codes: 
        /// Household suspended = 1009, Invalid purchase = 3000, Cancellation window period expired = 3001, Content already consumed = 3005</remarks>
        [Route("cancel"), HttpPost]
        [ApiAuthorize]
        [OldStandard("assetId", "asset_id")]
        [OldStandard("transactionType", "transaction_type")]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public bool Cancel(int assetId, KalturaTransactionType transactionType)
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
                response = ClientsManager.ConditionalAccessClient().CancelServiceNow(groupId, (int)domain, assetId, transactionType, false);
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
        /// Immediately cancel a subscription, PPV or collection. Cancel applies regardless of cancellation window and content consumption status
        /// </summary>                
        /// <param name="assetId">The mediaFileID to cancel</param>        
        /// <param name="transactionType">The transaction type for the cancelation</param>
        /// <remarks>Possible status codes: 
        /// Household suspended = 1009, Invalid purchase = 3000</remarks>
        [Route("forceCancel"), HttpPost]
        [ApiAuthorize]
        [OldStandard("assetId", "asset_id")]
        [OldStandard("transactionType", "transaction_type")]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public bool ForceCancel(int assetId, KalturaTransactionType transactionType)
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
                response = ClientsManager.ConditionalAccessClient().CancelServiceNow(groupId, (int)domain, assetId, transactionType, true);
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
        /// Cancel a household service subscription at the next renewal. The subscription stays valid till the next renewal.        
        /// </summary>        
        /// <param name="subscriptionId">Subscription Code</param>
        /// <remarks>Possible status codes: 
        /// Household suspended = 1009, Invalid purchase = 3000, SubscriptionNotRenewable = 300</remarks>
        [Route("cancelRenewal"), HttpPost]
        [ApiAuthorize]
        [OldStandard("subscriptionId", "subscription_id")]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public void CancelRenewal(string subscriptionId)
        {
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(subscriptionId))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "subscription code not valid");
            }
            try
            {
                // get domain       
                var domain = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // check if the user performing the action is domain master
                if (domain == 0)
                {
                    throw new ForbiddenException();
                }
                // call client
                ClientsManager.ConditionalAccessClient().CancelSubscriptionRenewal(groupId, (int)domain, subscriptionId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Gets all the entitled media items for a household
        /// </summary>        
        /// <param name="filter">Request filter</param>
        /// <remarks></remarks>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaEntitlementListResponse ListOldStandard(KalturaEntitlementsFilter filter)
        {
            List<KalturaEntitlement> response = new List<KalturaEntitlement>();

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter cannot be null");
            }

            try
            {
                // call client
                switch (filter.By)
                {
                    case KalturaEntityReferenceBy.user:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetUserEntitlements(groupId, KS.GetFromRequest().UserId, filter.EntitlementType);
                        }
                        break;
                    case KalturaEntityReferenceBy.household:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetDomainEntitlements(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), filter.EntitlementType);
                        }
                        break;
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "unknown reference type");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaEntitlementListResponse() { Entitlements = response, TotalCount = response.Count };
        }

        /// <summary>
        /// Gets all the entitled media items for a household
        /// </summary>        
        /// <param name="filter">Request filter</param>
        /// <param name="pager">Request pager</param>1
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaEntitlementListResponse List(KalturaEntitlementFilter filter, KalturaFilterPager pager = null)
        {
            List<KalturaEntitlement> response = new List<KalturaEntitlement>();

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter cannot be null");
            }

            try
            {
                // call client
                switch (filter.EntityReferenceEqual)
                {
                    case KalturaEntityReferenceBy.user:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetUserEntitlements(groupId, KS.GetFromRequest().UserId, filter.EntitlementTypeEqual,
                                filter.getIsExpiredEqual(), pager.getPageSize(), pager.getPageIndex(), filter.OrderBy);
                        }
                        break;
                    case KalturaEntityReferenceBy.household:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetDomainEntitlements(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), filter.EntitlementTypeEqual,
                                filter.getIsExpiredEqual(), pager.getPageSize(), pager.getPageIndex(), filter.OrderBy);
                        }
                        break;
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "unknown reference type");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaEntitlementListResponse() { Entitlements = response, TotalCount = response.Count };
        }

        /// <summary>
        /// Retrieve the household’s expired entitlements – PPV and subscriptions. Response is ordered by expiry date
        /// </summary>        
        /// <param name="filter">Request filter</param>
        /// <param name="pager">Paging the request</param>
        /// <remarks></remarks>
        [Route("listExpired"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaEntitlementListResponse ListExpired(KalturaEntitlementsFilter filter, KalturaFilterPager pager = null)
        {
            List<KalturaEntitlement> response = new List<KalturaEntitlement>();

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter cannot be null");
            }
            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                // call client
                switch (filter.By)
                {
                    case KalturaEntityReferenceBy.user:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetUserEntitlements(groupId, KS.GetFromRequest().UserId, filter.EntitlementType, true, pager.getPageSize(), pager.getPageIndex());
                        }
                        break;
                    case KalturaEntityReferenceBy.household:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetDomainEntitlements(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), filter.EntitlementType, true, pager.getPageSize(), pager.getPageIndex());
                        }
                        break;
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "unknown reference type");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaEntitlementListResponse() { Entitlements = response, TotalCount = response.Count };
        }

        /// <summary>        
        /// Grant household for an entitlement for a PPV or Subscription.
        /// </summary>
        /// <param name="contentId">Identifier for the content. Relevant only if Product type = PPV</param>
        /// <param name="productId">Identifier for the product package from which this content is offered  </param>
        /// <param name="productType">Product package type. Possible values: PPV, Subscription, Collection</param>
        /// <param name="history">Controls if the new entitlements grant will appear in the user’s history. True – will add a history entry. False (or if ommited) – no history entry will be added</param>
        /// <remarks>Possible status codes: 
        /// User not in domain = 1005, User does not exist = 2000, User suspended = 2001, PPV purchased = 3021, Free = 3022, For purchase subscription only = 3023,
        /// Subscription purchased = 3024, Not for purchase = 3025, Collection purchased = 3027, UnKnown PPV module = 6001
        ///,       
        /// </remarks>
        [Route("grant"), HttpPost]
        [ApiAuthorize]
        [OldStandard("productId", "product_id")]
        [OldStandard("productType", "product_type")]
        [OldStandard("contentId", "content_id")]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public bool Grant(int productId, KalturaTransactionType productType, bool history, int contentId = 0)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            long domainID = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GrantEntitlements(groupId, userId, domainID, contentId, productId,
                    productType, history, string.Empty);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// (Deprecated - use Transaction.purchase) Charges a user for subscription or PPV      
        /// </summary>
        /// <remarks>
        /// Possible status codes: 
        /// Price not correct = 6000, Unknown PPV module = 6001, Expired credit card = 6002, Cellular permissions error (for cellular charge) = 6003, Unknown billing provider = 6004
        /// </remarks>
        /// <param name="udid">Device UDID</param>
        /// <param name="itemId">The identifier of the item to buy, can be PPV identifier or subscription identifier</param>
        /// <param name="fileId">File identifier</param>
        /// <param name="isSubscription">True for buying subscription, false for buying ppv</param>        
        /// <param name="price">Price</param>
        /// <param name="currency">Currency</param>
        /// <param name="couponCode">Coupon code</param>
        /// <param name="extraParams">Custom extra parameters (changes between different billing providers)</param>
        /// <param name="encryptedCvv">Encrypted credit card CVV</param>
        [Route("buy"), HttpPost]
        [Obsolete]
        [ApiAuthorize]
        [OldStandard("itemId", "item_id")]
        [OldStandard("isSubscription", "is_subscription")]
        [OldStandard("couponCode", "coupon_code")]
        [OldStandard("extraParams", "extra_params")]
        [OldStandard("encryptedCvv", "encrypted_cvv")]
        [OldStandard("fileId", "file_id")]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public KalturaBillingResponse Buy(string itemId, bool isSubscription, double price, string currency, string couponCode, string extraParams,
            string encryptedCvv, int fileId = 0, string udid = null)
        {
            KalturaBillingResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                if (isSubscription)
                {
                    // call client
                    response = ClientsManager.ConditionalAccessClient().ChargeUserForSubscription(groupId, userId, price, currency, itemId, couponCode,
                        extraParams, udid, encryptedCvv);
                }
                else
                {
                    // call client
                    response = ClientsManager.ConditionalAccessClient().ChargeUserForMediaFile(groupId, userId, price, currency, fileId, itemId, couponCode,
                        extraParams, udid, encryptedCvv);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Reconcile the user household's entitlements with an external entitlements source. This request is frequency protected to avoid too frequent calls per household. 
        /// </summary>
        /// /// <remarks>
        /// Possible status codes: 
        /// User not in household = 1005, User does not exist = 2000, User suspended = 2001, Reconciliation too frequent = 3029, Adapter application failure = 6012
        /// </remarks>
        [Route("externalReconcile"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public bool ExternalReconcile()
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            long domainID = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                response = ClientsManager.ConditionalAccessClient().ReconcileEntitlements(groupId, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


    }
}