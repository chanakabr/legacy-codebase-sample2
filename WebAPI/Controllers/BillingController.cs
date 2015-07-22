using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Billing;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("billing")]
    public class BillingController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Charges a user for a media file with the given PPV module entitlements         
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Price not correct = 6000, Unknown PPV module = 6001, Expired credit card = 6002, Cellular permissions error (for cellular charge) = 6003, Unknown billing provider = 6004
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="ppv_id">PPV module identifier</param>
        /// <param name="request">Charge request parameters</param>
        [Route("ppvs/{ppv_id}/buy"), HttpPost]
        public BillingResponse ChargeUserForMediaFile([FromUri] string partner_id, [FromUri] string ppv_id, [FromBody] ChargePPV request, [FromUri]string udid = null)
        {
            BillingResponse response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().ChargeUserForMediaFile(groupId, request.UserId, request.Price, request.Currency, request.FileId, ppv_id, request.CouponCode,
                    request.ExtraParams, udid, request.EncryptedCvv);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Charges a user for subscription        
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Price not correct = 6000, Unknown PPV module = 6001, Expired credit card = 6002, Cellular permissions error (for cellular charge) = 6003, Unknown billing provider = 6004
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="sub_id">Subscription identifier</param>
        /// <param name="request">Charge request parameters</param>
        [Route("subscriptions/{sub_id}/buy"), HttpPost]
        public BillingResponse ChargeUserForSubscription([FromUri] string partner_id, [FromUri] string sub_id, [FromBody] Charge request, [FromUri]string udid = null)
        {
            BillingResponse response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().ChargeUserForSubscription(groupId, request.UserId, request.Price, request.Currency, sub_id, request.CouponCode,
                    request.ExtraParams, udid, request.EncryptedCvv);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


        #region Payment GateWay

        /// <summary>
        /// Returns all payment gateway settings for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>       
        [Route("payment_gateways/settings"), HttpGet]
        public Models.Billing.PaymentGWSettingsResponse GetPaymentGWSettings([FromUri] string partner_id)
        {
            Models.Billing.PaymentGWSettingsResponse response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetPaymentGWSettings(groupId);
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
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>       
        [Route("payment_gateways"), HttpGet]
        public Models.Billing.PaymentGWResponse GetPaymentGW([FromUri] string partner_id)
        {
            Models.Billing.PaymentGWResponse response = null;

            int groupId = int.Parse(partner_id);

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
        public bool DeletePaymentGW([FromUri] string partner_id, [FromUri] int payment_gateway_id)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

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
        /// Delete payment gateway specific settings by settings keys 
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param>
        /// <param name="settings">Dictionary (string,string) for partner specific settings</param>
        [Route("payment_gateways/{payment_gateway_id}/settings/delete"), HttpPost]
        public bool DeletePaymentGWSettings([FromUri] string partner_id, [FromUri] int payment_gateway_id, [FromBody] Dictionary<string, string> settings)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().DeletePaymentGWSettings(groupId, payment_gateway_id, settings);
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
        /// <param name="pgs">Payment GateWay Settings Object</param>
        [Route("payment_gateway/add"), HttpPost]
        public bool InsertPaymentGW([FromUri] string partner_id, [FromBody] PaymentGW pgs)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().InsertPaymentGW(groupId, pgs);
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
        /// Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="settings">Dictionary (string,string) for partner specific settings </param>
        [Route("payment_gateways/{payment_gateway_id}/settings/add"), HttpPost]
        public bool InsertPaymentGWSettings([FromUri] string partner_id, [FromUri] int payment_gateway_id, [FromBody] Dictionary<string, string> settings)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().InsertPaymentGWSettings(groupId, payment_gateway_id, settings);
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
        ///<param name="url">Payment Gateway Url</param>
        ///<param name="is_default">Payment Gateway is default or not </param>
        ///<param name="is_active">Payment Gateway is active or not </param>
        ///<param name="external_identifier">Payment Gateway external identifier</param>
        [Route("payment_gateways/{payment_gateway_id}/update"), HttpPost]
        public bool SetPaymentGW([FromUri] string partner_id, [FromUri] int payment_gateway_id, [FromUri] string name, [FromUri] string url, [FromUri] int is_default, [FromUri] int is_active, 
            [FromUri] string external_identifier, [FromUri]  int pendding_interval, [FromUri] int pendding_retries,  [FromUri] string shared_secret)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().SetPaymentGW(groupId, payment_gateway_id, name, url, external_identifier, pendding_interval,
                    pendding_retries,shared_secret, is_default, is_active);
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
        [Route("payment_gateways/{payment_gateway_id}/settings/update"), HttpPost]
        public bool SetPaymentGWSettings([FromUri] string partner_id, [FromUri] int payment_gateway_id, [FromBody] Dictionary<string,string> settings)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().SetPaymentGWSettings(groupId, payment_gateway_id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        #endregion
    }
}