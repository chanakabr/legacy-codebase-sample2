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
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Clients
{
    public class DomainsClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public DomainsClient()
        { 
        }

        #region Properties

        protected WebAPI.Domains.module Domains
        {
            get
            {
                return (Module as WebAPI.Domains.module);
            }
        }

        #endregion


        internal KalturaHousehold GetDomainInfo(int groupId, int domainId)
        {
            KalturaHousehold result = null;
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Domains.DomainResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.GetDomainInfo(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

            if (response.Domain == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            result = Mapper.Map<KalturaHousehold>(response.Domain);

            return result;
        }

        internal KalturaHousehold AddDomain(int groupId, string domainName, string domainDescription, string masterUserId)
        {
            KalturaHousehold result = null;
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Domains.DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.AddDomain(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainName, domainDescription, int.Parse(masterUserId));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            result = Mapper.Map<KalturaHousehold>(response.DomainResponse.m_oDomain);

            return result;
        }

        internal bool RemoveUserFromDomain(int groupId, int domainId, string userId)
        {
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Domains.DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.RemoveUserFromDomain(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return true;
        }

        internal bool AddUserToDomain(int groupId, int domainId, string userId, string masterUserId, bool isMaster)
        {
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Domains.DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.AddUserToDomain(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId, int.Parse(userId), int.Parse(masterUserId), isMaster);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return true;
        }

        internal bool RemoveDeviceFromDomain(int groupId, int domainId, string udid)
        {
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Domains.DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.RemoveDeviceFromDomain(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId, udid);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return true;
        }

        internal KalturaDevice RegisterDeviceByPin(int groupId, int domainId, string deviceName, string pin)
        {
            KalturaDevice result = null;
            WebAPI.Domains.DeviceResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.RegisterDeviceToDomainWithPIN(group.DomainsCredentials.Username, group.DomainsCredentials.Password, pin, domainId, deviceName);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

            if (response.Device == null || response.Device.m_oDevice == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            result = Mapper.Map<KalturaDevice>(response.Device.m_oDevice);

            return result;
        }

        internal KalturaDeviceRegistrationStatus GetDeviceRegistrationStatus(int groupId, int householdId, string udid)
        {
            KalturaDeviceRegistrationStatus result;
            WebAPI.Domains.DeviceRegistrationStatusResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.GetDeviceRegistrationStatus(group.DomainsCredentials.Username, group.DomainsCredentials.Password, udid, householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

            result = WebAPI.Mapping.ObjectsConvertor.DomainsMappings.ConvertRegistrationStatus(response.DeviceRegistrationStatus);

            return result;
        }
    }
}