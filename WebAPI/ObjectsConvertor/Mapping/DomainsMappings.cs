using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Mapping.ObjectsConvertor
{
    public class DomainsMappings
    {
        public static void RegisterMappings()
        {
            //Device
            Mapper.CreateMap<WebAPI.Domains.Device, KalturaHouseholdDevice>()
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.m_deviceUDID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_deviceName))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.m_deviceBrand))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.m_deviceBrandID))
                .ForMember(dest => dest.ActivatedOn, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_activationDate)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ConvertDeviceStatus(src.m_state)))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => ConvertDeviceState(src.m_state)))
                .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.m_domainID));

            Mapper.CreateMap<WebAPI.Domains.Device, KalturaDevice>()
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.m_deviceUDID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_deviceName))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.m_deviceBrand))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.m_deviceBrandID))
                .ForMember(dest => dest.ActivatedOn, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_activationDate)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ConvertDeviceStatus(src.m_state)))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => ConvertDeviceState(src.m_state)))
                .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.m_domainID));

            //HomeNetwork
            Mapper.CreateMap<WebAPI.Domains.HomeNetwork, KalturaHomeNetwork>()
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.UID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            //DeviceContainer to KalturaHouseholdDeviceFamily
            Mapper.CreateMap<WebAPI.Domains.DeviceContainer, KalturaDeviceFamily>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_deviceFamilyID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_deviceFamilyName))
                .ForMember(dest => dest.DeviceLimit, opt => opt.MapFrom(src => src.m_deviceLimit))
                .ForMember(dest => dest.ConcurrentLimit, opt => opt.MapFrom(src => src.m_deviceConcurrentLimit))
                .ForMember(dest => dest.Devices, opt => opt.MapFrom(src => src.DeviceInstances));

            //DeviceFamilyLimitations to KalturaDeviceFamily
            Mapper.CreateMap<WebAPI.Domains.DeviceFamilyLimitations, KalturaHouseholdDeviceFamilyLimitations>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.deviceFamily))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.deviceFamilyName))
                .ForMember(dest => dest.DeviceLimit, opt => opt.MapFrom(src => src.quantity))
                .ForMember(dest => dest.ConcurrentLimit, opt => opt.MapFrom(src => src.concurrency))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency));

            //Domain
            Mapper.CreateMap<WebAPI.Domains.Domain, KalturaHousehold>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nDomainID))
                .ForMember(dest => dest.DefaultUsers, opt => opt.MapFrom(src => src.m_DefaultUsersIDs))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.ConcurrentLimit, opt => opt.MapFrom(src => src.m_nConcurrentLimit))
                .ForMember(dest => dest.DevicesLimit, opt => opt.MapFrom(src => src.m_nDeviceLimit))
                .ForMember(dest => dest.HouseholdLimitationsId, opt => opt.MapFrom(src => src.m_nLimit))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_sCoGuid))
                .ForMember(dest => dest.FrequencyNextDeviceAction, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_NextActionFreq)))
                .ForMember(dest => dest.FrequencyNextUserAction, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_NextUserActionFreq)))
                .ForMember(dest => dest.IsFrequencyEnabled, opt => opt.MapFrom(src => src.m_frequencyFlag))
                .ForMember(dest => dest.MasterUsers, opt => opt.MapFrom(src => src.m_masterGUIDs))
                .ForMember(dest => dest.PendingUsers, opt => opt.MapFrom(src => src.m_PendingUsersIDs))
                .ForMember(dest => dest.RegionId, opt => opt.MapFrom(src => src.m_nRegion))
                .ForMember(dest => dest.Restriction, opt => opt.MapFrom(src => ConvertDomainRestriction(src.m_DomainRestriction)))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => ConvertDomainStatus(src.m_DomainStatus)))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.m_UsersIDs))
                .ForMember(dest => dest.UsersLimit, opt => opt.MapFrom(src => src.m_nUserLimit))
                .ForMember(dest => dest.DeviceFamilies, opt => opt.MapFrom(src => src.m_deviceFamilies));

            //string (pin) to KalturaDevicePin
            Mapper.CreateMap<string, KalturaDevicePin>()
                .ForMember(dest => dest.Pin, opt => opt.MapFrom(src => src));

            //DLM to KalturaHouseholdLimitationModule
            Mapper.CreateMap<WebAPI.Domains.LimitationsManager, KalturaHouseholdLimitations>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.domianLimitID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DomainLimitName))
                .ForMember(dest => dest.ConcurrentLimit, opt => opt.MapFrom(src => src.Concurrency))
                .ForMember(dest => dest.DeviceFrequency, opt => opt.MapFrom(src => src.Frequency))
                .ForMember(dest => dest.DeviceFrequencyDescription, opt => opt.MapFrom(src => src.FrequencyDescription))
                .ForMember(dest => dest.DeviceLimit, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.NpvrQuotaInSeconds, opt => opt.MapFrom(src => src.npvrQuotaInSecs))
                .ForMember(dest => dest.UsersLimit, opt => opt.MapFrom(src => src.nUserLimit))
                .ForMember(dest => dest.UserFrequency, opt => opt.MapFrom(src => src.UserFrequency))
                .ForMember(dest => dest.UserFrequencyDescription, opt => opt.MapFrom(src => src.UserFrequencyDescrition))
                .ForMember(dest => dest.DeviceFamiliesLimitations, opt => opt.MapFrom(src => src.lDeviceFamilyLimitations));
        }
        private static KalturaHouseholdState ConvertDomainStatus(WebAPI.Domains.DomainStatus type)
        {
            KalturaHouseholdState result;
            switch (type)
            {
                case WebAPI.Domains.DomainStatus.OK:
                    result = KalturaHouseholdState.ok;
                    break;
                case WebAPI.Domains.DomainStatus.DomainCreatedWithoutNPVRAccount:
                    result = KalturaHouseholdState.created_without_npvr_account;
                    break;
                case WebAPI.Domains.DomainStatus.DomainSuspended:
                    result = KalturaHouseholdState.suspended;
                    break;
                case WebAPI.Domains.DomainStatus.NoUsersInDomain:
                    result = KalturaHouseholdState.no_users_in_household;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown domain state");
            }
            return result;
        }

        private static KalturaHouseholdRestriction ConvertDomainRestriction(WebAPI.Domains.DomainRestriction type)
        {
            KalturaHouseholdRestriction result;
            switch (type)
            {
                case WebAPI.Domains.DomainRestriction.Unrestricted:
                    result = KalturaHouseholdRestriction.not_restricted;
                    break;
                case WebAPI.Domains.DomainRestriction.DeviceMasterRestricted:
                    result = KalturaHouseholdRestriction.device_master_restricted;
                    break;
                case WebAPI.Domains.DomainRestriction.UserMasterRestricted:
                    result = KalturaHouseholdRestriction.user_master_restricted;
                    break;
                case WebAPI.Domains.DomainRestriction.DeviceUserMasterRestricted:
                    result = KalturaHouseholdRestriction.device_user_master_restricted;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown domain_restriction value");
            }
            return result;
        }

        private static KalturaDeviceState? ConvertDeviceState(WebAPI.Domains.DeviceState type)
        {
            KalturaDeviceState result;
            switch (type)
            {
                case WebAPI.Domains.DeviceState.Activated:
                    result = KalturaDeviceState.activated;
                    break;
                case WebAPI.Domains.DeviceState.Pending:
                    result = KalturaDeviceState.pending;
                    break;
                case WebAPI.Domains.DeviceState.UnActivated:
                    result = KalturaDeviceState.not_activated;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown device state");
            }
            return result;
        }

        private static KalturaDeviceStatus? ConvertDeviceStatus(WebAPI.Domains.DeviceState type)
        {
            KalturaDeviceStatus result;
            switch (type)
            {
                case WebAPI.Domains.DeviceState.Activated:
                    result = KalturaDeviceStatus.ACTIVATED;
                    break;
                case WebAPI.Domains.DeviceState.Pending:
                    result = KalturaDeviceStatus.PENDING;
                    break;
                case WebAPI.Domains.DeviceState.UnActivated:
                    result = KalturaDeviceStatus.NOT_ACTIVATED;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown device state");
            }
            return result;
        }

        public static KalturaDeviceRegistrationStatus ConvertRegistrationStatus(WebAPI.Domains.DeviceRegistrationStatus status)
        {
            KalturaDeviceRegistrationStatus result;
            switch (status)
            {
                case WebAPI.Domains.DeviceRegistrationStatus.NotRegistered:
                    result = KalturaDeviceRegistrationStatus.not_registered;
                    break;
                case WebAPI.Domains.DeviceRegistrationStatus.Registered:
                    result = KalturaDeviceRegistrationStatus.registered;
                    break;
                case WebAPI.Domains.DeviceRegistrationStatus.RegisteredToAnotherDomain:
                    result = KalturaDeviceRegistrationStatus.registered_to_another_household;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown device registration status");
            }
            return result;
        }
        
        public static int ConvertKalturaHouseholdFrequency(KalturaHouseholdFrequencyType householdFrequencyType)
        {
            int result;

            switch (householdFrequencyType)
            {
                case KalturaHouseholdFrequencyType.devices:
                    result = 1; 
                    break;
                case KalturaHouseholdFrequencyType.users:
                    result = 2; 
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown household frequency type");                    
            }

            return result;
        }
    
    }
}