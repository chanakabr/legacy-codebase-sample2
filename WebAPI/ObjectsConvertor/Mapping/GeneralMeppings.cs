using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class GeneralMeppings
    {
        public static void RegisterMappings()
        {
            // KalturaStringValue
            Mapper.CreateMap<string, KalturaStringValue>()
                .ForMember(dest => dest.value, opt => opt.MapFrom(src => src));

            // KalturaBooleanValue
            Mapper.CreateMap<bool, KalturaBooleanValue>()
                .ForMember(dest => dest.value, opt => opt.MapFrom(src => src));

            // KalturaDoubleValue
            Mapper.CreateMap<double, KalturaDoubleValue>()
                .ForMember(dest => dest.value, opt => opt.MapFrom(src => src));

            // KalturaIntegerValue
            Mapper.CreateMap<int, KalturaIntegerValue>()
                .ForMember(dest => dest.value, opt => opt.MapFrom(src => src));
        }
    }
}