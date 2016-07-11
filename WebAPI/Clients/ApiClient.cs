using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebAPI.Api;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Utils;
using System.Linq;
using WebAPI.Models.ConditionalAccess;
using System.Net;
using System.Web;

namespace WebAPI.Clients
{
    public class ApiClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ApiClient()
        {
        }

        protected WebAPI.Api.API Api
        {
            get
            {
                return (Module as WebAPI.Api.API);
            }
        }

        public LanguageObj[] GetGroupLanguages(string username, string password)
        {
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    return Api.GetGroupLanguages(username, password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get languages. username: {0}, exception: {1}", username, ex);
                throw new ClientException((int)StatusCode.InternalConnectionIssue, "Error while calling API web service");
            }
        }

        public LanguageObj[] GetGroupLanguages(int groupId)
        {
            Group group = GroupsManager.GetGroup(groupId);
            return GetGroupLanguages(group.ApiCredentials.Username, group.ApiCredentials.Password);
        }

        #region Parental Rules

        internal List<Models.API.KalturaParentalRule> GetGroupParentalRules(int groupId)
        {
            ParentalRulesResponse response = null;
            List<Models.API.KalturaParentalRule> rules = new List<Models.API.KalturaParentalRule>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetParentalRules(group.ApiCredentials.Username, group.ApiCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.status.Code, response.status.Message);
            }

            rules = AutoMapper.Mapper.Map<List<WebAPI.Models.API.KalturaParentalRule>>(response.rules);

            return rules;
        }

        internal List<Models.API.KalturaParentalRule> GetUserParentalRules(int groupId, string userId)
        {
            ParentalRulesResponse response = null;
            List<Models.API.KalturaParentalRule> rules = new List<Models.API.KalturaParentalRule>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetUserParentalRules(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.status.Code, response.status.Message);
            }

            rules = AutoMapper.Mapper.Map<List<WebAPI.Models.API.KalturaParentalRule>>(response.rules);

            return rules;
        }

        internal List<Models.API.KalturaParentalRule> GetDomainParentalRules(int groupId, int domainId)
        {
            ParentalRulesResponse response = null;
            List<Models.API.KalturaParentalRule> rules = new List<Models.API.KalturaParentalRule>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetDomainParentalRules(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.status.Code, response.status.Message);
            }

            rules = AutoMapper.Mapper.Map<List<WebAPI.Models.API.KalturaParentalRule>>(response.rules);

            return rules;
        }

        internal bool SetUserParentalRule(int groupId, string userId, long ruleId, int isActive)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.SetUserParentalRules(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, ruleId, isActive, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;
        }

        internal bool SetDomainParentalRules(int groupId, int domainId, long ruleId, int isActive)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.SetDomainParentalRules(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, ruleId, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;
        }

        internal KalturaPin GetUserParentalPIN(int groupId, string userId, int householdId = 0)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetParentalPIN(group.ApiCredentials.Username, group.ApiCredentials.Password, householdId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            KalturaPin response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPin>(webServiceResponse);

            return response;
        }

        [Obsolete]
        internal WebAPI.Models.API.KalturaPinResponse GetUserParentalPINOldStandard(int groupId, string userId, int householdId = 0)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetParentalPIN(group.ApiCredentials.Username, group.ApiCredentials.Password, householdId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            WebAPI.Models.API.KalturaPinResponse response = null;

            response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPinResponse>(webServiceResponse);

            return response;
        }

        internal KalturaPin GetDomainParentalPIN(int groupId, int domainId)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetParentalPIN(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            KalturaPin response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPin>(webServiceResponse);

            return response;
        }

        [Obsolete]
        internal WebAPI.Models.API.KalturaPinResponse GetDomainParentalPinOldStandard(int groupId, int domainId)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetParentalPIN(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            WebAPI.Models.API.KalturaPinResponse response = null;

            response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPinResponse>(webServiceResponse);

            return response;
        }

        internal KalturaPin SetUserParentalPIN(int groupId, string userId, string pin)
        {
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.UpdateParentalPIN(group.ApiCredentials.Username, group.ApiCredentials.Password, 0, userId, pin);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            KalturaPin response = AutoMapper.Mapper.Map<KalturaPin>(webServiceResponse);

            return response;
        }

        internal KalturaPin SetDomainParentalRules(int groupId, int domainId, string pin)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.UpdateParentalPIN(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty, pin);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            KalturaPin response = AutoMapper.Mapper.Map<KalturaPin>(webServiceResponse);

            return response;
        }

        internal KalturaPurchaseSettings SetUserPurchaseSettings(int groupId, string userId, int settings)
        {
            Group group = GroupsManager.GetGroup(groupId);

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.UpdatePurchaseSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, 0, userId, settings);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            
            KalturaPurchaseSettings response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPurchaseSettings>(webServiceResponse);

            return response;
        }

        internal KalturaPurchaseSettings SetDomainPurchaseSettings(int groupId, int domainId, int settings)
        {
            Group group = GroupsManager.GetGroup(groupId);

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.UpdatePurchaseSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty, settings);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }

            KalturaPurchaseSettings response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPurchaseSettings>(webServiceResponse);

            return response;
        }

        internal KalturaPurchaseSettings GetUserPurchasePIN(int groupId, string userId, int householdId = 0)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetPurchasePIN(group.ApiCredentials.Username, group.ApiCredentials.Password, householdId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            KalturaPurchaseSettings response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPurchaseSettings>(webServiceResponse);

            return response;
        }

        [Obsolete]
        internal WebAPI.Models.API.KalturaPurchaseSettingsResponse GetUserPurchasePinOldStandard(int groupId, string userId, int householdId = 0)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetPurchasePIN(group.ApiCredentials.Username, group.ApiCredentials.Password, householdId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            WebAPI.Models.API.KalturaPurchaseSettingsResponse response = null;

            response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPurchaseSettingsResponse>(webServiceResponse);

            return response;
        }

        internal KalturaPurchaseSettings GetDomainPurchasePIN(int groupId, int domainId)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetPurchasePIN(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            KalturaPurchaseSettings response = AutoMapper.Mapper.Map<KalturaPurchaseSettings>(webServiceResponse);

            return response;
        }

        [Obsolete]
        internal WebAPI.Models.API.KalturaPurchaseSettingsResponse GetDomainPurchasePinOldstandard(int groupId, int domainId)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetPurchasePIN(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            WebAPI.Models.API.KalturaPurchaseSettingsResponse response = null;

            response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPurchaseSettingsResponse>(webServiceResponse);

            return response;
        }

        internal WebAPI.Models.API.KalturaPurchaseSettings GetUserPurchaseSettings(int groupId, string userId, int householdId = 0)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetPurchaseSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, householdId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            WebAPI.Models.API.KalturaPurchaseSettings response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPurchaseSettings>(webServiceResponse);

            return response;
        }

        [Obsolete]
        internal WebAPI.Models.API.KalturaPurchaseSettingsResponse GetUserPurchaseSettingsOldStandard(int groupId, string userId, int householdId = 0)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetPurchaseSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, householdId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            WebAPI.Models.API.KalturaPurchaseSettingsResponse response = null;

            response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPurchaseSettingsResponse>(webServiceResponse);

            return response;
        }

        internal WebAPI.Models.API.KalturaPurchaseSettings GetDomainPurchaseSettings(int groupId, int domainId)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetPurchaseSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            WebAPI.Models.API.KalturaPurchaseSettings response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPurchaseSettings>(webServiceResponse);

            return response;
        }

        [Obsolete]
        internal WebAPI.Models.API.KalturaPurchaseSettingsResponse GetDomainPurchaseSettingsOldStandard(int groupId, int domainId)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetPurchaseSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }
            else
            {
                pin = webServiceResponse.pin;
            }

            WebAPI.Models.API.KalturaPurchaseSettingsResponse response = null;

            response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPurchaseSettingsResponse>(webServiceResponse);

            return response;
        }

        internal KalturaPurchaseSettings SetUserPurchasePIN(int groupId, string userId, string pin)
        {
            Group group = GroupsManager.GetGroup(groupId);

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.UpdatePurchasePIN(group.ApiCredentials.Username, group.ApiCredentials.Password, 0, userId, pin);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }

            KalturaPurchaseSettings response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPurchaseSettings>(webServiceResponse);

            return response;
        }

        internal KalturaPurchaseSettings SetDomainPurchasePIN(int groupId, int domainId, string pin)
        {
            Group group = GroupsManager.GetGroup(groupId);

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.UpdatePurchasePIN(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty, pin);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status.Code, webServiceResponse.status.Message);
            }

            KalturaPurchaseSettings response = AutoMapper.Mapper.Map<WebAPI.Models.API.KalturaPurchaseSettings>(webServiceResponse);

            return response;
        }

        internal List<Models.API.KalturaParentalRule> GetUserMediaParentalRules(int groupId, string userId, long mediaId)
        {
            ParentalRulesResponse response = null;
            List<Models.API.KalturaParentalRule> rules = new List<Models.API.KalturaParentalRule>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetParentalMediaRules(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, mediaId, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.status.Code, response.status.Message);
            }

            rules = AutoMapper.Mapper.Map<List<WebAPI.Models.API.KalturaParentalRule>>(response.rules);

            return rules;
        }

        internal List<Models.API.KalturaParentalRule> GetUserEPGParentalRules(int groupId, string userId, long epgId)
        {
            ParentalRulesResponse response = null;
            List<Models.API.KalturaParentalRule> rules = new List<Models.API.KalturaParentalRule>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetParentalEPGRules(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, epgId, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.status.Code, response.status.Message);
            }

            rules = AutoMapper.Mapper.Map<List<WebAPI.Models.API.KalturaParentalRule>>(response.rules);

            return rules;
        }

        internal bool ValidateParentalPIN(int groupId, string userId, string pin)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.ValidateParentalPIN(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, pin, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;
        }

        internal bool ValidatePurchasePIN(int groupId, string userId, string pin)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.ValidatePurchasePIN(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, pin, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;
        }

        internal bool DisableUserDefaultParentalRule(int groupId, string userId)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.DisableUserDefaultParentalRule(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;
        }

        internal bool DisableDomainDefaultParentalRule(int groupId, int domainId)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.DisableDomainDefaultParentalRule(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;
        }

        [Obsolete]
        internal List<Models.API.KalturaGenericRule> GetMediaRulesOldStandard(int groupId, string userId, long mediaId, int domainId, string udid, 
            KalturaUserAssetRuleOrderBy orderBy = KalturaUserAssetRuleOrderBy.NAME_ASC)
        {
            GenericRuleResponse response = null;
            List<Models.API.KalturaGenericRule> rules = new List<Models.API.KalturaGenericRule>();

            Group group = GroupsManager.GetGroup(groupId);

            //convert order by
            GenericRuleOrderBy wsOrderBy = ApiMappings.ConvertUserAssetRuleOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetMediaRules(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, mediaId, domainId, Utils.Utils.GetClientIP(), udid, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            rules = AutoMapper.Mapper.Map<List<WebAPI.Models.API.KalturaGenericRule>>(response.Rules);

            return rules;
        }

        internal List<KalturaUserAssetRule> GetMediaRules(int groupId, string userId, long mediaId, int domainId, string udid, KalturaUserAssetRuleOrderBy orderBy = KalturaUserAssetRuleOrderBy.NAME_ASC)
        {
            GenericRuleResponse response = null;
            List<KalturaUserAssetRule> rules = new List<KalturaUserAssetRule>();

            Group group = GroupsManager.GetGroup(groupId);

            //convert order by
            GenericRuleOrderBy wsOrderBy = ApiMappings.ConvertUserAssetRuleOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetMediaRules(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, mediaId, domainId, Utils.Utils.GetClientIP(), udid, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            rules = AutoMapper.Mapper.Map<List<KalturaUserAssetRule>>(response.Rules);

            return rules;
        }

        [Obsolete]
        internal List<Models.API.KalturaGenericRule> GetEpgRulesOldStandard(int groupId, string userId, long epgId, int domainId, KalturaUserAssetRuleOrderBy orderBy = KalturaUserAssetRuleOrderBy.NAME_ASC)
        {
            GenericRuleResponse response = null;
            List<Models.API.KalturaGenericRule> rules = new List<Models.API.KalturaGenericRule>();

            Group group = GroupsManager.GetGroup(groupId);
            
            //convert order by
            GenericRuleOrderBy wsOrderBy = ApiMappings.ConvertUserAssetRuleOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetEpgRules(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, epgId, 0, domainId, Utils.Utils.GetClientIP(), wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            rules = AutoMapper.Mapper.Map<List<WebAPI.Models.API.KalturaGenericRule>>(response.Rules);

            return rules;
        }

        internal List<KalturaUserAssetRule> GetEpgRules(int groupId, string userId, long epgId, int domainId, KalturaUserAssetRuleOrderBy orderBy = KalturaUserAssetRuleOrderBy.NAME_ASC)
        {
            GenericRuleResponse response = null;
            List<KalturaUserAssetRule> rules = new List<KalturaUserAssetRule>();

            Group group = GroupsManager.GetGroup(groupId);
            
            //convert order by
            GenericRuleOrderBy wsOrderBy = ApiMappings.ConvertUserAssetRuleOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetEpgRules(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, epgId, 0, domainId, Utils.Utils.GetClientIP(), wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            rules = AutoMapper.Mapper.Map<List<KalturaUserAssetRule>>(response.Rules);

            return rules;
        }
        #endregion

        internal Dictionary<string,int> GetErrorCodesDictionary()
        {
            StatusErrorCodesResponse response = null;
            Dictionary<string, int> codes = new Dictionary<string, int>();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetErrorCodesDictionary("api_1", "11111");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            codes = WebAPI.ObjectsConvertor.Mapping.ApiMappings.ConvertErrorsDictionary(response.ErrorsDictionary);

            return codes;
        }

        #region OSS Adapter

        internal List<KalturaOSSAdapterBaseProfile> GetOSSAdapter(int groupId)
        {
            List<Models.API.KalturaOSSAdapterBaseProfile> KalturaOSSAdapterBaseProfileList = null;
            WebAPI.Api.OSSAdapterResponseList response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetOSSAdapter(group.ApiCredentials.Username, group.ApiCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetOSSAdapter. groupID: {0}, exception: {1}", groupId, ex);
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

            KalturaOSSAdapterBaseProfileList = Mapper.Map<List<Models.API.KalturaOSSAdapterBaseProfile>>(response.OSSAdapters);

            return KalturaOSSAdapterBaseProfileList;
        }

        internal bool DeleteOSSAdapter(int groupId, int ossAdapterId)
        {
            WebAPI.Api.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.DeleteOSSAdapter(group.ApiCredentials.Username, group.ApiCredentials.Password, ossAdapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteOSSAdapter.  groupID: {0}, paymentGWID: {1}, exception: {2}", groupId, ossAdapterId, ex);
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

        internal KalturaOSSAdapterProfile InsertOSSAdapter(int groupId, KalturaOSSAdapterProfile ossAdapter)
        {
            WebAPI.Api.OSSAdapterResponse response = null;
            KalturaOSSAdapterProfile kalturaOSSAdapterProfile = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.OSSAdapter request = Mapper.Map<WebAPI.Api.OSSAdapter>(ossAdapter);
                    response = Api.InsertOSSAdapter(group.ApiCredentials.Username, group.ApiCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertOSSAdapter.  groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaOSSAdapterProfile = Mapper.Map<Models.API.KalturaOSSAdapterProfile>(response);
            return kalturaOSSAdapterProfile;
        }

        internal KalturaOSSAdapterProfile SetOSSAdapter(int groupId, KalturaOSSAdapterProfile ossAdapter)
        {
            WebAPI.Api.OSSAdapterResponse response = null;
            KalturaOSSAdapterProfile kalturaOSSAdapterProfile = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.OSSAdapter request = Mapper.Map<WebAPI.Api.OSSAdapter>(ossAdapter);
                    response = Api.SetOSSAdapter(group.ApiCredentials.Username, group.ApiCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetOSSAdapter. groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaOSSAdapterProfile = Mapper.Map<Models.API.KalturaOSSAdapterProfile>(response);
            return kalturaOSSAdapterProfile;
        }

        internal List<KalturaOSSAdapterProfile> GetOSSAdapterSettings(int groupId)
        {
            List<Models.API.KalturaOSSAdapterProfile> KalturaOSSAdapterProfileList = null;
            WebAPI.Api.OSSAdapterSettingsResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetOSSAdapterSettings(group.ApiCredentials.Username, group.ApiCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetOSSAdapterSettings. groupID: {0}, exception: {1}", groupId, ex);
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


            KalturaOSSAdapterProfileList = Mapper.Map<List<Models.API.KalturaOSSAdapterProfile>>(response.OSSAdapters);

            return KalturaOSSAdapterProfileList;
        }

        internal bool DeleteOSSAdapterSettings(int groupId, int ossAdapterId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            WebAPI.Api.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.OSSAdapterSettings[] request = ApiMappings.ConvertOSSAdapterSettings(settings);
                    response = Api.DeleteOSSAdapterSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, ossAdapterId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteOSSAdapterSettings.  groupID: {0}, ossAdapterId: {1}, exception: {2}", groupId, ossAdapterId, ex);
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

        internal bool InsertOSSAdapterSettings(int groupId, int ossAdapterId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            WebAPI.Api.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.OSSAdapterSettings[] request = ApiMappings.ConvertOSSAdapterSettings(settings);
                    response = Api.InsertOSSAdapterSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, ossAdapterId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertOSSAdapterSettings. groupID: {0}, oss_adapter_id: {1} ,exception: {2}", groupId, ossAdapterId, ex);
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

        internal bool SetOSSAdapterSettings(int groupId, int ossAdapterId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            WebAPI.Api.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.OSSAdapterSettings[] configs = ApiMappings.ConvertOSSAdapterSettings(settings);
                    response = Api.SetOSSAdapterSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, ossAdapterId, configs);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetOSSAdapterSettings. groupID: {0}, ossAdapterId: {1}, exception: {2}", groupId, ossAdapterId, ex);
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

        internal KalturaOSSAdapterProfile GenerateOSSSharedSecret(int groupId, int ossAdapterId)
        {
            WebAPI.Api.OSSAdapterResponse response = null;
            KalturaOSSAdapterProfile kalturaOSSAdapterProfile = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GenerateOSSSharedSecret(group.ApiCredentials.Username, group.ApiCredentials.Password, ossAdapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GenerateOSSSharedSecret. groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaOSSAdapterProfile = Mapper.Map<Models.API.KalturaOSSAdapterProfile>(response);
            return kalturaOSSAdapterProfile;
        }


        #endregion

        #region Recommendation Engine

        internal List<KalturaRecommendationProfile> GetRecommendationEngines(int groupId)
        {
            List<Models.API.KalturaRecommendationProfile> kalturaRecommendationEngineProfile = null;
            WebAPI.Api.RecommendationEnginesResponseList response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetRecommendationEngines(group.ApiCredentials.Username, group.ApiCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetRecommendationEngines. groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaRecommendationEngineProfile = Mapper.Map<List<Models.API.KalturaRecommendationProfile>>(response.RecommendationEngines);

            return kalturaRecommendationEngineProfile;
        }

        internal bool DeleteRecommendationEngine(int groupId, int recommendatioEngineId)
        {
            WebAPI.Api.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.DeleteRecommendationEngine(group.ApiCredentials.Username, group.ApiCredentials.Password, recommendatioEngineId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteRecommendationEngine.  groupID: {0}, recommendatioEngineId: {1}, exception: {2}", groupId, recommendatioEngineId, ex);
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

        internal KalturaRecommendationProfile InsertRecommendationEngine(int groupId, KalturaRecommendationProfile recommendationEngine)
        {
            WebAPI.Api.RecommendationEngineResponse response = null;
            KalturaRecommendationProfile kalturaRecommendationEngineProfile = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.RecommendationEngine request = Mapper.Map<WebAPI.Api.RecommendationEngine>(recommendationEngine);
                    response = Api.InsertRecommendationEngine(group.ApiCredentials.Username, group.ApiCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertRecommendationEngine.  groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaRecommendationEngineProfile = Mapper.Map<Models.API.KalturaRecommendationProfile>(response);
            return kalturaRecommendationEngineProfile;
        }

        internal KalturaRecommendationProfile SetRecommendationEngine(int groupId, int recommendationEngineId, KalturaRecommendationProfile recommendationEngine)
        {
            WebAPI.Api.RecommendationEngineResponse response = null;
            KalturaRecommendationProfile kalturaRecommendationEngineProfile = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.RecommendationEngine request = Mapper.Map<WebAPI.Api.RecommendationEngine>(recommendationEngine);
                    response = Api.SetRecommendationEngine(group.ApiCredentials.Username, group.ApiCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetRecommendationEngine. groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaRecommendationEngineProfile = Mapper.Map<Models.API.KalturaRecommendationProfile>(response);
            return kalturaRecommendationEngineProfile;
        }

        internal List<KalturaRecommendationProfile> GetRecommendationEngineSettings(int groupId)
        {
            List<Models.API.KalturaRecommendationProfile> KalturaRecommendationEngineList = null;
            WebAPI.Api.RecommendationEngineSettinsResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetRecommendationEngineSettings(group.ApiCredentials.Username, group.ApiCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetRecommendationEngineSettings. groupID: {0}, exception: {1}", groupId, ex);
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


            KalturaRecommendationEngineList = Mapper.Map<List<Models.API.KalturaRecommendationProfile>>(response.RecommendationEngines);

            return KalturaRecommendationEngineList;
        }

        internal bool DeleteRecommendationEngineSettings(int groupId, int recommendationEngineId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            WebAPI.Api.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.RecommendationEngineSettings[] request = ApiMappings.ConvertRecommendationEngineSettings(settings);
                    response = Api.DeleteRecommendationEngineSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, recommendationEngineId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteRecommendationEngineSettings.  groupID: {0}, recommendationEngineId: {1}, exception: {2}", groupId, recommendationEngineId, ex);
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

        internal bool InsertRecommendationEngineSettings(int groupId, int recommendationEngineId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            WebAPI.Api.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.RecommendationEngineSettings[] request = ApiMappings.ConvertRecommendationEngineSettings(settings);
                    response = Api.InsertRecommendationEngineSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, recommendationEngineId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertRecommendationEngineSettings. groupID: {0}, recommendationEngineId: {1} ,exception: {2}", groupId, recommendationEngineId, ex);
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

        internal bool SetRecommendationEngineSettings(int groupId, int recommendationEngineId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            WebAPI.Api.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.RecommendationEngineSettings[] configs = ApiMappings.ConvertRecommendationEngineSettings(settings);
                    response = Api.SetRecommendationEngineSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, recommendationEngineId, configs);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetRecommendationEngineSettings. groupID: {0}, recommendationEngineId: {1}, exception: {2}", groupId, recommendationEngineId, ex);
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

        internal KalturaRecommendationProfile GeneratereRecommendationEngineSharedSecret(int groupId, int recommendationEngineId)
        {
            WebAPI.Api.RecommendationEngineResponse response = null;
            KalturaRecommendationProfile kalturaRecommendationEngineProfile = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GenerateRecommendationEngineSharedSecret(group.ApiCredentials.Username, group.ApiCredentials.Password, recommendationEngineId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GeneratereRecommendationEngineSharedSecret. groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaRecommendationEngineProfile = Mapper.Map<Models.API.KalturaRecommendationProfile>(response);

            return kalturaRecommendationEngineProfile;
        }
        
        #endregion

        #region ExternalChannel
        internal KalturaExternalChannelProfile InsertExternalChannel(int groupId, KalturaExternalChannelProfile externalChannel)
        {
            WebAPI.Api.ExternalChannelResponse response = null;
            KalturaExternalChannelProfile kalturaExternalChannelProfile = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.ExternalChannel request = Mapper.Map<WebAPI.Api.ExternalChannel>(externalChannel);
                    response = Api.InsertExternalChannel(group.ApiCredentials.Username, group.ApiCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertExternalChannel.  groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaExternalChannelProfile = Mapper.Map<Models.API.KalturaExternalChannelProfile>(response);
            return kalturaExternalChannelProfile;
        }

        internal KalturaExternalChannelProfile SetExternalChannel(int groupId, KalturaExternalChannelProfile externalChannel)
        {
            WebAPI.Api.ExternalChannelResponse response = null;
            KalturaExternalChannelProfile kalturaExternalChannelProfile = null;


            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.ExternalChannel request = Mapper.Map<WebAPI.Api.ExternalChannel>(externalChannel);
                    response = Api.SetExternalChannel(group.ApiCredentials.Username, group.ApiCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetExternalChannel. groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaExternalChannelProfile = Mapper.Map<KalturaExternalChannelProfile>(response);
            return kalturaExternalChannelProfile;
        }

        internal bool DeleteExternalChannel(int groupId, int externalChannelId)
        {
            WebAPI.Api.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.DeleteExternalChannel(group.ApiCredentials.Username, group.ApiCredentials.Password, externalChannelId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteExternalChannel.  groupID: {0}, externalChannelId: {1}, exception: {2}", groupId, externalChannelId, ex);
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

        internal List<KalturaExternalChannelProfile> GetExternalChannels(int groupId)
        {
            List<Models.API.KalturaExternalChannelProfile> kalturaExternalChannelList = null;
            WebAPI.Api.ExternalChannelResponseList response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetExternalChannels(group.ApiCredentials.Username, group.ApiCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetExternalChannels. groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaExternalChannelList = Mapper.Map<List<Models.API.KalturaExternalChannelProfile>>(response.ExternalChannels);

            return kalturaExternalChannelList;
        }
        #endregion

        internal KalturaExportTask AddBulkExportTask(int groupId, string externalKey, string name, Models.API.KalturaExportDataType dataType, string filter, Models.API.KalturaExportType exportType, long frequency,
            string notificationUrl, List<int> vodTypes, bool isActive)
        {
            KalturaExportTask task = null;

            Group group = GroupsManager.GetGroup(groupId);

            BulkExportTaskResponse response = null;

            eBulkExportExportType wsExportType = ApiMappings.ConvertExportType(exportType);
            eBulkExportDataType wsDataType = ApiMappings.ConvertExportDataType(dataType);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.AddBulkExportTask(group.ApiCredentials.Username, group.ApiCredentials.Password, externalKey, name, wsDataType, filter, wsExportType, frequency,
                        notificationUrl, vodTypes != null ? vodTypes.ToArray(): null, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }
            
            task = AutoMapper.Mapper.Map<KalturaExportTask>(response.Task);

            return task;

        }

        internal KalturaExportTask UpdateBulkExportTask(int groupId, long id, string externalKey, string name, Models.API.KalturaExportDataType dataType, string filter, Models.API.KalturaExportType exportType, long frequency,
            string notificationUrl, List<int> vodTypes, bool? isActive)
        {
            Group group = GroupsManager.GetGroup(groupId);

            BulkExportTaskResponse response = null;

            WebAPI.Api.eBulkExportExportType wsExportType = ApiMappings.ConvertExportType(exportType);
            WebAPI.Api.eBulkExportDataType wsDataType = ApiMappings.ConvertExportDataType(dataType);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.UpdateBulkExportTask(group.ApiCredentials.Username, group.ApiCredentials.Password, id, externalKey, name, wsDataType, filter, wsExportType, frequency,
                        notificationUrl, vodTypes != null ? vodTypes.ToArray() : null, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            KalturaExportTask task = null;
            task = AutoMapper.Mapper.Map<KalturaExportTask>(response.Task);

            return task;
        }

        internal bool DeleteBulkExportTask(int groupId, long id, string externalKey)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.DeleteBulkExportTask(group.ApiCredentials.Username, group.ApiCredentials.Password, id, externalKey);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;
        }

        internal List<Models.API.KalturaExportTask> GetBulkExportTasks(int groupId, long[] ids, string[] externalKeys, KalturaExportTaskOrderBy orderBy)
        {
            List<Models.API.KalturaExportTask> tasks = new List<Models.API.KalturaExportTask>();
            BulkExportTasksResponse response = null;


            Group group = GroupsManager.GetGroup(groupId);
            BulkExportTaskOrderBy wsOrderBy = ApiMappings.ConvertExportTaskOrderBy(orderBy);


            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetBulkExportTasks(group.ApiCredentials.Username, group.ApiCredentials.Password, ids, externalKeys, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            tasks = AutoMapper.Mapper.Map<List<WebAPI.Models.API.KalturaExportTask>>(response.Tasks);

            return tasks;
        }

        internal List<KalturaUserRole> GetRoles(string username, string password, long[] roleIds = null)
        {
            List<KalturaUserRole> roles = new List<KalturaUserRole>();
            RolesResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetRoles(username, password, roleIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            roles = AutoMapper.Mapper.Map<List<KalturaUserRole>>(response.Roles);

            return roles;
        }

        public List<KalturaUserRole> GetRoles(int groupId, long[] roleIds = null)
        {
            Group group = GroupsManager.GetGroup(groupId);
            return GetRoles(group.ApiCredentials.Username, group.ApiCredentials.Password, roleIds);
        }

        internal List<KalturaUserRole> GetRoles()
        {
            try
            {
                return GetRoles("api_1", "11111");
            }
            catch (Exception ex)
            {
                log.Error("Failed to get roles for default group (api_1)", ex);
                return null;
            }
        }

        internal List<KalturaPermission> GetPermissions(int groupId, long[] ids)
        {
            List<KalturaPermission> permissions = new List<KalturaPermission>();
            PermissionsResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetPermissions(group.ApiCredentials.Username, group.ApiCredentials.Password, ids);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            permissions = AutoMapper.Mapper.Map<List<KalturaPermission>>(response.Permissions);

            return permissions;
        }

        internal KalturaUserRole AddRole(int groupId, KalturaUserRole role)
        {
            KalturaUserRole userRole = new KalturaUserRole();
            RolesResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //response = Api.AddRole(group.ApiCredentials.Username, group.ApiCredentials.Password, role.Name, role.Permissions);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            userRole = AutoMapper.Mapper.Map<KalturaUserRole>(response.Roles);

            return role;
        }

        internal KalturaPermission AddPermission(int groupId, KalturaPermission permission)
        {
            KalturaPermission addedPermission = new KalturaPermission();
            PermissionResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            ePermissionType type;
            string usersGroup = null;

            if (permission is KalturaGroupPermission)
            {
                type = ePermissionType.Group;
                usersGroup = ((KalturaGroupPermission)permission).Group;
            }
            else
            {
                type = ePermissionType.Normal;
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.AddPermission(group.ApiCredentials.Username, group.ApiCredentials.Password, permission.Name, 
                        permission.PermissionItems != null ? permission.PermissionItems.Select(p => p.getId()).ToArray() : null, type, usersGroup, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            addedPermission = AutoMapper.Mapper.Map<KalturaPermission>(response.Permission);

            return addedPermission;
        }

        internal bool AddPermissionToRole(int groupId, long roleId, long permissionId)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.AddPermissionToRole(group.ApiCredentials.Username, group.ApiCredentials.Password, roleId, permissionId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;
        }

        internal bool AddPermissionItemToPermission(int groupId, long permissionId, long permissionItemId)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.AddPermissionItemToPermission(group.ApiCredentials.Username, group.ApiCredentials.Password, permissionId, permissionItemId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;
        }


        #region KSQL Channel
        internal KalturaChannelProfile InsertKSQLChannel(int groupId, KalturaChannelProfile channel)
        {
            WebAPI.Api.KSQLChannelResponse response = null;
            KalturaChannelProfile profile = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.KSQLChannel request = Mapper.Map<WebAPI.Api.KSQLChannel>(channel);
                    response = Api.InsertKSQLChannel(group.ApiCredentials.Username, group.ApiCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertKSQLChannel.  groupID: {0}, exception: {1}", groupId, ex);
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

            profile = Mapper.Map<Models.API.KalturaChannelProfile>(response.Channel);
            return profile;
        }

        internal KalturaChannelProfile SetKSQLChannel(int groupId, KalturaChannelProfile channel)
        {
            WebAPI.Api.KSQLChannelResponse response = null;
            KalturaChannelProfile profile = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.KSQLChannel request = Mapper.Map<WebAPI.Api.KSQLChannel>(channel);
                    response = Api.SetKSQLChannel(group.ApiCredentials.Username, group.ApiCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetKSQLChannel. groupID: {0}, exception: {1}", groupId, ex);
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

            profile = Mapper.Map<KalturaChannelProfile>(response.Channel);
            return profile;
        }

        internal bool DeleteKSQLChannel(int groupId, int channelId)
        {
            WebAPI.Api.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.DeleteKSQLChannel(group.ApiCredentials.Username, group.ApiCredentials.Password, channelId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteKSQLChannel.  groupID: {0}, channelId: {1}, exception: {2}", groupId, channelId, ex);
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

        internal KalturaChannelProfile GetKSQLChannel(int groupId, int channelId)
        {
            KalturaChannelProfile profile = null;
            WebAPI.Api.KSQLChannelResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetKSQLChannel(group.ApiCredentials.Username, group.ApiCredentials.Password, channelId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetKSQLChannel. groupID: {0}, channelId: {1}, exception: {2}", groupId, channelId, ex);
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

            profile = Mapper.Map<KalturaChannelProfile>(response.Channel);

            return profile;
        }
        #endregion

        internal bool CleanUserHistory(int groupId, string userId, List<Models.Catalog.KalturaSlimAsset> assetsList)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;
            int mediaId = 0;
            List<int> mediaIds = new List<int>();            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    if (assetsList != null)
                    {
                       mediaIds = assetsList.Where(assetType => assetType.Type == Models.Catalog.KalturaAssetType.media
                            && int.TryParse(assetType.Id, out mediaId)).Select(x => int.Parse(x.Id)).ToList();
                    }

                    response = Api.CleanUserHistory(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, mediaIds.ToArray());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;
        }

        internal List<KalturaRegistrySettings> GetAllRegistry(int groupId)
        {
            List<KalturaRegistrySettings> registrySettings = new List<KalturaRegistrySettings>();

            Group group = GroupsManager.GetGroup(groupId);

            RegistryResponse response = null;
           try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetAllRegistry(group.ApiCredentials.Username, group.ApiCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }
            else
            {
                registrySettings = AutoMapper.Mapper.Map<List<KalturaRegistrySettings>>(response.registrySettings);

                return registrySettings;
            }
        }

        internal KalturaTimeShiftedTvPartnerSettings GetTimeShiftedTvPartnerSettings(int groupID)
        {
            Group group = GroupsManager.GetGroup(groupID);
            TimeShiftedTvPartnerSettingsResponse response = null;
            KalturaTimeShiftedTvPartnerSettings settings = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetTimeShiftedTvPartnerSettings(group.ApiCredentials.Username, group.ApiCredentials.Password);
                }                
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            else if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }
            else
            {
                settings = AutoMapper.Mapper.Map<KalturaTimeShiftedTvPartnerSettings>(response.Settings);

                return settings;
            }
        }

        internal bool UpdateTimeShiftedTvPartnerSettings(int groupID, KalturaTimeShiftedTvPartnerSettings settings)
        {
            bool isSuccess = false;
            Status response = null;
            Group group = GroupsManager.GetGroup(groupID);
            try
            {

                TimeShiftedTvPartnerSettings tstvSettings = null;
                tstvSettings = AutoMapper.Mapper.Map<TimeShiftedTvPartnerSettings>(settings);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.UpdateTimeShiftedTvPartnerSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, tstvSettings);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            else if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal KalturaCDNAdapterProfileListResponse GetCDNRAdapters(int groupId)
        {
            KalturaCDNAdapterProfileListResponse result = new KalturaCDNAdapterProfileListResponse() { TotalCount = 0 };

            Group group = GroupsManager.GetGroup(groupId);

            CDNAdapterListResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetCDNAdapters(group.ApiCredentials.Username, group.ApiCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Adapters.Length > 0)
            {
                result.TotalCount = response.Adapters.Length;
                result.Adapters = AutoMapper.Mapper.Map<List<KalturaCDNAdapterProfile>>(response.Adapters);
            }

            return result;
        }

        internal bool DeleteCDNAdapter(int groupId, int adapterId)
        {
            Group group = GroupsManager.GetGroup(groupId);

            Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.DeleteCDNAdapter(group.ApiCredentials.Username, group.ApiCredentials.Password, adapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaCDNAdapterProfile InsertCDNAdapter(int groupId, KalturaCDNAdapterProfile cdnAdapter)
        {
            KalturaCDNAdapterProfile adapter = new KalturaCDNAdapterProfile();

            Group group = GroupsManager.GetGroup(groupId);

            CDNAdapterResponse response = null;

            CDNAdapter wsAdapter = AutoMapper.Mapper.Map<CDNAdapter>(cdnAdapter);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.InsertCDNAdapter(group.ApiCredentials.Username, group.ApiCredentials.Password, wsAdapter);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            adapter = AutoMapper.Mapper.Map<KalturaCDNAdapterProfile>(response.Adapter);

            return adapter;
        }

        internal KalturaCDNAdapterProfile SetCDNAdapter(int groupId, KalturaCDNAdapterProfile adapter, int adapterId)
        {
            KalturaCDNAdapterProfile adapterResponse = new KalturaCDNAdapterProfile();

            Group group = GroupsManager.GetGroup(groupId);

            CDNAdapterResponse response = null;

            CDNAdapter wsAdapter = AutoMapper.Mapper.Map<CDNAdapter>(adapter);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.SetCDNAdapter(group.ApiCredentials.Username, group.ApiCredentials.Password, wsAdapter, adapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            adapterResponse = AutoMapper.Mapper.Map<KalturaCDNAdapterProfile>(response.Adapter);

            return adapterResponse;
        }

        internal KalturaCDNAdapterProfile GenerateCDNSharedSecret(int groupId, int adapterId)
        {
            KalturaCDNAdapterProfile adapter = new KalturaCDNAdapterProfile();

            Group group = GroupsManager.GetGroup(groupId);

            CDNAdapterResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GenerateCDNSharedSecret(group.ApiCredentials.Username, group.ApiCredentials.Password, adapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            adapter = AutoMapper.Mapper.Map<KalturaCDNAdapterProfile>(response.Adapter);

            return adapter;
        }

        internal KalturaCDNPartnerSettings GetCDNPartnerSettings(int groupId)
        {
            KalturaCDNPartnerSettings settings = new KalturaCDNPartnerSettings();

            Group group = GroupsManager.GetGroup(groupId);

            CDNPartnerSettingsResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetCDNPartnerSettings(group.ApiCredentials.Username, group.ApiCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            settings = AutoMapper.Mapper.Map<KalturaCDNPartnerSettings>(response.CDNPartnerSettings);

            return settings;
        }

        internal KalturaCDNPartnerSettings UpdateCDNSettings(int groupId, KalturaCDNPartnerSettings settings)
        {
            KalturaCDNPartnerSettings responseSettings = new KalturaCDNPartnerSettings();

            Group group = GroupsManager.GetGroup(groupId);

            CDNPartnerSettingsResponse response = null;
            try
            {
                CDNPartnerSettings requestSettings = AutoMapper.Mapper.Map<CDNPartnerSettings>(settings);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.UpdateCDNPartnerSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, requestSettings);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", Api.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            responseSettings = AutoMapper.Mapper.Map<KalturaCDNPartnerSettings>(response.CDNPartnerSettings);

            return responseSettings;
        }
    }
}

namespace WebAPI.Api
{
    // adding request ID to header
    public partial class API
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