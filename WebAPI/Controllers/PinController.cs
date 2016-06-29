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
        /// Retrieve the parental or purchase PIN that applies for the household or user. Includes specification of where the PIN was defined at – account, household or user  level
        /// </summary>
        /// <param name="type">The PIN type to retrieve</param>
        /// <param name="by">Reference type to filter by</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001, No PIN defined = 5001</remarks>
        /// <returns>The PIN that applies for the user</returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaPinResponse Get(KalturaEntityReferenceBy by, KalturaPinType type)
        {
            KalturaPinResponse pinResponse = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                if (by == KalturaEntityReferenceBy.USER)
                {
                    string userId = KS.GetFromRequest().UserId;

                    if (type == KalturaPinType.PARENTAL)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetUserParentalPIN(groupId, userId, householdId);
                    }
                    else if (type == KalturaPinType.PURCHASE)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetUserPurchasePIN(groupId, userId, householdId); 
                    }
                }
                else if (by == KalturaEntityReferenceBy.HOUSEHOLD)
                {

                    if (type == KalturaPinType.PARENTAL)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetDomainParentalPIN(groupId, householdId);
                    }
                    else if (type == KalturaPinType.PURCHASE)
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
        /// <remarks>Possible status codes: 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="pin">New PIN to set</param>
        /// <returns>Success / Fail</returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(string pin, KalturaEntityReferenceBy by, KalturaPinType type)
        {
            bool success = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (by == KalturaEntityReferenceBy.USER)
                {
                    string userId = KS.GetFromRequest().UserId;

                    if (type == KalturaPinType.PARENTAL)
                    {
                        // call client
                        success = ClientsManager.ApiClient().SetUserParentalPIN(groupId, userId, pin);
                    }
                    else if (type == KalturaPinType.PURCHASE)
                    {
                        // call client
                        success = ClientsManager.ApiClient().SetUserPurchasePIN(groupId, userId, pin);
                    }
                }
                else if (by == KalturaEntityReferenceBy.HOUSEHOLD)
                {
                    int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                    if (type == KalturaPinType.PARENTAL)
                    {
                        // call client
                        success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, householdId, pin);
                    }
                    else if (type == KalturaPinType.PURCHASE)
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
        /// Validate a purchase or parental PIN for a user.
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

                if (type == KalturaPinType.PARENTAL)
                {
                    // call client
                    success = ClientsManager.ApiClient().ValidateParentalPIN(groupId, userId, pin);
                }
                else if (type == KalturaPinType.PURCHASE)
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