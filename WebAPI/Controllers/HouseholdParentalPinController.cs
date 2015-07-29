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
    [RoutePrefix("household_parental_pin")]
    public class HouseholdParentalPinController : ApiController
    {
        /// <summary>
        /// Retrieve the parental PIN that applies for the household.
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006</remarks>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <returns>The PIN that applies for the household</returns>
        [Route("get"), HttpPost]
        public KalturaPinResponse Get([FromUri] string partner_id, [FromUri] int household_id)
        {
            KalturaPinResponse pinResponse = null;
            
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                pinResponse = ClientsManager.ApiClient().GetDomainParentalPIN(groupId, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return pinResponse;
        }

        /// <summary>
        /// Set the parental PIN that applies for the household.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="pin">New PIN to set</param>
        /// <returns>Success / Fail</returns>
        [Route("update"), HttpPost]
        public bool Update([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string pin)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, household_id, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }
    }
}