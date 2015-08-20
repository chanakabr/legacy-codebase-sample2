using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/userPurchaseSettings/action")]
    public class UserPurchaseSettingsController : ApiController
    {
        /// <summary>
        /// Retrieve the purchase settings that applies for the user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <returns>The PIN that applies for the user</returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaPurchaseSettingsResponse Get()
        {
            KalturaPurchaseSettingsResponse purchaseResponse = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                purchaseResponse = ClientsManager.ApiClient().GetUserPurchaseSettings(groupId, KS.GetFromRequest().UserId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return purchaseResponse;
        }

        /// <summary>
        /// Set the purchase settings that applies for the user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="setting">New settings to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(int setting)
        {
            bool success = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetUserPurchaseSettings(groupId, KS.GetFromRequest().UserId, setting);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }
    }
}