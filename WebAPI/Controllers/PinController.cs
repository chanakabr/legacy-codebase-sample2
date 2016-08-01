using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/pin/action")]
    [OldStandard("getOldStandard", "get")]
    [OldStandard("updateOldStandard", "update")]
    public class PinController : ApiController
    {
        /// <summary>
        /// Retrieve the parental or purchase PIN that applies for the household or user. Includes specification of where the PIN was defined at – account, household or user  level
        /// </summary>
        /// <param name="type">The PIN type to retrieve</param>
        /// <param name="by">Reference type to filter by</param>
        /// <param name="ruleId">Rule ID - for PIN per rule (MediaCorp): BEO-1923</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001, No PIN defined = 5001</remarks>
        /// <returns>The PIN that applies for the user</returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public KalturaPin Get(KalturaEntityReferenceBy by, KalturaPinType type, int? ruleId = null)
        {
            KalturaPin pinResponse = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                if (by == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetUserParentalPIN(groupId, userId, householdId, ruleId);
                    }
                    else if (type == KalturaPinType.purchase)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetUserPurchasePIN(groupId, userId, householdId);
                    }
                }
                else if (by == KalturaEntityReferenceBy.household)
                {

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetDomainParentalPIN(groupId, householdId, ruleId);
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
        /// <param name="ruleId">Rule ID - for PIN per rule (MediaCorp): BEO-1923</param>
        /// <remarks>Possible status codes: 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="pin">PIN to set</param>
        /// <returns>The PIN</returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public KalturaPin Update(KalturaEntityReferenceBy by, KalturaPinType type, KalturaPin pin, int? ruleId = null)
        {
            KalturaPin response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (by == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        response = ClientsManager.ApiClient().SetUserParentalPIN(groupId, userId, pin.PIN, ruleId);
                    }
                    else if (type == KalturaPinType.purchase)
                    {
                        // call client
                        response = ClientsManager.ApiClient().SetUserPurchasePIN(groupId, userId, pin.PIN);
                    }
                }
                else if (by == KalturaEntityReferenceBy.household)
                {
                    int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        response = ClientsManager.ApiClient().SetDomainParentalPIN(groupId, householdId, pin.PIN, ruleId);
                    }
                    else if (type == KalturaPinType.purchase)
                    {
                        // call client
                        response = ClientsManager.ApiClient().SetDomainPurchasePIN(groupId, householdId, pin.PIN);
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Retrieve the parental or purchase PIN that applies for the household or user. Includes specification of where the PIN was defined at – account, household or user  level
        /// </summary>
        /// <param name="type">The PIN type to retrieve</param>
        /// <param name="by">Reference type to filter by</param>
        /// <param name="ruleId">Rule ID - for PIN per rule (MediaCorp): BEO-1923</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001, No PIN defined = 5001</remarks>
        /// <returns>The PIN that applies for the user</returns>
        [Route("getOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaPinResponse GetOldStandard(KalturaEntityReferenceBy by, KalturaPinType type, int? ruleId = null)
        {
            KalturaPinResponse pinResponse = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                if (by == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetUserParentalPINOldStandard(groupId, userId, householdId, ruleId);
                    }
                    else if (type == KalturaPinType.purchase)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetUserPurchasePinOldStandard(groupId, userId, householdId);
                    }
                }
                else if (by == KalturaEntityReferenceBy.household)
                {

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetDomainParentalPinOldStandard(groupId, householdId, ruleId);
                    }
                    else if (type == KalturaPinType.purchase)
                    {
                        // call client
                        pinResponse = ClientsManager.ApiClient().GetDomainPurchasePinOldstandard(groupId, householdId);
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
        /// <param name="ruleId">Rule ID - for PIN per rule (MediaCorp): BEO-1923</param>
        /// <remarks>Possible status codes: 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="pin">New PIN to set</param>
        /// <returns>Success / Fail</returns>
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool UpdateOldStandard(string pin, KalturaEntityReferenceBy by, KalturaPinType type, int? ruleId = null)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (by == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        ClientsManager.ApiClient().SetUserParentalPIN(groupId, userId, pin, ruleId);
                    }
                    else if (type == KalturaPinType.purchase)
                    {
                        // call client
                        ClientsManager.ApiClient().SetUserPurchasePIN(groupId, userId, pin);
                    }
                }
                else if (by == KalturaEntityReferenceBy.household)
                {
                    int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                    if (type == KalturaPinType.parental)
                    {
                        // call client
                        ClientsManager.ApiClient().SetDomainParentalPIN(groupId, householdId, pin, ruleId);
                    }
                    else if (type == KalturaPinType.purchase)
                    {
                        // call client
                        ClientsManager.ApiClient().SetDomainPurchasePIN(groupId, householdId, pin);
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Validate a purchase or parental PIN for a user.
        /// </summary>
        /// <param name="type">The PIN type to retrieve</param>
        /// <param name="ruleId">Rule ID - for PIN per rule (MediaCorp): BEO-1923</param>
        /// <remarks>Possible status codes: 
        /// No PIN defined = 5001, PIN mismatch = 5002, User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <param name="pin">PIN to validate</param>
        /// <returns>Success / fail</returns>
        [Route("validate"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool Validate(string pin, KalturaPinType type, int? ruleId = null)
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
                    success = ClientsManager.ApiClient().ValidateParentalPIN(groupId, userId, pin, ruleId);
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