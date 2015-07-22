using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Api;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.ObjectsConvertor.Mapping
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
            Mapper.CreateMap<WebAPI.Models.Catalog.KalturaAssetType, WebAPI.Catalog.StatsType>().ConstructUsing((KalturaAssetType type) =>
            {
                WebAPI.Catalog.StatsType result;
                switch (type)
                {
                    case KalturaAssetType.media:
                        result = WebAPI.Catalog.StatsType.MEDIA;
                        break;
                    case KalturaAssetType.epg:
                        result = WebAPI.Catalog.StatsType.EPG;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown asset type");
                }
                return result;
            });

            #region Parental Rules

            // ParentalRule
            Mapper.CreateMap<WebAPI.Api.ParentalRule, WebAPI.Models.API.ParentalRule>()
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
                .ForMember(dest => dest.origin, opt => opt.MapFrom(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.ruleType, opt => opt.MapFrom(src => ConvertParentalRuleType(src.ruleType)));

            // PinResponse
            Mapper.CreateMap<WebAPI.Api.PinResponse, WebAPI.Models.API.PinResponse>()
                .ForMember(dest => dest.origin, opt => opt.MapFrom(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.PIN, opt => opt.MapFrom(src => src.pin));

            // Purchase Settings
            Mapper.CreateMap<WebAPI.Api.PurchaseSettingsResponse, WebAPI.Models.API.PurchaseSettingsResponse>()
                .ForMember(dest => dest.origin, opt => opt.MapFrom(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.pin, opt => opt.MapFrom(src => src.pin))
                .ForMember(dest => dest.type, opt => opt.MapFrom(src => ConvertPurchaseSetting(src.type)));

            // Purchase Settings
            Mapper.CreateMap<WebAPI.Api.GenericRule, WebAPI.Models.API.GenericRule>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RuleType, opt => opt.MapFrom(src => ConvertRuleType(src.RuleType)));

            #endregion

        }

        private static Models.API.eParentalRuleType ConvertParentalRuleType(WebAPI.Api.eParentalRuleType type)
        {
            WebAPI.Models.API.eParentalRuleType result = Models.API.eParentalRuleType.all;

            switch (type)
            {
                case WebAPI.Api.eParentalRuleType.All:
                result = Models.API.eParentalRuleType.all;
                break;
                case WebAPI.Api.eParentalRuleType.Movies:
                result = Models.API.eParentalRuleType.movies;
                break;
                case WebAPI.Api.eParentalRuleType.TVSeries:
                result = Models.API.eParentalRuleType.tv_series;
                break;
                default:
                throw new ClientException((int)StatusCode.Error, "Unknown asset type");
            }

            return result;
        }

        private static Models.API.eRuleLevel ConvertRuleLevel(WebAPI.Api.eRuleLevel? type)
        {
            WebAPI.Models.API.eRuleLevel result = Models.API.eRuleLevel.invalid;

            switch (type)
            {
                case WebAPI.Api.eRuleLevel.User:
                result = Models.API.eRuleLevel.user;
                break;
                case WebAPI.Api.eRuleLevel.Domain:
                result = Models.API.eRuleLevel.household;
                break;
                case WebAPI.Api.eRuleLevel.Group:
                result = Models.API.eRuleLevel.account;
                break;
                default:
                throw new ClientException((int)StatusCode.Error, "Unknown rule level");

            }

            return result;
        }

        private static Models.API.ePurchaeSettingsType ConvertPurchaseSetting(WebAPI.Api.ePurchaeSettingsType? type)
        {
            WebAPI.Models.API.ePurchaeSettingsType result = Models.API.ePurchaeSettingsType.block;

            switch (type)
            {
                case WebAPI.Api.ePurchaeSettingsType.Allow:
                result = Models.API.ePurchaeSettingsType.allow;
                break;
                case WebAPI.Api.ePurchaeSettingsType.Ask:
                result = Models.API.ePurchaeSettingsType.ask;
                break;
                case WebAPI.Api.ePurchaeSettingsType.Block:
                result = Models.API.ePurchaeSettingsType.block;
                break;
                default:
                throw new ClientException((int)StatusCode.Error, "Unknown purchase setting");

            }

            return result;
        }

        private static Models.API.RuleType ConvertRuleType(WebAPI.Api.RuleType type)
        {
            WebAPI.Models.API.RuleType result;

            switch (type)
            {
                case RuleType.Parental:
                    result = Models.API.RuleType.Parental;
                    break;
                case RuleType.Geo:
                    result = Models.API.RuleType.Geo;
                    break;
                case RuleType.UserType:
                    result = Models.API.RuleType.UserType;
                    break;
                case RuleType.Device:
                    result = Models.API.RuleType.Device;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown rule type");
            }

            return result;
        }
    }
}