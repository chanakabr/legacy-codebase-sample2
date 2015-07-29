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
    [RoutePrefix("entitlement")]
    public class EntitlementController : ApiController
    {
        /// <summary>
        /// Gets list of Entitlement (subscriptions) by a given user.    
        /// </summary>        
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User Id</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("{user_id}/subscriptions/permitted"), HttpGet]
        public KalturaEntitlementsList List([FromUri] string partner_id, [FromUri] string user_id)
        {
            List<KalturaEntitlement> response = new List<KalturaEntitlement>();

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GetUserSubscriptions(groupId, user_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaEntitlementsList() { Entitlements = response };
        }

        /// <summary>
        /// Immediately cancel a household subscription or PPV or collection 
        /// Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided.        
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="asset_id">Asset identifier to cancel</param>        
        /// <param name="transaction_type">The transaction type for the cancelation</param>
        /// <param name="is_force">If 'true', cancels the service regardless of whether the service was used or not</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, Household suspended = 1009, Invalid purchase = 3000, Cancellation window period expired = 3001, Content already consumed = 3005</remarks>
        [Route("{household_id}/cancel/{asset_id}"), HttpDelete]
        public bool Cancel([FromUri] string partner_id, [FromUri] int household_id, [FromUri] int asset_id, KalturaTransactionType transaction_type, [FromUri] bool is_force = false)
        {
            bool response = false;

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

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
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="sub_id">Subscription Code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        ///  Household does not exist = 1006, Household suspended = 1009, Invalid purchase = 3000, SubscriptionNotRenewable = 300</remarks>
        [Route("{household_id}/subscriptions/{sub_id}/renewal"), HttpDelete]
        public void CancelRenewal([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string sub_id)
        {
            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

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
    }
}