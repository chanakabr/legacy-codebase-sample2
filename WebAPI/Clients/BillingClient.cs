using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebAPI.Billing;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Billing;
using WebAPI.Models.General;
using WebAPI.Models.Partner;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;
using System.Net;
using System.Web;

namespace WebAPI.Clients
{
    public class BillingClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public BillingClient()
        {

        }

        protected WebAPI.Billing.module Billing
        {
            get
            {
                return (Module as WebAPI.Billing.module);
            }
        }


        #region Payment GateWay

        public List<Models.Billing.KalturaPaymentGatewayProfile> GetPaymentGatewaySettings(int groupId)
        {
            List<Models.Billing.KalturaPaymentGatewayProfile> KalturaPaymentGatewayProfileList = null;
            WebAPI.Billing.PaymentGatewaySettingsResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.GetPaymentGatewateSettings(group.BillingCredentials.Username, group.BillingCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetPaymentGatewateSettings. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }


            KalturaPaymentGatewayProfileList = Mapper.Map<List<Models.Billing.KalturaPaymentGatewayProfile>>(response.pgw);

            return KalturaPaymentGatewayProfileList;
        }

        public List<Models.Billing.KalturaPaymentGatewayProfile> GetPaymentGateway(int groupId)
        {
            List<Models.Billing.KalturaPaymentGatewayProfile> KalturaPaymentGatewayProfileList = null;
            WebAPI.Billing.PaymentGatewayResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.GetPaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetPaymentGateway. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }

            KalturaPaymentGatewayProfileList = Mapper.Map<List<Models.Billing.KalturaPaymentGatewayProfile>>(response.pgw);

            return KalturaPaymentGatewayProfileList;
        }

        public List<Models.Billing.KalturaPaymentGatewayBaseProfile> GetHouseholdPaymentGateways(int groupId, string siteGuid, long householdId)
        {
            List<Models.Billing.KalturaPaymentGatewayBaseProfile> KalturaPaymentGatewayBaseProfileList = null;
            WebAPI.Billing.PaymentGatewayListResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.GetHouseholdPaymentGateways(group.BillingCredentials.Username, group.BillingCredentials.Password, siteGuid, (int)householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetHouseHoldPaymentGW.  groupID: {0}, siteGuid: {1}, exception: {1}", groupId, siteGuid, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            KalturaPaymentGatewayBaseProfileList = Mapper.Map<List<Models.Billing.KalturaPaymentGatewayBaseProfile>>(response.PaymentGateways);

            return KalturaPaymentGatewayBaseProfileList;
        }

        internal KalturaPaymentGatewayProfile SetPaymentGateway(int groupId, int paymentGatewayId, KalturaPaymentGatewayProfile paymentGateway)
        {
            PaymentGatewayItemResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGateway request = Mapper.Map<WebAPI.Billing.PaymentGateway>(paymentGateway);
                    response = Billing.UpdatePaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGatewayId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetPaymentGateway. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            KalturaPaymentGatewayProfile paymentGatewayProfile = Mapper.Map<KalturaPaymentGatewayProfile>(response);
            return paymentGatewayProfile;
        }

        public bool SetPaymentGatewaySettings(int groupId, int paymentGWID, Dictionary<string, KalturaStringValue> payment_gateway_settings)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGatewaySettings[] configs = BillingMappings.ConvertPaymentGatewaySettings(payment_gateway_settings);
                    response = Billing.SetPaymentGWSettings(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGWID, configs);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetPaymentGatewaySettings. groupID: {0}, paymentGWID: {1}, exception: {2}", groupId, paymentGWID, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        public bool DeletePaymentGateway(int groupId, int paymentGwID)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.DeletePaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGwID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeletePaymentGateway.  groupID: {0}, paymentGWID: {1}, exception: {2}", groupId, paymentGwID, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        public bool DeletePaymentGatewaySettings(int groupId, int paymentGwID, Dictionary<string, KalturaStringValue> payment_gateway_settings)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGatewaySettings[] settings = BillingMappings.ConvertPaymentGatewaySettings(payment_gateway_settings);
                    response = Billing.DeletePaymentGWSettings(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGwID, settings);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeletePaymentGWParams.  groupID: {0}, paymentGWID: {1}, exception: {2}", groupId, paymentGwID, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        public bool DeleteHouseholdPaymentGateway(int groupId, int paymentGatewayId, string siteGuid, long householdID)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    int house_hold_id = (int)householdID;
                    response = Billing.DeleteHouseholdPaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGatewayId, siteGuid, house_hold_id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeletePaymentGWHouseHold.  groupID: {0}, paymentGWID: {1}, siteGuid: {2}, exception: {3}", groupId, paymentGatewayId, siteGuid, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        public KalturaPaymentGatewayProfile InsertPaymentGateway(int groupId, Models.Billing.KalturaPaymentGatewayProfile pgw)
        {
            PaymentGatewayItemResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGateway request = Mapper.Map<WebAPI.Billing.PaymentGateway>(pgw);
                    response = Billing.AddPaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertPaymentGateway.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            KalturaPaymentGatewayProfile paymentGatewayProfile = Mapper.Map<KalturaPaymentGatewayProfile>(response);
            return paymentGatewayProfile;
        }

        public KalturaPaymentGatewayProfile InsertPaymentGatewaySettings(int groupId, int paymentGatewayId, SerializableDictionary<string, KalturaStringValue> payment_gateway_settings)
        {
            PaymentGatewayItemResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGatewaySettings[] request = BillingMappings.ConvertPaymentGatewaySettings(payment_gateway_settings);
                    response = Billing.AddPaymentGatewaySettings(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGatewayId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertPaymentGatewaySettings. groupID: {0}, paymentGwID: {1} ,exception: {2}", groupId, paymentGatewayId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            KalturaPaymentGatewayProfile paymentGatewayProfile = Mapper.Map<KalturaPaymentGatewayProfile>(response);
            return paymentGatewayProfile;
        }

        public bool SetHouseholdPaymentGateway(int groupId, int paymentGwID, string siteGuid, long householdID)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.SetHouseholdPaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGwID, siteGuid, (int)householdID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetHouseholdPaymentGateway.  groupID: {0}, paymentGwID: {1} ,siteGuid: {2}, exception: {3}", groupId, paymentGwID, siteGuid, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal bool SetHouseholdChargeID(int groupId, string externalIdentifier, int householdId, string chargeId)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.SetHouseholdChargeID(group.BillingCredentials.Username, group.BillingCredentials.Password, externalIdentifier, householdId, chargeId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetHouseholdChargeID.  groupID: {0}, external Identifier: {1} , exception: {2}", groupId, externalIdentifier, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal string GetHouseholdChargeID(int groupId, string externalIdentifier, int householdId)
        {
            WebAPI.Billing.PaymentGatewayChargeIDResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.GetHouseholdChargeID(group.BillingCredentials.Username, group.BillingCredentials.Password, externalIdentifier, householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetHouseholdChargeID.  groupID: {0}, external Identifier: {1} , exception: {2}", groupId, externalIdentifier, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.ResponseStatus.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.ResponseStatus.Code, response.ResponseStatus.Message);
            }

            return response.ChargeID;
        }

        internal KalturaPaymentGatewayProfile GeneratePaymentGatewaySharedSecret(int groupId, int paymentGatewayId)
        {
            WebAPI.Billing.PaymentGatewayItemResponse response = null;
            KalturaPaymentGatewayProfile kalturaPaymentGatewayProfile = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.GeneratePaymentGatewaySharedSecret(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGatewayId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GeneratePaymentGatewaySharedSecret. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            kalturaPaymentGatewayProfile = Mapper.Map<KalturaPaymentGatewayProfile>(response);

            return kalturaPaymentGatewayProfile;
        }

        #endregion


        internal bool SetPartnerConfiguration(int groupId, KalturaBillingPartnerConfig partnerConfig)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PartnerConfiguration request = Mapper.Map<WebAPI.Billing.PartnerConfiguration>(partnerConfig);
                    response = Billing.SetPartnerConfiguration(group.BillingCredentials.Username, group.BillingCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetPartnerConfiguration. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal KalturaPaymentGatewayConfiguration GetPaymentGatewayConfiguration(int groupId, string alias, string intent, List<KalturaKeyValue> extraParams)
        {
            Models.Billing.KalturaPaymentGatewayConfiguration configuration = null;
            WebAPI.Billing.PaymentGatewayConfigurationResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);
            List<KeyValuePair> keyValuePairs = Mapper.Map<List<KeyValuePair>>(extraParams);
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.GetPaymentGatewayConfiguration(group.BillingCredentials.Username, group.BillingCredentials.Password, alias, intent, keyValuePairs.ToArray());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetPaymentGatewayConfiguration.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            configuration = Mapper.Map<WebAPI.Models.Billing.KalturaPaymentGatewayConfiguration>(response);

            return configuration;
        }

        internal bool SetPaymentGatewayHouseholdPaymentMethod(int groupId, string externalIdentifier, int householdId, string paymentMethodName, string paymentDetails, string paymentMethodExternalId)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.SetPaymentGatewayHouseholdPaymentMethod(group.BillingCredentials.Username, group.BillingCredentials.Password, externalIdentifier, householdId, paymentMethodName,
                        paymentDetails, paymentMethodExternalId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetHouseholdChargeID.  groupID: {0}, external Identifier: {1} , exception: {2}", groupId, externalIdentifier, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal List<KalturaPaymentMethodProfile> GetPaymentGatewayPaymentMethods(int groupId, int paymentGatewayId)
        {
            WebAPI.Billing.PaymentMethodsResponse response = null;
            List<KalturaPaymentMethodProfile> paymentMethods = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.GetPaymentGatewayPaymentMethods(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGatewayId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetPaymentGatewayPaymentMethods.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            paymentMethods = Mapper.Map<List<KalturaPaymentMethodProfile>>(response.PaymentMethods);

            return paymentMethods;
        }

        internal KalturaPaymentMethodProfile AddPaymentMethodToPaymentGateway(int groupId, int paymentGatewayId, string name, bool allowMultiInstance)
        {
            WebAPI.Billing.PaymentMethodsResponse response = null;
            KalturaPaymentMethodProfile paymentMethod = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.AddPaymentMethodToPaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGatewayId, name, allowMultiInstance);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while AddPaymentMethodToPaymentGateway.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            if (response.PaymentMethods == null || response.PaymentMethods.Length == 0)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            paymentMethod = Mapper.Map<KalturaPaymentMethodProfile>(response.PaymentMethods[0]);

            return paymentMethod;
        }

        internal bool UpdatePaymentGatewayPaymentMethod(int groupId, int paymentGatewayId, int paymentMethodId, string name, bool allowMultiInstance)
        {
            WebAPI.Billing.Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.UpdatePaymentGatewayPaymentMethod(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGatewayId, paymentMethodId, name, allowMultiInstance);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdatePaymentGatewayPaymentMethod.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal bool DeletePaymentGatewayPaymentMethod(int groupId, int paymentGatewayId, int paymentMethodId)
        {
            WebAPI.Billing.Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.DeletePaymentGatewayPaymentMethod(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGatewayId, paymentMethodId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeletePaymentGatewayPaymentMethod.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal bool SetPaymentMethodHouseholdPaymentGateway(int groupId, int paymentGatewayId, string userID, long householdId, int paymentMethodId)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.SetPaymentMethodHouseholdPaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGatewayId, userID,
                        (int)householdId, paymentMethodId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetPaymentMethodHouseholdPaymentGateway.  groupID: {0}, paymentGatewayId {1} , paymentMethodId {2}, householdId {3},  exception: {4}", groupId,
                    paymentGatewayId, paymentMethodId, householdId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }
    }
}

namespace WebAPI.Billing
{
    // adding request ID to header
    public partial class module
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);

            if (request.Headers != null &&
                request.Headers[Constants.REQUEST_ID_KEY] == null &&
                HttpContext.Current.Items[Constants.REQUEST_ID_KEY] != null)
            {
                request.Headers.Add(Constants.REQUEST_ID_KEY, HttpContext.Current.Items[Constants.REQUEST_ID_KEY].ToString());
            }
            return request;
        }
    }
}