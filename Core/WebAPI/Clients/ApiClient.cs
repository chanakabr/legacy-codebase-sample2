using ApiLogic.Api.Managers;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.BulkExport;
using ApiObjects.CDNAdapter;
using ApiObjects.Response;
using ApiObjects.Roles;
using ApiObjects.Rules;
using ApiObjects.Segmentation;
using ApiObjects.TimeShiftedTv;
using AutoMapper;
using Core.Catalog.CatalogManagement;
using Core.Pricing;
using Couchbase.IO.Operations.Errors;
using Phx.Lib.Log;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiLogic.Catalog;
using Core.Api;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Api;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Partner;
using WebAPI.Models.Segmentation;
using WebAPI.Models.Users;
using WebAPI.ModelsFactory;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;
using WebAPI.ObjectsConvertor.Extensions;
using KalturaPersonalListListResponse = WebAPI.Models.Api.KalturaPersonalListListResponse;
using UserSegments = ApiObjects.Segmentation.UserSegments;

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

        internal List<KalturaUserRole> GetUserRoles(int groupId, string userId)
        {
            List<KalturaUserRole> roles = new List<KalturaUserRole>();
            RolesResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetUserRoles(groupId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            roles = AutoMapper.Mapper.Map<List<KalturaUserRole>>(response.Roles);

            return roles;

        }

        #region Parental Rules

        internal List<Models.API.KalturaParentalRule> GetGroupParentalRules(int groupId, bool isAllowedToViewInactiveAssets = false)
        {
            ParentalRulesResponse response = null;
            List<Models.API.KalturaParentalRule> rules = new List<Models.API.KalturaParentalRule>();



            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetParentalRules(groupId, isAllowedToViewInactiveAssets);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.status);
            }

            rules = AutoMapper.Mapper.Map<List<WebAPI.Models.API.KalturaParentalRule>>(response.rules);

            return rules;
        }

        internal KalturaUserRole UpdateRole(int groupId, long id, KalturaUserRole role)
        {
            KalturaUserRole userRole = new KalturaUserRole();
            RolesResponse response = null;

            try
            {
                Role roleRequest = AutoMapper.Mapper.Map<Role>(role);
                roleRequest.Id = id;
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.UpdateRole(groupId, roleRequest);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Roles != null && response.Roles.Count > 0)
            {
                userRole = AutoMapper.Mapper.Map<KalturaUserRole>(response.Roles[0]);
            }

            return userRole;
        }

        internal bool DeleteRole(int groupId, long id)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.DeleteRole(groupId, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteRole.  groupID: {0}, id: {1}, exception: {2}", groupId, id, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }

            return true;
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (webServiceResponse.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(webServiceResponse.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            rules = AutoMapper.Mapper.Map<List<KalturaUserAssetRule>>(response.Rules);

            return rules;
        }

        internal List<KalturaUserAssetRule> GetNPVRRules(int groupId, string userId, long mediaId, int domainId, KalturaUserAssetRuleOrderBy orderBy = KalturaUserAssetRuleOrderBy.NAME_ASC)
        {
            GenericRuleResponse response = null;
            List<KalturaUserAssetRule> rules = new List<KalturaUserAssetRule>();

            //convert order by
            GenericRuleOrderBy wsOrderBy = ApiMappings.ConvertUserAssetRuleOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetNPVRRules(groupId, userId, mediaId, domainId, Utils.Utils.GetClientIP(), wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            rules = AutoMapper.Mapper.Map<List<KalturaUserAssetRule>>(response.Rules);

            return rules;
        }


        internal KalturaParentalRule AddParentalRule(int groupId, KalturaParentalRule parentalRule, long userId)
        {
            Func<ParentalRule, GenericResponse<ParentalRule>> addParentalRuleFunc = (ParentalRule parentalRuleToAdd) => Core.Api.api.AddParentalRule(groupId, parentalRuleToAdd, userId);
            return ClientUtils.GetResponseFromWS<KalturaParentalRule, ParentalRule>(parentalRule, addParentalRuleFunc);
        }

        internal KalturaParentalRule UpdateParentalRule(int groupId, long id, KalturaParentalRule parentalRule, long userId)
        {
            Func<ParentalRule, GenericResponse<ParentalRule>> updateParentalRuleFunc = (ParentalRule parentalRuleToUpdate) => Core.Api.api.UpdateParentalRule(groupId, id, parentalRuleToUpdate, userId);
            return ClientUtils.GetResponseFromWS<KalturaParentalRule, ParentalRule>(parentalRule, updateParentalRuleFunc);
        }

        internal KalturaParentalRule GetParentalRule(int groupId, long id, bool isAllowedToViewInactiveAssets)
        {
            Func<GenericResponse<ParentalRule>> getParentalRuleFunc = () => Core.Api.api.GetParentalRule(groupId, id, isAllowedToViewInactiveAssets);
            return ClientUtils.GetResponseFromWS<KalturaParentalRule, ParentalRule>(getParentalRuleFunc);
        }

        internal bool DeleteParentalRule(int groupId, long id, long userId)
        {
            Func<Status> deleteParentalRuletFunc = () => Core.Api.api.DeleteParentalRule(groupId, id, userId);
            return ClientUtils.GetResponseStatusFromWS(deleteParentalRuletFunc);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            kalturaRecommendationEngineProfile = Mapper.Map<Models.API.KalturaRecommendationProfile>(response);

            return kalturaRecommendationEngineProfile;
        }

        #endregion

        #region ExternalChannel
        internal KalturaExternalChannelProfile InsertExternalChannel(int groupId, KalturaExternalChannelProfile externalChannel, long userId)
        {
            ExternalChannelResponse response = null;
            KalturaExternalChannelProfile kalturaExternalChannelProfile = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    ExternalChannel request = Mapper.Map<ExternalChannel>(externalChannel);
                    response = Core.Api.Module.InsertExternalChannel(groupId, request, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertExternalChannel.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }


            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            kalturaExternalChannelProfile = Mapper.Map<Models.API.KalturaExternalChannelProfile>(response);
            return kalturaExternalChannelProfile;
        }

        internal KalturaExternalChannelProfile SetExternalChannel(int groupId, KalturaExternalChannelProfile externalChannel, long userId)
        {
            ExternalChannelResponse response = null;
            KalturaExternalChannelProfile kalturaExternalChannelProfile = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    ExternalChannel request = Mapper.Map<ExternalChannel>(externalChannel);
                    response = Core.Api.Module.SetExternalChannel(groupId, request, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetExternalChannel. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            kalturaExternalChannelProfile = Mapper.Map<KalturaExternalChannelProfile>(response);
            return kalturaExternalChannelProfile;
        }

        internal bool DeleteExternalChannel(int groupId, int externalChannelId, long userId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.DeleteExternalChannel(groupId, externalChannelId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteExternalChannel.  groupID: {0}, externalChannelId: {1}, exception: {2}", groupId, externalChannelId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }

            return true;
        }

        internal List<KalturaExternalChannelProfile> GetExternalChannels(int groupId, long userId)
        {
            List<Models.API.KalturaExternalChannelProfile> kalturaExternalChannelList = null;
            ExternalChannelResponseList response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.ListExternalChannels(groupId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetExternalChannels. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            tasks = AutoMapper.Mapper.Map<List<WebAPI.Models.API.KalturaExportTask>>(response.Tasks);

            return tasks;
        }

        public List<KalturaUserRole> GetRoles(int groupId, List<long> roleIds = null)
        {
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            var roles = Mapper.Map<List<KalturaUserRole>>(response.Roles);

            return roles;
        }

        internal KalturaPermissionListResponse GetPermissions(int groupId, long userId, long? roleIdIn)
        {
            KalturaPermissionListResponse result = new KalturaPermissionListResponse();
            PermissionsResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    if (userId > 0)
                    {
                        response = Core.Api.Module.GetUserPermissions(groupId, userId.ToString());
                    }
                    else
                    {
                        response = Core.Api.Module.GetGroupPermissions(groupId, roleIdIn);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            result.Permissions = AutoMapper.Mapper.Map<List<KalturaPermission>>(response.Permissions);
            result.TotalCount = result.Permissions.Count;

            return result;
        }

        internal string GetCurrentUserPermissions(int groupId, string userId)
        {
            string result = string.Empty;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    result = Core.Api.Module.GetCurrentUserPermissions(groupId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (string.IsNullOrEmpty(result))
            {
                throw new ClientException(StatusCode.Error);
            }

            return result;
        }

        internal KalturaUserRole AddRole(int groupId, KalturaUserRole role)
        {
            KalturaUserRole userRole = new KalturaUserRole();
            RolesResponse response = null;

            try
            {
                Role roleRequest = AutoMapper.Mapper.Map<Role>(role);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.AddRole(groupId, roleRequest);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Roles != null && response.Roles.Count > 0)
            {
                userRole = AutoMapper.Mapper.Map<KalturaUserRole>(response.Roles[0]);
            }
            return userRole;
        }

        internal void DeletePermission(int groupId, long id)
        {
            Func<Status> deletePermissionFunc = () => Core.Api.Module.DeletePermission(groupId, id);
            ClientUtils.GetResponseStatusFromWS(deletePermissionFunc);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }
            else
            {
                success = true;
            }

            return success;
        }

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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }
            else if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }
            else if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }
            else
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal KalturaCDNAdapterProfileListResponse GetCDNAdapters(int groupId)
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            responseSettings = AutoMapper.Mapper.Map<KalturaCDNPartnerSettings>(response.CDNPartnerSettings);

            return responseSettings;
        }

        internal KalturaDeviceFamily AddDeviceFamily(long groupId, KalturaDeviceFamily deviceFamily, long updaterId)
        {
            Func<DeviceFamily, GenericResponse<DeviceFamily>> addFunc = deviceFamilyToAdd => DeviceFamilyManager.Instance.Add(groupId, deviceFamilyToAdd, updaterId);
            var result = ClientUtils.GetResponseFromWS(deviceFamily, addFunc);

            return result;
        }

        internal KalturaDeviceFamily UpdateDeviceFamily(long groupId, KalturaDeviceFamily deviceFamily, long updaterId)
        {
            Func<DeviceFamily, GenericResponse<DeviceFamily>> updateFunc = deviceFamilyToUpdate => DeviceFamilyManager.Instance.Update(groupId, deviceFamilyToUpdate, updaterId);
            var result = ClientUtils.GetResponseFromWS(deviceFamily, updateFunc);

            return result;
        }

        internal KalturaDeviceFamilyListResponse GetDeviceFamilies(long groupId, long? id, string name, bool? isSystem, bool orderByIdAsc, int pageIndex, int pageSize)
        {
            Func<GenericListResponse<DeviceFamily>> listFunc = () => DeviceFamilyManager.Instance.List(groupId, id, name, isSystem, orderByIdAsc, pageIndex, pageSize);
            var response = ClientUtils.GetResponseListFromWS<KalturaDeviceFamily, DeviceFamily>(listFunc);

            var result = new KalturaDeviceFamilyListResponse
            {
                Objects = response.Objects,
                TotalCount = response.TotalCount
            };

            return result;
        }

        public KalturaDeviceBrand AddDeviceBrand(long groupId, KalturaDeviceBrand deviceBrand, long updaterId)
        {
            Func<DeviceBrand, GenericResponse<DeviceBrand>> addFunc = deviceBrandToAdd => DeviceBrandManager.Instance.Add(groupId, deviceBrandToAdd, updaterId);
            var result = ClientUtils.GetResponseFromWS(deviceBrand, addFunc);

            return result;
        }

        public KalturaDeviceBrand UpdateDeviceBrand(long groupId, KalturaDeviceBrand deviceBrand, long updaterId)
        {
            Func<DeviceBrand, GenericResponse<DeviceBrand>> updateFunc = deviceBrandToUpdate => DeviceBrandManager.Instance.Update(groupId, deviceBrandToUpdate, updaterId);
            var result = ClientUtils.GetResponseFromWS(deviceBrand, updateFunc);

            return result;
        }

        public KalturaDeviceBrandListResponse GetDeviceBrandList(long groupId, long? id, long? deviceFamilyId, string name, bool? isSystem, bool orderByIdAsc, int pageIndex, int pageSize)
        {
            Func<GenericListResponse<DeviceBrand>> listFunc = () => DeviceBrandManager.Instance.List(groupId, id, deviceFamilyId, name, isSystem, orderByIdAsc, pageIndex, pageSize);
            var response = ClientUtils.GetResponseListFromWS<KalturaDeviceBrand, DeviceBrand>(listFunc);

            var result = new KalturaDeviceBrandListResponse
            {
                Objects = response.Objects,
                TotalCount = response.TotalCount
            };

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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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

        internal KalturaMetaListResponse GetGroupMeta(int groupId, KalturaAssetType? assetType, KalturaMetaType? metaType, KalturaMetaFieldName? fieldNameEqual, KalturaMetaFieldName? fieldNameNotEqual, List<KalturaMetaFeatureType> metaFeatureTypeList)
        {

            KalturaMetaListResponse result = new KalturaMetaListResponse();
            MetaResponse response = null;
            try
            {
                eAssetTypes wsAssetType = AutoMapper.Mapper.Map<eAssetTypes>(assetType);
                ApiObjects.MetaType wsMetaType = ApiMappings.ConvertMetaType(metaType);
                MetaFieldName wsFieldNameEqual = ApiMappings.ConvertMetaFieldName(fieldNameEqual);
                MetaFieldName wsFieldNameNotEqual = ApiMappings.ConvertMetaFieldName(fieldNameNotEqual);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = TopicManager.Instance.GetGroupMetaList(groupId, wsAssetType, wsMetaType, wsFieldNameEqual, wsFieldNameNotEqual, ApiMappings.ConvertMetaFeatureTypes(metaFeatureTypeList));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            result.Metas = AutoMapper.Mapper.Map<List<KalturaMeta>>(response.MetaList);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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

        internal void SaveSearchHistory(string name, string service, string action, string language, int groupId, string userId, string deviceId, JObject persistedFilter)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.SaveSearchHistory(name, service, action, language, groupId, userId, deviceId, persistedFilter);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Searches != null && response.Searches.Count > 0)
            {
                result.Objects = AutoMapper.Mapper.Map<List<KalturaSearchHistory>>(response.Searches);
                result.TotalCount = response.TotalItems;
            }

            return result;
        }

        internal bool CleanSearchHistory(int groupId, string userId)
        {
            bool success = false;

            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.CleanSearchHistory(groupId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }
            else
            {
                success = true;
            }

            return success;
        }

        internal bool DeleteSearchHistory(int groupId, string userId, string id)
        {
            bool success = false;

            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.DeleteSearchHistory(groupId, userId, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }
            else
            {
                success = true;
            }

            return success;
        }

        internal void CleanUserAssetHistory(int groupId, long userId, string udid, List<string> assetIds, List<int> assetTypes, KalturaWatchStatus watchStatus, int days, string ksql)
        {
            Status response = null;
            
            KalturaAssetHistoryListResponse historyResponse = ClientsManager.CatalogClient().getAssetHistory(groupId, userId.ToString(), udid, string.Empty, 0, 0, watchStatus, days, assetTypes,
                            assetIds, false, ksql);

            if (historyResponse != null && historyResponse.Objects != null && historyResponse.Objects.Count > 0)
            {
                try
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        response = UserWatchHistoryManager.Instance.Clean(groupId, userId,
                            historyResponse.Objects.Select(a => new KeyValuePair<int, eAssetTypes>((int)a.AssetId, AutoMapper.Mapper.Map<eAssetTypes>(a.AssetType))).ToList());
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                    ErrorUtils.HandleWSException(ex);
                }

                if (response == null)
                {
                    throw new ClientException(StatusCode.Error);
                }

                if (response.Code != (int)StatusCode.OK)
                {
                    throw new ClientException(response);
                }
            }
        }

        internal string GetCustomDrmAssetLicenseData(int groupId, int drmAdapterId, string userId, string assetId, KalturaAssetType kalturaAssetType, int fileId, string externalFileId,
            string udid, KalturaPlaybackContextType? context, string recordingId, out string code, out string message)
        {
            StringResponse drmAdapterResponse = null;
            message = null;
            code = null;

            try
            {
                PlayContextType contextType = PlayContextType.Playback;

                if (context.HasValue)
                {
                    contextType = ConditionalAccessMappings.ConvertPlayContextType(context.Value) == PlayContextType.Download ? PlayContextType.Download : PlayContextType.Playback;
                }
                var assetType = AutoMapper.Mapper.Map<eAssetTypes>(kalturaAssetType);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    drmAdapterResponse = Core.Api.Module.GetCustomDrmAssetLicenseData(groupId, drmAdapterId, userId, assetId, assetType, fileId,
                        externalFileId, Utils.Utils.GetClientIP(), udid, contextType, recordingId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (drmAdapterResponse == null || drmAdapterResponse.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (drmAdapterResponse.Status.Code != (int)eResponseStatus.OK && drmAdapterResponse.Status.Code != (int)eResponseStatus.AdapterNotExists &&
                drmAdapterResponse.Status.Code != (int)eResponseStatus.AdapterUrlRequired && drmAdapterResponse.Status.Code != (int)eResponseStatus.AdapterAppFailure)
            {
                throw new ClientException(drmAdapterResponse.Status);
            }
            else if (drmAdapterResponse.Status.Code != (int)eResponseStatus.OK)
            {
                message = string.Format("Failed for sourceId = {0}, reason: {1}", fileId, drmAdapterResponse.Status.Message);
                code = drmAdapterResponse.Status.Code.ToString();
            }

            return drmAdapterResponse.Value;
        }

        #region AssetRule

        internal KalturaAssetRule UpdateAssetRule(int groupId, long id, KalturaAssetRule assetRule)
        {
            assetRule.Id = id;

            Func<AssetRule, GenericResponse<AssetRule>> updateAssetRuleFunc = assetRuleToUpdate =>
                Core.Api.Module.UpdateAssetRule(groupId, assetRuleToUpdate);

            return ClientUtils.GetResponseFromWS(assetRule, updateAssetRuleFunc);
        }

        internal KalturaAssetRule AddAssetRule(int groupId, KalturaAssetRule assetRule)
        {
            Func<AssetRule, GenericResponse<AssetRule>> addAssetRuleFunc = assetRuleToAdd =>
                Core.Api.Module.AddAssetRule(groupId, assetRuleToAdd);

            return ClientUtils.GetResponseFromWS(assetRule, addAssetRuleFunc);
        }

        internal bool DeleteAssetRule(int groupId, long id)
        {
            Func<Status> deleteAssetRuleFunc = () => Core.Api.Module.DeleteAssetRule(groupId, id);
            ClientUtils.GetResponseStatusFromWS(deleteAssetRuleFunc);

            return true;
        }

        internal KalturaAssetRuleListResponse GetAssetRules(int groupId, KalturaAssetRuleFilter filter)
        {
            RuleConditionType assetRuleConditionType = Mapper.Map<RuleConditionType>(filter.ConditionsContainType);
            RuleActionType? ruleActionType = filter.ActionsContainType.HasValue ? Mapper.Map<RuleActionType?>(filter.ActionsContainType.Value) : null;
            var orderBy = Mapper.Map<AssetRuleOrderBy>(filter.OrderBy);

            SlimAsset slimAsset = null;
            if (filter.AssetApplied != null)
            {
                slimAsset = Mapper.Map<SlimAsset>(filter.AssetApplied);
            }

            KalturaAssetRuleListResponse result = new KalturaAssetRuleListResponse();

            Func<GenericListResponse<AssetRule>> getAssetRulesFunc = () =>
               Core.Api.Module.GetAssetRules(assetRuleConditionType, groupId, slimAsset, ruleActionType, filter.NameContains, orderBy);

            KalturaGenericListResponse<KalturaAssetRule> response =
                ClientUtils.GetResponseListFromWS<KalturaAssetRule, AssetRule>(getAssetRulesFunc);

            result.Objects = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaAssetRule GetAssetRule(int groupId, long assetRuleId)
        {
            Func<GenericResponse<AssetRule>> getAssetRuleFunc = () =>
                Core.Api.Module.GetAssetRule(groupId, assetRuleId);

            KalturaAssetRule result =
                ClientUtils.GetResponseFromWS<KalturaAssetRule, AssetRule>(getAssetRuleFunc);
            return result;
        }
        #endregion

        #region AssetUserRule

        internal KalturaAssetUserRuleListResponse GetAssetUserRules(int groupId, long? userId = null, KalturaRuleActionType? actionsContainType = null, KalturaRuleConditionType? conditionsContainType = null, bool returnConfigError = false)
        {
            var ruleActionType = Mapper.Map<RuleActionType?>(actionsContainType);
            var ruleConditionType = Mapper.Map<RuleConditionType?>(conditionsContainType);

            Func<GenericListResponse<AssetUserRule>> getAssetUserRuleListFunc = () =>
                Core.Api.Module.GetAssetUserRuleList(groupId, userId, ruleActionType, ruleConditionType, returnConfigError);

            KalturaGenericListResponse<KalturaAssetUserRule> response =
                ClientUtils.GetResponseListFromWS<KalturaAssetUserRule, AssetUserRule>(getAssetUserRuleListFunc);

            var result = new KalturaAssetUserRuleListResponse
            {
                Objects = response.Objects,
                TotalCount = response.TotalCount
            };

            return result;
        }

        internal KalturaAssetUserRule AddAssetUserRule(int groupId, KalturaAssetUserRule assetUserRule)
        {
            Func<AssetUserRule, GenericResponse<AssetUserRule>> addAssetUserRuleFunc = (AssetUserRule assetUserRuleToAdd) =>
                Core.Api.Module.AddAssetUserRule(groupId, assetUserRuleToAdd);

            KalturaAssetUserRule result =
                ClientUtils.GetResponseFromWS<KalturaAssetUserRule, AssetUserRule>(assetUserRule, addAssetUserRuleFunc);

            return result;
        }

        internal KalturaAssetUserRule UpdateAssetUserRule(int groupId, long assetUserRuleId, KalturaAssetUserRule assetUserRule, long userId)
        {
            Func<AssetUserRule, GenericResponse<AssetUserRule>> updateAssetUserRuleFunc = (AssetUserRule assetUserRuleToUpdate) =>
                Core.Api.Module.UpdateAssetUserRule(groupId, assetUserRuleId, assetUserRuleToUpdate, userId);

            KalturaAssetUserRule result =
                ClientUtils.GetResponseFromWS<KalturaAssetUserRule, AssetUserRule>(assetUserRule, updateAssetUserRuleFunc);

            return result;
        }

        internal void DeleteAssetUserRule(int groupId, long assetUserRuleId, long userId)
        {
            Func<Status> deleteAssetUserRuleFunc = () => Core.Api.Module.DeleteAssetUserRule(groupId, assetUserRuleId, userId);
            ClientUtils.GetResponseStatusFromWS(deleteAssetUserRuleFunc);
        }

        internal void AddAssetUserRuleToUser(long userId, long ruleId, int groupId)
        {
            Func<Status> addAssetUserRuleToUserFunc = () => Core.Api.Module.AddAssetUserRuleToUser(userId, ruleId, groupId);
            ClientUtils.GetResponseStatusFromWS(addAssetUserRuleToUserFunc);
        }

        internal void DeleteAssetUserRuleFromUser(long userId, long ruleId, int groupId)
        {
            Func<Status> deleteAssetUserRuleFromUserFunc = () => Core.Api.Module.DeleteAssetUserRuleFromUser(userId, ruleId, groupId);
            ClientUtils.GetResponseStatusFromWS(deleteAssetUserRuleFromUserFunc);
        }

        #endregion

        internal KalturaPartnerConfigurationListResponse GetConcurrencyPartner(int groupId)
        {
            KalturaPartnerConfigurationListResponse result = new KalturaPartnerConfigurationListResponse();

            Func<GenericListResponse<DeviceConcurrencyPriority>> getConcurrencyPartnerFunc = () =>
                Core.Api.Module.GetConcurrencyPartner(groupId);

            KalturaGenericListResponse<KalturaConcurrencyPartnerConfig> response =
                ClientUtils.GetResponseListFromWS<KalturaConcurrencyPartnerConfig, DeviceConcurrencyPriority>(getConcurrencyPartnerFunc);

            result.Objects = new List<KalturaPartnerConfiguration>(response.Objects);
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal bool UpdateConcurrencyPartner(int groupId, KalturaConcurrencyPartnerConfig partnerConfig)
        {
            Func<DeviceConcurrencyPriority, Status> updateConcurrencyPartnerFunc =
                (DeviceConcurrencyPriority deviceConcurrencyPriorityToUpdate) =>
                    Core.Api.Module.UpdateConcurrencyPartner(groupId, deviceConcurrencyPriorityToUpdate);

            ClientUtils.GetResponseStatusFromWS<KalturaConcurrencyPartnerConfig, DeviceConcurrencyPriority>(updateConcurrencyPartnerFunc, partnerConfig);

            return true;
        }

        internal KalturaMediaConcurrencyRuleListResponse GetMediaConcurrencyRules(int groupId)
        {
            KalturaMediaConcurrencyRuleListResponse result = new KalturaMediaConcurrencyRuleListResponse();

            Func<GenericListResponse<MediaConcurrencyRule>> getMediaConcurrencyRuleListFunc = () =>
               Core.Api.Module.GetMediaConcurrencyRules(groupId);

            KalturaGenericListResponse<KalturaMediaConcurrencyRule> response =
                ClientUtils.GetResponseListFromWS<KalturaMediaConcurrencyRule, MediaConcurrencyRule>(getMediaConcurrencyRuleListFunc);

            result.Objects = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaPersonalListListResponse GetPersonalListItems(int groupId, int userId, int pageSize, int pageIndex, KalturaPersonalListOrderBy orderBy, HashSet<int> partnerListTypes)
        {
            KalturaPersonalListListResponse result = new KalturaPersonalListListResponse();

            // create order object
            OrderDiretion order = OrderDiretion.Desc;
            if (orderBy == KalturaPersonalListOrderBy.CREATE_DATE_ASC)
                order = OrderDiretion.Asc;

            Func<GenericListResponse<PersonalListItem>> getUserPersonalListItemsFunc = () =>
               Core.Api.Module.GetUserPersonalListItems(groupId, userId, pageSize, pageIndex, order, partnerListTypes);

            KalturaGenericListResponse<KalturaPersonalList> response =
                ClientUtils.GetResponseListFromWS<KalturaPersonalList, PersonalListItem>(getUserPersonalListItemsFunc);

            result.PersonalListList = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal void DeletePersonalListItemFromUser(int groupId, long personalListItemId, int userId)
        {
            Func<Status> deletePersonalListItemFromUserFunc = () => Core.Api.Module.DeletePersonalListItemForUser(groupId, personalListItemId, userId);
            ClientUtils.GetResponseStatusFromWS(deletePersonalListItemFromUserFunc);
        }

        internal KalturaPersonalList AddPersonalListItemToUser(int groupId, KalturaPersonalList kalturaPersonalList, int userId)
        {
            Func<PersonalListItem, GenericResponse<PersonalListItem>> addPersonalListItemToUserFunc = (PersonalListItem personalListItemToFollow) =>
                Core.Api.Module.AddPersonalListItemForUser(groupId, personalListItemToFollow, userId);

            KalturaPersonalList result =
                ClientUtils.GetResponseFromWS<KalturaPersonalList, PersonalListItem>(kalturaPersonalList, addPersonalListItemToUserFunc);

            return result;
        }

        internal KalturaSegmentationType AddSegmentationType(int groupId, KalturaSegmentationType kalturaSegmentationType, long userId)
        {
            Func<SegmentationType, GenericResponse<SegmentationType>> addSegmentationTypeFunc = (SegmentationType segmentTypeToAdd) =>
                Core.Api.Module.AddSegmentationType(groupId, segmentTypeToAdd, userId);

            KalturaSegmentationType result =
                ClientUtils.GetResponseFromWS<KalturaSegmentationType, SegmentationType>(kalturaSegmentationType, addSegmentationTypeFunc);

            return result;
        }

        internal KalturaSegmentationType UpdateSegmentationType(int groupId, KalturaSegmentationType kalturaSegmentationType, long userId)
        {
            Func<SegmentationType, GenericResponse<SegmentationType>> updateSegmentationTypeFunc = (SegmentationType segmentTypeToUpdate) =>
                Core.Api.Module.UpdateSegmentationType(groupId, segmentTypeToUpdate, userId);

            KalturaSegmentationType result =
                ClientUtils.GetResponseFromWS<KalturaSegmentationType, SegmentationType>(kalturaSegmentationType, updateSegmentationTypeFunc);

            return result;
        }

        internal bool DeleteSegmentationType(int groupId, long id, long userId)
        {
            Func<Status> deleteSegmentationTypeFunc = () => Core.Api.Module.DeleteSegmentationType(groupId, id, userId);
            return ClientUtils.GetResponseStatusFromWS(deleteSegmentationTypeFunc);
        }

        internal KalturaSegmentationTypeListResponse ListSegmentationTypes(int groupId, HashSet<long> ids, int pageIndex, int pageSize, AssetSearchDefinition assetSearchDefinition)
        {
            KalturaSegmentationTypeListResponse result = new KalturaSegmentationTypeListResponse();

            Func<GenericListResponse<SegmentationType>> getListSegmentationTypesFunc = () =>
               Core.Api.Module.Instance.ListSegmentationTypes(groupId, ids, pageIndex, pageSize, assetSearchDefinition);

            KalturaGenericListResponse<KalturaSegmentationType> response =
                ClientUtils.GetResponseListFromWS<KalturaSegmentationType, SegmentationType>(getListSegmentationTypesFunc);

            result.SegmentationTypes = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }
        internal KalturaUserSegmentListResponse GetUserSegments(int groupId, string userId, AssetSearchDefinition assetSearchDefinition, int pageIndex, int pageSize)
        {
            KalturaUserSegmentListResponse result = new KalturaUserSegmentListResponse();

            Func<GenericListResponse<UserSegment>> getUserSegmentsFunc = () =>
               Core.Api.Module.GetUserSegments(groupId, userId, assetSearchDefinition, pageIndex, pageSize);

            KalturaGenericListResponse<KalturaUserSegment> response =
                ClientUtils.GetResponseListFromWS<KalturaUserSegment, UserSegment>(getUserSegmentsFunc);

            result.Segments = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaUserSegment AddUserSegment(int groupId, KalturaUserSegment kalturaUserSegment)
        {
            Func<UserSegment, GenericResponse<UserSegment>> addUserSegmentFunc = (UserSegment userSegmentToAdd) =>
                Core.Api.Module.AddUserSegment(groupId, userSegmentToAdd);

            KalturaUserSegment result =
                ClientUtils.GetResponseFromWS<KalturaUserSegment, UserSegment>(kalturaUserSegment, addUserSegmentFunc);

            return result;
        }

        internal bool DeleteUserSegment(int groupId, string userId, long segmentId)
        {
            Func<Status> deleteUserSegmentFunc = () => Core.Api.Module.DeleteUserSegment(groupId, userId, segmentId);
            return ClientUtils.GetResponseStatusFromWS(deleteUserSegmentFunc);
        }

        internal KalturaHouseholdSegment AddHouseholdSegment(int groupId, KalturaHouseholdSegment householdSegment)
        {
            Func<HouseholdSegment, GenericResponse<HouseholdSegment>> addHouseholdSegmentFunc = (HouseholdSegment householdSegmentToAdd) =>
                Core.Api.Module.AddHouseholdSegment(groupId, householdSegmentToAdd);

            KalturaHouseholdSegment result =
                ClientUtils.GetResponseFromWS<KalturaHouseholdSegment, HouseholdSegment>(householdSegment, addHouseholdSegmentFunc);

            return result;
        }

        internal bool DeleteHouseholdSegment(int groupId, long householdId, long segmentId)
        {
            Func<Status> deleteHouseholdSegmentFunc = () => Core.Api.Module.DeleteHouseholdSegment(groupId, householdId, segmentId);
            return ClientUtils.GetResponseStatusFromWS(deleteHouseholdSegmentFunc);
        }

        internal KalturaBusinessModuleRuleListResponse GetBusinessModuleRules(int groupId, KalturaBusinessModuleRuleFilter filter)
        {
            KalturaBusinessModuleRuleListResponse result = new KalturaBusinessModuleRuleListResponse();

            var ruleActionType = filter.ActionsContainType.HasValue ? Mapper.Map<RuleActionType?>(filter.ActionsContainType.Value) : null;
            var conditionScopeFilter = Mapper.Map<BusinessModuleRuleConditionScope>(filter);

            Func<GenericListResponse<BusinessModuleRule>> getBusinessModuleRulesFunc = () =>
               Core.Api.Module.GetBusinessModuleRules(groupId, conditionScopeFilter, ruleActionType);

            KalturaGenericListResponse<KalturaBusinessModuleRule> response =
                ClientUtils.GetResponseListFromWS<KalturaBusinessModuleRule, BusinessModuleRule>(getBusinessModuleRulesFunc);

            result.Objects = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaBusinessModuleRule AddBusinessModuleRule(int groupId, KalturaBusinessModuleRule businessModuleRule)
        {
            Func<BusinessModuleRule, GenericResponse<BusinessModuleRule>> addBusinessModuleRuleFunc = (BusinessModuleRule businessModuleRuleToAdd) =>
               Core.Api.Module.AddBusinessModuleRule(groupId, businessModuleRuleToAdd);

            KalturaBusinessModuleRule result =
                ClientUtils.GetResponseFromWS<KalturaBusinessModuleRule, BusinessModuleRule>(businessModuleRule, addBusinessModuleRuleFunc);

            return result;
        }

        internal KalturaBusinessModuleRule UpdateBusinessModuleRule(int groupId, long id, KalturaBusinessModuleRule businessModuleRule)
        {
            businessModuleRule.Id = id;

            Func<BusinessModuleRule, GenericResponse<BusinessModuleRule>> updateBusinessModuleRuleFunc = (BusinessModuleRule businessModuleRuleToUpdate) =>
                Core.Api.Module.UpdateBusinessModuleRule(groupId, businessModuleRuleToUpdate);

            KalturaBusinessModuleRule result =
                ClientUtils.GetResponseFromWS<KalturaBusinessModuleRule, BusinessModuleRule>(businessModuleRule, updateBusinessModuleRuleFunc);

            return result;
        }

        internal void DeleteBusinessModuleRule(int groupId, long id)
        {
            Func<Status> deleteBusinessModuleRuleFunc = () => Core.Api.Module.DeleteBusinessModuleRule(groupId, id);
            ClientUtils.GetResponseStatusFromWS(deleteBusinessModuleRuleFunc);
        }

        internal KalturaBusinessModuleRule GetBusinessModuleRule(int groupId, long id)
        {
            Func<GenericResponse<BusinessModuleRule>> GetBusinessModuleRuleFunc = () =>
                Core.Api.Module.GetBusinessModuleRule(groupId, id);

            KalturaBusinessModuleRule result =
                ClientUtils.GetResponseFromWS<KalturaBusinessModuleRule, BusinessModuleRule>(GetBusinessModuleRuleFunc);
            return result;
        }

        #region PlaybackProfile
        internal KalturaPlaybackProfileListResponse GetPlaybackProfiles(int groupId)
        {
            KalturaPlaybackProfileListResponse result = new KalturaPlaybackProfileListResponse() { TotalCount = 0 };

            Func<GenericListResponse<PlaybackProfile>> getPlaybackProfilesFunc = () =>
               Core.Api.Module.GetPlaybackProfiles(groupId);

            KalturaGenericListResponse<KalturaPlaybackProfile> response =
                ClientUtils.GetResponseListFromWS<KalturaPlaybackProfile, PlaybackProfile>(getPlaybackProfilesFunc);

            result.PlaybackProfiles = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaPlaybackProfileListResponse GetPlaybackProfile(int groupId, long playbackProfileId)
        {
            KalturaPlaybackProfileListResponse result = new KalturaPlaybackProfileListResponse() { TotalCount = 0 };

            Func<GenericListResponse<PlaybackProfile>> getPlaybackProfileFunc = () =>
               Core.Api.Module.GetPlaybackProfile(groupId, playbackProfileId, true);

            KalturaGenericListResponse<KalturaPlaybackProfile> response =
                ClientUtils.GetResponseListFromWS<KalturaPlaybackProfile, PlaybackProfile>(getPlaybackProfileFunc);

            result.PlaybackProfiles = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaPlaybackProfile InsertPlaybackProfile(int groupId, string userId, KalturaPlaybackProfile playbackProfile)
        {
            Func<PlaybackProfile, GenericResponse<PlaybackProfile>> addBusinessModuleRuleFunc = (PlaybackProfile playbackAdapterToAdd) =>
              Core.Api.Module.AddPlaybackAdapter(groupId, userId, playbackAdapterToAdd);

            KalturaPlaybackProfile result =
                ClientUtils.GetResponseFromWS<KalturaPlaybackProfile, PlaybackProfile>(playbackProfile, addBusinessModuleRuleFunc);

            return result;
        }

        internal KalturaPlaybackProfile GeneratePlaybackAdapterSharedSecret(int groupId, long playbackAdapterId)
        {
            KalturaPlaybackProfile kalturaPlaybackProfile = null;
            Func<PlaybackProfile, GenericResponse<PlaybackProfile>> generateAdapterSharedSecretFunc = (PlaybackProfile playbackAdapter) =>
             Core.Api.Module.GeneratePlaybackAdapterSharedSecret(groupId, playbackAdapterId);

            KalturaPlaybackProfile result =
                ClientUtils.GetResponseFromWS<KalturaPlaybackProfile, PlaybackProfile>(kalturaPlaybackProfile, generateAdapterSharedSecretFunc);

            return result;
        }

        internal KalturaPlaybackProfile SetPlaybackProfile(int groupId, string userId, KalturaPlaybackProfile playbackProfile)
        {
            Func<PlaybackProfile, GenericResponse<PlaybackProfile>> updateBusinessModuleRuleFunc = (PlaybackProfile playbackAdapterToUpdate) =>
             Core.Api.Module.UpdatePlaybackAdapter(groupId, userId, playbackAdapterToUpdate);

            KalturaPlaybackProfile result =
                ClientUtils.GetResponseFromWS<KalturaPlaybackProfile, PlaybackProfile>(playbackProfile, updateBusinessModuleRuleFunc);

            return result;
        }

        internal bool DeletePlaybackProfile(int groupId, string userId, int id)
        {
            Func<Status> deletePlaybackProfileFunc = () => Core.Api.Module.DeletePlaybackAdapter(groupId, userId, id);
            ClientUtils.GetResponseStatusFromWS(deletePlaybackProfileFunc);

            return true;
        }
        #endregion

        internal KalturaPlaybackContext GetPlaybackAdapterContext(long adapterId, int groupId, string userId, string udid, string ip, KalturaPlaybackContext kalturaPlaybackContext,
                                                                string assetId, KalturaAssetType assetType, KalturaPlaybackContextOptions contextDataParams)
        {

            ApiObjects.PlaybackAdapter.RequestPlaybackContextOptions requestPlaybackContextOptions = AutoMapper.Mapper.Map<ApiObjects.PlaybackAdapter.RequestPlaybackContextOptions>(contextDataParams);
            requestPlaybackContextOptions.AssetId = assetId;
            requestPlaybackContextOptions.AssetType = AutoMapper.Mapper.Map<eAssetTypes>(assetType);

            Func<ApiObjects.PlaybackAdapter.PlaybackContext, GenericResponse<ApiObjects.PlaybackAdapter.PlaybackContext>> updateBusinessModuleRuleFunc = (ApiObjects.PlaybackAdapter.PlaybackContext getPlaybackContext) =>
             Core.Api.Module.GetPlaybackContext(adapterId, groupId, userId, udid, ip, getPlaybackContext, requestPlaybackContextOptions);

            KalturaPlaybackContext result =
                ClientUtils.GetResponseFromWS<KalturaPlaybackContext, ApiObjects.PlaybackAdapter.PlaybackContext>(kalturaPlaybackContext, updateBusinessModuleRuleFunc);

            return result;
        }

        internal KalturaTvmRuleListResponse GetTvmRules(int groupId, KalturaTvmRuleType? ruleTypeEqual, string nameEqual)
        {
            KalturaTvmRuleListResponse result = new KalturaTvmRuleListResponse();
            var ruleType = AutoMapper.Mapper.Map<TvmRuleType?>(ruleTypeEqual);

            Func<GenericListResponse<TvmRule>> getTvmRulesFunc = () =>
                TvmRuleManager.GetTvmRules(groupId, ruleType, nameEqual);

            KalturaGenericListResponse<KalturaTvmRule> response =
                ClientUtils.GetResponseListFromWS<KalturaTvmRule, TvmRule>(getTvmRulesFunc);

            result.Objects = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal bool UpdateGeneralPartnerConfiguration(int groupId, KalturaGeneralPartnerConfig partnerConfig)
        {
            Func<GeneralPartnerConfig, Status> UpdateGeneralPartnerConfigFunc = (GeneralPartnerConfig partnerConfigToUpdate) =>
                   Core.Api.Module.UpdateGeneralPartnerConfig(groupId, partnerConfigToUpdate);

            ClientUtils.GetResponseStatusFromWS<KalturaGeneralPartnerConfig, GeneralPartnerConfig>(UpdateGeneralPartnerConfigFunc, partnerConfig);

            return true;
        }

        internal bool UpdateOpcPartnerConfiguration(int groupId, KalturaOpcPartnerConfiguration partnerConfig)
        {
            Func<OpcPartnerConfig, Status> UpdateOpcPartnerConfigFunc = (OpcPartnerConfig partnerConfigToUpdate) =>
                   Core.Api.Module.UpdateOpcPartnerConfig(groupId, partnerConfigToUpdate);

            ClientUtils.GetResponseStatusFromWS<KalturaOpcPartnerConfiguration, OpcPartnerConfig>(UpdateOpcPartnerConfigFunc, partnerConfig);

            return true;
        }

        internal KalturaPartnerConfigurationListResponse GetOpcPartnerConfiguration(int groupId)
        {
            var result = new KalturaPartnerConfigurationListResponse();

            Func<GenericListResponse<OpcPartnerConfig>> getOpcPartnerConfigFunc = () =>
                Core.Api.Module.GetOpcPartnerConfiguration(groupId);

            KalturaGenericListResponse<KalturaOpcPartnerConfiguration> response =
                ClientUtils.GetResponseListFromWS<KalturaOpcPartnerConfiguration, OpcPartnerConfig>(getOpcPartnerConfigFunc);

            result.Objects = new List<KalturaPartnerConfiguration>(response.Objects);
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaPartnerConfigurationListResponse GetGeneralPartnerConfiguration(int groupId)
        {
            KalturaPartnerConfigurationListResponse result = new KalturaPartnerConfigurationListResponse();

            Func<GenericListResponse<GeneralPartnerConfig>> getGeneralPartnerConfigFunc = () =>
                Core.Api.Module.GetGeneralPartnerConfiguration(groupId);

            KalturaGenericListResponse<KalturaGeneralPartnerConfig> response =
                ClientUtils.GetResponseListFromWS<KalturaGeneralPartnerConfig, GeneralPartnerConfig>(getGeneralPartnerConfigFunc);

            result.Objects = new List<KalturaPartnerConfiguration>(response.Objects);
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaLanguageListResponse GetLanguageList(int groupId, KalturaLanguageOrderBy? orderBy = null)
        {
            KalturaLanguageListResponse result = new KalturaLanguageListResponse() { TotalCount = 0 };
            LanguageResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetAllLanguageList(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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

        internal KalturaCurrencyListResponse GetAllCurrencies(int groupId, KalturaCurrencyOrderBy? orderBy = null)
        {
            KalturaCurrencyListResponse result = new KalturaCurrencyListResponse() { TotalCount = 0 };
            CurrencyResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetCurrencyList(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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

        internal KalturaIngestProfile InsertIngestProfile(int groupId, KalturaIngestProfile ingestProfile, int userId)
        {
            return ClientUtils.GetResponseFromWS<KalturaIngestProfile, IngestProfile>(ingestProfile, p => Core.Profiles.IngestProfileManager.AddIngestProfile(groupId, userId, p));

        }

        internal KalturaIngestProfileListResponse GetIngestProfiles(int groupId)
        {
            var response = ClientUtils.GetResponseListFromWS<KalturaIngestProfile, IngestProfile>(() => Core.Profiles.IngestProfileManager.GetIngestProfiles(groupId));
            return new KalturaIngestProfileListResponse
            {
                Objects = response.Objects,
                TotalCount = response.TotalCount,
            };
        }

        public bool DeleteIngestProfiles(int groupId, int ingestProfileId, int userId)
        {
            var status = ClientUtils.GetResponseStatusFromWS(() => Core.Profiles.IngestProfileManager.DeleteIngestProfile(groupId, userId, ingestProfileId));
            return status;

        }

        public KalturaIngestProfile UpdateIngestProfile(int groupId, int ingestProfileId, KalturaIngestProfile ingestProfile, int userId)
        {
            return ClientUtils.GetResponseFromWS<KalturaIngestProfile, IngestProfile>(ingestProfile, p => Core.Profiles.IngestProfileManager.UpdateIngestProfile(groupId, userId, ingestProfileId, p));
        }

        internal KalturaPermission AddPermission(int groupId, KalturaPermission permission, long userId)
        {
            Func<Permission, GenericResponse<Permission>> addPermissionFunc = (Permission permissionToAdd) =>
              Core.Api.Module.AddPermission(groupId, permissionToAdd, userId);

            KalturaPermission result =
                ClientUtils.GetResponseFromWS<KalturaPermission, Permission>(permission, addPermissionFunc);

            return result;
        }

        internal List<string> GetGroupFeatures(int groupId)
        {
            List<string> response = new List<string>();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetGroupFeatures(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            return response;
        }

        public Dictionary<string, List<string>> GetPermissionItemsToFeatures(int groupId)
        {
            Dictionary<string, List<string>> response = new Dictionary<string, List<string>>();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Api.Module.GetPermissionItemsToFeatures(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            return response;

        }
        
        #region

        internal KalturaGenericListResponse<KalturaRegion> GetRegions(int groupId, RegionFilter filter, int pageIndex, int pageSize)
        {
            var isMultiLcnsEnabled = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupId)?.EnableMultiLcns == true;

            Func<GenericListResponse<Region>> getRegionsFunc = () => Core.Api.Module.GetRegions(groupId, filter, pageIndex, pageSize);
            var response = ClientUtils.GetResponseListFromWS<KalturaRegion, Region>(getRegionsFunc);
            response.Objects = GetUpdatedKalturaRegions(isMultiLcnsEnabled, response.Objects);

            return response;
        }

        internal KalturaRegionListResponse GetDefaultRegion(int groupId)
        {
            var isMultiLcnsEnabled = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupId)?.EnableMultiLcns == true;

            Func<GenericListResponse<Region>> getDefaultRegionFunc = () => Core.Api.Module.GetDefaultRegion(groupId);
            var response = ClientUtils.GetResponseListFromWS<KalturaRegion, Region>(getDefaultRegionFunc);
            response.Objects = GetUpdatedKalturaRegions(isMultiLcnsEnabled, response.Objects);

            var result = new KalturaRegionListResponse
            {
                Regions = new List<KalturaRegion>(response.Objects),
                TotalCount = response.TotalCount
            };

            return result;
        }

        internal KalturaRegion AddRegion(int groupId, KalturaRegion region, long userId)
        {
            var isMultiLcnsEnabled = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupId)?.EnableMultiLcns == true;
            region.RegionalChannels = GetKalturaRegionalChannelsList(isMultiLcnsEnabled, region.RegionalChannels);

            Func<Region, GenericResponse<Region>> addRegionFunc = regionToToAdd => Core.Api.Module.AddRegion(groupId, regionToToAdd, userId);
            var response = ClientUtils.GetResponseFromWS(region, addRegionFunc);
            response.RegionalChannels = GetUpdatedKalturaRegionalChannels(isMultiLcnsEnabled, response.RegionalChannels);

            return response;
        }

        internal KalturaRegion UpdateRegion(int groupId, KalturaRegion region, long userId)
        {
            var isMultiLcnsEnabled = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupId)?.EnableMultiLcns == true;
            region.RegionalChannels = GetKalturaRegionalChannelsList(isMultiLcnsEnabled, region.RegionalChannels);

            Func<Region, GenericResponse<Region>> updateRegionFunc = regionToToUpdate => Core.Api.Module.UpdateRegion(groupId, regionToToUpdate, userId);
            var response = ClientUtils.GetResponseFromWS(region, updateRegionFunc);
            response.RegionalChannels = GetUpdatedKalturaRegionalChannels(isMultiLcnsEnabled, response.RegionalChannels);

            return response;
        }

        internal void DeleteRegion(int groupId, int id, long userId)
        {
            Func<Status> deleteRegionFunc = () => Core.Api.Module.DeleteRegion(groupId, id, userId);
            ClientUtils.GetResponseStatusFromWS(deleteRegionFunc);
        }

        internal bool BulkUpdateRegions(int groupId, long userId, long linearChannelId, IReadOnlyCollection<KalturaRegionChannelNumber> regionChannelNumbers)
        {
            var isMultiLcnsEnabled = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupId)?.EnableMultiLcns == true;
            regionChannelNumbers = GetKalturaRegionChannelNumberList(isMultiLcnsEnabled, regionChannelNumbers);

            var mappedRegionChannelNumbers = Mapper.Map<List<RegionChannelNumber>>(regionChannelNumbers);

            Func<Status> updateFunc = () => Core.Api.Module.BulkUpdateRegions(groupId, userId, linearChannelId, mappedRegionChannelNumbers);
            var response = ClientUtils.GetResponseStatusFromWS(updateFunc);

            return response;
        }

        private static List<KalturaRegionalChannel> GetKalturaRegionalChannelsList(bool isMultiLcnsEnabled, IReadOnlyCollection<KalturaRegionalChannel> regionalChannels)
        {
            if (regionalChannels == null)
            {
                return null;
            }

            var multiLcnRegionalChannels = regionalChannels
                .OfType<KalturaRegionalChannelMultiLcns>()
                .ToArray();
            var newRegionalChannels = isMultiLcnsEnabled
                ? multiLcnRegionalChannels.SelectMany(x => x.ParsedLcns().Select(_ => RegionalChannelFactory.Create(x.LinearChannelId, _)))
                : multiLcnRegionalChannels.Select(x => RegionalChannelFactory.Create(x.LinearChannelId, x.ParsedLcns().First()));

            var result = regionalChannels
                .Except(multiLcnRegionalChannels)
                .Concat(newRegionalChannels)
                .ToList();

            return result;
        }

        private static List<KalturaRegionChannelNumber> GetKalturaRegionChannelNumberList(bool enableMultiLcns, IReadOnlyCollection<KalturaRegionChannelNumber> regionChannelNumbers)
        {
            var multiRegionChannelNumbers = regionChannelNumbers
                .OfType<KalturaRegionChannelNumberMultiLcns>()
                .ToArray();
            var newRegionChannelNumbers = enableMultiLcns
                ? multiRegionChannelNumbers.SelectMany(x => x.ParsedLcns().Select(_ => RegionChannelNumberFactory.Create(x.RegionId, _)))
                : multiRegionChannelNumbers.Select(x => RegionChannelNumberFactory.Create(x.RegionId, x.ParsedLcns().First()));

            var result = regionChannelNumbers
                .Except(multiRegionChannelNumbers)
                .Concat(newRegionChannelNumbers)
                .ToList();

            return result;
        }

        private static List<KalturaRegion> GetUpdatedKalturaRegions(bool isMultiLcnsEnabled, List<KalturaRegion> regions)
        {
            if (isMultiLcnsEnabled)
            {
                foreach (var region in regions)
                {
                    region.RegionalChannels = GetUpdatedKalturaRegionalChannels(true, region.RegionalChannels);
                }
            }

            return regions;
        }

        private static List<KalturaRegionalChannel> GetUpdatedKalturaRegionalChannels(bool isMultiLcnsEnabled, IEnumerable<KalturaRegionalChannel> regionalChannels)
        {
            return isMultiLcnsEnabled
                ? regionalChannels
                    .GroupBy(x => x.LinearChannelId, x => x.ChannelNumber)
                    .Select(x => RegionalChannelFactory.Create(x.Key, x.First(), string.Join(",", x)))
                    .Cast<KalturaRegionalChannel>()
                    .ToList()
                : regionalChannels
                    .ToList();
        }

        #endregion

        internal bool UpdateObjectVirtualAssetPartnerConfiguration(int groupId, KalturaObjectVirtualAssetPartnerConfig partnerConfig)
        {
            Func<ObjectVirtualAssetPartnerConfig, Status> UpdateConfigFunc = (ObjectVirtualAssetPartnerConfig partnerConfigToUpdate) =>
            Core.Api.Module.UpdateObjectVirtualAssetPartnerConfiguration(groupId, partnerConfigToUpdate);

            ClientUtils.GetResponseStatusFromWS<KalturaObjectVirtualAssetPartnerConfig, ObjectVirtualAssetPartnerConfig>(UpdateConfigFunc, partnerConfig);

            return true;
        }

        internal KalturaPartnerConfigurationListResponse GetObjectVirtualAssetPartnerConfiguration(int groupId)
        {
            KalturaPartnerConfigurationListResponse result = new KalturaPartnerConfigurationListResponse();

            Func<GenericListResponse<ObjectVirtualAssetPartnerConfig>> getPartnerConfigFunc = () => Core.Api.Module.GetObjectVirtualAssetPartnerConfiguration(groupId);

            KalturaGenericListResponse<KalturaObjectVirtualAssetPartnerConfig> response =
                ClientUtils.GetResponseListFromWS<KalturaObjectVirtualAssetPartnerConfig, ObjectVirtualAssetPartnerConfig>(getPartnerConfigFunc);

            result.Objects = new List<KalturaPartnerConfiguration>(response.Objects);
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal bool IncrementLayeredCacheGroupConfigVersion(int groupId)
        {
            Func<bool> incrementLayeredCacheGroupConfigVersion = () => Core.Api.Module.IncrementLayeredCacheGroupConfigVersion(groupId);
            return ClientUtils.GetBoolResponseFromWS(incrementLayeredCacheGroupConfigVersion);
        }

        internal KalturaStringValue GetLayeredCacheGroupConfig(int groupId)
        {
            KalturaStringValue result = new KalturaStringValue();
            Func<string> getLayeredCacheGroupConfig = () => Core.Api.Module.GetLayeredCacheGroupConfig(groupId);
            result.value = ClientUtils.GetStringResponseFromWS(getLayeredCacheGroupConfig);
            if (string.IsNullOrEmpty(result.value))
            {
                throw new ClientException(1, $"Failed to get layeredCache group config for groupId {groupId}");
            }

            return result;
        }

        internal bool InvalidateLayeredCacheInvalidationKey(string key)
        {
            Func<bool> invalidateLayeredCacheInvalidationKey = () => Core.Api.Module.SetLayeredCacheInvalidationKey(key);
            return ClientUtils.GetBoolResponseFromWS(invalidateLayeredCacheInvalidationKey);
        }

        internal KalturaLongValue GetInvalidationKeyValue(int groupId, string layeredCacheConfigName, string invalidationKey)
        {
            KalturaLongValue result = new KalturaLongValue();
            Func<long> getInvalidationKeyValue = () => Core.Api.Module.GetInvalidationKeyValue(groupId, layeredCacheConfigName, invalidationKey);
            result.value = ClientUtils.GetLongResponseFromWS(getInvalidationKeyValue);

            return result;
        }

        internal bool ClearLocalServerCache(string action, string key)
        {
            Func<Status> clearCache = () => Core.Api.Module.ClearLocalServerCache(action, key);
            return ClientUtils.GetResponseStatusFromWS(clearCache);
        }

        internal KalturaPlaybackContext GetPlaybackAdapterManifest(long adapterId, int groupId, KalturaPlaybackContext kalturaPlaybackContext,
                                                                string assetId, KalturaAssetType assetType, KalturaPlaybackContextOptions contextDataParams,
                                                                string userId, string udid, string ip)
        {
            ApiObjects.PlaybackAdapter.RequestPlaybackContextOptions requestPlaybackContextOptions = AutoMapper.Mapper.Map<ApiObjects.PlaybackAdapter.RequestPlaybackContextOptions>(contextDataParams);
            requestPlaybackContextOptions.AssetId = assetId;
            requestPlaybackContextOptions.AssetType = AutoMapper.Mapper.Map<eAssetTypes>(assetType);
            ;

            Func<ApiObjects.PlaybackAdapter.PlaybackContext, GenericResponse<ApiObjects.PlaybackAdapter.PlaybackContext>> updateBusinessModuleRuleFunc = (ApiObjects.PlaybackAdapter.PlaybackContext playbackContext) =>
             Core.Api.Module.GetPlaybackManifest(adapterId, groupId, playbackContext, requestPlaybackContextOptions, userId, udid, ip);

            KalturaPlaybackContext result =
                ClientUtils.GetResponseFromWS<KalturaPlaybackContext, ApiObjects.PlaybackAdapter.PlaybackContext>(kalturaPlaybackContext, updateBusinessModuleRuleFunc);

            return result;
        }

        internal KalturaSegmentationTypeListResponse GetSegmentationTypesBySegmentIds(int groupId, List<long> ids,
            AssetSearchDefinition assetSearchDefinition, int pageIndex, int pageSize)
        {
            KalturaSegmentationTypeListResponse result = new KalturaSegmentationTypeListResponse();

            Func<GenericListResponse<SegmentationType>> getSegmentationTypesBySegmentIdsFunc = () =>
               Core.Api.Module.GetSegmentationTypesBySegmentIds(groupId, ids, pageIndex, pageSize, assetSearchDefinition);

            KalturaGenericListResponse<KalturaSegmentationType> response =
                ClientUtils.GetResponseListFromWS<KalturaSegmentationType, SegmentationType>(getSegmentationTypesBySegmentIdsFunc);

            result.SegmentationTypes = response.Objects;
            result.TotalCount = response.TotalCount;
            return result;
        }

        internal KalturaPartnerConfigurationListResponse GetCommerceConfigList(int groupId)
        {
            var result = new KalturaPartnerConfigurationListResponse();

            Func<GenericListResponse<CommercePartnerConfig>> getCommercePartnerConfigListFunc = () =>
                PartnerConfigurationManager.GetCommercePartnerConfigList(groupId);

            KalturaGenericListResponse<KalturaCommercePartnerConfig> response =
                ClientUtils.GetResponseListFromWS<KalturaCommercePartnerConfig, CommercePartnerConfig>(getCommercePartnerConfigListFunc);

            result.Objects = new List<KalturaPartnerConfiguration>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;
        }

        internal KalturaPartnerConfigurationListResponse GetPlaybackAdapterConfiguration(int groupId)
        {
            var result = new KalturaPartnerConfigurationListResponse();

            Func<GenericListResponse<PlaybackPartnerConfig>> getPartnerConfigListFunc = () =>
                PartnerConfigurationManager.GetPlaybackConfigList(groupId);

            KalturaGenericListResponse<KalturaPlaybackPartnerConfig> response =
                ClientUtils.GetResponseListFromWS<KalturaPlaybackPartnerConfig, PlaybackPartnerConfig>(getPartnerConfigListFunc);

            result.Objects = new List<KalturaPartnerConfiguration>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;
        }

        internal void AddPermissionItemToPermission(int groupId, long permissionId, long permissionItemId)
        {
            Func<Status> addPermissionItemToPermissionFunc = () => RolesPermissionsManager.AddPermissionItemToPermission(groupId, permissionId, permissionItemId);
            ClientUtils.GetResponseStatusFromWS(addPermissionItemToPermissionFunc);
        }

        internal void RemovePermissionItemFromPermission(int groupId, long permissionId, long permissionItemId)
        {
            Func<Status> removePermissionItemToPermissionFunc = () => RolesPermissionsManager.RemovePermissionItemFromPermission(groupId, permissionId, permissionItemId);
            ClientUtils.GetResponseStatusFromWS(removePermissionItemToPermissionFunc);
        }

        internal KalturaExternalChannelProfileListResponse GetExternalChannels(int groupId, long userId, List<long> list)
        {
            var result = new KalturaExternalChannelProfileListResponse();

            Func<GenericListResponse<ExternalChannel>> getListFunc = () =>
                Core.Api.Module.ListExternalChannels(groupId, userId, list);

            KalturaGenericListResponse<KalturaExternalChannelProfile> response =
                ClientUtils.GetResponseListFromWS<KalturaExternalChannelProfile, ExternalChannel>(getListFunc);

            result.Objects = new List<KalturaExternalChannelProfile>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;
        }

        internal KalturaPartnerConfigurationListResponse GetPaymentConfiguration(int groupId)
        {
            var result = new KalturaPartnerConfigurationListResponse();

            Func<GenericListResponse<PaymentPartnerConfig>> getPartnerConfigListFunc = () =>
                PartnerConfigurationManager.GetPaymentConfigList(groupId);

            KalturaGenericListResponse<KalturaPaymentPartnerConfig> response =
                ClientUtils.GetResponseListFromWS<KalturaPaymentPartnerConfig, PaymentPartnerConfig>(getPartnerConfigListFunc);

            result.Objects = new List<KalturaPartnerConfiguration>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;
        }

        internal KalturaPartnerConfigurationListResponse GetCatalogConfiguration(int groupId)
        {
            var result = new KalturaPartnerConfigurationListResponse();

            Func<GenericListResponse<CatalogPartnerConfig>> getPartnerConfigListFunc = () =>
                CatalogPartnerConfigManager.Instance.GetCatalogConfigList(groupId);

            KalturaGenericListResponse<KalturaCatalogPartnerConfig> response =
                ClientUtils.GetResponseListFromWS<KalturaCatalogPartnerConfig, CatalogPartnerConfig>(getPartnerConfigListFunc);

            result.Objects = new List<KalturaPartnerConfiguration>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;
        }

        internal KalturaPartnerConfigurationListResponse GetParentalDefaultPartnerConfiguration(int groupId)
        {
            var result = new KalturaPartnerConfigurationListResponse();

            Func<GenericListResponse<DefaultParentalSettingsPartnerConfig>> getOpcPartnerConfigFunc = () =>
            DefaultParentalSettingsPartnerConfigManager.Instance.GetDefaultParentalSettingsConfigList(groupId);

            KalturaGenericListResponse<KalturaDefaultParentalSettingsPartnerConfig> response =
                ClientUtils.GetResponseListFromWS<KalturaDefaultParentalSettingsPartnerConfig, DefaultParentalSettingsPartnerConfig>(getOpcPartnerConfigFunc);

            result.Objects = new List<KalturaPartnerConfiguration>(response.Objects);
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaPartnerConfigurationListResponse GetCustomFieldsConfiguration(int groupId)
        {
            var result = new KalturaPartnerConfigurationListResponse();

            Func<CustomFieldsPartnerConfig> getListFunc = () => CustomFieldsPartnerConfigManager.Instance.GetCustomFieldsConfigFromCache(groupId);
            var response = ClientUtils.GetResponseFromWS<KalturaCustomFieldsPartnerConfiguration, CustomFieldsPartnerConfig>(getListFunc);

            if (response != null)
            {
                result.Objects = new List<KalturaPartnerConfiguration>{ response };
                result.TotalCount = 1;
            }

            return result;
        }

        internal KalturaPartnerConfigurationListResponse GetSecurityConfiguration(int groupId)
        {
            var result = new KalturaPartnerConfigurationListResponse();

            Func<GenericListResponse<SecurityPartnerConfig>> getPartnerConfigListFunc = () =>
                PartnerConfigurationManager.GetSecurityConfigList(groupId);

            KalturaGenericListResponse<KalturaSecurityPartnerConfig> response =
                ClientUtils.GetResponseListFromWS<KalturaSecurityPartnerConfig, SecurityPartnerConfig>(getPartnerConfigListFunc);

            result.Objects = new List<KalturaPartnerConfiguration>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;
        }

        internal KalturaPermission UpdatePermission(int groupId, long id, KalturaPermission permission, long userId)
        {
            Func<Permission, GenericResponse<Permission>> updatePermissionFunc = (Permission permissionToUpdate) =>
                         Core.Api.Module.UpdatePermission(groupId, userId, id, permissionToUpdate);

            KalturaPermission result =
                ClientUtils.GetResponseFromWS<KalturaPermission, Permission>(permission, updatePermissionFunc);

            return result;
        }
    }


}