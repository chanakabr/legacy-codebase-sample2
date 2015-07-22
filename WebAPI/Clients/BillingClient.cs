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

        public Models.Billing.KalturaPaymentGWResponse GetPaymentGW(int groupId)
        {
            Models.Billing.KalturaPaymentGWResponse paymentGW = null;
            WebAPI.Billing.PaymentGWResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.GetPaymentGW(group.BillingCredentials.Username, group.BillingCredentials.Password);
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

        public Models.Billing.KalturaPaymentGWResponse GetHouseHoldPaymentGW(int groupId, string siteGuid, string householdID)
        {
            Models.Billing.KalturaPaymentGWResponse paymentGW = null;
            WebAPI.Billing.PaymentGWResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    int house_hold_id = int.Parse(householdID);
                    response = Billing.GetHouseHoldPaymentGW(group.BillingCredentials.Username, group.BillingCredentials.Password, siteGuid, house_hold_id);
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

        public bool SetPaymentGW(int groupId, int paymentGWID, string name, string url, string externalIdentifier, int pendingInterval, int pendingRetries,
            string sharedSecret, int? isDefault, int? isActive)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.SetPaymentGW(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGWID, name, url, externalIdentifier,
                        pendingInterval, pendingRetries, sharedSecret, isDefault, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetPaymentGW.  groupID: {0}, paymentGWID: {1}, name: {2}, url: {3}, isDefault: {4}, isActive: {5}, exception: {6}",
                    groupId, paymentGWID, name, url, isDefault, isActive, ex);
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
                    response = Billing.DeletePaymentGW(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGwID);
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

        public bool DeletePaymentGWHouseHold(int groupId, int paymentGwID, string siteGuid, string householdID)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    int house_hold_id = int.Parse(householdID);
                    response = Billing.DeletePaymentGWHouseHold(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGwID, siteGuid, house_hold_id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeletePaymentGWHouseHold.  groupID: {0}, paymentGWID: {1}, siteGuid: {2}, exception: {3}", groupId, paymentGwID, siteGuid, ex);
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

        public bool InsertPaymentGW(int groupId, Models.Billing.KalturaPaymentGW pgw)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGW request = Mapper.Map<WebAPI.Billing.PaymentGW>(pgw);
                    response = Billing.InsertPaymentGW(group.BillingCredentials.Username, group.BillingCredentials.Password, request);
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

        public bool InsertPaymentGWSettings(int groupId, int paymentGwID, Dictionary<string, string> payment_gateway_settings)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGWSettings[] request = BillingMappings.ConvertPaymentGatewaySettings(payment_gateway_settings);
                    response = Billing.InsertPaymentGWSettings(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGwID, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertPaymentGWParams.  groupID: {0}, paymentGwID: {1} ,exception: {2}", groupId, paymentGwID, ex);
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

        public bool InsertPaymentGWHouseHold(int groupId, int paymentGwID, string siteGuid, string householdID, string ChargeID)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    int house_hold_id = int.Parse(householdID);
                    response = Billing.InsertPaymentGWHouseHold(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGwID, siteGuid, house_hold_id, ChargeID);
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

        internal bool SetHouseholdChargeID(int groupId, string externalIdentifier, string householdId, string chargeId)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    int hhID = int.Parse(householdId);
                    response = Billing.SetHouseholdChargeID(group.BillingCredentials.Username, group.BillingCredentials.Password, externalIdentifier, hhID, chargeId);
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

        internal Models.Billing.KalturaPaymentGWHouseholdResponse GetHouseholdChargeID(int groupId, string externalIdentifier, string householdId)
        {
            Models.Billing.KalturaPaymentGWHouseholdResponse paymentGWHouseholdResponse = null;
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

            paymentGWHouseholdResponse = Mapper.Map<WebAPI.Models.Billing.KalturaPaymentGWHouseholdResponse>(response);

            return paymentGWHouseholdResponse;
        }

        #endregion
    }
}