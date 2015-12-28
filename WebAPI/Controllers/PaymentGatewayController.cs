using KLogMonitor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/paymentGateway/action")]
    public class PaymentGatewayController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get a list of all configured Payment Gateways providers available for the partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// User Suspended = 2001, Household Not Set To Payment Gateway = 6027 
        /// </remarks>        
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<Models.Billing.KalturaPaymentGatewayBaseProfile> List()
        {
            List<Models.Billing.KalturaPaymentGatewayBaseProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            // get domain id      
            var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetHouseholdPaymentGateways(groupId, KS.GetFromRequest().UserId, domainId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get the Payment Gateway provider configured for the household, or the default payment gateway provider is a provider is not configured for the household
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// User Suspended = 2001, Household Not Set To Payment Gateway = 6027
        /// </remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public Models.Billing.KalturaPaymentGateway Get()
        {
            Models.Billing.KalturaPaymentGateway response = null;

            int groupId = KS.GetFromRequest().GroupId;

            // get domain id      
            var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
            
            try
            {
                // call client
                response = ClientsManager.BillingClient().GetSelectedHouseholdPaymentGateway(groupId, domainId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Set a Payment Gateway provider for the household. It also clear the Charge ID.
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// user suspended = 2001, payment gateway id is required = 6005, error saving payment gateway household = 6017, household already set to payment gateway = 6024, payment gateway selection is disabled = 6028,
        /// payment gateway not valid = 6043
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        [Route("set"), HttpPost]
        [ApiAuthorize]
        public bool Set(int payment_gateway_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                
                // call client
                response = ClientsManager.BillingClient().SetHouseholdPaymentGateway(groupId, payment_gateway_id, userID, domainId);
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
        /// Possible status codes:       
        /// user suspended = 2001, payment gateway identifier is missing = 6005, payment gateway not exist = 6008, household not set to payment gateway = 6027
        /// payment gateway selection is disabled = 6028, service forbidden = 500004
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param>
        [Route("delete"), HttpPost]     
        public bool Delete(int payment_gateway_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain id        
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.BillingClient().DeleteHouseholdPaymentGateway(groupId, payment_gateway_id, userID, domainId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }      
    }
}