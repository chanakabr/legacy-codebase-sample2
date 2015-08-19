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
    [RoutePrefix("_service/userParentalRule/action")]
    public class UserParentalRuleController : ApiController 
    {
        /// <summary>
        /// Return the parental rules that applies to the user. Can include rules that have been associated in account, household, or user level.        
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001
        /// </remarks>
        /// <returns>List of parental rules applied to the user</returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaParentalRuleListResponse List()
        {
            List<KalturaParentalRule> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetUserParentalRules(groupId, KS.GetFromRequest().UserId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaParentalRuleListResponse() { ParentalRule = response, TotalCount = response.Count };
        }

        /// <summary>
        /// Enabled a parental rule for a specific user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001, Invalid rule = 5003</remarks>
        /// <param name="rule_id">Rule Identifier</param>
        /// <returns>Success or failure and reason</returns>
        [Route("enable"), HttpPost]
        [ApiAuthorize]
        public bool Enable(long rule_id)
        {
            bool success = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetUserParentalRule(groupId, KS.GetFromRequest().UserId, rule_id, 1);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Disables a parental rule for a specific user.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001, Invalid rule = 5003</remarks>
        /// <param name="rule_id">Rule Identifier</param>
        /// <returns>Success or failure and reason</returns>
        [Route("disable"), HttpPost]
        [ApiAuthorize]
        public bool Disable(long rule_id)
        {
            bool success = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetUserParentalRule(groupId, KS.GetFromRequest().UserId, rule_id, 0);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }
    }
}