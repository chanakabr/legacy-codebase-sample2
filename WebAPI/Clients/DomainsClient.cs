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
using ApiObjects;
using Core.Users;

namespace WebAPI.Clients
{
    public class DomainsClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public DomainsClient()
        {
        }

        internal KalturaHousehold GetDomainInfo(int groupId, int domainId)
        {
            KalturaHousehold result = null;
            

            DomainResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.GetDomainInfo(groupId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            

            DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    if (string.IsNullOrEmpty(externalId))
                    {
                        response = Core.Domains.Module.AddDomain(groupId, domainName, domainDescription, int.Parse(masterUserId));
                    }
                    else
                    {
                        response = Core.Domains.Module.AddDomainWithCoGuid(groupId, domainName, domainDescription, int.Parse(masterUserId), externalId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            

            DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.RemoveUserFromDomain(groupId, domainId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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

        internal KalturaHousehold AddUserToDomain(int groupId, int domainId, string userId, string masterUserId, bool isMaster)
        {
            

            DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.AddUserToDomain(groupId, domainId, int.Parse(userId), int.Parse(masterUserId), isMaster);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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

            return Mapper.Map<KalturaHousehold>(response.DomainResponse.m_oDomain);
        }

        internal bool RemoveDeviceFromDomain(int groupId, int domainId, string udid)
        {
            

            DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.RemoveDeviceFromDomain(groupId, domainId, udid);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DeviceResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.RegisterDeviceToDomainWithPIN(groupId, pin, domainId, deviceName);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DeviceRegistrationStatusResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.GetDeviceRegistrationStatus(groupId, udid, householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DeviceResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.GetDevice(groupId, udid, householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DomainResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.GetDomainByUser(groupId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DomainStatusResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.AddDeviceToDomain(groupId, domainId, udid, deviceName, deviceBrandId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DeviceResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.AddDevice(groupId, domainId, udid, deviceName, deviceBrandId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DevicePinResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.GetPINForDevice(groupId, udid, deviceBrandId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DLMResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.GetDLM(groupId, dlmId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DomainStatusResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    int frequency = DomainsMappings.ConvertKalturaHouseholdFrequency(householdFrequency);
                    response = Core.Domains.Module.ResetDomainFrequency(groupId, domainId, frequency);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DeviceResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.SetDevice(groupId, udid, deviceName);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DomainStatusResponse response = null;
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.SetDomainInfo(groupId, domainId, name, description);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            

            DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.GetDomainByCoGuid(groupId, externalId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            ApiObjects.Response.Status response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.RemoveDomainById(groupId, householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            ApiObjects.Response.Status response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.RemoveDomainByCoGuid(groupId, externalId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            ApiObjects.Response.Status response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.SuspendDomain(groupId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            ApiObjects.Response.Status response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.ResumeDomain(groupId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            HomeNetworkResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.AddDomainHomeNetwork(groupId, domainId, externalId, name, description, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            HomeNetworksResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.GetDomainHomeNetworks(groupId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.SetDomainHomeNetwork(groupId, domainId, externalId, name, description, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            ApiObjects.Response.Status response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.RemoveDomainHomeNetwork(groupId, domainId, externalId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            DomainStatusResponse response = null;

            

            bool isActive = deviceStatus == KalturaDeviceStatus.ACTIVATED ? true : false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.ChangeDeviceDomainStatus(groupId, domainId, udid, isActive);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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

        internal KalturaHouseholdDevice SubmitAddDeviceToDomain(int groupId, int domainId, string userId, string udid, string deviceName, int deviceBrandId)
        {
            DeviceResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.SubmitAddDeviceToDomain(groupId, domainId, userId, udid, deviceName, deviceBrandId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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

        internal KalturaHousehold SubmitAddUserToDomainRequest(int groupId, string userId, string householdMasterUsername)
        {
            KalturaHousehold household = null;

            


            DomainStatusResponse response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.SubmitAddUserToDomainRequest(groupId, int.Parse(userId), householdMasterUsername);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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

            household = Mapper.Map<KalturaHousehold>(response.DomainResponse.m_oDomain);

            return household;
        }

        internal List<KalturaHouseholdUser> GetHouseholdUsers(int groupId, KalturaHousehold household)
        {
            Dictionary<string, KalturaHouseholdUser> response = new Dictionary<string, KalturaHouseholdUser>();
            KalturaHouseholdUser householdUser = null;

            foreach (var user in household.MasterUsers)
            {
                if (response.ContainsKey(user.Id))
                    continue;

                householdUser = new KalturaHouseholdUser()
                {
                    HouseholdId = (int)household.Id,
                    Status = KalturaHouseholdUserStatus.OK,
                    UserId = user.Id,
                    IsMaster = true
                };

                response.Add(householdUser.UserId, householdUser);
            }

            foreach (var user in household.Users)
            {
                if (response.ContainsKey(user.Id))
                    continue;

                householdUser = new KalturaHouseholdUser()
                {
                    HouseholdId = (int)household.Id,
                    Status = KalturaHouseholdUserStatus.OK,
                    UserId = user.Id,
                };

                response.Add(householdUser.UserId, householdUser);
            }

            foreach (var user in household.PendingUsers)
            {
                if (response.ContainsKey(user.Id))
                    continue;

                householdUser = new KalturaHouseholdUser()
                {
                    HouseholdId = (int)household.Id,
                    Status = KalturaHouseholdUserStatus.PENDING,
                    UserId = user.Id,
                };

                response.Add(householdUser.UserId, householdUser);
            }

            foreach (var user in household.DefaultUsers)
            {
                if (response.ContainsKey(user.Id))
                {
                    response[user.Id].IsDefault = true;
                }
                else
                {
                    householdUser = new KalturaHouseholdUser()
                    {
                        HouseholdId = (int)household.Id,
                        Status = KalturaHouseholdUserStatus.OK,
                        UserId = user.Id,
                        IsDefault = true
                    };

                    response.Add(householdUser.UserId, householdUser);
                }
            }

            return response.Values.ToList();
        }

        internal KalturaHouseholdDeviceListResponse GetHouseholdDevices(int groupId, KalturaHousehold household, List<long> deviceFamilyIds)
        {
            KalturaHouseholdDeviceListResponse response = new KalturaHouseholdDeviceListResponse() { TotalCount = 0, Objects = new List<KalturaHouseholdDevice>() };
            foreach (KalturaDeviceFamily family in household.DeviceFamilies)
            {
                if (deviceFamilyIds == null || deviceFamilyIds.Contains(family.Id.Value))
                {
                    List<KalturaDevice> familyDevices = family.Devices;
                    foreach (KalturaDevice device in familyDevices)
                    {
                        KalturaHouseholdDevice householdDevice = (KalturaHouseholdDevice)device;
                        householdDevice.DeviceFamilyId = family.Id;
                        response.Objects.Add(householdDevice);
                        response.TotalCount++;
                    }
                }
            }

            return response;
        }

    }
}
