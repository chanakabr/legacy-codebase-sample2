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

            int groupId = int.Parse(partner_id);

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
    }
}