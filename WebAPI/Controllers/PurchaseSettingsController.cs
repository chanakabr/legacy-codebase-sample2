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
        /// Retrieve the purchase settings that applies for the household or a user        
        /// </summary>
        /// <param name="by">Reference type to filter by</param>
        /// <param name="household_user_id">The identifier of the household user for whom to get the setting (if getting by user)</param> 
        /// <remarks>
        /// Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001
        /// </remarks>
        /// <returns>The purchase settings that apply for the user</returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaPurchaseSettingsResponse Get(KalturaEntityReferenceBy by, string household_user_id = null)
        {
            KalturaPurchaseSettingsResponse purchaseResponse = null;

            int groupId = KS.GetFromRequest().GroupId;                       

            try
            {
                if (by == KalturaEntityReferenceBy.user)
                {
                    if (string.IsNullOrEmpty(household_user_id))
                    {
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "household_user_id cannot be empty when getting by user");
                    }

                    // check if the household_user_id belongs to the callers (ks) household 
                    AuthorizationManager.CheckAdditionalUserId(household_user_id, groupId);

                    // call client
                    purchaseResponse = ClientsManager.ApiClient().GetUserPurchaseSettings(groupId, household_user_id);
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
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="setting">New settings to apply</param>
        /// <param name="by">Reference type to filter by</param>
        /// <param name="household_user_id">The identifier of the household user for whom to update the setting (if updating by user)</param> 
        /// <returns>Success / Fail</returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(int setting, KalturaEntityReferenceBy by, string household_user_id = null)
        {
            bool success = false;

            int groupId = KS.GetFromRequest().GroupId;                       

            try
            {
                if (by == KalturaEntityReferenceBy.user)
                {
                    if (string.IsNullOrEmpty(household_user_id))
                    {
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "household_user_id cannot be empty when getting by user");
                    }

                    // check if the household_user_id belongs to the callers (ks) household 
                    AuthorizationManager.CheckAdditionalUserId(household_user_id, groupId);

                    // call client
                    success = ClientsManager.ApiClient().SetUserPurchaseSettings(groupId, household_user_id, setting);
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