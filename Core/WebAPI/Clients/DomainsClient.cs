using ApiObjects;
using AutoMapper;
using Core.Users;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Mapping.ObjectsConvertor;
using WebAPI.Models.Domains;
using WebAPI.Models.Users;
using WebAPI.Utils;
using ApiObjects.Response;
using WebAPI.Models.General;
using ApiLogic.Users.Managers;
using ApiObjects.Base;
using ApiLogic.Users.Services;

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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.Domain == null)
            {
                throw new ClientException(StatusCode.Error);
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
                    household.Users = users.Where(u => userIds.Contains(u.Id)).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                    household.MasterUsers = users.Where(u => masterUserIds.Contains(u.Id)).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                    household.DefaultUsers = users.Where(u => defaultUserIds.Contains(u.Id)).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                    household.PendingUsers = users.Where(u => pendingUserIds.Contains(u.Id)).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                }
                if (with.Where(x => x.type == KalturaHouseholdWith.users_full_info).FirstOrDefault() != null)
                {
                    household.Users = users.Where(u => userIds.Contains(u.Id)).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                    household.MasterUsers = users.Where(u => masterUserIds.Contains(u.Id)).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                    household.DefaultUsers = users.Where(u => defaultUserIds.Contains(u.Id)).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                    household.PendingUsers = users.Where(u => pendingUserIds.Contains(u.Id)).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                }
            }
        }

        internal KalturaHousehold AddDomain(int groupId, string domainName, string domainDescription, string masterUserId, int? regionId, string externalId = null)
        {
            KalturaHousehold result = null;


            DomainStatusResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    if (string.IsNullOrEmpty(externalId))
                    {
                        response = Core.Domains.Module.AddDomain(groupId, domainName, domainDescription, int.Parse(masterUserId), regionId);
                    }
                    else
                    {
                        response = Core.Domains.Module.AddDomainWithCoGuid(groupId, domainName, domainDescription, int.Parse(masterUserId), externalId, regionId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException(StatusCode.Error);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException(StatusCode.Error);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException(StatusCode.Error);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException(StatusCode.Error);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.Device == null || response.Device.m_oDevice == null)
            {
                throw new ClientException(StatusCode.Error);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            result = WebAPI.Mapping.ObjectsConvertor.DomainsMappings.ConvertRegistrationStatus(response.DeviceRegistrationStatus);

            return result;
        }

        internal KalturaHouseholdDevice GetDevice(int groupId, int householdId, string udid, string userId)
        {
            KalturaHouseholdDevice result;
            DeviceResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.GetDevice(groupId, udid, householdId, userId, Utils.Utils.GetClientIP());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.Device == null)
            {
                throw new ClientException(StatusCode.Error);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            result = Mapper.Map<KalturaHousehold>(response.DomainResponse.m_oDomain);

            return result;
        }

        internal void ValidateDeviceReferencesData(ContextData contextData, KalturaHouseholdDevice device)
        {
            if (device == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(device.Model) && string.IsNullOrEmpty(device.Manufacturer))
            {
                throw new ClientException((int)StatusCode.Error, $"Can't add device with model and without a manufacturer");
            }

            if (!string.IsNullOrEmpty(device.Manufacturer))
            {
                device.ManufacturerId = AddNewManufacturer(contextData, device.Manufacturer);
            }
        }

        internal long? AddNewManufacturer(ContextData contextData, string manufacturer)
        {
            long? response = null;
            var searchManufacturer = manufacturer.Trim().ToUpper();
            var filter = new DeviceManufacturersReferenceDataFilter { NameEqual = searchManufacturer };
            var referenceData = DeviceReferenceDataManager.Instance.ListByManufacturer(contextData, filter);
            if (!referenceData.HasObjects())
            {
                var request = new DeviceManufacturerInformation { Name = searchManufacturer };
                var newManufacturer = DeviceReferenceDataManager.Instance.Add(contextData, request);
                if (newManufacturer == null || !newManufacturer.IsOkStatusCode())
                {
                    throw new ClientException(newManufacturer.Status);
                }
                response = newManufacturer.Object.Id;
            }
            else
            {
                response = referenceData.Objects[0].Id;
            }
            return response;
        }

        internal KalturaHouseholdDevice AddDevice(int groupId, int domainId, KalturaHouseholdDevice device)
        {
            DeviceResponse response = null;

            try
            {
                var dDevice = CastToDomainDevice(device);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.AddDevice(groupId, domainId, dDevice);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.Device == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            var result = Mapper.Map<KalturaHouseholdDevice>(response.Device.m_oDevice);

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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            result = Mapper.Map<KalturaDevicePin>(response.Pin);

            return result;
        }

        internal KalturaHouseholdLimitations AddDomainLimitationModule(int groupId, KalturaHouseholdLimitations householdLimitations, long userId)
        {
            householdLimitations.DeviceFamiliesLimitations = householdLimitations.DeviceFamiliesLimitations ?? new List<KalturaHouseholdDeviceFamilyLimitations>();
            var limitationManager = Mapper.Map<LimitationsManager>(householdLimitations);
            Func<GenericResponse<LimitationsManager>> addDlmFunc = () => new Core.Domains.Module().AddDLM(groupId, userId, limitationManager);
            var response = ClientUtils.GetResponseFromWS<KalturaHouseholdLimitations, LimitationsManager>(addDlmFunc);

            return response;
        }

        internal KalturaHouseholdLimitations GetDomainLimitationModule(int groupId, int dlmId)
        {
            KalturaHouseholdLimitations result;
            DLMResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.Instance.GetDLM(groupId, dlmId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.resp == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.resp);
            }

            result = Mapper.Map<KalturaHouseholdLimitations>(response.dlm);

            return result;
        }

        internal KalturaHouseholdLimitations UpdateDLM(int limitId, int groupId, KalturaHouseholdLimitations householdLimitations, long userId)
        {
            DLMResponse response = new DLMResponse();

            try
            {
                var dlmToUpdate = GetDomainLimitationModule(groupId, limitId);

                householdLimitations.DeviceLimit = householdLimitations.DeviceLimit ?? dlmToUpdate.DeviceLimit;
                householdLimitations.DeviceFrequency = householdLimitations.DeviceFrequency ?? dlmToUpdate.DeviceFrequency;
                householdLimitations.ConcurrentLimit = householdLimitations.ConcurrentLimit ?? dlmToUpdate.ConcurrentLimit;
                householdLimitations.Description = householdLimitations.Description ?? dlmToUpdate.Description;
                householdLimitations.DeviceFrequencyDescription = householdLimitations.DeviceFrequencyDescription ?? dlmToUpdate.DeviceFrequencyDescription;
                householdLimitations.NpvrQuotaInSeconds = householdLimitations.NpvrQuotaInSeconds ?? dlmToUpdate.NpvrQuotaInSeconds;
                householdLimitations.AssociatedDeviceFamiliesIdsIn = householdLimitations.AssociatedDeviceFamiliesIdsIn ?? dlmToUpdate.AssociatedDeviceFamiliesIdsIn;
                householdLimitations.UsersLimit = householdLimitations.UsersLimit ?? dlmToUpdate.UsersLimit;
                householdLimitations.UserFrequencyDescription = householdLimitations.UserFrequencyDescription ?? dlmToUpdate.UserFrequencyDescription;
                householdLimitations.DeviceFamiliesLimitations = householdLimitations.DeviceFamiliesLimitations ?? new List<KalturaHouseholdDeviceFamilyLimitations>();
                if (string.IsNullOrEmpty(householdLimitations.AssociatedDeviceFamiliesIdsIn) && householdLimitations.DeviceFamiliesLimitations.Count == 0)
                {
                    householdLimitations.DeviceFamiliesLimitations = dlmToUpdate.DeviceFamiliesLimitations;
                }

                if (string.IsNullOrWhiteSpace(householdLimitations.Name))
                {
                    householdLimitations.Name = dlmToUpdate.Name;
                }

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    LimitationsManager request = Mapper.Map<LimitationsManager>(householdLimitations);
                    response = new Core.Domains.Module().UpdateDLM(limitId, groupId, userId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling householdLimitations Update. groupID: {0}, dlmId:{1}, exception: {2}", groupId, limitId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (!response.resp.IsOkStatusCode())
            {
                throw new ClientException(response.resp);
            }

            KalturaHouseholdLimitations newHouseholdLimitations = Mapper.Map<KalturaHouseholdLimitations>(response.dlm);
            return newHouseholdLimitations;
        }

        public bool IsDLMInUse(int groupId, int dlmId) 
        {
            var response = new GenericResponse<bool>();
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // Checking if DLM exist - throws error if not
                    GetDomainLimitationModule(groupId, dlmId);

                    response = new Core.Domains.Module().IsDLMInUse(dlmId, groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat($"Exception received while calling DLM service.(IsUsed)  exception: {ex}");
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            return response.Object;
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            result = Mapper.Map<KalturaHousehold>(response.DomainResponse.m_oDomain);

            return result;
        }

        public KalturaHouseholdDevice SetDeviceInfo(int groupId, string device_name, string udid)
        {
            var dDevice = new DomainDevice { Name = device_name, Udid = udid };
            return SetDeviceInfo(groupId, dDevice);
        }

        internal KalturaHouseholdDevice SetDeviceInfo(int groupId, KalturaHouseholdDevice device, bool allowNullExternalId = false, bool allowNullMacAddress = false, bool allowNullDynamicData = false)
        {
            var dDevice = CastToDomainDevice(device);
            return SetDeviceInfo(groupId, dDevice, allowNullExternalId, allowNullMacAddress, allowNullDynamicData);
        }

        internal KalturaHouseholdDevice SetDeviceInfo(int groupId, DomainDevice dDevice, bool allowNullExternalId = false, bool allowNullMacAddress = false, bool allowNullDynamicData = false)
        {
            KalturaHouseholdDevice result;
            DeviceResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.SetDevice(groupId, dDevice, allowNullExternalId, allowNullMacAddress, allowNullDynamicData);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Device == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            result = Mapper.Map<KalturaHouseholdDevice>(response.Device.m_oDevice);

            return result;
        }

        internal KalturaHousehold SetDomainInfo(int groupId, int domainId, string name, string description, int? regionId, string externalId = null)
        {
            KalturaHousehold result = null;
            DomainStatusResponse response = null;


            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.SetDomainInfo(groupId, domainId, name, description, regionId, externalId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException(StatusCode.Error);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null || response.DomainResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            result = Mapper.Map<KalturaHousehold>(response.DomainResponse.m_oDomain);

            return result;
        }

        internal bool RemoveDomain(int groupId, int householdId, bool purge)
        {
            ApiObjects.Response.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.RemoveDomainById(groupId, householdId, purge);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

        internal bool RemoveDomain(int groupId, string externalId, bool purge)
        {
            ApiObjects.Response.Status response = null;



            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.RemoveDomainByCoGuid(groupId, externalId, purge);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

        internal bool Suspend(int groupId, int domainId, int? roleId = null)
        {
            ApiObjects.Response.Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.SuspendDomain(groupId, domainId, roleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(response.Status);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            return true;
        }

        internal KalturaHouseholdDevice SubmitAddDeviceToDomain(
            int groupId,
            int domainId,
            string userId,
            KalturaHouseholdDevice device)
        {
            DeviceResponse response = null;

            try
            {
                var dDevice = CastToDomainDevice(device);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.SubmitAddDeviceToDomain(groupId, domainId, userId, dDevice);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.Device == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            KalturaHouseholdDevice result = Mapper.Map<KalturaHouseholdDevice>(response.Device.m_oDevice);

            return result;
        }

        private static DomainDevice CastToDomainDevice(KalturaHouseholdDevice device)
        {
            var domainDevice = Mapper.Map<DomainDevice>(device);

            // BEO-8671: as part of business requirements, entries with empty values are not saved
            domainDevice.DynamicData = domainDevice.DynamicData?
                .Where(x => !string.IsNullOrEmpty(x.value))
                .Select(x => new ApiObjects.KeyValuePair(x.key, x.value))
                .ToList();

            return domainDevice;
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

            if (response.DomainResponse == null || response.DomainResponse.m_oDomain == null)
            {
                throw new ClientException(StatusCode.Error);
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

        internal KalturaHouseholdDeviceListResponse GetHouseholdDevices(int groupId, KalturaHousehold household, List<long> deviceFamilyIds,
            string externalId, KalturaHouseholdDeviceOrderBy orderBy = default)
        {
            if (household == null)
            {
                var deviceId = DeviceRepository.GetDeviceIdByExternalId(groupId, externalId);
                int.TryParse(deviceId, out int _deviceId);
                var device = new Device(groupId);
                device.Initialize(_deviceId);
                household = ClientsManager.DomainsClient().GetDomainInfo(groupId, device.m_domainID);
            }

            var response = new KalturaHouseholdDeviceListResponse() { TotalCount = 0, Objects = new List<KalturaHouseholdDevice>() };

            bool checkExternal = !string.IsNullOrEmpty(externalId);
            var userId = 0;
            var _userId = household.MasterUsers?.FirstOrDefault()?.Id;

            if (!string.IsNullOrEmpty(_userId))
            {
                int.TryParse(_userId, out userId);
            }

            foreach (KalturaDeviceFamily family in household.DeviceFamilies)
            {
                if (deviceFamilyIds == null || deviceFamilyIds.Contains(family.Id.Value))
                {
                    List<KalturaDevice> familyDevices = family.Devices;
                    foreach (KalturaDevice device in familyDevices)
                    {
                        device.LastActivityTime = GetLastActivityTime(groupId, device.Udid);
                        KalturaHouseholdDevice householdDevice = (KalturaHouseholdDevice)device;
                        householdDevice.DeviceFamilyId = family.Id;

                        if (checkExternal)
                        {
                            if (device.ExternalId == externalId)
                            {
                                householdDevice.DeviceFamilyId = family.Id;
                                response.Objects.Add(householdDevice);
                                response.TotalCount = 1;

                                return response;
                            }
                        }
                        else
                        {
                            response.Objects.Add(householdDevice);
                            response.TotalCount++;
                        }
                    }
                }
            }

            if (orderBy != default && response.Objects?.Count > 1)
            {
                switch (orderBy)
                {
                    case KalturaHouseholdDeviceOrderBy.CREATED_DATE_ASC:
                        response.Objects = response.Objects.OrderBy(device => device.ActivatedOn.HasValue)
                            .ThenBy(device => device.ActivatedOn).ToList();
                        break;
                    case KalturaHouseholdDeviceOrderBy.CREATED_DATE_DESC:
                        response.Objects = response.Objects.OrderByDescending(device => device.ActivatedOn.HasValue)
                            .ThenBy(device => device.ActivatedOn).ToList();
                        break;
                    default:
                        break;
                }
            }

            return response;
        }

        private long? GetLastActivityTime(int groupId, string udid)
        {
            return DeviceRemovalPolicyHandler.Instance.GetUdidLastActivity(groupId, udid);
        }

        internal bool DeleteDevice(int groupId, string udid, out long domainId)
        {
            ApiObjects.Response.Status response = null;
            domainId = 0;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.DeleteDevice(groupId, udid, out domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

        internal KalturaHousehold GetDomain(int groupId, int domainId)
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
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            result = Mapper.Map<KalturaHousehold>(response.Domain);

            return result;
        }

        internal bool ShouldPurgeDomain(int groupId, int householdId)
        {
            ApiObjects.Response.Status response = null;
            bool shouldPurgeDomain = false;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.ShouldPurgeDomain(groupId, householdId, out shouldPurgeDomain);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code == (int)StatusCode.OK)
            {
                return shouldPurgeDomain;
            }
            else
            {
                throw new ClientException(response);
            }

            return true;
        }

        internal bool PurgeDomain(int groupId, int householdId)
        {
            ApiObjects.Response.Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.PurgeDomain(groupId, householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service.  exception: {0}", ex);
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

        internal KalturaHouseholdLimitationsListResponse GetDomainLimitationModule(int groupId)
        {
            KalturaHouseholdLimitationsListResponse result = new KalturaHouseholdLimitationsListResponse() { TotalCount = 0 };

            Func<GenericListResponse<LimitationsManager>> getLimitationsManagerFunc = () => Core.Domains.Module.GetDLMList(groupId);

            KalturaGenericListResponse<KalturaHouseholdLimitations> response =
                ClientUtils.GetResponseListFromWS<KalturaHouseholdLimitations, LimitationsManager>(getLimitationsManagerFunc);

            result.Objects = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        public bool DeleteDomainLimitationModule(int groupId, int householdLimitationsId, long userId)
        {
            Func<ApiObjects.Response.Status> deleteDlmFunc = () => new Core.Domains.Module().DeleteDLM(groupId, householdLimitationsId, userId);
            var response = ClientUtils.GetResponseStatusFromWS(deleteDlmFunc);

            return response;
        }

        internal KalturaHouseholdListResponse GetDomains(ContextData contextData, KalturaHouseholdFilter kalturaHouseholdFilter)
        {
            var result = new KalturaHouseholdListResponse();

            var filter = Mapper.Map<DomainFilter>(kalturaHouseholdFilter);

            Func<GenericListResponse<Domain>> getDomainsFunc = () =>
                Core.Domains.Module.GetDomains(contextData, filter);

            KalturaGenericListResponse<KalturaHousehold> response =
                ClientUtils.GetResponseListFromWS<KalturaHousehold, Domain>(getDomainsFunc);

            result.Objects = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaDynamicData UpsertDeviceDynamicData(int groupId, string udid, string key, KalturaStringValue value)
        {
            Func<GenericResponse<ApiObjects.KeyValuePair>> upsertDataFunc = () => Core.Domains.Module.UpsertDeviceDynamicData(groupId, udid, new ApiObjects.KeyValuePair(key, value.value));
            var response = ClientUtils.GetResponseFromWS<KalturaDynamicData, ApiObjects.KeyValuePair>(upsertDataFunc);

            return response;
        }

        internal bool DeleteDeviceDynamicData(int groupId, string udid, string key)
        {
            Func<ApiObjects.Response.Status> deleteDataFunc = () => Core.Domains.Module.DeleteDeviceDynamicData(groupId, udid, key);
            var response = ClientUtils.GetResponseStatusFromWS(deleteDataFunc);

            return response;
        }
    }
}
