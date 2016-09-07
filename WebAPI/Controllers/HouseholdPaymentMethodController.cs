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
    [RoutePrefix("_service/householdPaymentMethod/action")]
    public class HouseholdPaymentMethodController : ApiController
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
        public KalturaHouseholdPaymentMethodListResponse List()
        {
            List<KalturaHouseholdPaymentMethod> list = null;

            int groupId = KS.GetFromRequest().GroupId;

            // get domain id      
            var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                // call client
                list = ClientsManager.BillingClient().GetHouseholdPaymentMethods(groupId, KS.GetFromRequest().UserId, domainId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaHouseholdPaymentMethodListResponse() { Objects = list, TotalCount = list.Count };
        }

        /// <summary>
        /// Set a payment method as default for the household. 
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// Payment gateway not set for household = 6007, Payment gateway not valid = 6043, Payment method not set for household = 6048,
        /// Error saving payment gateway household payment method = 6052, Payment gateway not support payment method = 6056
        /// </remarks>
        /// <param name="paymentGatewayId">Payment Gateway Identifier</param> 
        /// <param name="paymentMethodId">Payment method Identifier</param> 
        [Route("setAsDefault"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool SetAsDefault(int paymentGatewayId, int paymentMethodId)
        {
            bool response = false;

            if (paymentMethodId <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment_method_id not valid");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.BillingClient().SetPaymentMethodHouseholdPaymentGateway(groupId, paymentGatewayId, userID, domainId, paymentMethodId);
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
        /// Payment method not set for household = 6048, PaymentMethodIsUsedByHousehold = 3041, Error removing payment gateway household payment method = 6057,
        /// PaymentGatewayNotExist = 6008, PaymentGatewayNotSetForHousehold = 6007,
        /// </remarks>
        /// <param name="paymentGatewayId">Payment Gateway Identifier</param> 
        /// <param name="paymentMethodId">Payment method Identifier</param>
        /// <returns></returns>
        [Route("remove"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool Remove(int paymentGatewayId, int paymentMethodId)
        {
            bool response = false;

            if (paymentMethodId <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment_method_id not valid");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.ConditionalAccessClient().RemovePaymentMethodHouseholdPaymentGateway(paymentGatewayId, groupId, userID, domainId, paymentMethodId);
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
        /// Payment method not set for household = 6048, Error removing payment gateway household payment method = 6057,
        /// PaymentGatewayNotExist = 6008, PaymentGatewayNotSetForHousehold = 6007
        /// </remarks>
        /// <param name="paymentGatewayId">Payment Gateway Identifier</param> 
        /// <param name="paymentMethodId">Payment method Identifier</param>
        /// <returns></returns>
        [Route("forceRemove"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool ForceRemove(int paymentGatewayId, int paymentMethodId)
        {
            bool response = false;

            if (paymentMethodId <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment_method_id not valid");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.ConditionalAccessClient().RemovePaymentMethodHouseholdPaymentGateway(paymentGatewayId, groupId, userID, domainId, paymentMethodId, true);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Set user billing payment method identifier (payment method external id), for a specific household and a specific payment gateway
        /// </summary>
        /// <remarks>
        /// Possible status codes:         
        /// Payment gateway not set for household = 6007, Payment gateway not exist = 6008, Payment method not exist = 6049,  Error saving payment gateway household payment method = 6052, 
        /// Payment method already set to household payment gateway = 6054, Payment gateway not support payment method = 6056
        /// </remarks>        
        /// <param name="paymentGatewayId">External identifier for the payment gateway  </param>
        /// <param name="paymentMethodName"></param>      
        /// <param name="paymentDetails"></param>      
        /// <param name="paymentMethodExternalId"></param>        
        [Route("setExternalId"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public int SetExternalId(string paymentGatewayId, string paymentMethodName, string paymentDetails, string paymentMethodExternalId)
        {
            int response = 0;

            if (string.IsNullOrEmpty(paymentGatewayId))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment_gateway_id cannot be empty");
            }

            if (string.IsNullOrEmpty(paymentMethodName))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment_method_name cannot be empty");
            }

            if (string.IsNullOrEmpty(paymentMethodExternalId))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment_method_external_id cannot be empty");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.BillingClient().SetPaymentGatewayHouseholdPaymentMethod(groupId, paymentGatewayId, (int)domainId, paymentMethodName, paymentDetails, paymentMethodExternalId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;

        }

        /// <summary>
        /// Add a new payment method for household
        /// </summary>
        /// <remarks>
        /// Possible status codes:  Payment gateway not set for household = 6007, Payment gateway not valid = 6043, Payment method not set for household = 6048,
        /// Error saving payment gateway household payment method = 6052, Payment gateway not support payment method = 6056
        /// </remarks>
        /// <param name="householdPaymentMethod">Household payment method</param> 
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaHouseholdPaymentMethod Add(KalturaHouseholdPaymentMethod householdPaymentMethod)
        {
            KalturaHouseholdPaymentMethod response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;
            long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                // call client
                response = ClientsManager.BillingClient().AddPaymentGatewayPaymentMethodToHousehold(groupId, domainId, householdPaymentMethod);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}