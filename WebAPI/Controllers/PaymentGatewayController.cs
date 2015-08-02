using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Billing;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("payment_gateway")]
    public class PaymentGatewayController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns payment gateway for household
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, UserDoesNotExist = 2000, UserNotInDomain = 1005, UserWithNoDomain = 2024, UserSuspended = 2001, DomainNotExists = 1006, Household Not Set To Payment Gateway = 6027 
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="household_id">Household Identifier</param>
        /// <param name="user_id">User Identifier</param>
        [Route("list"), HttpPost]
        public Models.Billing.KalturaPaymentGWResponse List([FromUri] string partner_id, [FromUri] long household_id, [FromUri] string user_id)
        {
            Models.Billing.KalturaPaymentGWResponse response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetHouseholdPaymentGateways(groupId, user_id, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new payment gateway for household
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, User Does Not Exist = 2000, User Not In Domain = 1005, User With No Domain = 2024, User Suspended = 2001, Domain Not Exists = 1006, 
        /// Payment Gateway ID Missing = 6005, Error Saving Payment Gateway Household = 6017, Household Already Set To Payment Gateway = 6024
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="household_id">Household Identifier</param>
        /// <param name="user_id">User Identifier</param>        
        [Route("set"), HttpPost]
        public bool Set([FromUri] string partner_id, [FromUri] int payment_gateway_id, [FromUri] long household_id, [FromUri] string user_id, [FromUri] string charge_id)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().SetHouseHoldPaymentGateway(groupId, payment_gateway_id, user_id, household_id, charge_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete payment gateway from household
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, UserDoesNotExist = 2000, UserNotInDomain = 1005, UserWithNoDomain = 2024, UserSuspended = 2001, DomainNotExists = 1006
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="user_id">User Identifier</param>
        [Route("delete"), HttpPost]
        public bool Delete([FromUri] string partner_id, [FromUri] int payment_gateway_id, [FromUri] string household_id, [FromUri] string user_id)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().DeletePaymentGWHouseHold(groupId, payment_gateway_id, user_id, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}