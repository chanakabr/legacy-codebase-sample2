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
    [OldStandard("listOldStandard", "list")]
    public class EntitlementController : ApiController
    {
        /// <summary>
        /// Immediately cancel a subscription, PPV or collection. Cancel is possible only if within cancellation window and content not already consumed
        /// </summary>                
        /// <param name="asset_id">The mediaFileID to cancel</param>        
        /// <param name="transaction_type">The transaction type for the cancelation</param>
        /// <remarks>Possible status codes: 
        /// Household suspended = 1009, Invalid purchase = 3000, Cancellation window period expired = 3001, Content already consumed = 3005</remarks>
        [Route("cancel"), HttpPost]
        [ApiAuthorize]
        public bool Cancel(int asset_id, KalturaTransactionType transaction_type)
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

                if (asset_id == 0)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id not valid");
                }

                // call client
                response = ClientsManager.ConditionalAccessClient().CancelServiceNow(groupId, (int)domain, asset_id, transaction_type, false);
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
        /// <param name="asset_id">The mediaFileID to cancel</param>        
        /// <param name="transaction_type">The transaction type for the cancelation</param>
        /// <remarks>Possible status codes: 
        /// Household suspended = 1009, Invalid purchase = 3000</remarks>
        [Route("forceCancel"), HttpPost]
        [ApiAuthorize]
        public bool ForceCancel(int asset_id, KalturaTransactionType transaction_type)
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

                if (asset_id == 0)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id not valid");
                }

                // call client
                response = ClientsManager.ConditionalAccessClient().CancelServiceNow(groupId, (int)domain, asset_id, transaction_type, true);
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
        /// <param name="subscription_id">Subscription Code</param>
        /// <remarks>Possible status codes: 
        /// Household suspended = 1009, Invalid purchase = 3000, SubscriptionNotRenewable = 300</remarks>
        [Route("cancelRenewal"), HttpPost]
        [ApiAuthorize]
        public void CancelRenewal(string subscription_id)
        {
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(subscription_id))
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
                ClientsManager.ConditionalAccessClient().CancelSubscriptionRenewal(groupId, (int)domain, subscription_id);
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
        [Route("listOldStandart"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaEntitlementListResponse ListOldStandart(KalturaEntitlementsFilter filter)
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
                                false, pager.getPageSize(), pager.getPageIndex(), filter.OrderBy);
                        }
                        break;
                    case KalturaEntityReferenceBy.household:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetDomainEntitlements(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), filter.EntitlementTypeEqual,
                                false, pager.getPageSize(), pager.getPageIndex(), filter.OrderBy);
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
        /// <param name="content_id">Identifier for the content. Relevant only if Product type = PPV</param>
        /// <param name="product_id">Identifier for the product package from which this content is offered  </param>
        /// <param name="product_type">Product package type. Possible values: PPV, Subscription, Collection</param>
        /// <param name="history">Controls if the new entitlements grant will appear in the user’s history. True – will add a history entry. False (or if ommited) – no history entry will be added</param>
        /// <remarks>Possible status codes: 
        /// User not in domain = 1005, User does not exist = 2000, User suspended = 2001, PPV purchased = 3021, Free = 3022, For purchase subscription only = 3023,
        /// Subscription purchased = 3024, Not for purchase = 3025, Collection purchased = 3027, UnKnown PPV module = 6001
        ///,       
        /// </remarks>
        [Route("grant"), HttpPost]
        [ApiAuthorize]
        public bool Grant(int product_id, KalturaTransactionType product_type, bool history, int content_id = 0)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            long domainID = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GrantEntitlements(groupId, userId, domainID, content_id, product_id,
                    product_type, history, string.Empty);
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
        /// <param name="item_id">The identifier of the item to buy, can be PPV identifier or subscription identifier</param>
        /// <param name="file_id">File identifier</param>
        /// <param name="is_subscription">True for buying subscription, false for buying ppv</param>        
        /// <param name="price">Price</param>
        /// <param name="currency">Currency</param>
        /// <param name="coupon_code">Coupon code</param>
        /// <param name="extra_params">Custom extra parameters (changes between different billing providers)</param>
        /// <param name="encrypted_cvv">Encrypted credit card CVV</param>
        [Route("buy"), HttpPost]
        [Obsolete]
        [ApiAuthorize]
        public KalturaBillingResponse Buy(string item_id, bool is_subscription, double price, string currency, string coupon_code, string extra_params,
            string encrypted_cvv, int file_id = 0, string udid = null)
        {
            KalturaBillingResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                if (is_subscription)
                {
                    // call client
                    response = ClientsManager.ConditionalAccessClient().ChargeUserForSubscription(groupId, userId, price, currency, item_id, coupon_code,
                        extra_params, udid, encrypted_cvv);
                }
                else
                {
                    // call client
                    response = ClientsManager.ConditionalAccessClient().ChargeUserForMediaFile(groupId, userId, price, currency, file_id, item_id, coupon_code,
                        extra_params, udid, encrypted_cvv);
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