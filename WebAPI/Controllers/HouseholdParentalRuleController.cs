using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("service/householdParentalRule/action")]
    public class HouseholdParentalRuleController : ApiController
    {
        /// <summary>
        /// Return the parental rules that applies to the household. 
        /// Can include rules that have been associated in account or household.
        /// </summary>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <returns>List of parental rules applied to the household</returns>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, Household does not exist = 1006</remarks>
        [Route("list"), HttpPost]
        public KalturaParentalRulesList List([FromUri] string partner_id, [FromUri] int household_id)
        {
            List<KalturaParentalRule> response = null;
            
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetDomainParentalRules(groupId, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaParentalRulesList() { ParentalRules = response };
        }

        /// <summary>
        /// Enabled a parental rule for a specific household.
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, Invalid rule = 5003</remarks>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <returns>Success or failure and reason</returns>
        [Route("enable"), HttpPost]
        public bool Enable([FromUri] string partner_id, [FromUri] int household_id, [FromUri] long rule_id)
        {
            bool success = false;
            
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, household_id, rule_id, 1);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Disables a parental rule for a specific household.        
        /// </summary>        
        /// <param name="household_id">Household Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <returns>Success or failure and reason</returns>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 
        /// Household does not exist = 1006, Invalid rule = 5003</remarks>
        [Route("disable"), HttpPost]
        public bool Disable([FromUri] string partner_id, [FromUri] int household_id, [FromUri] long rule_id)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, household_id, rule_id, 0);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }
    }
}