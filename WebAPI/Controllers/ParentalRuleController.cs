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
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/parentalRule/action")]
    public class ParentalRuleController : ApiController 
    {
        /// <summary>
        /// Return the parental rules that applies to the user or a household. Can include rules that have been associated in account, household, or user level.        
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <remarks>
        /// Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001
        /// </remarks>
        /// <returns>List of parental rules applied to the user</returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaParentalRuleListResponse List(KalturaRuleFilter filter)
        {
            List<KalturaParentalRule> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter cannot be null");
            }

            try
            {
                if (filter.By == KalturaEntityReferenceBy.user)
                {
                    if (string.IsNullOrEmpty(filter.HouseholdUserId))
                    {
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "household_user_id cannot be empty when getting by user");
                    }

                    // check if the household_user_id belongs to the callers (ks) household 
                    AuthorizationManager.CheckAdditionalUserId(filter.HouseholdUserId, groupId);

                    // call client
                    response = ClientsManager.ApiClient().GetUserParentalRules(groupId, filter.HouseholdUserId);
                }
                else if (filter.By == KalturaEntityReferenceBy.household)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetDomainParentalRules(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId));
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaParentalRuleListResponse() { ParentalRule = response, TotalCount = response.Count };
        }

        /// <summary>
        /// Enabled a parental rule for a specific user or household    
        /// </summary>
        /// <param name="by">Reference type to filter by</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001, Invalid rule = 5003</remarks>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="household_user_id">The identifier of the household user for whom to enable the rule (if enabling by user)</param> 
        /// <returns>Success or failure and reason</returns>
        [Route("enable"), HttpPost]
        [ApiAuthorize]
        public bool Enable(long rule_id, KalturaEntityReferenceBy by, string household_user_id = null)
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
                    success = ClientsManager.ApiClient().SetUserParentalRule(groupId, household_user_id, rule_id, 1);
                }
                else if (by == KalturaEntityReferenceBy.household)
                {
                    // call client
                    success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), rule_id, 1);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Disables a parental rule for a specific user or household     
        /// </summary>
        /// <param name="by">Reference type to filter by</param>
        /// <param name="household_user_id">The identifier of the household user for whom to disable the rule (if disabling by user)</param> 
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001, Invalid rule = 5003</remarks>
        /// <param name="rule_id">Rule Identifier</param>
        /// <returns>Success or failure and reason</returns>
        [Route("disable"), HttpPost]
        [ApiAuthorize]
        public bool Disable(long rule_id, KalturaEntityReferenceBy by, string household_user_id = null)
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
                    success = ClientsManager.ApiClient().SetUserParentalRule(groupId, household_user_id, rule_id, 0);
                }
                else if (by == KalturaEntityReferenceBy.household)
                {
                    // call client
                    success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), rule_id, 0);
                }
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Disables the partner's default rule for this household        
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001
        /// </remarks>
        /// <param name="by">Reference type to filter by</param>
        /// <param name="household_user_id">The identifier of the household user for whom to disable the default rule (if disabling by user)</</param> 
        /// <returns>Success / fail</returns>
        [Route("disableDefault"), HttpPost]
        [ApiAuthorize]
        public bool DisableDefault(KalturaEntityReferenceBy by, string household_user_id = null)
        {
            bool success = false;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (by == KalturaEntityReferenceBy.household)
                {
                    // call client
                    success = ClientsManager.ApiClient().DisableDomainDefaultParentalRule(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId));
                }
                else if (by == KalturaEntityReferenceBy.user)
                {
                    if (string.IsNullOrEmpty(household_user_id))
                    {
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "household_user_id cannot be empty when getting by user");
                    }

                    // check if the household_user_id belongs to the callers (ks) household 
                    AuthorizationManager.CheckAdditionalUserId(household_user_id, groupId);

                    // call client
                    success = ClientsManager.ApiClient().DisableUserDefaultParentalRule(groupId, household_user_id);
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