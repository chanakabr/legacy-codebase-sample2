using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Api;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Mapping.ObjectsConvertor
{
    public class ApiMappings
    {
        public static void RegisterMappings()
        {
            //Language 
            Mapper.CreateMap<LanguageObj, Language>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));

            //AssetType to Catalog.StatsType
            Mapper.CreateMap<WebAPI.Models.Catalog.AssetType, WebAPI.Catalog.StatsType>().ConstructUsing((AssetType type) =>
            {
                WebAPI.Catalog.StatsType result;
                switch (type)
                {
                    case AssetType.Media:
                        result = WebAPI.Catalog.StatsType.MEDIA;
                        break;
                    case AssetType.Epg:
                        result = WebAPI.Catalog.StatsType.EPG;
                        break;
                    default:
                        throw new InternalServerErrorException((int)StatusCode.Error, "Unknown asset type");
                }
                return result;
            });
        }
    }
}