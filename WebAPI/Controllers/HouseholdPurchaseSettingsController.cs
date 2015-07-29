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
    [RoutePrefix("household_purchase_settings")]
    public class HouseholdPurchaseSettingsController : ApiController
    {
        /// <summary>
        /// Retrieve the purchase settings that applies for the household.        
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006
        /// </remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <returns>The purchase settings that apply for the user</returns>
        [Route("get"), HttpPost]
        public KalturaPurchaseSettingsResponse Get([FromUri] string partner_id, [FromUri] int household_id)
        {
            KalturaPurchaseSettingsResponse purchaseResponse = null;
            
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                purchaseResponse = ClientsManager.ApiClient().GetDomainPurchaseSettings(groupId, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return purchaseResponse;
        }

        /// <summary>
        /// Set the purchase settings that applies for the household.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="setting">New settings to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("update"), HttpPost]
        public bool Update([FromUri] string partner_id, [FromUri] int household_id, [FromUri] int setting)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainPurchaseSettings(groupId, household_id, setting);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }
    }
}