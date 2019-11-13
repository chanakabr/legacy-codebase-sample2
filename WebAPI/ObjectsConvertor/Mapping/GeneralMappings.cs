using WebAPI.Models.General;
using AutoMapper.Configuration;
using System;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class GeneralMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            // KalturaStringValue
            cfg.CreateMap<string, KalturaStringValue>()
                .ForMember(dest => dest.value, opt => opt.MapFrom(src => src));

            // KalturaBooleanValue
            cfg.CreateMap<bool, KalturaBooleanValue>()
                .ForMember(dest => dest.value, opt => opt.MapFrom(src => src));

            // KalturaDoubleValue
            cfg.CreateMap<double, KalturaDoubleValue>()
                .ForMember(dest => dest.value, opt => opt.MapFrom(src => src));

            // KalturaIntegerValue
            cfg.CreateMap<int, KalturaIntegerValue>()
                .ForMember(dest => dest.value, opt => opt.MapFrom(src => src));

            // KalturaLongValue
            cfg.CreateMap<long, KalturaLongValue>()
                .ForMember(dest => dest.value, opt => opt.MapFrom(src => src));
        }

        public static log4net.Core.Level ConvertLogLevel(KalturaLogLevel src)
        {
            switch (src)
            {
                case KalturaLogLevel.TRACE:
                    return log4net.Core.Level.Trace;
                case KalturaLogLevel.DEBUG:
                    return log4net.Core.Level.Debug;
                case KalturaLogLevel.INFO:
                    return log4net.Core.Level.Info;
                case KalturaLogLevel.WARN:
                    return log4net.Core.Level.Warn;
                case KalturaLogLevel.ERROR:
                    return log4net.Core.Level.Error;
                case KalturaLogLevel.ALL:
                    return log4net.Core.Level.All;
                default:
                    return log4net.Core.Level.All;
            }
        }
    }
}