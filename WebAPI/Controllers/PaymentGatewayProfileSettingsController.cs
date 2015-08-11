using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Billing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/paymentGatewayProfileSettings/action")]
    public class PaymentGatewayProfileSettingsController : ApiController
    {
        /// <summary>
        /// Returns all payment gateway settings for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>       
        [Route("list"), HttpPost]
        public Models.Billing.KalturaPaymentGatewaySettingsResponse List(string partner_id)
        {
            Models.Billing.KalturaPaymentGatewaySettingsResponse response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetPaymentGatewateSettings(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete payment gateway specific settings by settings keys 
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param>
        /// <param name="settings">Dictionary (string,string) for partner specific settings</param>
        [Route("delete"), HttpPost]
        public bool Delete(string partner_id, int payment_gateway_id, SerializableDictionary<string, string> settings)
        {
            bool response = false;
            
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().DeletePaymentGatewaySettings(groupId, payment_gateway_id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new settings for payment gateway for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, Payment gateway id required = 6005, Payment gateway params required = 6006
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="settings">Dictionary (string,string) for partner specific settings </param>
        [Route("add"), HttpPost]
        public bool Add(string partner_id, int payment_gateway_id, SerializableDictionary<string, string> settings)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().InsertPaymentGatewaySettings(groupId, payment_gateway_id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update settings for payment gateway 
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="settings">Dictionary (string,string) for partner specific settings </param>
        [Route("update"), HttpPost]
        public bool Update(string partner_id, int payment_gateway_id, SerializableDictionary<string, string> settings)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().SetPaymentGatewaySettings(groupId, payment_gateway_id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}