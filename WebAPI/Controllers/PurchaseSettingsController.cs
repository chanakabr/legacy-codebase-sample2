using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/purchaseSettings/action")]
    public class PurchaseSettingsController : ApiController
    {
        /// <summary>
        /// Retrieve the purchase settings.
        /// Includes specification of where these settings were defined – account, household or user        
        /// </summary>
        /// <param name="by">Reference type to filter by</param>
        /// <remarks>
        /// Possible status codes: 
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
                int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                if (by == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;   

                    // call client
                    purchaseResponse = ClientsManager.ApiClient().GetUserPurchaseSettings(groupId, userId, householdId);
                }
                else if (by == KalturaEntityReferenceBy.household)
                {
                    // call client
                    purchaseResponse = ClientsManager.ApiClient().GetDomainPurchaseSettings(groupId, householdId);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return purchaseResponse;
        }

        /// <summary>
        /// Set a purchase PIN for the household or user        
        /// </summary>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001,
        /// purchase settings type invalid = 5015 </remarks>
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
                    string userId = KS.GetFromRequest().UserId;   

                    // call client
                    success = ClientsManager.ApiClient().SetUserPurchaseSettings(groupId, userId, setting);
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