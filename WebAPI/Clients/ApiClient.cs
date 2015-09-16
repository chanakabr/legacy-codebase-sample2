using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Api;
using WebAPI.Exceptions;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Models;
using WebAPI.Models.General;
using WebAPI.Utils;
using KLogMonitor;
using System.Reflection;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using AutoMapper;
using WebAPI.ObjectsConvertor.Mapping;


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

        internal WebAPI.Models.API.KalturaPinResponse GetUserParentalPIN(int groupId, string userId)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetParentalPIN(group.ApiCredentials.Username, group.ApiCredentials.Password, 0, userId);
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

        internal WebAPI.Models.API.KalturaPinResponse GetDomainParentalPIN(int groupId, int domainId)
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

        internal bool SetUserParentalPIN(int groupId, string userId, string pin)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.SetParentalPIN(group.ApiCredentials.Username, group.ApiCredentials.Password, 0, userId, pin);
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

        internal bool SetDomainParentalRules(int groupId, int domainId, string pin)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.SetParentalPIN(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty, pin);
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

        internal bool SetUserPurchaseSettings(int groupId, string userId, int settings)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.SetPurchaseSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, 0, userId, settings);
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

        internal bool SetDomainPurchaseSettings(int groupId, int domainId, int settings)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.SetPurchaseSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty, settings);
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

        internal WebAPI.Models.API.KalturaPurchaseSettingsResponse GetUserPurchasePIN(int groupId, string userId)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetPurchasePIN(group.ApiCredentials.Username, group.ApiCredentials.Password, 0, userId);
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

        internal WebAPI.Models.API.KalturaPurchaseSettingsResponse GetDomainPurchasePIN(int groupId, int domainId)
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

        internal WebAPI.Models.API.KalturaPurchaseSettingsResponse GetUserPurchaseSettings(int groupId, string userId)
        {
            string pin = string.Empty;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Api.GetPurchaseSettings(group.ApiCredentials.Username, group.ApiCredentials.Password, 0, userId);
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

        internal WebAPI.Models.API.KalturaPurchaseSettingsResponse GetDomainPurchaseSettings(int groupId, int domainId)
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

        internal bool SetUserPurchasePIN(int groupId, string userId, string pin)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.SetPurchasePIN(group.ApiCredentials.Username, group.ApiCredentials.Password, 0, userId, pin);
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

        internal bool SetDomainPurchasePIN(int groupId, int domainId, string pin)
        {
            bool success = false;

            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Api.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.SetPurchasePIN(group.ApiCredentials.Username, group.ApiCredentials.Password, domainId, string.Empty, pin);
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

        internal List<Models.API.KalturaGenericRule> GetMediaRules(int groupId, string userId, long mediaId, int domainId, string udid)
        {
            GenericRuleResponse response = null;
            List<Models.API.KalturaGenericRule> rules = new List<Models.API.KalturaGenericRule>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetMediaRules(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, mediaId, domainId, Utils.Utils.GetClientIP(), udid);
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

        internal List<Models.API.KalturaGenericRule> GetEpgRules(int groupId, string userId, long epgId, int domainId)
        {
            GenericRuleResponse response = null;
            List<Models.API.KalturaGenericRule> rules = new List<Models.API.KalturaGenericRule>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Api.GetEpgRules(group.ApiCredentials.Username, group.ApiCredentials.Password, userId, epgId, 0, domainId, Utils.Utils.GetClientIP());
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

        internal List<KalturaOSSAdapterBaseProfile> GetOSSAdapter(int groupId)
        {
            List<Models.API.KalturaOSSAdapterBaseProfile> KalturaOSSAdapterBaseProfileList = null;
            WebAPI.Api.OSSAdapterResponse response = null;
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

        internal bool InsertOSSAdapter(int groupId, KalturaOSSAdapterProfile ossAdapter)
        {
            WebAPI.Api.Status response = null;
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

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal bool SetOSSAdapter(int groupId, int ossAdapterId, KalturaOSSAdapterProfile ossAdapter)
        {
            WebAPI.Api.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Api.OSSAdapter request = Mapper.Map<WebAPI.Api.OSSAdapter>(ossAdapter);
                    response = Api.SetOSSAdapter(group.ApiCredentials.Username, group.ApiCredentials.Password, ossAdapterId, request);
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

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
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
    }
}