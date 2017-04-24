using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
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
using System.ServiceModel;
using WebAPI.Models.Catalog;
using WebAPI.Models.Domains;
using WebAPI.Models.Users;
using ApiObjects;
using ApiObjects.Rules;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using ApiObjects.Roles;
using ApiObjects.CDNAdapter;
using ApiObjects.BulkExport;
using Core.Pricing;
using Newtonsoft.Json.Linq;
using Core.Api.Modules;

namespace WebAPI.Clients
{
    public class ApiClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ApiClient()
        {
        }

        public List<LanguageObj> GetGroupLanguages(int groupId)
        {

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    return Core.Api.Module.GetGroupLanguages(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get languages. group id: {0}, exception: {1}", groupId, ex);
                throw new ClientException((int)StatusCode.InternalConnectionIssue, "Error while calling API web service");
            }
        }

        #region Parental Rules

        internal List<Models.API.KalturaParentalRule> GetGroupParentalRules(int groupId)
        {
            ParentalRulesResponse response = null;
            List<Models.API.KalturaParentalRule> rules = new List<Models.API.KalturaParentalRule>();

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetParentalRules(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetUserParentalRules(groupId, userId, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetDomainParentalRules(groupId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.SetUserParentalRules(groupId, userId, ruleId, isActive, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.SetDomainParentalRules(groupId, domainId, ruleId, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

        internal KalturaPin GetUserParentalPIN(int groupId, string userId, int householdId = 0, int? ruleId = null)
        {
            string pin = string.Empty;

            

            PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetParentalPIN(groupId, householdId, userId, ruleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
        internal WebAPI.Models.API.KalturaPinResponse GetUserParentalPINOldStandard(int groupId, string userId, int householdId = 0, int? ruleId = null)
        {
            string pin = string.Empty;

            

            PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetParentalPIN(groupId, householdId, userId, ruleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

        internal KalturaPin GetDomainParentalPIN(int groupId, int domainId, int? ruleId = null)
        {
            string pin = string.Empty;

            

            PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetParentalPIN(groupId, domainId, string.Empty, ruleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
        internal WebAPI.Models.API.KalturaPinResponse GetDomainParentalPinOldStandard(int groupId, int domainId, int? ruleId)
        {
            string pin = string.Empty;

            

            PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetParentalPIN(groupId, domainId, string.Empty, ruleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

        internal KalturaPin SetUserParentalPIN(int groupId, string userId, string pin, int? ruleId)
        {
            

            PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.UpdateParentalPIN(groupId, 0, userId, pin, ruleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

        internal KalturaPin SetDomainParentalPIN(int groupId, int domainId, string pin, int? ruleId)
        {
            

            PinResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.UpdateParentalPIN(groupId, domainId, string.Empty, pin, ruleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.UpdatePurchaseSettings(groupId, 0, userId, settings);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.UpdatePurchaseSettings(groupId, domainId, string.Empty, settings);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetPurchasePIN(groupId, householdId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetPurchasePIN(groupId, householdId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetPurchasePIN(groupId, domainId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetPurchasePIN(groupId, domainId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetPurchaseSettings(groupId, householdId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetPurchaseSettings(groupId, householdId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetPurchaseSettings(groupId, domainId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.GetPurchaseSettings(groupId, domainId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.UpdatePurchasePIN(groupId, 0, userId, pin);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
            

            PurchaseSettingsResponse webServiceResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    webServiceResponse = Core.Api.Module.UpdatePurchasePIN(groupId, domainId, string.Empty, pin);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetParentalMediaRules(groupId, userId, mediaId, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetParentalEPGRules(groupId, userId, epgId, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

        internal bool ValidateParentalPIN(int groupId, string userId, string pin, int? ruleId)
        {
            bool success = false;

            

            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.ValidateParentalPIN(groupId, userId, pin, 0, ruleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.ValidatePurchasePIN(groupId, userId, pin, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.DisableUserDefaultParentalRule(groupId, userId, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.DisableDomainDefaultParentalRule(groupId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            //convert order by
            GenericRuleOrderBy wsOrderBy = ApiMappings.ConvertUserAssetRuleOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetMediaRules(groupId, userId, mediaId, domainId, Utils.Utils.GetClientIP(), udid, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            //convert order by
            GenericRuleOrderBy wsOrderBy = ApiMappings.ConvertUserAssetRuleOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetMediaRules(groupId, userId, mediaId, domainId, Utils.Utils.GetClientIP(), udid, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            //convert order by
            GenericRuleOrderBy wsOrderBy = ApiMappings.ConvertUserAssetRuleOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetEpgRules(groupId, userId, epgId, 0, domainId, Utils.Utils.GetClientIP(), wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            //convert order by
            GenericRuleOrderBy wsOrderBy = ApiMappings.ConvertUserAssetRuleOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetEpgRules(groupId, userId, epgId, 0, domainId, Utils.Utils.GetClientIP(), wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

        internal Dictionary<string, int> GetErrorCodesDictionary()
        {
            StatusErrorCodesResponse response = null;
            Dictionary<string, int> codes = new Dictionary<string, int>();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetErrorCodesDictionary();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

        internal List<KalturaOSSAdapterProfile> GetOSSAdapter(int groupId)
        {
            List<KalturaOSSAdapterProfile> KalturaOSSAdapterBaseProfileList = null;
            OSSAdapterResponseList response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetOSSAdapter(groupId);
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

            KalturaOSSAdapterBaseProfileList = Mapper.Map<List<KalturaOSSAdapterProfile>>(response.OSSAdapters);

            return KalturaOSSAdapterBaseProfileList;
        }

        internal KalturaOSSAdapterProfile GetOSSAdapter(int groupId, int ossAdapterId)
        {
            KalturaOSSAdapterProfile ossAdapter = null;
            OSSAdapterResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetOSSAdapterProfile(groupId, ossAdapterId);
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

            ossAdapter = Mapper.Map<Models.API.KalturaOSSAdapterProfile>(response);

            return ossAdapter;
        }

        internal bool DeleteOSSAdapter(int groupId, int ossAdapterId)
        {
            Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.DeleteOSSAdapter(groupId, ossAdapterId);
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
            OSSAdapterResponse response = null;
            KalturaOSSAdapterProfile kalturaOSSAdapterProfile = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    OSSAdapter request = Mapper.Map<OSSAdapter>(ossAdapter);
                    response = Core.Api.Module.InsertOSSAdapter(groupId, request);
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
            OSSAdapterResponse response = null;
            KalturaOSSAdapterProfile kalturaOSSAdapterProfile = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    OSSAdapter request = Mapper.Map<OSSAdapter>(ossAdapter);
                    response = Core.Api.Module.SetOSSAdapter(groupId, request);
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
            OSSAdapterSettingsResponse response = null;
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetOSSAdapterSettings(groupId);
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
            Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<OSSAdapterSettings> request = ApiMappings.ConvertOSSAdapterSettings(settings);
                    response = Core.Api.Module.DeleteOSSAdapterSettings(groupId, ossAdapterId, request);
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
            Status response = null;
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<OSSAdapterSettings> request = ApiMappings.ConvertOSSAdapterSettings(settings);
                    response = Core.Api.Module.InsertOSSAdapterSettings(groupId, ossAdapterId, request);
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
            Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<OSSAdapterSettings> configs = ApiMappings.ConvertOSSAdapterSettings(settings);
                    response = Core.Api.Module.SetOSSAdapterSettings(groupId, ossAdapterId, configs);
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
            OSSAdapterResponse response = null;
            KalturaOSSAdapterProfile kalturaOSSAdapterProfile = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GenerateOSSSharedSecret(groupId, ossAdapterId);
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
            RecommendationEnginesResponseList response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.ListRecommendationEngines(groupId);
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
            Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.DeleteRecommendationEngine(groupId, recommendatioEngineId);
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
            RecommendationEngineResponse response = null;
            KalturaRecommendationProfile kalturaRecommendationEngineProfile = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    RecommendationEngine request = Mapper.Map<RecommendationEngine>(recommendationEngine);
                    response = Core.Api.Module.InsertRecommendationEngine(groupId, request);
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
            RecommendationEngineResponse response = null;
            KalturaRecommendationProfile kalturaRecommendationEngineProfile = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    RecommendationEngine request = Mapper.Map<RecommendationEngine>(recommendationEngine);
                    request.ID = recommendationEngineId;
                    response = Core.Api.Module.SetRecommendationEngine(groupId, request);
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
            RecommendationEngineSettinsResponse response = null;
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetRecommendationEngineSettings(groupId);
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
            Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<RecommendationEngineSettings> request = ApiMappings.ConvertRecommendationEngineSettings(settings);
                    response = Core.Api.Module.DeleteRecommendationEngineSettings(groupId, recommendationEngineId, request);
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
            Status response = null;
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<RecommendationEngineSettings> request = ApiMappings.ConvertRecommendationEngineSettings(settings);
                    response = Core.Api.Module.InsertRecommendationEngineSettings(groupId, recommendationEngineId, request);
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
            Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<RecommendationEngineSettings> configs = ApiMappings.ConvertRecommendationEngineSettings(settings);
                    response = Core.Api.Module.SetRecommendationEngineSettings(groupId, recommendationEngineId, configs);
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
            RecommendationEngineResponse response = null;
            KalturaRecommendationProfile kalturaRecommendationEngineProfile = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GenerateRecommendationEngineSharedSecret(groupId, recommendationEngineId);
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
            ExternalChannelResponse response = null;
            KalturaExternalChannelProfile kalturaExternalChannelProfile = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    ExternalChannel request = Mapper.Map<ExternalChannel>(externalChannel);
                    response = Core.Api.Module.InsertExternalChannel(groupId, request);
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
            ExternalChannelResponse response = null;
            KalturaExternalChannelProfile kalturaExternalChannelProfile = null;


            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    ExternalChannel request = Mapper.Map<ExternalChannel>(externalChannel);
                    response = Core.Api.Module.SetExternalChannel(groupId, request);
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
            Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.DeleteExternalChannel(groupId, externalChannelId);
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
            ExternalChannelResponseList response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.ListExternalChannels(groupId);
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

            

            BulkExportTaskResponse response = null;

            eBulkExportExportType wsExportType = ApiMappings.ConvertExportType(exportType);
            eBulkExportDataType wsDataType = ApiMappings.ConvertExportDataType(dataType);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.AddBulkExportTask(groupId, externalKey, name, wsDataType, filter, wsExportType, frequency,
                        notificationUrl, vodTypes != null ? vodTypes : null, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
            

            BulkExportTaskResponse response = null;

            eBulkExportExportType wsExportType = ApiMappings.ConvertExportType(exportType);
            eBulkExportDataType wsDataType = ApiMappings.ConvertExportDataType(dataType);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.UpdateBulkExportTask(groupId, id, externalKey, name, wsDataType, filter, wsExportType, frequency,
                        notificationUrl, vodTypes != null ? vodTypes : null, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.DeleteBulkExportTask(groupId, id, externalKey);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

        internal List<Models.API.KalturaExportTask> GetBulkExportTasks(int groupId, List<long> ids, List<string> externalKeys, KalturaExportTaskOrderBy orderBy)
        {
            List<Models.API.KalturaExportTask> tasks = new List<Models.API.KalturaExportTask>();
            BulkExportTasksResponse response = null;


            
            BulkExportTaskOrderBy wsOrderBy = ApiMappings.ConvertExportTaskOrderBy(orderBy);


            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetBulkExportTasks(groupId, ids, externalKeys, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

        public List<KalturaUserRole> GetRoles(int groupId, List<long> roleIds = null)
        {

            List<KalturaUserRole> roles = new List<KalturaUserRole>();
            RolesResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetRoles(groupId, roleIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

        internal List<KalturaUserRole> GetRoles()
        {
            try
            {
                return GetRoles(1);
            }
            catch (Exception ex)
            {
                log.Error("Failed to get roles for default group (api_1)", ex);
                return null;
            }
        }

        internal List<KalturaPermission> GetPermissions(int groupId, List<long> ids)
        {
            List<KalturaPermission> permissions = new List<KalturaPermission>();
            PermissionsResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetPermissions(groupId, ids);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //response = Core.Api.Module.AddRole(groupId, role.Name, role.Permissions);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
                    List<long> permissionItemIds = null;
                    if (permission.PermissionItems != null)
                    {
                        permissionItemIds = new List<long>();
                        permissionItemIds.AddRange(permission.PermissionItems.Select(p => p.getId()));
                    }
                    response = Core.Api.Module.AddPermission(groupId, permission.Name, permissionItemIds, type, usersGroup, 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.AddPermissionToRole(groupId, roleId, permissionId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.AddPermissionItemToPermission(groupId, permissionId, permissionItemId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
        internal KalturaChannel InsertKSQLChannel(int groupId, KalturaChannel channel)
        {
            KSQLChannelResponse response = null;
            KalturaChannel profile = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = Core.Api.Module.InsertKSQLChannel(groupId, request);
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

            profile = Mapper.Map<KalturaChannel>(response.Channel);
            return profile;
        }

        internal KalturaChannel SetKSQLChannel(int groupId, KalturaChannel channel)
        {
            KSQLChannelResponse response = null;
            KalturaChannel profile = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = Core.Api.Module.SetKSQLChannel(groupId, request);
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

            profile = Mapper.Map<KalturaChannel>(response.Channel);
            return profile;
        }

        [Obsolete]
        internal KalturaChannelProfile InsertKSQLChannelProfile(int groupId, KalturaChannelProfile channel)
        {
            KSQLChannelResponse response = null;
            KalturaChannelProfile profile = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = Core.Api.Module.InsertKSQLChannel(groupId, request);
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

        [Obsolete]
        internal KalturaChannelProfile SetKSQLChannelProfile(int groupId, KalturaChannelProfile channel)
        {
            KSQLChannelResponse response = null;
            KalturaChannelProfile profile = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = Core.Api.Module.SetKSQLChannel(groupId, request);
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
            Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.DeleteKSQLChannel(groupId, channelId);
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
            KSQLChannelResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetKSQLChannel(groupId, channelId);
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

            

            Status response = null;
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

                    response = Core.Api.Module.CleanUserHistory(groupId, userId, mediaIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            RegistryResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetAllRegistry(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

        internal KalturaTimeShiftedTvPartnerSettings GetTimeShiftedTvPartnerSettings(int groupId)
        {
            
            TimeShiftedTvPartnerSettingsResponse response = null;
            KalturaTimeShiftedTvPartnerSettings settings = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetTimeShiftedTvPartnerSettings(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

        internal bool UpdateTimeShiftedTvPartnerSettings(int groupId, KalturaTimeShiftedTvPartnerSettings settings)
        {
            bool isSuccess = false;
            Status response = null;
            
            try
            {

                TimeShiftedTvPartnerSettings tstvSettings = null;
                tstvSettings = AutoMapper.Mapper.Map<TimeShiftedTvPartnerSettings>(settings);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.UpdateTimeShiftedTvPartnerSettings(groupId, tstvSettings);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            

            CDNAdapterListResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetCDNAdapters(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            if (response.Adapters.Count > 0)
            {
                result.TotalCount = response.Adapters.Count;
                result.Adapters = AutoMapper.Mapper.Map<List<KalturaCDNAdapterProfile>>(response.Adapters);
            }

            return result;
        }

        internal bool DeleteCDNAdapter(int groupId, int adapterId)
        {
            

            Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.DeleteCDNAdapter(groupId, adapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            

            CDNAdapterResponse response = null;

            CDNAdapter wsAdapter = AutoMapper.Mapper.Map<CDNAdapter>(cdnAdapter);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.InsertCDNAdapter(groupId, wsAdapter);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            

            CDNAdapterResponse response = null;

            CDNAdapter wsAdapter = AutoMapper.Mapper.Map<CDNAdapter>(adapter);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.SetCDNAdapter(groupId, wsAdapter, adapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            

            CDNAdapterResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GenerateCDNSharedSecret(groupId, adapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            

            CDNPartnerSettingsResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetCDNPartnerSettings(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            

            CDNPartnerSettingsResponse response = null;
            try
            {
                CDNPartnerSettings requestSettings = AutoMapper.Mapper.Map<CDNPartnerSettings>(settings);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.UpdateCDNPartnerSettings(groupId, requestSettings);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

        internal KalturaRegionListResponse GetRegions(int groupId, List<string> externalIds, KalturaRegionOrderBy orderBy)
        {
            List<KalturaRegion> regions = new List<KalturaRegion>();
            RegionsResponse response = null;

            RegionOrderBy wsOrderBy = ApiMappings.ConvertRegionOrderBy(orderBy);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetRegions(groupId, externalIds != null ? externalIds : null, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            regions = AutoMapper.Mapper.Map<List<KalturaRegion>>(response.Regions);

            return new KalturaRegionListResponse() { Regions = regions, TotalCount = regions != null ? regions.Count : 0 };
        }

        internal KalturaDeviceFamilyListResponse GetDeviceFamilyList(int groupId)
        {          
            
            KalturaDeviceFamilyListResponse result = new KalturaDeviceFamilyListResponse() { TotalCount = 0 };
            DeviceFamilyResponse response = null;            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetDeviceFamilyList(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            result.Objects = AutoMapper.Mapper.Map<List<KalturaDeviceFamily>>(response.DeviceFamilies);
            result.TotalCount = response.TotalItems;

            return result;
        }

        internal KalturaDeviceBrandListResponse GetDeviceBrandList(int groupId)
        {
            
            KalturaDeviceBrandListResponse result = new KalturaDeviceBrandListResponse() { TotalCount = 0 };
            DeviceBrandResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetDeviceBrandList(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            result.Objects = AutoMapper.Mapper.Map<List<KalturaDeviceBrand>>(response.DeviceBrands);
            result.TotalCount = response.TotalItems;

            return result;
        }

        internal KalturaCountryListResponse GetCountryList(int groupId, List<int> countryIds, KalturaCountryOrderBy? orderBy)
        {            
            KalturaCountryListResponse result = new KalturaCountryListResponse() { TotalCount = 0, Objects = new List<KalturaCountry>() };
            CountryLocaleResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetCountryList(groupId, countryIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            if (response.CountryLocales != null && response.CountryLocales.Count > 0)
            {
                result.Objects = AutoMapper.Mapper.Map<List<KalturaCountry>>(response.CountryLocales);                
            }

            if (response.Countries != null && response.Countries.Count > 0)
            {
                result.Objects.AddRange(AutoMapper.Mapper.Map<List<KalturaCountry>>(response.Countries));                
            }

            if (orderBy.HasValue)
            {
                switch (orderBy.Value)
                {
                    case KalturaCountryOrderBy.NAME_ASC:
                        result.Objects = result.Objects.OrderBy(x => x.Name).ToList();
                        break;
                    default:
                        break;
                }
            }

            result.TotalCount = result.Objects.Count;

            return result;
        }

        internal KalturaMetaListResponse GetGroupMeta(int groupId, KalturaAssetType? assetType, KalturaMetaType? metaType, KalturaMetaFieldName? fieldNameEqual, KalturaMetaFieldName? fieldNameNotEqual)
        {
            
            KalturaMetaListResponse result = new KalturaMetaListResponse();
            MetaResponse response = null;
            try
            {
                eAssetTypes wsAssetType = ApiMappings.ConvertAssetType(assetType);
                MetaType wsMetaType = ApiMappings.ConvertMetaType(metaType);
                MetaFieldName wsFieldNameEqual = ApiMappings.ConvertMetaFieldName(fieldNameEqual);
                MetaFieldName wsFieldNameNotEqual = ApiMappings.ConvertMetaFieldName(fieldNameNotEqual);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetGroupMetaList(groupId, wsAssetType, wsMetaType, wsFieldNameEqual, wsFieldNameNotEqual);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            result.Objects = AutoMapper.Mapper.Map<List<KalturaMeta>>(response.MetaList);
            result.TotalCount = response.MetaList.Count();

            return result;
        }

        internal KalturaCountryListResponse GetCountryListByIp(int groupId, string ip, bool? shouldUseCurrentRequestIp, KalturaCountryOrderBy? orderBy)
        {
            KalturaCountryListResponse result = new KalturaCountryListResponse() { TotalCount = 0, Objects = new List<KalturaCountry>() };
            CountryLocaleResponse response = null;
            if (shouldUseCurrentRequestIp.HasValue && shouldUseCurrentRequestIp.Value)
            {
                ip = Utils.Utils.GetClientIP();
            }
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetCountryLocaleByIp(groupId, ip);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            if (response.CountryLocales != null && response.CountryLocales.Count > 0)
            {
                result.Objects = AutoMapper.Mapper.Map<List<KalturaCountry>>(response.CountryLocales);
            }

            if (response.Countries != null && response.Countries.Count > 0)
            {
                result.Objects.AddRange(AutoMapper.Mapper.Map<List<KalturaCountry>>(response.Countries));
            }

            if (orderBy.HasValue)
            {
                switch (orderBy.Value)
                {
                    case KalturaCountryOrderBy.NAME_ASC:
                        result.Objects = result.Objects.OrderBy(x => x.Name).ToList();
                        break;
                    default:
                        break;
                }
            }

            result.TotalCount = result.Objects.Count;

            return result;
        }

        internal KalturaLanguageListResponse GetLanguageList(int groupId, List<string> languageCodes, KalturaLanguageOrderBy? orderBy = null)
        {
            KalturaLanguageListResponse result = new KalturaLanguageListResponse() { TotalCount = 0 };
            LanguageResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetLanguageList(groupId, languageCodes);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            if (response.Languages != null && response.Languages.Count > 0)
            {
                result.Objects = AutoMapper.Mapper.Map<List<KalturaLanguage>>(response.Languages);
                result.TotalCount = result.Objects.Count;
            }            

            if (result.TotalCount > 0 && orderBy.HasValue)
            {
                switch (orderBy.Value)
                {
                    case KalturaLanguageOrderBy.SYSTEM_NAME_ASC:
                        result.Objects = result.Objects.OrderBy(x => x.SystemName).ToList();
                        break;
                    case KalturaLanguageOrderBy.SYSTEM_NAME_DESC:
                        result.Objects = result.Objects.OrderByDescending(x => x.SystemName).ToList();
                        break;
                    case KalturaLanguageOrderBy.CODE_ASC:
                        result.Objects = result.Objects.OrderBy(x => x.Code).ToList();
                        break;
                    case KalturaLanguageOrderBy.CODE_DESC:
                        result.Objects = result.Objects.OrderByDescending(x => x.Code).ToList();
                        break;
                    default:
                        break;
                }
            }            

            return result;
        }

        internal KalturaCurrencyListResponse GetCurrencyList(int groupId, List<string> currencyCodes, KalturaCurrencyOrderBy? orderBy = null)
        {
            KalturaCurrencyListResponse result = new KalturaCurrencyListResponse() { TotalCount = 0 };
            CurrencyResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetCurrencyList(groupId, currencyCodes);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            if (response.Currencies != null && response.Currencies.Count > 0)
            {
                result.Objects = AutoMapper.Mapper.Map<List<KalturaCurrency>>(response.Currencies);
                result.TotalCount = result.Objects.Count;
            }            

            if (result.TotalCount > 0 && orderBy.HasValue)
            {
                switch (orderBy.Value)
                {
                    case KalturaCurrencyOrderBy.NAME_ASC:
                        result.Objects = result.Objects.OrderBy(x => x.Name).ToList();
                        break;
                    case KalturaCurrencyOrderBy.NAME_DESC:
                        result.Objects = result.Objects.OrderByDescending(x => x.Name).ToList();
                        break;
                    case KalturaCurrencyOrderBy.CODE_ASC:
                        result.Objects = result.Objects.OrderBy(x => x.Code).ToList();
                        break;
                    case KalturaCurrencyOrderBy.CODE_DESC:
                        result.Objects = result.Objects.OrderByDescending(x => x.Code).ToList();
                        break;
                    default:
                        break;
                }
            }

            return result;
        }


        internal void SaveSearchHistory(string name, string service, string action, string language, string userId, string deviceId, JObject persistedFilter)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.SaveSearchHistory(name, service, action, language, userId, deviceId, persistedFilter);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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
        }

        public KalturaSearchHistoryListResponse GetSearchHistory(int groupId, string userId, string udid, string language, int pageIndex, int? pageSize)
        {
            KalturaSearchHistoryListResponse result = new KalturaSearchHistoryListResponse()
            {
                TotalCount = 0
            };

            Core.Api.Modules.SearchHistoryResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetSearchHistory(groupId, userId, udid, language, pageIndex, pageSize);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
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

            if (response.Searches != null && response.Searches.Count > 0)
            {
                result.Objects = AutoMapper.Mapper.Map<List<KalturaSearchHistory>>(response.Searches);
                result.TotalCount = result.Objects.Count;
            }

            return result;
        }
    }
}
