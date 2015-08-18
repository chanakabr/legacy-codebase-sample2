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
    [RoutePrefix("_service/entitlement/action")]
    public class EntitlementController : ApiController
    {
        ///// <summary>
        ///// Gets list of Entitlement (subscriptions) by a given user.    
        ///// </summary>        
        ///// <param name="partner_id">Partner Identifier</param>
        ///// <param name="user_id">User Id</param>
        ///// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        //[Route("list"), HttpPost]
        //public KalturaEntitlementsList List(string partner_id, string user_id)
        //{
        //    List<KalturaEntitlement> response = new List<KalturaEntitlement>();

        //    int groupId = int.Parse(partner_id);

        //    try
        //    {
        //        // call client
        //        response = ClientsManager.ConditionalAccessClient().GetUserSubscriptions(groupId, user_id);
        //    }
        //    catch (ClientException ex)
        //    {
        //        ErrorUtils.HandleClientException(ex);
        //    }

        //    return new KalturaEntitlementsList() { Entitlements = response };
        //}

        /// <summary>
        /// Immediately cancel a household subscription or PPV or collection 
        /// Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided.        
        /// </summary>        
        /// <param name="household_id">Household identifier</param>
        /// <param name="asset_id">Asset identifier to cancel</param>        
        /// <param name="transaction_type">The transaction type for the cancelation</param>
        /// <param name="is_force">If 'true', cancels the service regardless of whether the service was used or not</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, Household suspended = 1009, Invalid purchase = 3000, Cancellation window period expired = 3001, Content already consumed = 3005</remarks>
        [Route("cancel"), HttpPost]
        [ApiAuthorize]
        public bool Cancel(int household_id, int asset_id, KalturaTransactionType transaction_type, bool is_force = false)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            if (asset_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id not valid");
            }
            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().CancelServiceNow(groupId, household_id, asset_id, transaction_type, is_force);
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
        /// <param name="household_id">Household identifier</param>
        /// <param name="sub_id">Subscription Code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        ///  Household does not exist = 1006, Household suspended = 1009, Invalid purchase = 3000, SubscriptionNotRenewable = 300</remarks>
        [Route("cancelRenewal"), HttpPost]
        [ApiAuthorize]
        public void CancelRenewal(int household_id, string sub_id)
        {
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(sub_id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "subscription code not valid");
            }
            try
            {
                // call client
                ClientsManager.ConditionalAccessClient().CancelSubscriptionRenewal(groupId, household_id, sub_id);
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
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaEntitlementListResponse List(KalturaEntitlementsFilter filter)
        {
            List<KalturaEntitlement> response = new List<KalturaEntitlement>();

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter cannot be null");
            }

            if (string.IsNullOrEmpty(filter.Id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id cannot be empty");
            }

            try
            {
                // call client
                switch (filter.By)
                {
                    case KalturaReferenceType.user:
                        response = ClientsManager.ConditionalAccessClient().GetUserEntitlements(groupId, filter.Id, filter.EntitlementType);
                        break;
                    case KalturaReferenceType.household:
                        {
                            int householdId;
                            if (int.TryParse(filter.Id, out householdId))
                            {
                                response = ClientsManager.ConditionalAccessClient().GetDomainEntitlements(groupId, householdId, filter.EntitlementType);
                            }
                            else
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "household id must be int");
                            }
                            break;
                        }
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "unknown reference type");
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaEntitlementListResponse() { Entitlements = response, TotalCount = response.Count };
        }

        /// <summary>
        /// Grant entitlements for a household for specific product or subscription. If a subscription is provided – the grant will apply only till the end of the first renewal period.
        /// </summary>
        /// <param name="content_id">Identifier for the content. Relevent only if Product type = PPV</param>
        /// <param name="product_id">Identifier for the product package from which this content is offered  </param>
        /// <param name="product_type">Product package type. Possible values: PPV, Subscription, Collection</param>
        /// <param name="history">Controls if the new entilements grant will appear in the user’s history. True – will add a history entry. False (or if ommited) – no history entry will be added</param>
        /// <remarks>Possible status codes: 
        /// User not in domain = 1005, User does not exist = 2000, User suspended = 2001, PPV purchased = 3021, Free = 3022, For purchase subscription only = 3023,
        /// Subscription purchased = 3024, Not for purchase = 3025, Collection purchased = 3027,
        /// Credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007,
        /// Partner is invalid = 500008</remarks>
        [Route("grant"), HttpPost]
        [ApiAuthorize]
        public void Grant(int content_id, int product_id, KalturaTransactionType product_type, bool history)
        {
            int groupId = KS.GetFromRequest().GroupId;

            // validate user id
            if (KS.GetFromRequest().UserId == "0")
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user_id cannot be empty");
            }

            try
            {
                // call client
                ClientsManager.ConditionalAccessClient().GrantEntitlements(groupId, KS.GetFromRequest().UserId, 0, content_id, product_id,
                    product_type, history, string.Empty);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

        }

    }
}