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
using WebAPI.Mapping.ObjectsConvertor;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;
using System.Net;
using System.ServiceModel;
using WebAPI.Domains;

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

        internal void EnrichHouseHold(List<KalturaHouseholdWithHolder> with, KalturaHousehold household, int groupId)
        {
            // get users ids lists
            var userIds = household.Users != null ? household.Users.Select(u => u.Id) : new List<string>();
            var masterUserIds = household.MasterUsers != null ? household.MasterUsers.Select(u => u.Id) : new List<string>();
            var defaultUserIds = household.DefaultUsers != null ? household.DefaultUsers.Select(u => u.Id) : new List<string>();
            var pendingUserIds = household.PendingUsers != null ? household.PendingUsers.Select(u => u.Id) : new List<string>();

            // merge all user ids to one list
            List<string> allUserIds = new List<string>();
            allUserIds.AddRange(userIds);
            allUserIds.AddRange(masterUserIds);
            allUserIds.AddRange(defaultUserIds);
            allUserIds.AddRange(pendingUserIds);
            allUserIds = allUserIds.Distinct().ToList();


            //get users
            List<KalturaOTTUser> users = null;
            if (allUserIds.Count > 0)
            {
                users = ClientsManager.UsersClient().GetUsersData(groupId, allUserIds);
            }

            if (users != null)
            {
                if (with.Where(x => x.type == KalturaHouseholdWith.users_base_info).FirstOrDefault() != null)
                {
                    household.Users = Mapper.Map<List<KalturaBaseOTTUser>>(users.Where(u => userIds.Contains(u.Id)));
                    household.MasterUsers = Mapper.Map<List<KalturaBaseOTTUser>>(users.Where(u => masterUserIds.Contains(u.Id)));
                    household.DefaultUsers = Mapper.Map<List<KalturaBaseOTTUser>>(users.Where(u => defaultUserIds.Contains(u.Id)));
                    household.PendingUsers = Mapper.Map<List<KalturaBaseOTTUser>>(users.Where(u => pendingUserIds.Contains(u.Id)));
                }
                if (with.Where(x => x.type == KalturaHouseholdWith.users_full_info).FirstOrDefault() != null)
                {
                    household.Users = Mapper.Map<List<KalturaOTTUser>>(users.Where(u => userIds.Contains(u.Id))).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                    household.MasterUsers = Mapper.Map<List<KalturaOTTUser>>(users.Where(u => masterUserIds.Contains(u.Id))).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                    household.DefaultUsers = Mapper.Map<List<KalturaOTTUser>>(users.Where(u => defaultUserIds.Contains(u.Id))).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                    household.PendingUsers = Mapper.Map<List<KalturaOTTUser>>(users.Where(u => pendingUserIds.Contains(u.Id))).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                }
            }
        }

        internal KalturaHousehold AddDomain(int groupId, string domainName, string domainDescription, string masterUserId, string externalId = null)
        {
            KalturaHousehold result = null;
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Domains.DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    if (string.IsNullOrEmpty(externalId))
                    {
                        response = Domains.AddDomain(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainName, domainDescription, int.Parse(masterUserId));
                    }
                    else
                    {
                        response = Domains.AddDomainWithCoGuid(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainName, domainDescription, int.Parse(masterUserId), externalId);
                    }
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

        internal KalturaHouseholdDevice RegisterDeviceByPin(int groupId, int domainId, string deviceName, string pin)
        {
            KalturaHouseholdDevice result = null;
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

            result = Mapper.Map<KalturaHouseholdDevice>(response.Device.m_oDevice);

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

        internal KalturaHouseholdDevice GetDevice(int groupId, int householdId, string udid)
        {
            KalturaHouseholdDevice result;
            WebAPI.Domains.DeviceResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.GetDevice(group.DomainsCredentials.Username, group.DomainsCredentials.Password, udid, householdId);
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

            if (response.Device == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            result = Mapper.Map<KalturaHouseholdDevice>(response.Device.m_oDevice);

            return result;
        }

        internal KalturaHousehold GetDomainByUser(int groupId, string userId)
        {
            KalturaHousehold result;
            WebAPI.Domains.DomainResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.GetDomainByUser(group.DomainsCredentials.Username, group.DomainsCredentials.Password, userId);
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

            result = Mapper.Map<KalturaHousehold>(response.Domain);

            return result;
        }

        internal KalturaHousehold AddDeviceToDomain(int groupId, int domainId, string deviceName, string udid, int deviceBrandId)
        {
            KalturaHousehold result;
            WebAPI.Domains.DomainStatusResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.AddDeviceToDomain(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId, udid, deviceName, deviceBrandId);
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

        internal KalturaHouseholdDevice AddDevice(int groupId, int domainId, string deviceName, string udid, int deviceBrandId)
        {
            WebAPI.Domains.DeviceResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.AddDevice(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId, udid, deviceName, deviceBrandId);
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

            if (response.Device == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            KalturaHouseholdDevice result = Mapper.Map<KalturaHouseholdDevice>(response.Device.m_oDevice);

            return result;
        }

        internal KalturaDevicePin GetPinForDevice(int groupId, string udid, int deviceBrandId)
        {
            KalturaDevicePin result;
            WebAPI.Domains.DevicePinResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.GetPINForDevice(group.DomainsCredentials.Username, group.DomainsCredentials.Password, udid, deviceBrandId);
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

            result = Mapper.Map<KalturaDevicePin>(response.Pin);

            return result;
        }

        internal KalturaHouseholdLimitations GetDomainLimitationModule(int groupId, int dlmId)
        {
            KalturaHouseholdLimitations result;
            WebAPI.Domains.DLMResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.GetDLM(group.DomainsCredentials.Username, group.DomainsCredentials.Password, dlmId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.resp == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.resp.Code, response.resp.Message);
            }

            result = Mapper.Map<KalturaHouseholdLimitations>(response.dlm);

            return result;
        }

        internal KalturaHousehold ResetFrequency(int groupId, int domainId, KalturaHouseholdFrequencyType householdFrequency)
        {
            KalturaHousehold result;
            WebAPI.Domains.DomainStatusResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    int frequency = DomainsMappings.ConvertKalturaHouseholdFrequency(householdFrequency);
                    response = Domains.ResetDomainFrequency(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId, frequency);
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

        internal KalturaHouseholdDevice SetDeviceInfo(int groupId, string deviceName, string udid)
        {
            KalturaHouseholdDevice result;
            WebAPI.Domains.DeviceResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.SetDevice(group.DomainsCredentials.Username, group.DomainsCredentials.Password, udid, deviceName);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Device == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            result = Mapper.Map<KalturaHouseholdDevice>(response.Device.m_oDevice);

            return result;
        }

        internal KalturaHousehold SetDomainInfo(int groupId, int domainId, string name, string description)
        {
            KalturaHousehold result = null;
            WebAPI.Domains.DomainStatusResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.SetDomainInfo(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId, name, description);
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

        internal KalturaHousehold GetDomainByCoGuid(int groupId, string externalId)
        {
            KalturaHousehold result = null;
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Domains.DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.GetDomainByCoGuid(group.DomainsCredentials.Username, group.DomainsCredentials.Password, externalId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null || response.DomainResponse == null)
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

        internal bool RemoveDomain(int groupId, int householdId)
        {
            WebAPI.Domains.Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.RemoveDomainById(group.DomainsCredentials.Username, group.DomainsCredentials.Password, householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

        internal bool RemoveDomain(int groupId, string externalId)
        {
            WebAPI.Domains.Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.RemoveDomainByCoGuid(group.DomainsCredentials.Username, group.DomainsCredentials.Password, externalId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

        internal bool Suspend(int groupId, int domainId)
        {
            WebAPI.Domains.Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.SuspendDomain(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

        internal bool Resume(int groupId, int domainId)
        {
            WebAPI.Domains.Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.ResumeDomain(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

        internal KalturaHomeNetwork AddDomainHomeNetwork(int groupId, long domainId, string externalId, string name, string description, bool isActive)
        {
            KalturaHomeNetwork result;
            WebAPI.Domains.HomeNetworkResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.AddDomainHomeNetwork(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId, externalId, name, description, isActive);
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

            result = Mapper.Map<KalturaHomeNetwork>(response.HomeNetwork);

            return result;
        }

        internal List<KalturaHomeNetwork> GetDomainHomeNetworks(int groupId, long domainId)
        {
            List<KalturaHomeNetwork> result;
            WebAPI.Domains.HomeNetworksResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.GetDomainHomeNetworks(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId);
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

            result = Mapper.Map<List<KalturaHomeNetwork>>(response.HomeNetworks);

            return result;
        }

        internal KalturaHomeNetwork UpdateDomainHomeNetwork(int groupId, long domainId, string externalId, string name, string description, bool isActive)
        {
            HomeNetworkResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.SetDomainHomeNetwork(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId, externalId, name, description, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

            KalturaHomeNetwork result = Mapper.Map<KalturaHomeNetwork>(response.HomeNetwork);
            return result;
        }

        internal bool RemoveDomainHomeNetwork(int groupId, long domainId, string externalId)
        {
            WebAPI.Domains.Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.RemoveDomainHomeNetwork(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId, externalId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
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

        internal bool ChangeDeviceDomainStatus(int groupId, int domainId, string udid, KalturaDeviceStatus deviceStatus)
        {
            bool result;
            WebAPI.Domains.DomainStatusResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            bool isActive = deviceStatus == KalturaDeviceStatus.ACTIVATED ? true : false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Domains.ChangeDeviceDomainStatus(group.DomainsCredentials.Username, group.DomainsCredentials.Password, domainId, udid, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", Domains.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            return true;
        }
    }
}

namespace WebAPI.Domains
{
    // adding request ID to header
    public partial class module
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);
            KlogMonitorHelper.MonitorLogsHelper.AddHeaderToWebService(request);
            return request;
        }
    }
}