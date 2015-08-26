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
    [RoutePrefix("_service/pin/action")]
    public class PinController : ApiController
    {
        /// <summary>
        /// Retrieve the parental or purchase PIN that applies for the user or the household.        
        /// </summary>
        /// <param name="type">The PIN type to retrieve</param>
        /// <param name="by">Reference type to filter by</param>
        /// <param name="household_user_id">The identifier of the household user for whom to get the PIN (if getting by user)</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001, No PIN defined = 5001</remarks>
        /// <returns>The PIN that applies for the user</returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaPinResponse Get(KalturaEntityReferenceBy by, KalturaPinType type, string household_user_id = null)
        {
            KalturaPinResponse pinResponse = null;

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
                    AuthorizationManager.ChackAdditionalUserId(household_user_id, groupId);

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetUserParentalPIN(groupId, household_user_id);
                    }
                    else if (type == KalturaPinType.purchase)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetUserPurchasePIN(groupId, household_user_id); 
                    }
                }
                else if (by == KalturaEntityReferenceBy.household)
                {
                    int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetDomainParentalPIN(groupId, householdId);
                    }
                    else if (type == KalturaPinType.purchase)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetDomainPurchasePIN(groupId, householdId); 
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return pinResponse;
        }

        /// <summary>
        /// Set the parental or purchase PIN that applies for the user or the household.        
        /// </summary>
        /// <param name="type">The PIN type to retrieve</param>
        /// <param name="by">Reference type to filter by</param>
        /// <param name="household_user_id">The identifier of the household user for whom to update the PIN (if updating by user)</param> 
        /// <remarks>Possible status codes: 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="pin">New PIN to set</param>
        /// <returns>Success / Fail</returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(string pin, KalturaEntityReferenceBy by, KalturaPinType type, string household_user_id = null)
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
                    AuthorizationManager.ChackAdditionalUserId(household_user_id, groupId);

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        success = ClientsManager.ApiClient().SetUserParentalPIN(groupId, household_user_id, pin);
                    }
                    else if (type == KalturaPinType.purchase)
                    {
                        // call client
                        success = ClientsManager.ApiClient().SetUserPurchasePIN(groupId, household_user_id, pin);
                    }
                }
                else if (by == KalturaEntityReferenceBy.household)
                {
                    int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, householdId, pin);
                    }
                    else if (type == KalturaPinType.purchase)
                    {
                        // call client
                        success = ClientsManager.ApiClient().SetDomainPurchasePIN(groupId, householdId, pin);
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Validate that a given parental or purchase PIN for a user is valid.        
        /// </summary>
        /// <param name="type">The PIN type to retrieve</param>
        /// <remarks>Possible status codes: 
        /// No PIN defined = 5001, PIN mismatch = 5002, User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="pin">PIN to validate</param>
        /// <returns>Success / fail</returns>
        [Route("validate"), HttpPost]
        [ApiAuthorize]
        public bool Validate(string pin, KalturaPinType type)
        {
            bool success = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            // parameters validation
            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "pin cannot be empty");
            }

            try
            {
                string userId = KS.GetFromRequest().UserId;

                if (type == KalturaPinType.parental)
                {
                    // call client
                    success = ClientsManager.ApiClient().ValidateParentalPIN(groupId, userId, pin);
                }
                else if (type == KalturaPinType.purchase)
                {
                    // call client
                    success = ClientsManager.ApiClient().ValidatePurchasePIN(groupId, userId, pin);
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