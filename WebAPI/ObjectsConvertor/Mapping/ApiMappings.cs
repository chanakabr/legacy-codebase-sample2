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
            Mapper.CreateMap<WebAPI.Api.ParentalRule, WebAPI.Models.API.KalturaParentalRule>()
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
            Mapper.CreateMap<WebAPI.Api.PinResponse, WebAPI.Models.API.KalturaPinResponse>()
                .ForMember(dest => dest.origin, opt => opt.MapFrom(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.PIN, opt => opt.MapFrom(src => src.pin));

            // Purchase Settings
            Mapper.CreateMap<WebAPI.Api.PurchaseSettingsResponse, WebAPI.Models.API.KalturaPurchaseSettingsResponse>()
                .ForMember(dest => dest.origin, opt => opt.MapFrom(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.pin, opt => opt.MapFrom(src => src.pin))
                .ForMember(dest => dest.type, opt => opt.MapFrom(src => ConvertPurchaseSetting(src.type)));

            // Purchase Settings
            Mapper.CreateMap<WebAPI.Api.GenericRule, WebAPI.Models.API.KalturaGenericRule>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RuleType, opt => opt.MapFrom(src => ConvertRuleType(src.RuleType)));

            #endregion

        }

        private static Models.API.KalturaParentalRuleType ConvertParentalRuleType(WebAPI.Api.eParentalRuleType type)
        {
            WebAPI.Models.API.KalturaParentalRuleType result = Models.API.KalturaParentalRuleType.all;

            switch (type)
            {
                case WebAPI.Api.eParentalRuleType.All:
                result = Models.API.KalturaParentalRuleType.all;
                break;
                case WebAPI.Api.eParentalRuleType.Movies:
                result = Models.API.KalturaParentalRuleType.movies;
                break;
                case WebAPI.Api.eParentalRuleType.TVSeries:
                result = Models.API.KalturaParentalRuleType.tv_series;
                break;
                default:
                throw new ClientException((int)StatusCode.Error, "Unknown asset type");
            }

            return result;
        }

        private static Models.API.KalturaRuleLevel ConvertRuleLevel(WebAPI.Api.eRuleLevel? type)
        {
            WebAPI.Models.API.KalturaRuleLevel result = Models.API.KalturaRuleLevel.invalid;

            switch (type)
            {
                case WebAPI.Api.eRuleLevel.User:
                result = Models.API.KalturaRuleLevel.user;
                break;
                case WebAPI.Api.eRuleLevel.Domain:
                result = Models.API.KalturaRuleLevel.household;
                break;
                case WebAPI.Api.eRuleLevel.Group:
                result = Models.API.KalturaRuleLevel.account;
                break;
                default:
                throw new ClientException((int)StatusCode.Error, "Unknown rule level");

            }

            return result;
        }

        private static Models.API.KalturaPurchaseSettingsType ConvertPurchaseSetting(WebAPI.Api.ePurchaeSettingsType? type)
        {
            WebAPI.Models.API.KalturaPurchaseSettingsType result = Models.API.KalturaPurchaseSettingsType.block;

            switch (type)
            {
                case WebAPI.Api.ePurchaeSettingsType.Allow:
                result = Models.API.KalturaPurchaseSettingsType.allow;
                break;
                case WebAPI.Api.ePurchaeSettingsType.Ask:
                result = Models.API.KalturaPurchaseSettingsType.ask;
                break;
                case WebAPI.Api.ePurchaeSettingsType.Block:
                result = Models.API.KalturaPurchaseSettingsType.block;
                break;
                default:
                throw new ClientException((int)StatusCode.Error, "Unknown purchase setting");

            }

            return result;
        }

        private static Models.API.KalturaRuleType ConvertRuleType(WebAPI.Api.RuleType type)
        {
            WebAPI.Models.API.KalturaRuleType result;

            switch (type)
            {
                case RuleType.Parental:
                    result = Models.API.KalturaRuleType.Parental;
                    break;
                case RuleType.Geo:
                    result = Models.API.KalturaRuleType.Geo;
                    break;
                case RuleType.UserType:
                    result = Models.API.KalturaRuleType.UserType;
                    break;
                case RuleType.Device:
                    result = Models.API.KalturaRuleType.Device;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown rule type");
            }

            return result;
        }
    }
}