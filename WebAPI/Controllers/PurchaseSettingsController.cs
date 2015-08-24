using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/purchaseSettings/action")]
    public class PurchaseSettingsController : ApiController
    {
        /// <summary>
        /// Retrieve the purchase settings that applies for the household or a user        
        /// </summary>
        /// /// <param name="by">Reference type to filter by</param>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001
        /// </remarks>
        /// <returns>The purchase settings that apply for the user</returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaPurchaseSettingsResponse Get(KalturaEntityReferenceBy by)
        {
            KalturaPurchaseSettingsResponse purchaseResponse = null;

            int groupId = KS.GetFromRequest().GroupId;                       

            try
            {
                if (by == KalturaEntityReferenceBy.user)
                {
                    // call client
                    purchaseResponse = ClientsManager.ApiClient().GetUserPurchaseSettings(groupId, KS.GetFromRequest().UserId);
                }
                else if (by == KalturaEntityReferenceBy.household)
                {
                    // call client
                    purchaseResponse = ClientsManager.ApiClient().GetDomainPurchaseSettings(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId));
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return purchaseResponse;
        }

        /// <summary>
        /// Set the purchase settings that applies for the household.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="setting">New settings to apply</param>
        /// <param name="by">Reference type to filter by</param>
        /// <returns>Success / Fail</returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(int setting, KalturaEntityReferenceBy by)
        {
            bool success = false;

            int groupId = KS.GetFromRequest().GroupId;                       

            try
            {
                if (by == KalturaEntityReferenceBy.user)
                {
                    // call client
                    success = ClientsManager.ApiClient().SetUserPurchaseSettings(groupId, KS.GetFromRequest().UserId, setting);
                }
                else if (by == KalturaEntityReferenceBy.household)
                {
                    // call client
                    success = ClientsManager.ApiClient().SetDomainPurchaseSettings(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), setting);
                }
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }
    }
}