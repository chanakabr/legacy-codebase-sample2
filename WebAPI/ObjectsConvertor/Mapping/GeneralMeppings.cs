using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.General;
using AutoMapper.Configuration;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class GeneralMeppings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            // KalturaStringValue
            cfg.CreateMap<string, KalturaStringValue>()
                .ForMember(dest => dest.value, opt => opt.ResolveUsing(src => src));

            // KalturaBooleanValue
            cfg.CreateMap<bool, KalturaBooleanValue>()
                .ForMember(dest => dest.value, opt => opt.ResolveUsing(src => src));

            // KalturaDoubleValue
            cfg.CreateMap<double, KalturaDoubleValue>()
                .ForMember(dest => dest.value, opt => opt.ResolveUsing(src => src));

            // KalturaIntegerValue
            cfg.CreateMap<int, KalturaIntegerValue>()
                .ForMember(dest => dest.value, opt => opt.ResolveUsing(src => src));

            // KalturaLongValue
            cfg.CreateMap<long, KalturaLongValue>()
                .ForMember(dest => dest.value, opt => opt.ResolveUsing(src => src));
        }
    }
}