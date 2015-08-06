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
    [RoutePrefix("_service/householdPurchasePin/action")]
    public class HouseholdPurchasePinController : ApiController
    {
        /// <summary>
        /// Retrieve the purchase PIN that applies for the household.        
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// 5001 = No PIN defined, Household does not exist = 1006
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <returns>The PIN that applies for the household</returns>
        [Route("get"), HttpPost]
        public KalturaPinResponse Get(string partner_id, int household_id)
        {
            KalturaPinResponse pinResponse = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                var response = ClientsManager.ApiClient().GetDomainPurchasePIN(groupId, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return pinResponse;
        }

        /// <summary>
        /// Set the purchase PIN that applies for the household.        
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006
        /// </remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="pin">New PIN to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("update"), HttpPost]
        public bool Update(string partner_id, int household_id, string pin)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainPurchasePIN(groupId, household_id, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }
    }
}