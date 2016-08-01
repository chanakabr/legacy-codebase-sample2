using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Billing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/householdPaymentGateway/action")]
    public class HouseholdPaymentGatewayController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get a list of all configured Payment Gateways providers available for the account. For each payment is provided with the household associated payment methods. 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// User Suspended = 2001
        /// </remarks>        
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaHouseholdPaymentGatewayListResponse List()
        {
            List<KalturaHouseholdPaymentGateway> list = null;

            int groupId = KS.GetFromRequest().GroupId;

            // get domain id      
            var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                // call client
                list = ClientsManager.BillingClient().GetHouseholdPaymentGatewaysList(groupId, KS.GetFromRequest().UserId, domainId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaHouseholdPaymentGatewayListResponse() { Objects = list, TotalCount = list.Count };
        }

        /// <summary>
        /// Set user billing account identifier (charge ID), for a specific household and a specific payment gateway
        /// </summary>
        /// <remarks>
        /// Possible status codes:         
        /// Payment gateway not exist = 6008, Payment gateway charge id required = 6009, External identifier required = 6016, Error saving payment gateway household = 6017, 
        /// Charge id already set to household payment gateway = 6025
        /// </remarks>        
        /// <param name="paymentGatewayExternalId">External identifier for the payment gateway  </param>
        /// <param name="chargeId">The billing user account identifier for this household at the given payment gateway</param>        
        [Route("setChargeID"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool SetChargeID(string paymentGatewayExternalId, string chargeId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.BillingClient().SetHouseholdChargeID(groupId, paymentGatewayExternalId, (int)domainId, chargeId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;

        }

        /// <summary>
        /// Get a household’s billing account identifier (charge ID) for a given payment gateway
        /// </summary>
        /// <remarks>
        /// Possible status codes: Payment gateway not exist for group = 6008, External identifier is required = 6016, Charge id not set to household = 6026
        /// </remarks>        
        /// <param name="paymentGatewayExternalId">External identifier for the payment gateway  </param>        
        [Route("getChargeID"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public string GetChargeID(string paymentGatewayExternalId)
        {
            string chargeId = string.Empty;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // get domain id       
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                chargeId = ClientsManager.BillingClient().GetHouseholdChargeID(groupId, paymentGatewayExternalId, (int)domainId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return chargeId;
        }
        
        /// <summary>
        /// Enable a payment-gateway provider for the household. 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// user suspended = 2001, payment gateway id is required = 6005, error saving payment gateway household = 6017, household already set to payment gateway = 6024, payment gateway selection is disabled = 6028,
        /// payment gateway not valid = 6043
        /// </remarks>
        /// <param name="paymentGatewayId">Payment Gateway Identifier</param> 
        [Route("set"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool Enable(int paymentGatewayId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                
                // call client
                response = ClientsManager.BillingClient().SetHouseholdPaymentGateway(groupId, paymentGatewayId, userID, domainId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Disable payment-gateway on the household
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// user suspended = 2001, payment gateway identifier is missing = 6005, payment gateway not exist = 6008, household not set to payment gateway = 6027
        /// payment gateway selection is disabled = 6028, service forbidden = 500004
        /// </remarks>
        /// <param name="paymentGatewayId">Payment Gateway Identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [ValidationException( SchemeValidationType.ACTION_NAME)]
        public bool Disable(int paymentGatewayId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain id        
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.BillingClient().DeleteHouseholdPaymentGateway(groupId, paymentGatewayId, userID, domainId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}