using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.Billing;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/paymentGatewayProfile/action")]
    [OldStandard("listOldStandard", "list")]
    [OldStandard("addOldStandard", "add")]
    [OldStandard("updateOldStandard", "update")]
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
        public KalturaPaymentGatewayProfileListResponse List()
        {
            KalturaPaymentGatewayProfileListResponse response = new KalturaPaymentGatewayProfileListResponse();

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response.PaymentGatewayProfiles = ClientsManager.BillingClient().GetPaymentGateway(groupId);
                response.TotalCount = response.PaymentGatewayProfiles.Count;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns all payment gateways for partner : id + name
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<Models.Billing.KalturaPaymentGatewayProfile> ListOldStandard()
        {
            List<Models.Billing.KalturaPaymentGatewayProfile> response = null;

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
        /// Payment gateway not exist = 6008
        /// </remarks>
        /// <param name="paymentGatewayId">Payment Gateway Identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandard("paymentGatewayId", "payment_gateway_id")]
        public bool Delete(int paymentGatewayId)
        {
            bool response = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().DeletePaymentGateway(groupId, paymentGatewayId);
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
        /// <param name="paymentGateway">Payment Gateway Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaPaymentGatewayProfile Add(KalturaPaymentGatewayProfile paymentGateway)
        {
            KalturaPaymentGatewayProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().InsertPaymentGateway(groupId, paymentGateway);
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
        /// <param name="paymentGateway">Payment Gateway Object</param>
        [Route("addOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [OldStandard("paymentGateway", "payment_gateway")]
        public bool AddOldStandard(KalturaPaymentGatewayProfile paymentGateway)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                ClientsManager.BillingClient().InsertPaymentGateway(groupId, paymentGateway);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Update payment gateway details
        /// </summary>
        /// <remarks>
        /// Possible status codes:      
        /// Action is not allowed = 5011, Payment gateway identifier is required = 6005, Name is required = 6020, Shared secret is required = 6021, External idntifier missing = 6016, 
        /// External identifier must be unique = 6040            
        /// </remarks>
        /// <param name="paymentGatewayId">Payment Gateway Identifier</param> 
        /// <param name="paymentGateway">Payment Gateway Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaPaymentGatewayProfile Update(int paymentGatewayId, KalturaPaymentGatewayProfile paymentGateway)
        {
            KalturaPaymentGatewayProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().SetPaymentGateway(groupId, paymentGatewayId, paymentGateway);
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
        /// Action is not allowed = 5011, Payment gateway identifier is required = 6005, Name is required = 6020, Shared secret is required = 6021, External idntifier missing = 6016, 
        /// External identifier must be unique = 6040            
        /// </remarks>
        /// <param name="paymentGatewayId">Payment Gateway Identifier</param> 
        /// <param name="paymentGateway">Payment Gateway Object</param>       
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [OldStandard("paymentGatewayId", "payment_gateway_id")]
        [OldStandard("paymentGateway", "payment_gateway")]
        [Obsolete]
        public bool UpdateOldStandard(int paymentGatewayId, KalturaPaymentGatewayProfile paymentGateway)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                ClientsManager.BillingClient().SetPaymentGateway(groupId, paymentGatewayId, paymentGateway);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Generate payment gateway shared secret
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// payment gateway id required = 6005, payment gateway not exist = 6008
        /// </remarks>
        /// <param name="payment_gateway_id">Payment gateway identifier</param>
        [Route("generateSharedSecret"), HttpPost]
        [ApiAuthorize]
        public KalturaPaymentGatewayProfile GenerateSharedSecret(int payment_gateway_id)
        {
            KalturaPaymentGatewayProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().GeneratePaymentGatewaySharedSecret(groupId, payment_gateway_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Gets the Payment Gateway Configuration for the payment gateway identifier given
        /// </summary>
        /// <param name="alias">The payemnt gateway for which to return the registration URL/s for the household. If omitted – return the regisration URL for the household for the default payment gateway</param>                
        /// <param name="intent">Represent the client’s intent for working with the payment gateway. Intent options to be coordinated with the applicable payment gateway adapter.</param>                
        /// <param name="extraParameters">Additional parameters to send to the payment gateway adapter.</param>
        /// <remarks>
        /// Possible status codes:       
        /// PaymentGatewayNotExist = 6008, SignatureMismatch = 6013
        /// </remarks>
        [Route("getConfiguration"), HttpPost]
        [ApiAuthorize]
        [OldStandard("extraParameters", "extra_parameters")]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public Models.Billing.KalturaPaymentGatewayConfiguration GetConfiguration(string alias, string intent, List<KalturaKeyValue> extraParameters)
        {
            Models.Billing.KalturaPaymentGatewayConfiguration response = null;

            int groupId = KS.GetFromRequest().GroupId;

            // get domain id      
            var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetPaymentGatewayConfiguration(groupId, alias, intent, extraParameters);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}