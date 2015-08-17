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

        public List<Models.Billing.KalturaPaymentGatewayProfile> GetPaymentGatewateSettings(int groupId)
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


            KalturaPaymentGatewayProfileList = Mapper.Map<List<Models.Billing.KalturaPaymentGatewayProfile>>(response.pgw);

            return KalturaPaymentGatewayProfileList;
        }

        public List<Models.Billing.KalturaPaymentGatewayBaseProfile> GetPaymentGateway(int groupId)
        {
            List<Models.Billing.KalturaPaymentGatewayBaseProfile>  KalturaPaymentGatewayBaseProfileList = null;
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

            KalturaPaymentGatewayBaseProfileList = Mapper.Map<List<Models.Billing.KalturaPaymentGatewayBaseProfile>>(response.pgw);

            return KalturaPaymentGatewayBaseProfileList;
        }

        public Models.Billing.KalturaPaymentGateway GetSelectedHouseholdPaymentGateway(int groupId, long householdId)
        {
            Models.Billing.KalturaPaymentGateway paymentGW = null;
            WebAPI.Billing.HouseholdPaymentGatewayResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Billing.GetSelectedHouseholdPaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, (int)householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetSelectedHouseholdPaymentGateway.  groupID: {0}, exception: {1}", groupId, ex);
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

            paymentGW = Mapper.Map<WebAPI.Models.Billing.KalturaPaymentGateway>(response);

            return paymentGW;
        }
        public List<Models.Billing.KalturaPaymentGatewayBaseProfile> GetHouseholdPaymentGateways(int groupId, string siteGuid, long householdId)
        {
            List<Models.Billing.KalturaPaymentGatewayBaseProfile> KalturaPaymentGatewayBaseProfileList = null;
            WebAPI.Billing.PaymentGatewayResponse response = null;
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

            KalturaPaymentGatewayBaseProfileList = Mapper.Map<List<Models.Billing.KalturaPaymentGatewayBaseProfile>>(response.pgw);

            return KalturaPaymentGatewayBaseProfileList;
        }

         internal bool SetPaymentGateway(int groupId, int paymentGatewayId, KalturaPaymentGatewayProfile paymentGateway)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGateway request = Mapper.Map<WebAPI.Billing.PaymentGateway>(paymentGateway);
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

        public bool InsertPaymentGateway(int groupId, Models.Billing.KalturaPaymentGatewayProfile pgw)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGateway request = Mapper.Map<WebAPI.Billing.PaymentGateway>(pgw);
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

        public bool InsertPaymentGatewaySettings(int groupId, int paymentGatewayId, Dictionary<string, KalturaStringValue> payment_gateway_settings)
        {
            WebAPI.Billing.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Billing.PaymentGatewaySettings[] request = BillingMappings.ConvertPaymentGatewaySettings(payment_gateway_settings);
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
                    response = Billing.SetHouseholdPaymentGateway(group.BillingCredentials.Username, group.BillingCredentials.Password, paymentGwID, siteGuid, (int)householdID);
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
            WebAPI.Billing.PaymentGatewayChargeIDResponse response = null;

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

            if (response.ResponseStatus.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.ResponseStatus.Code, response.ResponseStatus.Message);
            }

            return response.ChargeID;
        }

        #endregion

    }
}