using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Billing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/paymentGatewayProfile/action")]
    public class PaymentGatewayProfileController : ApiController
    {
        /// <summary>
        /// Returns all payment gateways for partner : id + name
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<Models.Billing.KalturaPaymentGatewayBaseProfile> List()
        {
            List<Models.Billing.KalturaPaymentGatewayBaseProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetPaymentGateway(groupId);
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
        /// Possible status codes:       
        ///    Payment gateway identifier is required = 6005, Payment gateway not exist = 6008
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int payment_gateway_id)
        {
            bool response = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().DeletePaymentGateway(groupId, payment_gateway_id);
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
        /// Possible status codes:       
        ///   External identifier is required = 6016, Name is required = 6020, Shared secret is required = 6021, External identifier must be unique = 6040, No payment gateway to insert = 6041
        /// </remarks>
        /// <param name="payment_gateway">Payment Gateway Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(KalturaPaymentGatewayProfile payment_gateway)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().InsertPaymentGateway(groupId, payment_gateway);
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
        /// Possible status codes:      
        ///   Payment gateway identifier is required = 6005, Name is required = 6020, Shared secret is required = 6021, External idntifier missing = 6016, 
        /// External identifier must be unique = 6040            
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="payment_gateway">Payment Gateway Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(int payment_gateway_id, KalturaPaymentGatewayProfile payment_gateway)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().SetPaymentGateway(groupId, payment_gateway_id, payment_gateway);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}