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
using WebAPI.Managers.Models;
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
        /// Possible status codes:       
        ///   User Does Not Exist = 2000, User Not In Domain = 1005, User With No Domain = 2024, User Suspended = 2001, 
        /// Domain Not Exists = 1006, Household Not Set To Payment Gateway = 6027 
        /// </remarks>
        /// <param name="household_id">Household Identifier</param>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<Models.Billing.KalturaPaymentGatewayBaseProfile> List(long household_id)
        {
            List<Models.Billing.KalturaPaymentGatewayBaseProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetHouseholdPaymentGateways(groupId, KS.GetFromRequest().UserId, household_id);
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
        /// Possible status codes:       
        ///   User Does Not Exist = 2000, User Not In Domain = 1005, User With No Domain = 2024, User Suspended = 2001, 
        /// Domain Not Exists = 1006, Household Not Set To Payment Gateway = 6027
        /// </remarks>
        /// <param name="household_id">Household Identifier</param>        
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public Models.Billing.KalturaPaymentGateway Get(long household_id)
        {
            Models.Billing.KalturaPaymentGateway response = null;

            int groupId = KS.GetFromRequest().GroupId;

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
        /// Possible status codes:       
        /// user does not exist = 2000, user not in domain = 1005, user with no domain = 2024, user suspended = 2001, domain not exists = 1006, 
        /// error saving payment gateway household = 6017, household already set to payment gateway = 6024, payment gateway selection is disabled = 6028,
        /// payment gateway not valid = 6043
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="household_id">Household Identifier</param>
        [Route("set"), HttpPost]
        [ApiAuthorize]
        public bool Set(int payment_gateway_id, long household_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain       
                var domain = ClientsManager.DomainsClient().GetDomainByUser(groupId, userID);
                
                // check if the user performing the action is domain master
                if (domain.MasterUsers.Where(u => u.Id == userID).FirstOrDefault() == null)
                {
                    throw new ForbiddenException();
                }

                // call client
                response = ClientsManager.BillingClient().SetHouseholdPaymentGateway(groupId, payment_gateway_id, KS.GetFromRequest().UserId, household_id);
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
        ///   User Does Not Exist = 2000, User Not In Domain = 1005, User With No Domain = 2024, User Suspended = 2001, Domain Not Exists = 1006
        /// Payment Gateway Id is Required = 6005, Payment gateway not exist = 6008, Household not set to payment gateway = 6027
        /// PaymentGatewaySelectionIsDisabled = 6028, ServiceForbidden = 500004
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        [Route("delete"), HttpPost]     
        public bool Delete(int payment_gateway_id, long household_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // get domain       
                var domain = ClientsManager.DomainsClient().GetDomainByUser(groupId, userID);
                
                // check if the user performing the action is domain master
                if (domain.MasterUsers.Where(u => u.Id == userID).FirstOrDefault() == null)
                {
                    throw new ForbiddenException();
                }

                // call client
                response = ClientsManager.BillingClient().DeleteHouseholdPaymentGateway(groupId, payment_gateway_id, KS.GetFromRequest().UserId, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }      
    }
}