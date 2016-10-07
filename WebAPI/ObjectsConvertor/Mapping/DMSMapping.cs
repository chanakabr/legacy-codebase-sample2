using ApiObjects.Response;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.DMS;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class DMSMapping
    {
        public static void RegisterMappings()
        {
            // from dms to local
            Mapper.CreateMap<DMSGroupConfiguration, KalturaConfigurationGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NumberOfDevices, opt => opt.MapFrom(src => src.NumberOfDevices))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId))
                .ForMember(dest => dest.ConfigFileIds, opt => opt.MapFrom(src => src.ConfigFileIds))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            // from local to dms  
            Mapper.CreateMap<KalturaConfigurationGroup, DMSGroupConfiguration>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NumberOfDevices, opt => opt.MapFrom(src => src.NumberOfDevices))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId))
                .ForMember(dest => dest.ConfigFileIds, opt => opt.MapFrom(src => src.ConfigFileIds))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            // from dms to local
            Mapper.CreateMap<DMSTagMapping, KalturaConfigurationGroupTag>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId))
                .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.Tag));

            // from local to dms  
            Mapper.CreateMap<KalturaConfigurationGroupTag, DMSTagMapping>()
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId))
                .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.Tag));
        }

        internal static eResponseStatus ConvertDMSStatus(DMSeResponseStatus dMSeResponseStatus)
        {
            switch (dMSeResponseStatus)
            {
                case DMSeResponseStatus.Unknown:
                case DMSeResponseStatus.Error:
                    return eResponseStatus.Error;
                case DMSeResponseStatus.OK:
                    return eResponseStatus.OK;
                case DMSeResponseStatus.Forbidden:
                    return eResponseStatus.Forbidden;
                case DMSeResponseStatus.IllegalQueryParams:
                    return eResponseStatus.IllegalQueryParams;
                case DMSeResponseStatus.IllegalPostData:
                    return eResponseStatus.IllegalPostData;
                case DMSeResponseStatus.NotExist:
                    return eResponseStatus.NotExist;
                case DMSeResponseStatus.PartnerMismatch:
                    return eResponseStatus.PartnerMismatch;
                case DMSeResponseStatus.AlreadyExist:
                    return eResponseStatus.ItemAlreadyExist;
                default:
                    return eResponseStatus.Error;
            }
        }
    }
}