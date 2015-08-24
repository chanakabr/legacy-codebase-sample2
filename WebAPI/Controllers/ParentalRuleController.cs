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
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001
        /// </remarks>
        /// <returns>List of parental rules applied to the user</returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaParentalRuleListResponse List(KalturaEntityReferenceByFilter filter)
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
                    // call client
                    response = ClientsManager.ApiClient().GetUserParentalRules(groupId, KS.GetFromRequest().UserId);
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
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001, Invalid rule = 5003</remarks>
        /// <param name="rule_id">Rule Identifier</param>
        /// <returns>Success or failure and reason</returns>
        [Route("enable"), HttpPost]
        [ApiAuthorize]
        public bool Enable(long rule_id, KalturaEntityReferenceBy by)
        {
            bool success = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (by == KalturaEntityReferenceBy.user)
                {
                    // call client
                    success = ClientsManager.ApiClient().SetUserParentalRule(groupId, KS.GetFromRequest().UserId, rule_id, 1);
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
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001, Invalid rule = 5003</remarks>
        /// <param name="rule_id">Rule Identifier</param>
        /// <returns>Success or failure and reason</returns>
        [Route("disable"), HttpPost]
        [ApiAuthorize]
        public bool Disable(long rule_id, KalturaEntityReferenceBy by)
        {
            bool success = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (by == KalturaEntityReferenceBy.user)
                {
                    // call client
                    success = ClientsManager.ApiClient().SetUserParentalRule(groupId, KS.GetFromRequest().UserId, rule_id, 0);
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
    }
}