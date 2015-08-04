using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Billing;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;

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

        public Models.Billing.KalturaPaymentGWSettingsResponse GetPaymentGWSettings(int groupId)
        {
            Models.Billing.KalturaPaymentGWSettingsResponse paymentGWSettings = null;
            WebAPI.Billing.PaymentGWSettingsResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.GetPaymentGWSettings(group.BillingCredentials.Username, group.BillingCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetPaymentGWSettings.  groupID: {0}, exception: {1}", groupId, ex);
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

            paymentGWSettings = Mapper.Map<WebAPI.Models.Billing.KalturaPaymentGWSettingsResponse>(response);

            return paymentGWSettings;
        }

        public Models.Billing.KalturaPaymentGWResponse GetPaymentGateway(int groupId)
        {
            Models.Billing.KalturaPaymentGWResponse paymentGW = null;
            WebAPI.Billing.PaymentGWResponse response = null;
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
                log.ErrorFormat("Error while GetPaymentGW.  groupID: {0}, exception: {1}", groupId, ex);
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

            paymentGW = Mapper.Map<WebAPI.Models.Billing.KalturaPaymentGWResponse>(response);

            return paymentGW;
        }

        public Models.Billing.KalturaPaymentGWResponse GetHouseholdPaymentGateways(int groupId, string siteGuid, long householdId)
        {
            Models.Billing.KalturaPaymentGWResponse paymentGW = null;
            WebAPI.Billing.PaymentGWResponse response = null;
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

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }

            paymentGW = Mapper.Map<WebAPI.Models.Billing.KalturaPaymentGWResponse>(response);

            return paymentGW;
        }

         internal bool SetPaymentGateway(int groupId, int paymentGatewayId, KalturaPaymentGatewayData paymentGateway)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGW request = Mapper.Map<WebAPI.Billing.PaymentGW>(paymentGateway);
                    response = Billing.SetPaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGatewayId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetPaymentGateway.  groupID: {0}, exception: {1}", groupId, ex);
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

        public bool SetPaymentGWSettings(int groupId, int paymentGWID, Dictionary<string, string> payment_gateway_settings)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGWSettings[] configs = BillingMappings.ConvertPaymentGatewaySettings(payment_gateway_settings);
                    response = Billing.SetPaymentGWSettings(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGWID, configs);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetPaymentGWParams.  groupID: {0}, paymentGWID: {1}, exception: {2}", groupId, paymentGWID, ex);
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

        public bool DeletePaymentGW(int groupId, int paymentGwID)
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
                log.ErrorFormat("Error while DeletePaymentGW.  groupID: {0}, paymentGWID: {1}, exception: {2}", groupId, paymentGwID, ex);
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

        public bool DeletePaymentGWSettings(int groupId, int paymentGwID, Dictionary<string, string> payment_gateway_settings)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGWSettings[] settings = BillingMappings.ConvertPaymentGatewaySettings(payment_gateway_settings);
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

        public bool DeleteHouseholdPaymentGateway(int groupId, int paymentGatewayId, string siteGuid, string householdID)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    int house_hold_id = int.Parse(householdID);
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

        public bool InsertPaymentGateway(int groupId, Models.Billing.KalturaPaymentGatewayData pgw)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGW request = Mapper.Map<WebAPI.Billing.PaymentGW>(pgw);
                    response = Billing.InsertPaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertPaymentGW.  groupID: {0}, exception: {1}", groupId, ex);
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

        public bool InsertPaymentGatewaySettings(int groupId, int paymentGatewayId, Dictionary<string, string> payment_gateway_settings)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGWSettings[] request = BillingMappings.ConvertPaymentGatewaySettings(payment_gateway_settings);
                    response = Billing.InsertPaymentGatewaySettings(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGatewayId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertPaymentGWParams.  groupID: {0}, paymentGwID: {1} ,exception: {2}", groupId, paymentGatewayId, ex);
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

        public bool SetHouseHoldPaymentGateway(int groupId, int paymentGwID, string siteGuid, long householdID)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.SetHouseHoldPaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGwID, siteGuid, (int)householdID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertPaymentGWHouseHold.  groupID: {0}, paymentGwID: {1} ,siteGuid: {2}, exception: {3}", groupId, paymentGwID, siteGuid, ex);
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

        internal string GetHouseholdChargeID(int groupId, string externalIdentifier, string householdId)
        {
            WebAPI.Billing.PaymentGWChargeIDResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    int hhID = int.Parse(householdId);
                    response = Billing.GetHouseholdChargeID(group.BillingCredentials.Username, group.BillingCredentials.Password, externalIdentifier, hhID);
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

            if (response.Resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Resp.Code, response.Resp.Message);
            }

            return response.ChargeID;
        }

        #endregion

    }
}