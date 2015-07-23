using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class GeneralMappings
    {
        public static void RegisterMappings()
        {
            //Device
            Mapper.CreateMap<WebAPI.Domains.Device, Device>()
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.m_deviceUDID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_deviceName))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.m_deviceBrand))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.m_deviceBrandID))
                .ForMember(dest => dest.ActivatedOn, opt => opt.MapFrom(src => src.m_activationDate))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => ConvertDeviceState(src.m_state)));

            //HomeNetwork
            Mapper.CreateMap<WebAPI.Domains.HomeNetwork, HomeNetwork>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            //DeviceContainer to DeviceFamily
            Mapper.CreateMap<WebAPI.Domains.DeviceContainer, DeviceFamily>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_deviceFamilyID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_deviceFamilyName))
                .ForMember(dest => dest.DeviceLimit, opt => opt.MapFrom(src => src.m_deviceLimit))
                .ForMember(dest => dest.ConcurrentLimit, opt => opt.MapFrom(src => src.m_deviceConcurrentLimit))
                .ForMember(dest => dest.Devices, opt => opt.MapFrom(src => src.DeviceInstances));

            //Domain
            Mapper.CreateMap<WebAPI.Domains.Domain, Household>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nDomainID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.ConcurrentLimit, opt => opt.MapFrom(src => src.m_nConcurrentLimit))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.m_DefaultUsersIDs))
                .ForMember(dest => dest.DevicesLimit, opt => opt.MapFrom(src => src.m_nDeviceLimit))
                .ForMember(dest => dest.DlmId, opt => opt.MapFrom(src => src.m_nLimit))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_sCoGuid))
                .ForMember(dest => dest.FrequencyNextDeviceAction, opt => opt.MapFrom(src => src.m_NextActionFreq))
                .ForMember(dest => dest.FrequencyNextUserAction, opt => opt.MapFrom(src => src.m_NextUserActionFreq))
                .ForMember(dest => dest.HomeNetworks, opt => opt.MapFrom(src => src.m_homeNetworks))
                .ForMember(dest => dest.IsFrequencyEnabled, opt => opt.MapFrom(src => src.m_frequencyFlag))
                .ForMember(dest => dest.MasterUsers, opt => opt.MapFrom(src => src.m_masterGUIDs))
                .ForMember(dest => dest.PendingUsers, opt => opt.MapFrom(src => src.m_PendingUsersIDs))
                .ForMember(dest => dest.RegionId, opt => opt.MapFrom(src => src.m_nRegion))
                .ForMember(dest => dest.Restriction, opt => opt.MapFrom(src => ConvertDomainRestriction(src.m_DomainRestriction)))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => ConvertDomainStatus(src.m_DomainStatus)))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.m_UsersIDs))
                .ForMember(dest => dest.UsersLimit, opt => opt.MapFrom(src => src.m_nUserLimit))
                .ForMember(dest => dest.DeviceFamilies, opt => opt.MapFrom(src => src.m_deviceFamilies));
        }

    }
}