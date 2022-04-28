using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Pricing;
using ApiObjects.Response;
using ApiObjects.Rules;
using AutoMapper;
using AutoMapper.Configuration;
using Core.ConditionalAccess;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using phoenix;
using Action = phoenix.RuleAction;
using AssetRule = phoenix.AssetRule;
using Status = phoenix.Status;
using Language = phoenix.Language;
using SlimAsset = phoenix.SlimAsset;

namespace GrpcAPI.Utils
{
    public static class GrpcMapping
    {
        static MapperConfigurationExpression cfg = new MapperConfigurationExpression();
        public static IMapper Mapper = new MapperConfiguration(cfg).CreateMapper();

        public static IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> map,
            Expression<Func<TDestination, object>> selector)
        {
            map.ForMember(selector, config => config.Ignore());
            return map;
        }
        
        public static void RegisterMappings()
        {
                cfg.ForAllPropertyMaps(
                    map => map.DestinationPropertyType.IsGenericType &&
                           map.DestinationPropertyType.GetGenericTypeDefinition() == typeof(RepeatedField<>),
                    (map, options) => options.UseDestinationValue());

                cfg.CreateMap<MediaConcurrencyRule, mediaConcurrencyRule>()
                .ForMember(dest => dest.RuleID, opt => opt.MapFrom(src => src.RuleID))
                .ForMember(dest => dest.TagTypeID, opt => opt.MapFrom(src => src.TagTypeID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TagType, opt => opt.MapFrom(src => src.TagType))
                .ForMember(dest => dest.AllTagValues, opt => opt.MapFrom(src => src.AllTagValues))
                .ForMember(dest => dest.TagValues, opt => opt.MapFrom(src => src.TagValues))
                .ForMember(dest => dest.BmId, opt => opt.MapFrom(src => src.bmId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.RestrictionPolicy, opt => opt.MapFrom(src => src.RestrictionPolicy))
                .ForMember(dest => dest.Limitation, opt => opt.MapFrom(src => src.Limitation));

            cfg.CreateMap<mediaConcurrencyRule, MediaConcurrencyRule>()
                .ForMember(dest => dest.RuleID, opt => opt.MapFrom(src => src.RuleID))
                .ForMember(dest => dest.TagTypeID, opt => opt.MapFrom(src => src.TagTypeID))
                .ForMember(dest => dest.TagType, opt => opt.MapFrom(src => src.TagType))
                .ForMember(dest => dest.AllTagValues, opt => opt.MapFrom(src => src.AllTagValues))
                .ForMember(dest => dest.TagValues, opt => opt.MapFrom(src => src.TagValues))
                .ForMember(dest => dest.bmId, opt => opt.MapFrom(src => src.BmId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.RestrictionPolicy, opt => opt.MapFrom(src => src.RestrictionPolicy))
                .ForMember(dest => dest.Limitation, opt => opt.MapFrom(src => src.Limitation));

            cfg.CreateMap<ApiObjects.MediaMarks.DevicePlayData, DevicePlayData>()
                .ForMember(dest => dest.UDID, opt => opt.MapFrom(src => src.UDID))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.PlayType, opt => opt.MapFrom(src => src.playType))
                .ForMember(dest => dest.AssetAction, opt => opt.MapFrom(src => src.AssetAction))
                .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(src => src.TimeStamp))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.MediaConcurrencyRuleIds, opt => opt.MapFrom(src => src.MediaConcurrencyRuleIds))
                .ForMember(dest => dest.AssetMediaConcurrencyRuleIds,
                    opt => opt.MapFrom(src => src.AssetMediaConcurrencyRuleIds))
                .ForMember(dest => dest.AssetEpgConcurrencyRuleIds,
                    opt => opt.MapFrom(src => src.AssetEpgConcurrencyRuleIds))
                .ForMember(dest => dest.DeviceFamilyId, opt => opt.MapFrom(src => src.DeviceFamilyId))
                .ForMember(dest => dest.NpvrId, opt => opt.MapFrom(src => src.NpvrId))
                .ForMember(dest => dest.ProgramId, opt => opt.MapFrom(src => src.ProgramId))
                .ForMember(dest => dest.DomainId, opt => opt.MapFrom(src => src.DomainId))
                .ForMember(dest => dest.PlayCycleKey, opt => opt.MapFrom(src => src.PlayCycleKey))
                .ForMember(dest => dest.BookmarkEventThreshold, opt => opt.MapFrom(src => src.BookmarkEventThreshold))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Revoke, opt => opt.MapFrom(src => src.Revoke))
                .ForMember(dest => dest.LinearWatchHistoryThreshold,
                    opt => opt.MapFrom(src => src.LinearWatchHistoryThreshold));

            cfg.CreateMap<DevicePlayData, ApiObjects.MediaMarks.DevicePlayData>()
                .ForMember(dest => dest.UDID, opt => opt.MapFrom(src => src.UDID))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.playType, opt => opt.MapFrom(src => src.PlayType))
                .ForMember(dest => dest.AssetAction, opt => opt.MapFrom(src => src.AssetAction))
                .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(src => src.TimeStamp))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.MediaConcurrencyRuleIds, opt => opt.MapFrom(src => src.MediaConcurrencyRuleIds))
                .ForMember(dest => dest.AssetMediaConcurrencyRuleIds,
                    opt => opt.MapFrom(src => src.AssetMediaConcurrencyRuleIds))
                .ForMember(dest => dest.AssetEpgConcurrencyRuleIds,
                    opt => opt.MapFrom(src => src.AssetEpgConcurrencyRuleIds))
                .ForMember(dest => dest.DeviceFamilyId, opt => opt.MapFrom(src => src.DeviceFamilyId))
                .ForMember(dest => dest.NpvrId, opt => opt.MapFrom(src => src.NpvrId))
                .ForMember(dest => dest.ProgramId, opt => opt.MapFrom(src => src.ProgramId))
                .ForMember(dest => dest.DomainId, opt => opt.MapFrom(src => src.DomainId))
                .ForMember(dest => dest.PlayCycleKey, opt => opt.MapFrom(src => src.PlayCycleKey))
                .ForMember(dest => dest.BookmarkEventThreshold, opt => opt.MapFrom(src => src.BookmarkEventThreshold))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Revoke, opt => opt.MapFrom(src => src.Revoke))
                .ForMember(dest => dest.LinearWatchHistoryThreshold,
                    opt => opt.MapFrom(src => src.LinearWatchHistoryThreshold));

            cfg.CreateMap<ApiObjects.Response.Status, Status>()
                .ForMember(dest => dest.Args, opt => opt.MapFrom(src => src.Args))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code));

            cfg.CreateMap<MediaFileItemPricesContainer, MediaFileItemPrice>()
                .ForMember(dest => dest.ItemPrices, opt => opt.MapFrom(src => src.m_oItemPrices))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_sProductCode))
                .ForMember(dest => dest.MediaFileID, opt => opt.MapFrom(src => src.m_nMediaFileID));
            
            cfg.CreateMap<MediaFileItemPrice, MediaFileItemPricesContainer>()
                .ForMember(dest => dest.m_oItemPrices, opt => opt.MapFrom(src => src.ItemPrices))
                .ForMember(dest => dest.m_sProductCode, opt => opt.MapFrom(src => src.ProductCode))
                .ForMember(dest => dest.m_nMediaFileID, opt => opt.MapFrom(src => src.MediaFileID));

            
            cfg.CreateMap<ItemPrice, ItemPriceContainer>()
                .ForMember(dest => dest.m_sPPVModuleCode, opt => opt.MapFrom(src => src.PPVModuleCode))
                .ForMember(dest => dest.m_bSubscriptionOnly, opt => opt.MapFrom(src => src.SubscriptionOnly))
                .ForMember(dest => dest.m_oPrice, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.OriginalPrice, opt => opt.MapFrom(src => src.OriginalPrice))
                .ForMember(dest => dest.m_oFullPrice, opt => opt.MapFrom(src => src.FullPrice))
                .ForMember(dest => dest.m_PriceReason, opt => opt.MapFrom(src => src.PriceReason))
                .ForMember(dest => dest.m_relevantSub, opt => opt.MapFrom(src => src.RelevantSub))
                .ForMember(dest => dest.m_relevantCol, opt => opt.MapFrom(src => src.RelevantCol))
                .ForMember(dest => dest.m_oPPVDescription, opt => opt.MapFrom(src => src.PPVDescription))
                .ForMember(dest => dest.m_couponStatus, opt => opt.MapFrom(src => src.CouponStatus))
                .ForMember(dest => dest.m_sFirstDeviceNameFound, opt => opt.MapFrom(src => src.FirstDeviceNameFound))
                .ForMember(dest => dest.m_bCancelWindow, opt => opt.MapFrom(src => src.CancelWindow))
                .ForMember(dest => dest.m_sPurchasedBySiteGuid, opt => opt.MapFrom(src => src.PurchasedBySiteGuid))
                .ForMember(dest => dest.m_lRelatedMediaFileIDs, opt => opt.MapFrom(src => src.RelatedMediaFileIDs))
                .ForMember(dest => dest.m_dtStartDate, opt => opt.MapFrom(src => FromUtcUnixTimestampSeconds(src.StartDate)))
                .ForMember(dest => dest.m_dtEndDate, opt => opt.MapFrom(src => FromUtcUnixTimestampSeconds(src.EndDate)))
                .ForMember(dest => dest.m_dtDiscountEndDate, opt => opt.MapFrom(src => FromUtcUnixTimestampSeconds(src.DiscountEndDate)))
                .ForMember(dest => dest.m_sProductCode, opt => opt.MapFrom(src => src.ProductCode));
            
                        
            cfg.CreateMap<ItemPriceContainer, ItemPrice>()
                .ForMember(dest => dest.PPVModuleCode, opt => opt.MapFrom(src => src.m_sPPVModuleCode))
                .ForMember(dest => dest.SubscriptionOnly , opt => opt.MapFrom(src => src.m_bSubscriptionOnly))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrice))
                .ForMember(dest => dest.OriginalPrice, opt => opt.MapFrom(src => src.OriginalPrice))
                .ForMember(dest => dest.FullPrice, opt => opt.MapFrom(src => src.m_oFullPrice))
                .ForMember(dest => dest.PriceReason, opt => opt.MapFrom(src => src.m_PriceReason))
                .ForMember(dest => dest.RelevantSub, opt => opt.MapFrom(src => src.m_relevantSub))
                .ForMember(dest => dest.RelevantCol, opt => opt.MapFrom(src => src.m_relevantCol))
                .ForMember(dest => dest.PPVDescription, opt => opt.MapFrom(src => src.m_oPPVDescription))
                .ForMember(dest => dest.CouponStatus, opt => opt.MapFrom(src => src.m_couponStatus))
                .ForMember(dest => dest.FirstDeviceNameFound, opt => opt.MapFrom(src => src.m_sFirstDeviceNameFound))
                .ForMember(dest => dest.CancelWindow, opt => opt.MapFrom(src => src.m_bCancelWindow))
                .ForMember(dest => dest.PurchasedBySiteGuid, opt => opt.MapFrom(src => src.m_sPurchasedBySiteGuid))
                .ForMember(dest => dest.RelatedMediaFileIDs, opt => opt.MapFrom(src => src.m_lRelatedMediaFileIDs))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dtStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dtEndDate)))
                .ForMember(dest => dest.DiscountEndDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dtDiscountEndDate)))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_sProductCode));
            
            cfg.CreateMap<Price, Core.Pricing.Price>()
                .ForMember(dest => dest.m_oCurrency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.m_dPrice, opt => opt.MapFrom(src => src.Price_))
                .ForMember(dest => dest.countryId, opt => opt.MapFrom(src => src.CountryId));
            
            cfg.CreateMap<Core.Pricing.Price, Price>()
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.m_oCurrency))
                .ForMember(dest => dest.Price_, opt => opt.MapFrom(src => src.m_dPrice))
                .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.countryId));

            
            cfg.CreateMap<Currency, Core.Pricing.Currency>()
                .ForMember(dest => dest.m_sCurrencyName, opt => opt.MapFrom(src => src.CurrencyName))
                .ForMember(dest => dest.m_sCurrencySign, opt => opt.MapFrom(src => src.CurrencySign))
                .ForMember(dest => dest.m_bIsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.m_sCurrencyCD2, opt => opt.MapFrom(src => src.CurrencyCD2))
                .ForMember(dest => dest.m_sCurrencyCD3, opt => opt.MapFrom(src => src.CurrencyCD3))
                .ForMember(dest => dest.m_nCurrencyID, opt => opt.MapFrom(src => src.CurrencyID));

            cfg.CreateMap<Core.Pricing.Currency, Currency>()
                .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.m_sCurrencyName))
                .ForMember(dest => dest.CurrencySign, opt => opt.MapFrom(src => src.m_sCurrencySign))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.m_bIsDefault))
                .ForMember(dest => dest.CurrencyCD2, opt => opt.MapFrom(src => src.m_sCurrencyCD2))
                .ForMember(dest => dest.CurrencyCD3, opt => opt.MapFrom(src => src.m_sCurrencyCD3))
                .ForMember(dest => dest.CurrencyID, opt => opt.MapFrom(src => src.m_nCurrencyID));

            cfg.CreateMap<Core.Pricing.Subscription, Subscription>()
                .ForMember(dest => dest.Codes, opt => opt.MapFrom(src => src.m_sCodes))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.StartDate,
                    opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dEndDate)))
                .ForMember(dest => dest.SFileTypes, opt => opt.MapFrom(src => src.m_sFileTypes))
                .ForMember(dest => dest.NumberOfRecPeriods, opt => opt.MapFrom(src => src.m_nNumberOfRecPeriods))
                .ForMember(dest => dest.SubscriptionPriceCode, opt => opt.MapFrom(src => src.m_oSubscriptionPriceCode))
                .ForMember(dest => dest.ExtDiscountModule, opt => opt.MapFrom(src => src.m_oExtDisountModule))
                .ForMember(dest => dest.SubscriptionUsageModule,
                    opt => opt.MapFrom(src => src.m_oSubscriptionUsageModule))
                .ForMember(dest => dest.FictivicMediaID, opt => opt.MapFrom(src => src.m_fictivicMediaID))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.m_Priority))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_ProductCode))
                .ForMember(dest => dest.SubscriptionCode, opt => opt.MapFrom(src => src.m_SubscriptionCode))
                .ForMember(dest => dest.MultiSubscriptionUsageModule,
                    opt => opt.MapFrom(src => src.m_MultiSubscriptionUsageModule))
                .ForMember(dest => dest.GeoCommerceID, opt => opt.MapFrom(src => src.n_GeoCommerceID))
                .ForMember(dest => dest.UserTypes, opt => opt.MapFrom(src => src.m_UserTypes))
                .ForMember(dest => dest.PreviewModule, opt => opt.MapFrom(src => src.m_oPreviewModule))
                .ForMember(dest => dest.DomainLimitationModule,
                    opt => opt.MapFrom(src => src.m_nDomainLimitationModule))
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.m_lServices))
                .ForMember(dest => dest.GracePeriodMinutes, opt => opt.MapFrom(src => src.m_GracePeriodMinutes))
                .ForMember(dest => dest.PreSaleDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.PreSaleDate)))
                .ForMember(dest => dest.SubscriptionSetIdsToPriority,
                    opt => opt.MapFrom(src => src.SubscriptionSetIdsToPriority))
                .ForMember(dest => dest.CouponsGroups, opt => opt.MapFrom(src => src.CouponsGroups))
                .ForMember(dest => dest.ExternalProductCodes, opt => opt.MapFrom(src => src.ExternalProductCodes))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.IsInfiniteRecurring, opt => opt.MapFrom(src => src.m_bIsInfiniteRecurring))
                .ForPath(dest => dest.PPVModule.Alias, opt => opt.MapFrom(src => src.alias))
                .ForPath(dest => dest.PPVModule.ObjectCode, opt => opt.MapFrom(src => src.m_sObjectCode))
                .ForPath(dest => dest.PPVModule.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForPath(dest => dest.PPVModule.PriceCode, opt => opt.MapFrom(src => src.m_oPriceCode))
                .ForPath(dest => dest.PPVModule.UsageModule, opt => opt.MapFrom(src => src.m_oUsageModule))
                .ForPath(dest => dest.PPVModule.ObjectVirtualName, opt => opt.MapFrom(src => src.m_sObjectVirtualName))
                .ForPath(dest => dest.PPVModule.SubscriptionOnly, opt => opt.MapFrom(src => src.m_bSubscriptionOnly))
                .ForPath(dest => dest.PPVModule.RelatedFileTypes, opt => opt.MapFrom(src => src.m_relatedFileTypes))
                .ForPath(dest => dest.PPVModule.ProductCode, opt => opt.MapFrom(src => src.m_oCouponsGroup))
                .ForPath(dest => dest.PPVModule.DiscountModule, opt => opt.MapFrom(src => src.m_oDiscountModule))
                .ForPath(dest => dest.PPVModule.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
                .ForPath(dest => dest.PPVModule.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForPath(dest => dest.PPVModule.CreateDate,
                    opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForPath(dest => dest.PPVModule.UpdateDate,
                    opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.UpdateDate)))
                .ForPath(dest => dest.PPVModule.FirstDeviceLimitation,
                    opt => opt.MapFrom(src => src.m_bFirstDeviceLimitation));

            cfg.CreateMap<Core.Pricing.Collection, Collection>()
                .ForMember(dest => dest.Codes, opt => opt.MapFrom(src => src.m_sCodes))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dEndDate)))
                .ForMember(dest => dest.StartDate,
                    opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dStartDate)))
                .ForMember(dest => dest.SFileTypes, opt => opt.MapFrom(src => src.m_sFileTypes))
                .ForMember(dest => dest.CollectionPriceCode, opt => opt.MapFrom(src => src.m_oCollectionPriceCode))
                .ForMember(dest => dest.ExtDiscountModule, opt => opt.MapFrom(src => src.m_oExtDisountModule))
                .ForMember(dest => dest.CollectionUsageModule, opt => opt.MapFrom(src => src.m_oCollectionUsageModule))
                .ForMember(dest => dest.FictivicMediaID, opt => opt.MapFrom(src => src.m_fictivicMediaID))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_ProductCode))
                .ForMember(dest => dest.CollectionCode, opt => opt.MapFrom(src => src.m_CollectionCode))
                .ForMember(dest => dest.CouponsGroups, opt => opt.MapFrom(src => src.CouponsGroups))
                .ForMember(dest => dest.ExternalProductCodes, opt => opt.MapFrom(src => src.ExternalProductCodes))
                .ForPath(dest => dest.PPVModule.Alias, opt => opt.MapFrom(src => src.alias))
                .ForPath(dest => dest.PPVModule.ObjectCode, opt => opt.MapFrom(src => src.m_sObjectCode))
                .ForPath(dest => dest.PPVModule.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForPath(dest => dest.PPVModule.PriceCode, opt => opt.MapFrom(src => src.m_oPriceCode))
                .ForPath(dest => dest.PPVModule.UsageModule, opt => opt.MapFrom(src => src.m_oUsageModule))
                .ForPath(dest => dest.PPVModule.ObjectVirtualName, opt => opt.MapFrom(src => src.m_sObjectVirtualName))
                .ForPath(dest => dest.PPVModule.SubscriptionOnly, opt => opt.MapFrom(src => src.m_bSubscriptionOnly))
                .ForPath(dest => dest.PPVModule.RelatedFileTypes, opt => opt.MapFrom(src => src.m_relatedFileTypes))
                .ForPath(dest => dest.PPVModule.ProductCode, opt => opt.MapFrom(src => src.m_oCouponsGroup))
                .ForPath(dest => dest.PPVModule.DiscountModule, opt => opt.MapFrom(src => src.m_oDiscountModule))
                .ForPath(dest => dest.PPVModule.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
                .ForPath(dest => dest.PPVModule.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForPath(dest => dest.PPVModule.CreateDate,
                    opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForPath(dest => dest.PPVModule.UpdateDate,
                    opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.UpdateDate)))
                .ForPath(dest => dest.PPVModule.FirstDeviceLimitation,
                    opt => opt.MapFrom(src => src.m_bFirstDeviceLimitation));

            cfg.CreateMap<Core.Pricing.PrePaidModule, PrePaidModule>()
                .ForMember(dest => dest.PriceCode, opt => opt.MapFrom(src => src.m_PriceCode))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_Description))
                .ForMember(dest => dest.CreditValue, opt => opt.MapFrom(src => src.m_CreditValue))
                .ForMember(dest => dest.UsageModule, opt => opt.MapFrom(src => src.m_UsageModule))
                .ForMember(dest => dest.DiscountModule, opt => opt.MapFrom(src => src.m_DiscountModule))
                .ForMember(dest => dest.CouponsGroup, opt => opt.MapFrom(src => src.m_CouponsGroup))
                .ForMember(dest => dest.ObjectCode, opt => opt.MapFrom(src => src.m_ObjectCode))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.m_Title))
                .ForMember(dest => dest.IsFixedCredit, opt => opt.MapFrom(src => src.m_isFixedCredit));

            cfg.CreateMap<Language, LanguageContainer>()
                .ForMember(dest => dest.m_sLanguageCode3, opt => opt.MapFrom(src => src.LanguageCode3))
                .ForMember(dest => dest.m_sValue, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));

            cfg.CreateMap<LanguageContainer, Language>()
                .ForMember(dest => dest.LanguageCode3, opt => opt.MapFrom(src => src.m_sLanguageCode3))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.m_sValue))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));

            cfg.CreateMap<BundleCodeContainer, BundleCode>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.m_sCode))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName));

            cfg.CreateMap<Core.Pricing.PriceCode, PriceCode>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.m_sCode))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrise))
                .ForMember(dest => dest.ObjectID, opt => opt.MapFrom(src => src.m_nObjectID));

            cfg.CreateMap<Core.Pricing.WhenAlgo, WhenAlgo>()
                .ForMember(dest => dest.AlgoType, opt => opt.MapFrom(src => src.m_eAlgoType))
                .ForMember(dest => dest.NTimes, opt => opt.MapFrom(src => src.m_nNTimes));

            cfg.CreateMap<Core.Pricing.SubscriptionCouponGroup, SubscriptionCouponGroup>()
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.endDate)))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.startDate)))
                .ForPath(dest => dest.CouponGroup.Alias, opt => opt.MapFrom(src => src.alias))
                .ForPath(dest => dest.CouponGroup.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForPath(dest => dest.CouponGroup.CouponGroupType, opt => opt.MapFrom(src => src.couponGroupType))
                .ForPath(dest => dest.CouponGroup.EndDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dEndDate)))
                .ForPath(dest => dest.CouponGroup.StartDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dStartDate)))
                .ForPath(dest => dest.CouponGroup.DiscountModuleCode, opt => opt.MapFrom(src => src.m_oDiscountCode))
                .ForPath(dest => dest.CouponGroup.DiscountCode, opt => opt.MapFrom(src => src.m_sDiscountCode))
                .ForPath(dest => dest.CouponGroup.GroupCode, opt => opt.MapFrom(src => src.m_sGroupCode))
                .ForPath(dest => dest.CouponGroup.GroupName, opt => opt.MapFrom(src => src.m_sGroupName))
                .ForPath(dest => dest.CouponGroup.MaxDomainUses, opt => opt.MapFrom(src => src.maxDomainUses))
                .ForPath(dest => dest.CouponGroup.FinancialEntityID,
                    opt => opt.MapFrom(src => src.m_nFinancialEntityID))
                .ForPath(dest => dest.CouponGroup.MaxRecurringUsesCountForCoupon,
                    opt => opt.MapFrom(src => src.m_nMaxRecurringUsesCountForCoupon))
                .ForPath(dest => dest.CouponGroup.MaxUseCountForCoupon,
                    opt => opt.MapFrom(src => src.m_nMaxUseCountForCoupon));

            cfg.CreateMap<Core.Pricing.DiscountModule, DiscountModule>()
                .ForMember(dest => dest.Percent, opt => opt.MapFrom(src => src.m_dPercent))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dEndDate)))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dStartDate)))
                .ForMember(dest => dest.TheRelationType, opt => opt.MapFrom(src => src.m_eTheRelationType))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.alias))
                .ForPath(dest => dest.PriceCode.Code, opt => opt.MapFrom(src => src.m_sCode))
                .ForPath(dest => dest.PriceCode.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForPath(dest => dest.PriceCode.Price, opt => opt.MapFrom(src => src.m_oPrise))
                .ForPath(dest => dest.PriceCode.ObjectID, opt => opt.MapFrom(src => src.m_nObjectID))
                .ForMember(dest => dest.WhenAlgo, opt => opt.MapFrom(src => src.m_oWhenAlgo));

            cfg.CreateMap<Core.Pricing.PreviewModule, PreviewModule>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.m_nID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.m_tsFullLifeCycle))
                .ForMember(dest => dest.NonRenewPeriod, opt => opt.MapFrom(src => src.m_tsNonRenewPeriod))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.alias));

            cfg.CreateMap<Core.Pricing.UsageModule, UsageModule>()
                .ForMember(dest => dest.ObjectID, opt => opt.MapFrom(src => src.m_nObjectID))
                .ForMember(dest => dest.VirtualName, opt => opt.MapFrom(src => src.m_sVirtualName))
                .ForMember(dest => dest.MaxNumberOfViews, opt => opt.MapFrom(src => src.m_nMaxNumberOfViews))
                .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.m_tsViewLifeCycle))
                .ForMember(dest => dest.MaxUsageModuleLifeCycle,
                    opt => opt.MapFrom(src => src.m_tsMaxUsageModuleLifeCycle))
                .ForMember(dest => dest.ExtDiscountId, opt => opt.MapFrom(src => src.m_ext_discount_id))
                .ForMember(dest => dest.InternalDiscountId, opt => opt.MapFrom(src => src.m_internal_discount_id))
                .ForMember(dest => dest.PricingId, opt => opt.MapFrom(src => src.m_pricing_id))
                .ForMember(dest => dest.CouponId, opt => opt.MapFrom(src => src.m_coupon_id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_type))
                .ForMember(dest => dest.SubscriptionOnly, opt => opt.MapFrom(src => src.m_subscription_only))
                .ForMember(dest => dest.IsRenew, opt => opt.MapFrom(src => src.m_is_renew))
                .ForMember(dest => dest.NumOfRecPeriods, opt => opt.MapFrom(src => src.m_num_of_rec_periods))
                .ForMember(dest => dest.DeviceLimitId, opt => opt.MapFrom(src => src.m_device_limit_id))
                .ForMember(dest => dest.CouponId, opt => opt.MapFrom(src => src.m_coupon_id))
                .ForMember(dest => dest.WaiverPeriod, opt => opt.MapFrom(src => src.m_nWaiverPeriod))
                .ForMember(dest => dest.IsOfflinePlayBack, opt => opt.MapFrom(src => src.m_bIsOfflinePlayBack))
                .ForMember(dest => dest.Waiver, opt => opt.MapFrom(src => src.m_bWaiver));

            cfg.CreateMap<ApiObjects.Rules.SlimAsset, SlimAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

            cfg.CreateMap<SlimAsset, ApiObjects.Rules.SlimAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

            cfg.CreateMap<ApiObjects.Rules.AssetRule, AssetRule>()
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ForPath(dest => dest.Rule.Id, opt => opt.MapFrom(src => src.Id))
                .ForPath(dest => dest.Rule.Description, opt => opt.MapFrom(src => src.Description))
                .ForPath(dest => dest.Rule.Label, opt => opt.MapFrom(src => src.Label))
                .ForPath(dest => dest.Rule.Name, opt => opt.MapFrom(src => src.Name))
                .ForPath(dest => dest.Rule.Status, opt => opt.MapFrom(src => src.Status))
                .ForPath(dest => dest.Rule.GroupId, opt => opt.MapFrom(src => src.GroupId));

            cfg.CreateMap<AssetRule, ApiObjects.Rules.AssetRule>()
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Rule.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Rule.Description))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Rule.Label))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Rule.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Rule.Status))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Rule.GroupId));

            cfg.CreateMap<ApiObjects.Rules.RuleCondition, RuleConditionKind>()
                .ConstructUsing(condition =>
                {
                    var conditionKind = new RuleConditionKind();
                    switch (condition.GetType().FullName)
                    {
                        case "ApiObjects.Rules.OrCondition":
                            conditionKind.OrCondition = Mapper.Map<phoenix.OrCondition>(condition);
                            break;
                        case "ApiObjects.Rules.NotRuleCondition":
                            conditionKind.NotRuleCondition = Mapper.Map<phoenix.NotRuleCondition>(condition);
                            break;
                        case "ApiObjects.Rules.BusinessModuleCondition":
                            conditionKind.BusinessModuleCondition =
                                Mapper.Map<phoenix.BusinessModuleCondition>(condition);
                            break;
                        case "ApiObjects.Rules.AssetCondition":
                            conditionKind.AssetCondition = Mapper.Map<phoenix.AssetCondition>(condition);
                            break;
                        case "ApiObjects.Rules.DateCondition":
                            conditionKind.DateCondition = Mapper.Map<phoenix.DateCondition>(condition);
                            break;
                        case "ApiObjects.Rules.ConcurrencyCondition":
                            conditionKind.ConcurrencyCondition =
                                Mapper.Map<phoenix.ConcurrencyCondition>(condition);
                            break;
                        case "ApiObjects.Rules.CountryCondition":
                            conditionKind.CountryCondition = Mapper.Map<phoenix.CountryCondition>(condition);
                            break;
                        case "ApiObjects.Rules.IpRangeCondition":
                            conditionKind.IpRangeCondition = Mapper.Map<phoenix.IpRangeCondition>(condition);
                            break;
                        case "ApiObjects.Rules.HeaderCondition":
                            conditionKind.HeaderCondition = Mapper.Map<phoenix.HeaderCondition>(condition);
                            break;
                        case "ApiObjects.Rules.UserSubscriptionCondition":
                            conditionKind.UserSubscriptionCondition =
                                Mapper.Map<phoenix.UserSubscriptionCondition>(condition);
                            break;
                        case "ApiObjects.Rules.AssetSubscriptionCondition":
                            conditionKind.AssetSubscriptionCondition =
                                Mapper.Map<phoenix.AssetSubscriptionCondition>(condition);
                            break;
                        case "ApiObjects.Rules.UserRoleCondition":
                            conditionKind.UserRoleCondition =
                                Mapper.Map<phoenix.UserRoleCondition>(condition);
                            break;
                        case "ApiObjects.Rules.UdidDynamicListCondition":
                            conditionKind.UdidDynamicListCondition =
                                Mapper.Map<phoenix.UdidDynamicListCondition>(condition);
                            break;
                        case "ApiObjects.Rules.UserSessionProfileCondition":
                            conditionKind.UserSessionProfileCondition =
                                Mapper.Map<phoenix.UserSessionProfileCondition>(condition);
                            break;
                        default:
                            break;
                    }

                    return conditionKind;
                });
                
              cfg.CreateMap<ApiObjects.Rules.RuleAction, RuleActionKind>()
                .ConstructUsing(action =>
                {
                    var actionKind = new RuleActionKind();
                    switch (action.GetType().FullName)
                    {
                        case "ApiObjects.Rules.AllowPlaybackAction":
                            actionKind.AllowPlaybackAction = Mapper.Map<phoenix.RuleAction.Types.AllowPlaybackAction>(action);
                            break;
                        case "ApiObjects.Rules.AssetBlockAction":
                            actionKind.AssetBlockAction = Mapper.Map<phoenix.RuleAction.Types.AssetBlockAction>(action);
                            break;
                        case "ApiObjects.Rules.BlockPlaybackAction":
                            actionKind.BlockPlaybackAction =
                                Mapper.Map<phoenix.RuleAction.Types.BlockPlaybackAction>(action);
                            break;
                        case "ApiObjects.Rules.ApplyFreePlaybackAction":
                            actionKind.ApplyFreePlaybackAction = Mapper.Map<phoenix.RuleAction.Types.ApplyFreePlaybackAction>(action);
                            break;
                        case "ApiObjects.Rules.ApplyDiscountModuleRuleAction":
                            actionKind.ApplyDiscountModuleRuleAction = Mapper.Map<phoenix.RuleAction.Types.ApplyDiscountModuleRuleAction>(action);
                            break;
                        case "ApiObjects.Rules.ApplyPlaybackAdapterRuleAction":
                            actionKind.ApplyPlaybackAdapterRuleAction =
                                Mapper.Map<phoenix.RuleAction.Types.ApplyPlaybackAdapterRuleAction>(action);
                            break;
                        case "ApiObjects.Rules.AssetUserRuleBlockAction":
                            actionKind.AssetUserRuleBlockAction = Mapper.Map<phoenix.RuleAction.Types.AssetUserRuleBlockAction>(action);
                            break;
                        case "ApiObjects.Rules.AssetUserRuleFilterAction":
                            actionKind.AssetUserRuleFilterAction = Mapper.Map<phoenix.RuleAction.Types.AssetUserRuleFilterAction>(action);
                            break;
                        case "ApiObjects.Rules.EndDateOffsetRuleAction":
                            actionKind.EndDateOffsetRuleAction = Mapper.Map<phoenix.RuleAction.Types.EndDateOffsetRuleAction>(action);
                            break;
                        case "ApiObjects.Rules.StartDateOffsetRuleAction":
                            actionKind.StartDateOffsetRuleAction =
                                Mapper.Map<phoenix.RuleAction.Types.StartDateOffsetRuleAction>(action);
                            break;
                        case "ApiObjects.Rules.AssetLifeCycleTagTransitionAction":
                            actionKind.AssetLifeCycleTagTransitionAction =
                                Mapper.Map<phoenix.RuleAction.Types.AssetLifeCycleTagTransitionAction>(action);
                            break;
                        case "ApiObjects.Rules.AssetLifeCycleBuisnessModuleTransitionAction":
                            actionKind.AssetLifeCycleBuisnessModuleTransitionAction =
                                Mapper.Map<phoenix.RuleAction.Types.AssetLifeCycleBuisnessModuleTransitionAction>(action);
                            break;
                        default:
                            break;
                    }

                    return actionKind;
                });
            
            cfg.CreateMap<phoenix.OrCondition, ApiObjects.Rules.OrCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ReverseMap();
                
            cfg.CreateMap<phoenix.BusinessModuleCondition, ApiObjects.Rules.BusinessModuleCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.BusinessModuleId, opt => opt.MapFrom(src => src.BusinessModuleId))
                .ForMember(dest => dest.BusinessModuleType, opt => opt.MapFrom(src => src.BusinessModuleType))
                .ReverseMap();
                
            cfg.CreateMap<phoenix.AssetCondition, ApiObjects.Rules.AssetCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql))
                .ReverseMap();
                
            cfg.CreateMap<phoenix.ConcurrencyCondition, ApiObjects.Rules.ConcurrencyCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Limit, opt => opt.MapFrom(src => src.Limit))
                .ForMember(dest => dest.RestrictionPolicy, opt => opt.MapFrom(src => src.RestrictionPolicy))
                .ReverseMap();

            cfg.CreateMap<phoenix.DateCondition, ApiObjects.Rules.DateCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ReverseMap();

            cfg.CreateMap<phoenix.IpRangeCondition, ApiObjects.Rules.IpRangeCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.FromIp, opt => opt.MapFrom(src => src.FromIp))
                .ForMember(dest => dest.ToIp, opt => opt.MapFrom(src => src.ToIp))
                .ForMember(dest => dest.IpFrom, opt => opt.MapFrom(src => src.IpFrom))
                .ForMember(dest => dest.IpTo, opt => opt.MapFrom(src => src.IpTo))
                .ReverseMap();

            cfg.CreateMap<phoenix.CountryCondition, ApiObjects.Rules.CountryCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not))
                .ForMember(dest => dest.Countries, opt => opt.MapFrom(src => src.Countries))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.HeaderCondition, ApiObjects.Rules.HeaderCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.UserSubscriptionCondition, ApiObjects.Rules.UserSubscriptionCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();
                
            cfg.CreateMap<phoenix.AssetSubscriptionCondition, ApiObjects.Rules.AssetSubscriptionCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.UdidDynamicListCondition, ApiObjects.Rules.UdidDynamicListCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.UserRoleCondition, ApiObjects.Rules.UserRoleCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.RoleIds, opt => opt.MapFrom(src => src.RoleIds))
                .ReverseMap();

            cfg.CreateMap<phoenix.UserSessionProfileCondition, ApiObjects.Rules.UserSessionProfileCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.SegmentsCondition, ApiObjects.Rules.SegmentsCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.SegmentIds, opt => opt.MapFrom(src => src.SegmentIds))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.DeviceBrandCondition, ApiObjects.Rules.DeviceBrandCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => src.IdIn))
                .ReverseMap();
                
            cfg.CreateMap<phoenix.DeviceFamilyCondition, ApiObjects.Rules.DeviceFamilyCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => src.IdIn))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.DeviceManufacturerCondition, ApiObjects.Rules.DeviceManufacturerCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => src.IdIn))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.DeviceModelCondition, ApiObjects.Rules.DeviceModelCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.RegexEqual, opt => opt.MapFrom(src => src.RegexEqual))
                .ReverseMap();
                
            cfg.CreateMap<phoenix.DynamicKeysCondition, ApiObjects.Rules.DynamicKeysCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.DeviceDynamicDataCondition, ApiObjects.Rules.DeviceDynamicDataCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction, ApiObjects.Rules.RuleAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.AssetBlockAction, ApiObjects.Rules.AssetBlockAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();            
            
            cfg.CreateMap<phoenix.RuleAction.Types.AssetUserRuleBlockAction, ApiObjects.Rules.AssetUserRuleBlockAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction.Types.AllowPlaybackAction, ApiObjects.Rules.AllowPlaybackAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction.Types.BlockPlaybackAction, ApiObjects.Rules.BlockPlaybackAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction.Types.TimeOffsetRuleAction, ApiObjects.Rules.TimeOffsetRuleAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Offset))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction.Types.TimeOffsetRuleAction, ApiObjects.Rules.EndDateOffsetRuleAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Offset))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction.Types.TimeOffsetRuleAction, ApiObjects.Rules.StartDateOffsetRuleAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Offset))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction.Types.AssetUserRuleFilterAction, ApiObjects.Rules.AssetUserRuleFilterAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ApplyOnChannel, opt => opt.MapFrom(src => src.ApplyOnChannel))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction.Types.ApplyDiscountModuleRuleAction, ApiObjects.Rules.ApplyDiscountModuleRuleAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.DiscountModuleId, opt => opt.MapFrom(src => src.DiscountModuleId))
                .ReverseMap();
                
            cfg.CreateMap<phoenix.RuleAction.Types.AssetLifeCycleTransitionAction, ApiObjects.Rules.AssetLifeCycleTransitionAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.ActionType))
                .ForMember(dest => dest.TransitionType, opt => opt.MapFrom(src => src.TransitionType))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction.Types.AssetLifeCycleTagTransitionAction, ApiObjects.Rules.AssetLifeCycleTagTransitionAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.ActionType))
                .ForMember(dest => dest.TransitionType, opt => opt.MapFrom(src => src.TransitionType))
                .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.TagIds))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction.Types.LifeCycleFileTypesAndPpvsTransitions, ApiObjects.AssetLifeCycleRules.LifeCycleFileTypesAndPpvsTransitions>()
                .ForMember(dest => dest.FileTypeIds, opt => opt.MapFrom(src => src.FileTypeIds))
                .ForMember(dest => dest.PpvIds, opt => opt.MapFrom(src => src.PpvIds))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction.Types.AssetLifeCycleBuisnessModuleTransitionAction, ApiObjects.Rules.AssetLifeCycleBuisnessModuleTransitionAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.ActionType))
                .ForMember(dest => dest.TransitionType, opt => opt.MapFrom(src => src.TransitionType))
                .ForMember(dest => dest.Transitions, opt => opt.MapFrom(src => src.Transitions))
                .ReverseMap();
            
            cfg.CreateMap<phoenix.RuleAction.Types.ApplyFreePlaybackAction, ApiObjects.Rules.ApplyFreePlaybackAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();
            
            cfg.CreateMap<Core.Users.DeviceContainer, deviceFamilyData>()
                .ForMember(dest => dest.FamilyId, opt => opt.MapFrom(src => src.m_deviceFamilyID))
                .ForMember(dest => dest.Concurrency, opt => opt.MapFrom(src => src.m_oLimitationsManager.Concurrency))
                .ForMember(dest => dest.Udids, opt => opt.MapFrom(src => src.DeviceInstances.Select(d => d.m_deviceUDID)));

            cfg.CreateMap<ApiObjects.TimeShiftedTv.Recording, Recording>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.EpgId))
                .ForMember(dest => dest.ChannelId, opt => opt.MapFrom(src => src.ChannelId))
                .ForMember(dest => dest.RecordingStatus, opt => opt.MapFrom(src => src.RecordingStatus))
                .ForMember(dest => dest.ExternalRecordingId, opt => opt.MapFrom(src => src.ExternalRecordingId))
                .ForMember(dest => dest.EpgStartDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.EpgStartDate)))
                .ForMember(dest => dest.EpgEndDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.EpgEndDate)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.GetStatusRetries, opt => opt.MapFrom(src => src.GetStatusRetries))
                .ForMember(dest => dest.ProtectedUntilDate, opt => opt.MapFrom(src => src.ProtectedUntilDate))
                .ForMember(dest => dest.ViewableUntilDate, opt => opt.MapFrom(src => src.ViewableUntilDate))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.UpdateDate)))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.Crid))
                .ForMember(dest => dest.IsExternalRecording, opt => opt.MapFrom(src => src.isExternalRecording))
                .ForMember(dest => dest.IsProtected, opt => opt.MapFrom(src => src.IsProtected))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            cfg.CreateMap<ApiObjects.EPGChannelProgrammeObject, phoenix.EPGChannelProgrammeObject>()
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.EPG_ID))
                .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.EPG_CHANNEL_ID))
                .ForMember(dest => dest.EpgIdentifier, opt => opt.MapFrom(src => src.EPG_IDENTIFIER))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DESCRIPTION))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.START_DATE))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.END_DATE))
                .ForMember(dest => dest.PicUrl, opt => opt.MapFrom(src => src.PIC_URL))
                .ForMember(dest => dest.PicId, opt => opt.MapFrom(src => src.PIC_ID))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.STATUS))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IS_ACTIVE))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.GROUP_ID))
                .ForMember(dest => dest.UpdaterId, opt => opt.MapFrom(src => src.UPDATER_ID))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UPDATE_DATE))
                .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.PUBLISH_DATE))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CREATE_DATE))
                .ForMember(dest => dest.LikeCounter, opt => opt.MapFrom(src => src.LIKE_COUNTER))
                .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.media_id))
                .ForMember(dest => dest.LinearMediaId, opt => opt.MapFrom(src => src.LINEAR_MEDIA_ID))
                .ForMember(dest => dest.EnableCdvr, opt => opt.MapFrom(src => src.ENABLE_CDVR))
                .ForMember(dest => dest.EnableCatchUp, opt => opt.MapFrom(src => src.ENABLE_CATCH_UP))
                .ForMember(dest => dest.ChannelCatchUpBuffer, opt => opt.MapFrom(src => src.CHANNEL_CATCH_UP_BUFFER))
                .ForMember(dest => dest.EnableStartOver, opt => opt.MapFrom(src => src.ENABLE_START_OVER))
                .ForMember(dest => dest.EnableTrickPlay, opt => opt.MapFrom(src => src.ENABLE_TRICK_PLAY))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.CRID));

            cfg.CreateMap<Core.Catalog.Response.MediaObj, GetMediaByIdResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.ExternalIDs, opt => opt.MapFrom(src => src.m_ExternalIDs))
                .ForMember(dest => dest.LastWatchedDevice, opt => opt.MapFrom(src => src.m_sLastWatchedDevice))
                .ForMember(dest => dest.SiteUserGuid, opt => opt.MapFrom(src => src.m_sSiteUserGuid))
                .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.EntryId))
                .ForMember(dest => dest.CoGuid, opt => opt.MapFrom(src => src.CoGuid))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.EnableCDVR, opt => opt.MapFrom(src => src.EnableCDVR))
                .ForMember(dest => dest.EnableCatchUp, opt => opt.MapFrom(src => src.EnableCatchUp))
                .ForMember(dest => dest.EnableStartOver, opt => opt.MapFrom(src => src.EnableStartOver))
                .ForMember(dest => dest.EnableTrickPlay, opt => opt.MapFrom(src => src.EnableTrickPlay))
                .ForMember(dest => dest.CatchUpBuffer, opt => opt.MapFrom(src => src.CatchUpBuffer))
                .ForMember(dest => dest.TrickPlayBuffer, opt => opt.MapFrom(src => src.TrickPlayBuffer))
                .ForMember(dest => dest.EnableRecordingPlaybackNonEntitledChannel,
                    opt => opt.MapFrom(src => src.EnableRecordingPlaybackNonEntitledChannel))
                .ForMember(dest => dest.ExternalCdvrId, opt => opt.MapFrom(src => src.ExternalCdvrId))
                .ForMember(dest => dest.WatchPermissionRule, opt => opt.MapFrom(src => src.WatchPermissionRule))
                .ForMember(dest => dest.GeoblockRule, opt => opt.MapFrom(src => src.GeoblockRule))
                .ForMember(dest => dest.DeviceRule, opt => opt.MapFrom(src => src.DeviceRule));

            cfg.CreateMap<Scheduling, GetProgramScheduleResponse>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.StartDate)))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.EndTime)));
            
            cfg.CreateMap<MeidaMaper, MediaMapper>()
                .ForMember(dest => dest.MediaFileID, opt => opt.MapFrom(src => src.m_nMediaFileID))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_sProductCode))
                .ForMember(dest => dest.MediaID, opt => opt.MapFrom(src => src.m_nMediaID));
            
            cfg.CreateMap<phoenix.MediaFile, ApiObjects.MediaFile>()
                .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.MediaId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.TypeId))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.AltUrl, opt => opt.MapFrom(src => src.AltUrl))
                .ForMember(dest => dest.DirectUrl, opt => opt.MapFrom(src => src.DirectUrl))
                .ForMember(dest => dest.AltDirectUrl, opt => opt.MapFrom(src => src.AltDirectUrl))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.StreamerType, opt => opt.MapFrom(src => src.StreamerType))
                .ForMember(dest => dest.IsTrailer, opt => opt.MapFrom(src => src.IsTrailer))
                .ForMember(dest => dest.CdnId, opt => opt.MapFrom(src => src.CdnId))
                .ForMember(dest => dest.AltCdnId, opt => opt.MapFrom(src => src.AltCdnId))
                .ForMember(dest => dest.DrmId, opt => opt.MapFrom(src => src.DrmId))
                .ForMember(dest => dest.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
                .ForMember(dest => dest.AdsParam, opt => opt.MapFrom(src => src.AdsParam))
                .ForMember(dest => dest.Opl, opt => opt.MapFrom(src => src.Opl))
                .ForMember(dest => dest.BusinessModuleDetails, opt => opt.MapFrom(src => src.BusinessModuleDetails))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.Labels)).ReverseMap();
            
            cfg.CreateMap<PaymentGateway, PaymentGatewayProfile>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.PaymentMethods, opt => opt.MapFrom(src => src.PaymentMethods))
                .ForMember(dest => dest.SupportPaymentMethod, opt => opt.MapFrom(src => src.SupportPaymentMethod))
                .ForMember(dest => dest.PendingInterval, opt => opt.MapFrom(src => src.PendingInterval))
                .ForMember(dest => dest.PendingRetries, opt => opt.MapFrom(src => src.PendingRetries))
                .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret))
                .ForMember(dest => dest.ExternalVerification, opt => opt.MapFrom(src => src.ExternalVerification))
                .ForMember(dest => dest.IsAsyncPolicy, opt => opt.MapFrom(src => src.IsAsyncPolicy))
                .ForMember(dest => dest.TransactUrl, opt => opt.MapFrom(src => src.TransactUrl))
                .ForMember(dest => dest.StatusUrl, opt => opt.MapFrom(src => src.StatusUrl))
                .ForMember(dest => dest.RenewUrl, opt => opt.MapFrom(src => src.RenewUrl))
                .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
                .ForMember(dest => dest.SkipSettings, opt => opt.MapFrom(src => src.SkipSettings))
                .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Settings))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Selected, opt => opt.MapFrom(src => src.Selected))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
            Mapper = new MapperConfiguration(cfg).CreateMapper();
        }

        private static Timestamp ToUtcUnixTimestampSeconds(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return Timestamp.FromDateTime(DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc));
            }

            return null;
        }
        
        private static DateTime? FromUtcUnixTimestampSeconds(Timestamp? timestamp)
        {
            if (timestamp != null)
            {
                return timestamp.ToDateTime();
            }

            return null;
        }
    }
}