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
    [RoutePrefix("payment_gateway_profile")]
    public class PaymentGatewayProfileController : ApiController
    {
        /// <summary>
        /// Returns all payment gateways for partner : id + name
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>       
        [Route("payment_gateways"), HttpPost]
        public Models.Billing.KalturaPaymentGWResponse List(string partner_id)
        {
            Models.Billing.KalturaPaymentGWResponse response = null;

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetPaymentGW(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete payment gateway by payment gateway id
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param>
        [Route("payment_gateways/{payment_gateway_id}/delete"), HttpPost]
        public bool Delete(string partner_id, int payment_gateway_id)
        {
            bool response = false;

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

            try
            {
                // call client
                response = ClientsManager.BillingClient().DeletePaymentGW(groupId, payment_gateway_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new payment gateway for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway">Payment GateWay Settings Object</param>
        [Route("payment_gateway/add"), HttpPost]
        public bool Add(string partner_id, KalturaPaymentGW payment_gateway)
        {
            bool response = false;

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

            try
            {
                // call client
                response = ClientsManager.BillingClient().InsertPaymentGW(groupId, payment_gateway);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update payment gateway details
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="name">Payment Gateway Name</param>
        ///<param name="adapter_url">Payment Gateway adapter url</param>
        ///<param name="transact_url">Payment Gateway transact url</param>
        ///<param name="status_url">Payment Gateway status url</param>
        ///<param name="renew_url">Payment Gateway renew url</param>
        ///<param name="is_default">Payment Gateway is default or not </param>
        ///<param name="is_active">Payment Gateway is active or not </param>
        ///<param name="external_identifier">Payment Gateway external identifier</param>
        ///<param name="pending_interval">Payment Gateway pending interval</param>
        ///<param name="pending_retries">Payment Gateway pending retries</param>
        ///<param name="shared_secret">Payment Gateway shared secret</param>
        [Route("payment_gateways/{payment_gateway_id}/update"), HttpPost]
        public bool Update([FromUri] string partner_id, [FromUri] int payment_gateway_id, [FromUri] string name, [FromUri] string adapter_url, [FromUri] string transact_url,
            [FromUri] string status_url, [FromUri] string renew_url, [FromUri] int is_default, [FromUri] int is_active, [FromUri] string external_identifier, [FromUri]  int pending_interval, 
            [FromUri] int pending_retries, [FromUri] string shared_secret)
        {
            bool response = false;

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

            try
            {
                // call client
                response = ClientsManager.BillingClient().SetPaymentGW(groupId, payment_gateway_id, name, adapter_url, transact_url, status_url, renew_url, external_identifier, pending_interval,
                    pending_retries, shared_secret, is_default, is_active);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}