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
    [RoutePrefix("_service/paymentGateway/action")]
    public class PaymentGatewayController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns payment gateway for household
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, User Does Not Exist = 2000, User Not In Domain = 1005, User With No Domain = 2024, User Suspended = 2001, 
        /// Domain Not Exists = 1006, Household Not Set To Payment Gateway = 6027 
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="household_id">Household Identifier</param>
        /// <param name="user_id">User Identifier</param>
        [Route("list"), HttpPost]
        public Models.Billing.KalturaPaymentGatewayResponse List(string partner_id, long household_id, string user_id)
        {
            Models.Billing.KalturaPaymentGatewayResponse response = null;

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
        /// Returns selected payment gateway for household
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, User Does Not Exist = 2000, User Not In Domain = 1005, User With No Domain = 2024, User Suspended = 2001, 
        /// Domain Not Exists = 1006, Household Not Set To Payment Gateway = 6027, Household required = 6044
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="household_id">Household Identifier</param>        
        [Route("get"), HttpPost]
        public Models.Billing.KalturaHouseholdPaymentGatewayResponse Get(string partner_id, long household_id)
        {
            Models.Billing.KalturaHouseholdPaymentGatewayResponse response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetSelectedHouseholdPaymentGateway(groupId, household_id);
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
        /// Payment Gateway ID Missing = 6005, Error Saving Payment Gateway Household = 6017, Household Already Set To Payment Gateway = 6024, Payment Gateway Selection Is Disabled = 6028,
        /// Payment gateway not valid = 6043
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="household_id">Household Identifier</param>
        /// <param name="user_id">User Identifier</param>        
        [Route("set"), HttpPost]
        public bool Set(string partner_id, int payment_gateway_id, long household_id, string user_id)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().SetHouseHoldPaymentGateway(groupId, payment_gateway_id, user_id, household_id);
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
        /// Not found = 500007, Partner is invalid = 500008, User Does Not Exist = 2000, User Not In Domain = 1005, User With No Domain = 2024, User Suspended = 2001, Domain Not Exists = 1006
        /// Payment Gateway Identifier is Missing = 6005, Payment gateway not exist = 6008, Household not set to payment gateway = 6027
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="user_id">User Identifier</param>
        [Route("delete"), HttpPost]
        public bool Delete(string partner_id, int payment_gateway_id, string household_id, string user_id)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().DeleteHouseholdPaymentGateway(groupId, payment_gateway_id, user_id, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}