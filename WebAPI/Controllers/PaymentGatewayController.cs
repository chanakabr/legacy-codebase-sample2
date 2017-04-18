using ApiObjects.Response;
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
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/paymentGateway/action")]
    [Obsolete]
    public class PaymentGatewayController : ApiController
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
        [Throws(eResponseStatus.UserSuspended)]
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
        /// Set a Payment Gateway provider for the household. 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// user suspended = 2001, payment gateway id is required = 6005, error saving payment gateway household = 6017, household already set to payment gateway = 6024, payment gateway selection is disabled = 6028,
        /// payment gateway not valid = 6043
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        [Route("set"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.PaymentGatewayIdRequired)]
        [Throws(eResponseStatus.ErrorSavingPaymentGatewayHousehold)]
        [Throws(eResponseStatus.HouseholdAlreadySetToPaymentGateway)]
        [Throws(eResponseStatus.PaymentGatewaySelectionIsDisabled)]
        [Throws(eResponseStatus.PaymentGatewayNotValid)]
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
        /// <param name="paymentGatewayId">Payment Gateway Identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandardArgument("paymentGatewayId", "payment_gateway_id")]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.PaymentGatewayIdRequired)]
        [Throws(eResponseStatus.PaymentGatewayNotExist)]
        [Throws(eResponseStatus.HouseholdNotSetToPaymentGateway)]
        [Throws(eResponseStatus.PaymentGatewaySelectionIsDisabled)]
        public bool Delete(int paymentGatewayId)
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

        /// <summary>
        /// Set a payment method to payment gateway for the household. 
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// Payment gateway not set for household = 6007, Payment gateway not valid = 6043, Payment method not set for household = 6048,
        /// Error saving payment gateway household payment method = 6052, Payment gateway not support payment method = 6056
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="payment_method_id">Payment method Identifier</param> 
        [Route("setPaymentMethod"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("payment_gateway_id", MinInteger = 1)]
        [SchemeArgument("payment_method_id", MinInteger = 1)]
        [Throws(eResponseStatus.PaymentGatewayNotSetForHousehold)]
        [Throws(eResponseStatus.PaymentGatewayNotValid)]
        [Throws(eResponseStatus.PaymentMethodNotSetForHousehold)]
        [Throws(eResponseStatus.ErrorSavingPaymentGatewayHouseholdPaymentMethod)]
        [Throws(eResponseStatus.PaymentGatewayNotSupportPaymentMethod)]
        public bool SetPaymentMethod(int payment_gateway_id, int payment_method_id)
        {
            bool response = false;


            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.BillingClient().SetPaymentMethodHouseholdPaymentGateway(groupId, payment_gateway_id, userID, domainId, payment_method_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Removes a payment method of the household. 
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// Payment method not set for household = 6048, PaymentMethodIsUsedByHousehold = 3041, 
        /// PaymentGatewayNotExist = 6008, PaymentGatewayNotSetForHousehold = 6007,
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="payment_method_id">Payment method Identifier</param>
        /// <returns></returns>
        [Route("removePaymentMethod"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("payment_gateway_id", MinInteger = 1)]
        [SchemeArgument("payment_method_id", MinInteger = 1)]
        [Throws(eResponseStatus.PaymentMethodNotSetForHousehold)]
        [Throws(eResponseStatus.PaymentMethodIsUsedByHousehold)]
        [Throws(eResponseStatus.PaymentGatewayNotExist)]
        [Throws(eResponseStatus.PaymentGatewayNotSetForHousehold)]
        public bool RemovePaymentMethod(int payment_gateway_id, int payment_method_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.ConditionalAccessClient().RemovePaymentMethodHouseholdPaymentGateway(payment_gateway_id, groupId, userID, domainId, payment_method_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Force remove of a payment method of the household. 
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// Payment method not set for household = 6048
        /// PaymentGatewayNotExist = 6008, PaymentGatewayNotSetForHousehold = 6007
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="payment_method_id">Payment method Identifier</param>
        /// <returns></returns>
        [Route("forceRemovePaymentMethod"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("payment_gateway_id", MinInteger = 1)]
        [SchemeArgument("payment_method_id", MinInteger = 1)]
        [Throws(eResponseStatus.PaymentMethodNotSetForHousehold)]
        [Throws(eResponseStatus.PaymentGatewayNotExist)]
        [Throws(eResponseStatus.PaymentGatewayNotSetForHousehold)]
        public bool ForceRemovePaymentMethod(int payment_gateway_id, int payment_method_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.ConditionalAccessClient().RemovePaymentMethodHouseholdPaymentGateway(payment_gateway_id, groupId, userID, domainId, payment_method_id, true);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}