using System;
using System.Collections.Generic;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.Billing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/paymentMethodProfile/action")]
    [OldStandardAction("addOldStandard", "add")]
    [OldStandardAction("updateOldStandard", "update")]
    [OldStandardAction("listOldStandard", "list")]
    public class PaymentMethodProfileController : ApiController
    {
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="filter">Payment gateway method profile filter</param>
        /// <remarks>
        /// Possible status codes: TBD       
        /// Payment gateway not exist = 6008
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaPaymentMethodProfileListResponse List(KalturaPaymentMethodProfileFilter filter)
        {
            List<KalturaPaymentMethodProfile> list = null;

            filter.Validate();

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                list = ClientsManager.BillingClient().GetPaymentGatewayPaymentMethods(groupId, filter.PaymentGatewayIdEqual.Value);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaPaymentMethodProfileListResponse() { PaymentMethodProfiles = list, TotalCount = list.Count };
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="payment_gateway_id"> Payment gateway identifier to list the payment methods for</param>
        /// <remarks>
        /// Possible status codes: TBD       
        /// Payment gateway not exist = 6008
        /// </remarks>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<Models.Billing.KalturaPaymentMethodProfile> ListOldStandard(int payment_gateway_id)
        {
            List<Models.Billing.KalturaPaymentMethodProfile> response = null;

            if (payment_gateway_id <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment_gateway_id cannot be empty");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetPaymentGatewayPaymentMethods(groupId, payment_gateway_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="paymentMethod">Payment method to add</param>
        /// <remarks>
        /// Possible status codes: 
        /// Payment gateway ID is required = 6005, Payment gateway does not exist = 6008, Payment method name required = 6055
        /// </remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaPaymentMethodProfile Add(KalturaPaymentMethodProfile paymentMethod)
        {
            KalturaPaymentMethodProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().AddPaymentMethodToPaymentGateway(groupId, paymentMethod.getPaymentGatewayId(), paymentMethod.Name, paymentMethod.getAllowMultiInstance());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="payment_gateway_id">Payment gateway identifier to add the payment method for</param>
        /// <param name="payment_method">Payment method to add</param>
        /// <remarks>
        /// Possible status codes: 
        /// Payment gateway ID is required = 6005, Payment gateway does not exist = 6008, Payment method name required = 6055
        /// </remarks>
        [Route("addOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaPaymentMethodProfile AddOldStandard(int payment_gateway_id, KalturaPaymentMethodProfile payment_method)
        {
            KalturaPaymentMethodProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().AddPaymentMethodToPaymentGateway(groupId, payment_gateway_id, payment_method.Name, payment_method.getAllowMultiInstance());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update payment method
        /// </summary>
        /// <param name="paymentMethodId">Payment method identifier to update</param>
        /// <param name="paymentMethod">Payment method to update</param>
        /// <remarks>
        /// Possible status codes: 
        /// Payment gateway ID is required = 6005, Payment gateway does not exist = 6008, Payment method does not exist = 6049, Payment method ID is required = 6050      
        /// </remarks>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaPaymentMethodProfile Update(int paymentMethodId, KalturaPaymentMethodProfile paymentMethod)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                return ClientsManager.BillingClient().UpdatePaymentMethod(groupId, paymentMethod.getId(), paymentMethod.Name, paymentMethod.getAllowMultiInstance());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="paymentGatewayId">Payment gateway identifier to update the payment method for</param>
        /// <param name="paymentMethod">Payment method to update</param>
        /// <remarks>
        /// Possible status codes: 
        /// Payment gateway ID is required = 6005, Payment gateway does not exist = 6008, Payment method does not exist = 6049, Payment method ID is required = 6050      
        /// </remarks>
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [OldStandard("paymentGatewayId", "payment_gateway_id")]
        [OldStandard("paymentMethod", "payment_method")]
        [Obsolete]
        public bool UpdateOldStandard(int paymentGatewayId, KalturaPaymentMethodProfile paymentMethod)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().UpdatePaymentGatewayPaymentMethod(groupId, paymentGatewayId, paymentMethod.getId(), paymentMethod.Name, paymentMethod.getAllowMultiInstance());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete payment method profile
        /// </summary>
        /// <param name="paymentMethodId">Payment method identifier to delete</param>
        /// <remarks>
        ///  Possible status codes: 
        ///  Payment gateway ID is required = 6005, Payment gateway does not exist = 6008, Payment method does not exist = 6049, Payment method ID is required = 6050    
        /// </remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int paymentMethodId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().DeletePaymentMethod(groupId, paymentMethodId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="payment_gateway_id">Payment gateway identifier to delete the payment method for</param>
        /// <param name="payment_method_id">Payment method identifier to delete</param>
        /// <remarks>
        ///  Possible status codes: 
        ///  Payment gateway ID is required = 6005, Payment gateway does not exist = 6008, Payment method does not exist = 6049, Payment method ID is required = 6050    
        /// </remarks>
        [Route("deleteOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool DeleteOldStandard(int payment_gateway_id, int payment_method_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().DeletePaymentGatewayPaymentMethod(groupId, payment_gateway_id, payment_method_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}