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
    [RoutePrefix("_service/paymentGatewayProfileSettings/action")]
    [OldStandardAction("addOldStandard", "add")]
    public class PaymentGatewayProfileSettingsController : ApiController
    {
        /// <summary>
        /// Returns all payment gateway settings for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<Models.Billing.KalturaPaymentGatewayProfile> List()
        {
            List<Models.Billing.KalturaPaymentGatewayProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetPaymentGatewaySettings(groupId);
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
        /// Possible status codes:       
        ///  
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param>
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int payment_gateway_id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().DeletePaymentGatewaySettings(groupId, payment_gateway_id, settings);
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
        /// Possible status codes:       
        /// Payment gateway params required = 6006
        /// </remarks>
        /// <param name="profile">Payment Gateway profile</param> 
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaPaymentGatewayProfile Add(KalturaPaymentGatewayProfile profile)
        {
            KalturaPaymentGatewayProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().InsertPaymentGatewaySettings(groupId, profile.getId(), profile.Settings);
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
        /// Possible status codes:       
        /// Payment gateway params required = 6006
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("addOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool AddOldStandard(int payment_gateway_id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                ClientsManager.BillingClient().InsertPaymentGatewaySettings(groupId, payment_gateway_id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Update settings for payment gateway 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        ///  
        /// </remarks>
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="settings">Dictionary (string,KalturaStringValue) for partner specific settings: Format Example
        /// "settings": { "key3": {"value": "value3"},
        ///"key1": {"value": "value2"}}
        ///</param>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(int payment_gateway_id, SerializableDictionary<string, KalturaStringValue> settings)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.BillingClient().SetPaymentGatewaySettings(groupId, payment_gateway_id, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}