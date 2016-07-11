using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/purchaseSettings/action")]
    [OldStandard("getOldStandard", "get")]
    [OldStandard("updateOldStandard", "update")]
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
        [ValidationException(SchemaValidationType.ACTION_ARGUMENTS)]
        public KalturaPurchaseSettings Get(KalturaEntityReferenceBy by)
        {
            KalturaPurchaseSettings purchaseResponse = null;

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
        /// Retrieve the purchase settings.
        /// Includes specification of where these settings were defined – account, household or user        
        /// </summary>
        /// <param name="by">Reference type to filter by</param>
        /// <remarks>
        /// Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001
        /// </remarks>
        /// <returns>The purchase settings that apply for the user</returns>
        [Route("getOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaPurchaseSettingsResponse GetOldStandard(KalturaEntityReferenceBy by)
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
                    purchaseResponse = ClientsManager.ApiClient().GetUserPurchaseSettingsOldStandard(groupId, userId, householdId);
                }
                else if (by == KalturaEntityReferenceBy.household)
                {
                    // call client
                    purchaseResponse = ClientsManager.ApiClient().GetDomainPurchaseSettingsOldStandard(groupId, householdId);
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
        /// <param name="entityReference">Reference type to filter by</param>
        /// <param name="settings">New settings to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_ARGUMENTS)]
        public KalturaPurchaseSettings Update(KalturaEntityReferenceBy entityReference, KalturaPurchaseSettings settings)
        {
            KalturaPurchaseSettings response = null;

            int groupId = KS.GetFromRequest().GroupId;                       

            try
            {
                if (entityReference == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;   

                    // call client
                    response = ClientsManager.ApiClient().SetUserPurchaseSettings(groupId, userId, (int)settings.Permission);
                }
                else if (entityReference == KalturaEntityReferenceBy.household)
                {
                    // call client
                    response = ClientsManager.ApiClient().SetDomainPurchaseSettings(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), (int)settings.Permission);
                }
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
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
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool UpdateOldStandard(int setting, KalturaEntityReferenceBy by)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (by == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;

                    // call client
                    ClientsManager.ApiClient().SetUserPurchaseSettings(groupId, userId, setting);
                }
                else if (by == KalturaEntityReferenceBy.household)
                {
                    // call client
                    ClientsManager.ApiClient().SetDomainPurchaseSettings(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), setting);
                }

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }
    }
}