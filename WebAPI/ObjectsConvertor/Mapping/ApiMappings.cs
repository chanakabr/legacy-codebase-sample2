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
                        throw new ClientException((int)StatusCode.Error, "Unknown asset type");
                }
                return result;
            });

            #region Parental Rules

            // Parental rule type
            Mapper.CreateMap<WebAPI.Api.eParentalRuleType, WebAPI.Models.General.eParentalRuleType>().ConstructUsing((WebAPI.Api.eParentalRuleType type) =>
            {
                WebAPI.Models.General.eParentalRuleType result;

                switch (type)
                {
                    case WebAPI.Api.eParentalRuleType.All:
                    result = Models.General.eParentalRuleType.all;
                    break;
                    case WebAPI.Api.eParentalRuleType.Movies:
                    result = Models.General.eParentalRuleType.movies;
                    break;
                    case WebAPI.Api.eParentalRuleType.TVSeries:
                    result = Models.General.eParentalRuleType.tv_series;
                    break;
                    default:
                    throw new ClientException((int)StatusCode.Error, "Unknown asset type");
                }

                return result;
            });


            // Rule level
            Mapper.CreateMap<WebAPI.Api.eRuleLevel, WebAPI.Models.General.eRuleLevel>().ConstructUsing((WebAPI.Api.eRuleLevel type) =>
            {
                WebAPI.Models.General.eRuleLevel result;

                switch (type)
                {
                    case WebAPI.Api.eRuleLevel.User:
                    result = Models.General.eRuleLevel.user;
                    break;
                    case WebAPI.Api.eRuleLevel.Domain:
                    result = Models.General.eRuleLevel.domain;
                    break;
                    case WebAPI.Api.eRuleLevel.Group:
                    result = Models.General.eRuleLevel.account;
                    break;
                    default:
                    throw new ClientException((int)StatusCode.Error, "Unknown asset type");

                }

                return result;
            });

            // ParentalRule
            Mapper.CreateMap<WebAPI.Api.ParentalRule, WebAPI.Models.General.ParentalRule>()
                .ForMember(dest => dest.blockAnonymousAccess, opt => opt.MapFrom(src => src.blockAnonymousAccess))
                .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.epgTagTypeId, opt => opt.MapFrom(src => src.epgTagTypeId))
                .ForMember(dest => dest.epgTagValues, opt => opt.MapFrom(src => src.epgTagValues))
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.isDefault, opt => opt.MapFrom(src => src.isDefault))
                .ForMember(dest => dest.mediaTagTypeId, opt => opt.MapFrom(src => src.mediaTagTypeId))
                .ForMember(dest => dest.mediaTagValues, opt => opt.MapFrom(src => src.mediaTagValues))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.order, opt => opt.MapFrom(src => src.order))
                .ForMember(dest => dest.origin, opt => opt.MapFrom(src => src.level))
                .ForMember(dest => dest.ruleType, opt => opt.MapFrom(src => src.ruleType));

            // PinResponse
            Mapper.CreateMap<WebAPI.Api.PinResponse, WebAPI.Models.General.PinResponse>()
                .ForMember(dest => dest.origin, opt => opt.MapFrom(src => src.level))
                .ForMember(dest => dest.PIN, opt => opt.MapFrom(src => src.pin));

            // Purchase Settings
            Mapper.CreateMap<WebAPI.Api.PurchaseSettingsResponse, WebAPI.Models.General.PurchaseSettingsResponse>()
                .ForMember(dest => dest.origin, opt => opt.MapFrom(src => src.level))
                .ForMember(dest => dest.pin, opt => opt.MapFrom(src => src.pin))
                .ForMember(dest => dest.type, opt => opt.MapFrom(src => src.type)); 

            #endregion

        }
    }
}