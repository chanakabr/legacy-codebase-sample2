using ApiObjects;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using ApiObjects.Pricing.Dto;
using AutoMapper.Configuration;
using Core.ConditionalAccess;
using Core.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Models.Users;
using WebAPI.ModelsFactory;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class PricingMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            // CouponsGroup
            cfg.CreateMap<CouponsGroup, KalturaCouponsGroup>()
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dEndDate)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sGroupCode))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sGroupName))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dStartDate)))
               .ForMember(dest => dest.MaxUsesNumber, opt => opt.MapFrom(src => src.m_nMaxUseCountForCoupon))
               .ForMember(dest => dest.MaxUsesNumberOnRenewableSub, opt => opt.MapFrom(src => src.m_nMaxRecurringUsesCountForCoupon))
               .ForMember(dest => dest.CouponGroupType, opt => opt.ResolveUsing(src => ConvertCouponGroupType(src.couponGroupType)))
               .ForMember(dest => dest.MaxHouseholdUses, opt => opt.MapFrom(src => src.maxDomainUses))
               .ForMember(dest => dest.DiscountCode, opt => opt.MapFrom(src => StringUtils.TryConvertTo<long>(src.m_sDiscountCode)))
               .ForMember(dest => dest.DiscountId, opt => opt.MapFrom(src => StringUtils.TryConvertTo<long>(src.m_sDiscountCode)));

            cfg.CreateMap<KalturaCouponsGroup, CouponsGroup>()
                .ForMember(dest => dest.m_sDescription, opt => opt.MapFrom(src => src.Descriptions))
                .ForMember(dest => dest.m_dEndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.EndDate.Value).DateTime : (DateTime?)null))
                .ForMember(dest => dest.m_sGroupCode, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.m_sGroupName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.m_dStartDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.StartDate.Value).DateTime : (DateTime?)null))
                .ForMember(dest => dest.m_nMaxUseCountForCoupon, opt => opt.MapFrom(src => src.MaxUsesNumber))
                .ForMember(dest => dest.m_nMaxRecurringUsesCountForCoupon, opt => opt.MapFrom(src => src.MaxUsesNumberOnRenewableSub))
                .ForMember(dest => dest.couponGroupType, opt => opt.ResolveUsing(src => ConvertCouponGroupType(src.CouponGroupType)))
                .ForMember(dest => dest.maxDomainUses, opt => opt.MapFrom(src => src.MaxHouseholdUses))
                .ForMember(dest => dest.m_sDiscountCode, opt => opt.MapFrom(src => src.DiscountCode.ToString()))
                .ForMember(dest => dest.m_sDiscountCode, opt => opt.MapFrom(src => src.DiscountId.ToString()));

            cfg.CreateMap<SubscriptionCouponGroup, KalturaCouponsGroup>()
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => (!src.endDate.HasValue || src.m_dEndDate < src.endDate.Value) ?
                   DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dEndDate) : DateUtils.DateTimeToUtcUnixTimestampSeconds(src.endDate.Value)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sGroupCode))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sGroupName))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => (!src.endDate.HasValue || src.m_dEndDate < src.endDate.Value) ?
                   DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dStartDate) : DateUtils.DateTimeToUtcUnixTimestampSeconds(src.startDate.HasValue ? src.startDate.Value : src.m_dStartDate)))
               .ForMember(dest => dest.MaxUsesNumber, opt => opt.MapFrom(src => src.m_nMaxUseCountForCoupon))
               .ForMember(dest => dest.MaxUsesNumberOnRenewableSub, opt => opt.MapFrom(src => src.m_nMaxRecurringUsesCountForCoupon))
               .ForMember(dest => dest.CouponGroupType, opt => opt.ResolveUsing(src => ConvertCouponGroupType(src.couponGroupType)))
               .ForMember(dest => dest.MaxHouseholdUses, opt => opt.MapFrom(src => src.maxDomainUses))
               .ForMember(dest => dest.DiscountCode, opt => opt.MapFrom(src => StringUtils.TryConvertTo<long>(src.m_sDiscountCode)))
               .ForMember(dest => dest.DiscountId, opt => opt.MapFrom(src => StringUtils.TryConvertTo<long>(src.m_sDiscountCode)));
            ;

            // SubscriptionCouponGroup
            cfg.CreateMap<KalturaCouponsGroup, SubscriptionCouponGroup>()
               .ForMember(dest => dest.m_sDescription, opt => opt.MapFrom(src => src.Descriptions))
               .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.EndDate.Value).DateTime : (DateTime?)null))
               .ForMember(dest => dest.m_sGroupCode, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.m_sGroupName, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.startDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.StartDate.Value).DateTime : (DateTime?)null))
               .ForMember(dest => dest.m_nMaxUseCountForCoupon, opt => opt.MapFrom(src => src.MaxUsesNumber))
               .ForMember(dest => dest.m_nMaxRecurringUsesCountForCoupon, opt => opt.MapFrom(src => src.MaxUsesNumberOnRenewableSub))
               .ForMember(dest => dest.couponGroupType, opt => opt.ResolveUsing(src => ConvertCouponGroupType(src.CouponGroupType)))
               .ForMember(dest => dest.maxDomainUses, opt => opt.MapFrom(src => src.MaxHouseholdUses))
               .ForMember(dest => dest.m_sDiscountCode, opt => opt.MapFrom(src => src.DiscountCode.ToString()))
               .ForMember(dest => dest.m_sDiscountCode, opt => opt.MapFrom(src => src.DiscountId.ToString()));
            ;

            // Price
            cfg.CreateMap<Price, KalturaPrice>()
               .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.m_dPrice))
               .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3))
               .ForMember(dest => dest.CurrencySign, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencySign))
               .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.countryId != 0 ? (long?)src.countryId : null))
               ;

            // Price
            cfg.CreateMap<KalturaPrice, Price>()
               .ForMember(dest => dest.m_dPrice, opt => opt.MapFrom(src => src.Amount))
               .ForMember(dest => dest.m_oCurrency, opt => opt.MapFrom(src => ConvertPriceCurrency(src)))
               .ForMember(dest => dest.countryId, opt => opt.MapFrom(src => src.CountryId))
               ;

            // PriceCode
            cfg.CreateMap<PriceCode, KalturaPriceDetails>()
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjectID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sCode))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrise));

            // PriceCode
            cfg.CreateMap<KalturaPriceDetails, PriceCode>()
                .ForMember(dest => dest.m_sDescription, opt => opt.MapFrom(src => src.Descriptions != null && src.Descriptions.Count > 0 ? src.Descriptions[0] : null)) // TODO: ???
               .ForMember(dest => dest.m_nObjectID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.m_sCode, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.m_oPrise, opt => opt.MapFrom(src => src.Price));

            // DiscountModule
            cfg.CreateMap<DiscountModule, KalturaDiscountModule>()
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dEndDate)))
               .ForMember(dest => dest.Percent, opt => opt.MapFrom(src => src.m_dPercent))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dStartDate)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjectID));

            // DiscountModule
            cfg.CreateMap<KalturaDiscountModule, DiscountModule>()
               .ForMember(dest => dest.m_dEndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.EndDate.Value).DateTime : (DateTime?)null))
               .ForMember(dest => dest.m_dPercent, opt => opt.MapFrom(src => src.Percent))
               .ForMember(dest => dest.m_nObjectID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.m_dStartDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.StartDate.Value).DateTime : (DateTime?)null));

            // UsageModule
            cfg.CreateMap<UsageModule, KalturaUsageModule>()
               .ForMember(dest => dest.CouponId, opt => opt.MapFrom(src => src.m_coupon_id))
               .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.m_tsMaxUsageModuleLifeCycle))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjectID))
               .ForMember(dest => dest.IsOfflinePlayback, opt => opt.MapFrom(src => src.m_bIsOfflinePlayBack))
               .ForMember(dest => dest.IsWaiverEnabled, opt => opt.MapFrom(src => src.m_bWaiver))
               .ForMember(dest => dest.MaxViewsNumber, opt => opt.MapFrom(src => src.m_nMaxNumberOfViews))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sVirtualName))
               .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.m_tsViewLifeCycle))
               .ForMember(dest => dest.WaiverPeriod, opt => opt.MapFrom(src => src.m_nWaiverPeriod));

            // KalturaUsageModule
            cfg.CreateMap<KalturaUsageModule, UsageModule>()
               .ForMember(dest => dest.m_coupon_id, opt => opt.MapFrom(src => src.CouponId))
               .ForMember(dest => dest.m_tsMaxUsageModuleLifeCycle, opt => opt.MapFrom(src => src.FullLifeCycle))
               .ForMember(dest => dest.m_nObjectID, opt => opt.MapFrom(src => src.Id.HasValue ? src.Id.Value : 0))
               .ForMember(dest => dest.m_bIsOfflinePlayBack, opt => opt.MapFrom(src => src.IsOfflinePlayback))
               .ForMember(dest => dest.m_bWaiver, opt => opt.MapFrom(src => src.IsWaiverEnabled))
               .ForMember(dest => dest.m_nMaxNumberOfViews, opt => opt.MapFrom(src => src.MaxViewsNumber))
               .ForMember(dest => dest.m_sVirtualName, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.m_tsViewLifeCycle, opt => opt.MapFrom(src => src.ViewLifeCycle))
               .ForMember(dest => dest.m_nWaiverPeriod, opt => opt.MapFrom(src => src.WaiverPeriod));

            // KalturaUsageModule
            cfg.CreateMap<KalturaUsageModule, UsageModuleForUpdate>()
                .ForMember(dest => dest.TsMaxUsageModuleLifeCycle, opt => opt.MapFrom(src => src.FullLifeCycle))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.HasValue ? src.Id.Value : 0))
                .ForMember(dest => dest.IsOfflinePlayBack, opt => opt.MapFrom(src => src.IsOfflinePlayback))
                .ForMember(dest => dest.Waiver, opt => opt.MapFrom(src => src.IsWaiverEnabled))
                .ForMember(dest => dest.MaxNumberOfViews, opt => opt.MapFrom(src => src.MaxViewsNumber))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TsViewLifeCycle, opt => opt.MapFrom(src => src.ViewLifeCycle))
                .ForMember(dest => dest.WaiverPeriod, opt => opt.MapFrom(src => src.WaiverPeriod));

            // UsageModule
            cfg.CreateMap<UsageModuleForUpdate, KalturaUsageModule>()
                .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.TsMaxUsageModuleLifeCycle))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsOfflinePlayback, opt => opt.MapFrom(src => src.IsOfflinePlayBack))
                .ForMember(dest => dest.IsWaiverEnabled, opt => opt.MapFrom(src => src.Waiver))
                .ForMember(dest => dest.MaxViewsNumber, opt => opt.MapFrom(src => src.MaxNumberOfViews))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.TsViewLifeCycle))
                .ForMember(dest => dest.WaiverPeriod, opt => opt.MapFrom(src => src.WaiverPeriod));

            // UserType
            cfg.CreateMap<UserType, KalturaOTTUserType>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // PreviewModule
            cfg.CreateMap<PreviewModule, KalturaPreviewModule>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nID))
               .ForMember(dest => dest.LifeCycle, opt => opt.MapFrom(src => src.m_tsFullLifeCycle))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
               .ForMember(dest => dest.NonRenewablePeriod, opt => opt.MapFrom(src => src.m_tsNonRenewPeriod));

            // KalturaPreviewModule
            cfg.CreateMap<KalturaPreviewModule, PreviewModule>()
               .ForMember(dest => dest.m_nID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.m_tsFullLifeCycle, opt => opt.MapFrom(src => src.LifeCycle))
               .ForMember(dest => dest.m_sName, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.m_tsNonRenewPeriod, opt => opt.MapFrom(src => src.NonRenewablePeriod));

            // ServiceObject to PremiumService
            cfg.CreateMap<ServiceObject, KalturaPremiumService>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            // KalturaPremiumService to ServiceObject
            cfg.CreateMap<KalturaPremiumService, ServiceObject>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<NpvrServiceObject, KalturaNpvrPremiumService>()
               .ForMember(dest => dest.QuotaInMinutes, opt => opt.MapFrom(src => src.Quota))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<KalturaNpvrPremiumService, NpvrServiceObject>()
                .IncludeBase<KalturaPremiumService, ServiceObject>()
                .ForMember(dest => dest.Quota, opt => opt.MapFrom(src => src.QuotaInMinutes));

            // LanguageContainer to TranslationContainer
            cfg.CreateMap<LanguageContainer, KalturaTranslationToken>()
               .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.m_sLanguageCode3))
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.m_sValue));

            cfg.CreateMap<KalturaTranslationToken, LanguageContainer>()
                .ForMember(dest => dest.m_sLanguageCode3, opt => opt.MapFrom(src => src.Language))
                .ForMember(dest => dest.m_sValue, opt => opt.MapFrom(src => src.Value));

            // BundleCodeContainer to SlimChannel
            cfg.CreateMap<BundleCodeContainer, KalturaBaseChannel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sCode));

            // KalturaBaseChannel
            cfg.CreateMap<KalturaBaseChannel, BundleCodeContainer>()
               .ForMember(dest => dest.m_sCode, opt => opt.MapFrom(src => src.Id));

            // BundleCodeContainer to SlimChannel
            cfg.CreateMap<PromotionInfo, KalturaPromotionInfo>()
               .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(src => src.CampaignId));

            // BundleCodeContainer to SlimChannel
            cfg.CreateMap<SubscriptionsPricesContainer, KalturaSubscriptionPrice>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sSubscriptionCode))
               .ForMember(dest => dest.PurchaseStatus, opt => opt.ResolveUsing(src => ConvertPriceReasonToPurchaseStatus(src.m_PriceReason)))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrice))
               .ForMember(dest => dest.FullPrice, opt => opt.MapFrom(src => src.OriginalPrice))
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sSubscriptionCode))
               .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => KalturaTransactionType.subscription))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.endDate.HasValue ? src.endDate.Value : 0))
               .ForMember(dest => dest.PromotionInfo, opt => opt.MapFrom(src => src.PromotionInfo))
               ;


            // Subscription
            cfg.CreateMap<Subscription, KalturaSubscription>()
               .ForMember(dest => dest.IsInfiniteRenewal, opt => opt.MapFrom(src => src.m_bIsInfiniteRecurring))
               .ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.m_bIsRecurring))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dStartDate)))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dEndDate)))
               .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.m_fictivicMediaID))
               //.ForMember(dest => dest.PremiumServices, opt => opt.MapFrom(src => src.m_lServices))
               .ForMember(dest => dest.PremiumServices, opt => opt.ResolveUsing(src => ConvertServices(src.m_lServices)))
               .ForMember(dest => dest.PricePlans, opt => opt.MapFrom(src => src.m_MultiSubscriptionUsageModule))
               .ForMember(dest => dest.HouseholdLimitationsId, opt => opt.MapFrom(src => src.m_nDomainLimitationModule))
               .ForMember(dest => dest.RenewalsNumber, opt => opt.MapFrom(src => src.m_nNumberOfRecPeriods))
               .ForMember(dest => dest.CouponsGroup, opt => opt.MapFrom(src => src.m_oCouponsGroup))
               .ForMember(dest => dest.DiscountModule, opt => opt.MapFrom(src => src.m_oDiscountModule))
               .ForMember(dest => dest.PreviewModule, opt => opt.MapFrom(src => src.m_oPreviewModule))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oSubscriptionPriceCode))
               .ForMember(dest => dest.MaxViewsNumber, opt => opt.MapFrom(src => src.m_oSubscriptionUsageModule.m_nMaxNumberOfViews))
               .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.m_oSubscriptionUsageModule.m_tsViewLifeCycle))
               .ForMember(dest => dest.WaiverPeriod, opt => opt.MapFrom(src => src.m_oSubscriptionUsageModule.m_nWaiverPeriod))
               .ForMember(dest => dest.IsWaiverEnabled, opt => opt.MapFrom(src => src.m_oSubscriptionUsageModule.m_bWaiver))
               .ForMember(dest => dest.ProrityInOrder, opt => opt.MapFrom(src => src.m_Priority))
               .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_ProductCode))
               .ForMember(dest => dest.Channels, opt => opt.MapFrom(src => src.m_sCodes))
               .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.m_sDescription)))
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.FileTypes, opt => opt.MapFrom(src => src.m_sFileTypes))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.m_sName)))
               .ForMember(dest => dest.Names, opt => opt.MapFrom(src => src.m_sName))
               .ForMember(dest => dest.GracePeriodMinutes, opt => opt.MapFrom(src => src.m_GracePeriodMinutes))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_SubscriptionCode))
               .ForMember(dest => dest.UserTypes, opt => opt.MapFrom(src => src.m_UserTypes))
               .ForMember(dest => dest.ProductCodes, opt => opt.ResolveUsing(src => ConvertProductCodes(src.ExternalProductCodes)))
               .ForMember(dest => dest.CouponGroups, opt => opt.ResolveUsing(src => ConvertCouponsGroup(src.GetValidSubscriptionCouponGroup())))
               .ForMember(dest => dest.DependencyType, opt => opt.ResolveUsing(src => ConvertSubscriptionType(src.Type)))
               .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_ProductCode))
               .ForMember(dest => dest.IsCancellationBlocked, opt => opt.MapFrom(src => src.BlockCancellation))
               .ForMember(dest => dest.PricePlanIds, opt => opt.MapFrom(src =>
                   src.m_MultiSubscriptionUsageModule != null && src.m_MultiSubscriptionUsageModule.Length > 0 ?
                   string.Join(",", src.m_MultiSubscriptionUsageModule.Select(um => um.m_nObjectID).ToArray()) :
                   string.Empty))
               .ForMember(dest => dest.PreSaleDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.PreSaleDate)))
               .ForMember(dest => dest.AdsParams, opt => opt.MapFrom(src => src.AdsParam))
               .ForMember(dest => dest.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
               .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.IsActive))
               .ForMember(dest => dest.ChannelsIds, opt => opt.MapFrom(src =>
                   src.m_sCodes != null && src.m_sCodes.Length > 0 ?
                   string.Join(",", src.m_sCodes.Select(um => um.m_sCode).ToArray()) :
                   string.Empty))
               .ForMember(dest => dest.FileTypesIds, opt => opt.MapFrom(src =>
                   src.m_sFileTypes != null && src.m_sFileTypes.Length > 0 ?
                   string.Join(",", src.m_sFileTypes) :
                   string.Empty))
               .ForMember(dest => dest.SubscriptionCouponGroup, opt => opt.MapFrom(src => ConvertSubCouponsGroup(src.CouponsGroups)))
               .ForMember(dest => dest.InternalDiscountModuleId, opt => opt.MapFrom(src =>
                   src.m_oDiscountModule != null ? (long?)src.m_oDiscountModule.m_nObjectID : null))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
               ;

            // KalturaSubscription to SubscriptionInternal
            cfg.CreateMap<KalturaSubscription, SubscriptionInternal>()
                .ForMember(dest => dest.AdsParams, opt => opt.MapFrom(src => src.AdsParams))
                .ForMember(dest => dest.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
                .ForMember(dest => dest.ChannelsIds, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.ChannelsIds) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.ChannelsIds, "KalturaSubscription.ChannelsIds", true) : null))
                .ForMember(dest => dest.CouponGroups, opt => opt.MapFrom(src => ConvertSubCouponGroup(src.SubscriptionCouponGroup)))
                .ForMember(dest => dest.DependencyType, opt => opt.MapFrom(src => ConvertSubscriptionType(src.DependencyType)))
                .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => ConvertLanguageContainer(src.Description)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.EndDate.Value).DateTime : (DateTime?)null))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.ExternalProductCodes, opt => opt.ResolveUsing(src => ConvertProductCodes(src.ProductCodes)))
                .ForMember(dest => dest.FileTypesIds, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.FileTypesIds) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.FileTypesIds, "KalturaSubscription.FileTypesIds", true) : null))
                .ForMember(dest => dest.GracePeriodMinutes, opt => opt.MapFrom(src => src.GracePeriodMinutes))
                .ForMember(dest => dest.HouseholdLimitationsId, opt => opt.MapFrom(src => src.HouseholdLimitationsId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.InternalDiscountModuleId, opt => opt.MapFrom(src => src.InternalDiscountModuleId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.IsCancellationBlocked, opt => opt.MapFrom(src => src.IsCancellationBlocked))
                .ForMember(dest => dest.Names, opt => opt.MapFrom(src => ConvertLanguageContainer(src.Name)))
                .ForMember(dest => dest.PremiumServices, opt => opt.ResolveUsing(src => ConvertServices(src.PremiumServices)))
                .ForMember(dest => dest.PreSaleDate, opt => opt.MapFrom(src => src.PreSaleDate))
                .ForMember(dest => dest.PreviewModuleId, opt => opt.MapFrom(src => src.PreviewModuleId))
                .ForMember(dest => dest.PricePlanIds, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.PricePlanIds) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.PricePlanIds, "KalturaSubscription.PricePlanIds", true) : null))
                .ForMember(dest => dest.ProrityInOrder, opt => opt.MapFrom(src => src.ProrityInOrder))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.StartDate.Value).DateTime : (DateTime?)null))
                .ForMember(dest => dest.NullableProperties, opt => opt.MapFrom(src => src.NullableProperties))
                .AfterMap((src, dest) => dest.ChannelsIds = src.ChannelsIds != null ? dest.ChannelsIds : null)
                .AfterMap((src, dest) => dest.FileTypesIds = src.FileTypesIds != null ? dest.FileTypesIds : null)
                .AfterMap((src, dest) => dest.PricePlanIds = src.PricePlanIds != null ? dest.PricePlanIds : null)
                .AfterMap((src, dest) => dest.CouponGroups = src.SubscriptionCouponGroup != null ? dest.CouponGroups : null)
                .AfterMap((src, dest) => dest.Names = src.Name != null ? dest.Names : null)
                .AfterMap((src, dest) => dest.Descriptions = src.Description != null ? dest.Descriptions : null)
                .AfterMap((src, dest) => dest.PremiumServices = src.PremiumServices != null ? dest.PremiumServices : null)
                .AfterMap((src, dest) => dest.ExternalProductCodes = src.ProductCodes != null ? dest.ExternalProductCodes : null)
                ;

            cfg.CreateMap<SubscriptionInternal, KalturaSubscription>()
               .ForMember(dest => dest.AdsParams, opt => opt.MapFrom(src => src.AdsParams))
               .ForMember(dest => dest.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
               .ForMember(dest => dest.ChannelsIds, opt => opt.MapFrom(src => src.ChannelsIds != null ? string.Join(",", src.ChannelsIds) : string.Empty))
               .ForMember(dest => dest.SubscriptionCouponGroup, opt => opt.Ignore())
               .ForMember(dest => dest.DependencyType, opt => opt.MapFrom(src => ConvertSubscriptionType(src.DependencyType)))
               .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Descriptions)))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.EndDate)))
               .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
               .ForMember(dest => dest.ProductCodes, opt => opt.MapFrom(src => src.ExternalProductCodes != null ? string.Join(",", src.ExternalProductCodes) : string.Empty))
               .ForMember(dest => dest.FileTypesIds, opt => opt.MapFrom(src => src.FileTypesIds != null ? string.Join(",", src.FileTypesIds) : string.Empty))
               .ForMember(dest => dest.GracePeriodMinutes, opt => opt.MapFrom(src => src.GracePeriodMinutes))
               .ForMember(dest => dest.HouseholdLimitationsId, opt => opt.MapFrom(src => src.HouseholdLimitationsId))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.InternalDiscountModuleId, opt => opt.MapFrom(src => src.InternalDiscountModuleId))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.IsCancellationBlocked, opt => opt.MapFrom(src => src.IsCancellationBlocked))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Names)))
               .ForMember(dest => dest.PremiumServices, opt => opt.ResolveUsing(src => ConvertServices(src.PremiumServices)))
               .ForMember(dest => dest.PreSaleDate, opt => opt.MapFrom(src => src.PreSaleDate))
               .ForMember(dest => dest.PreviewModuleId, opt => opt.MapFrom(src => src.PreviewModuleId))
               .ForMember(dest => dest.PricePlanIds, opt => opt.MapFrom(src => src.PricePlanIds != null ? string.Join(",", src.PricePlanIds) : string.Empty))
               .ForMember(dest => dest.ProrityInOrder, opt => opt.MapFrom(src => src.ProrityInOrder))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.StartDate)));

            cfg.CreateMap<AdsPolicy, KalturaAdsPolicy>()
              .ConvertUsing(adsPolicy =>
              {
                  switch (adsPolicy)
                  {
                      case AdsPolicy.NoAds:
                          return KalturaAdsPolicy.NO_ADS;
                      case AdsPolicy.KeepAds:
                          return KalturaAdsPolicy.KEEP_ADS;
                      default:
                          throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown adsPolicy  value : {0}", adsPolicy.ToString()));
                  }
              });

            cfg.CreateMap<KalturaAdsPolicy, AdsPolicy>()
              .ConvertUsing(adsPolicy =>
              {
                  switch (adsPolicy)
                  {
                      case KalturaAdsPolicy.NO_ADS:
                          return AdsPolicy.NoAds;
                      case KalturaAdsPolicy.KEEP_ADS:
                          return AdsPolicy.KeepAds;
                      default:
                          throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown adsPolicy  value : {0}", adsPolicy.ToString()));
                  }
              });

            cfg.CreateMap<KalturaSubscriptionFilter, SubscriptionFilter>()
             .ForMember(dest => dest.OrderBy, opt => opt.MapFrom(src => ConvertSubscriptionOrderBy(src.OrderBy)));

            cfg.CreateMap<KalturaSubscriptionDependencyType, SubscriptionType>()
               .ConvertUsing(type =>
               {
                   switch (type)
                   {
                       case KalturaSubscriptionDependencyType.NOTAPPLICABLE:
                           return SubscriptionType.NotApplicable;
                       case KalturaSubscriptionDependencyType.BASE:
                           return SubscriptionType.Base;
                       case KalturaSubscriptionDependencyType.ADDON:
                           return SubscriptionType.AddOn;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown KalturaSubscriptionDependencyType value : {type.ToString()}");
                   }
               });

            // KalturaPricePlan
            cfg.CreateMap<UsageModule, KalturaPricePlan>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjectID))
               .ForMember(dest => dest.CouponId, opt => opt.MapFrom(src => src.m_coupon_id))
               .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.m_tsMaxUsageModuleLifeCycle))
               .ForMember(dest => dest.IsOfflinePlayback, opt => opt.MapFrom(src => src.m_bIsOfflinePlayBack))
               .ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.m_is_renew))
               .ForMember(dest => dest.IsWaiverEnabled, opt => opt.MapFrom(src => src.m_bWaiver))
               .ForMember(dest => dest.MaxViewsNumber, opt => opt.MapFrom(src => src.m_nMaxNumberOfViews))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sVirtualName))
               .ForMember(dest => dest.PriceId, opt => opt.MapFrom(src => src.m_pricing_id))
               .ForMember(dest => dest.RenewalsNumber, opt => opt.MapFrom(src => src.m_num_of_rec_periods))
               .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.m_tsViewLifeCycle))
               .ForMember(dest => dest.WaiverPeriod, opt => opt.MapFrom(src => src.m_nWaiverPeriod))
               .ForMember(dest => dest.DiscountId, opt => opt.MapFrom(src => src.m_ext_discount_id))
               .ForMember(dest => dest.PriceDetailsId, opt => opt.MapFrom(src => src.m_pricing_id))
               ;

            cfg.CreateMap<PricePlan, KalturaPricePlan>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.FullLifeCycle))
               .ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.IsRenewable))
               .ForMember(dest => dest.MaxViewsNumber, opt => opt.MapFrom(src => src.MaxViewsNumber))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.ViewLifeCycle))
               .ForMember(dest => dest.DiscountId, opt => opt.MapFrom(src => src.DiscountId))
               .ForMember(dest => dest.PriceDetailsId, opt => opt.MapFrom(src => src.PriceDetailsId))
               .ForMember(dest => dest.RenewalsNumber, opt => opt.MapFrom(src => src.RenewalsNumber))
               .ForMember(dest => dest.WaiverPeriod, opt => opt.MapFrom(src => src.WaiverPeriod))
               .ForMember(dest => dest.IsWaiverEnabled, opt => opt.MapFrom(src => src.IsWaiverEnabled))
               .ForMember(dest => dest.IsOfflinePlayback, opt => opt.MapFrom(src => src.IsOfflinePlayBack))
                ;

            cfg.CreateMap<KalturaPricePlan, PricePlan>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.FullLifeCycle))
               .ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.IsRenewable))
               .ForMember(dest => dest.RenewalsNumber, opt => opt.MapFrom(src => src.RenewalsNumber))
               .ForMember(dest => dest.MaxViewsNumber, opt => opt.MapFrom(src => src.MaxViewsNumber))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.ViewLifeCycle))
               .ForMember(dest => dest.DiscountId, opt => opt.MapFrom(src => src.DiscountId))
               .ForMember(dest => dest.PriceDetailsId, opt => opt.MapFrom(src => src.PriceDetailsId))
               .ForMember(dest => dest.WaiverPeriod, opt => opt.MapFrom(src => src.WaiverPeriod))
               .ForMember(dest => dest.IsWaiverEnabled, opt => opt.MapFrom(src => src.IsWaiverEnabled))
               .ForMember(dest => dest.IsOfflinePlayBack, opt => opt.MapFrom(src => src.IsOfflinePlayback))
               ;

            // ItemPriceContainer to PPVItemPriceDetails
            cfg.CreateMap<ItemPriceContainer, KalturaPPVItemPriceDetails>()
               .ForMember(dest => dest.CollectionId, opt => opt.MapFrom(src => src.m_relevantCol))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.m_dtEndDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtEndDate.Value) : 0))
               .ForMember(dest => dest.DiscountEndDate, opt => opt.MapFrom(src => src.m_dtDiscountEndDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtDiscountEndDate.Value) : 0))
               .ForMember(dest => dest.FirstDeviceName, opt => opt.MapFrom(src => src.m_sFirstDeviceNameFound))
               .ForMember(dest => dest.FullPrice, opt => opt.MapFrom(src => src.m_oFullPrice))
               .ForMember(dest => dest.IsInCancelationPeriod, opt => opt.MapFrom(src => src.m_bCancelWindow))
               .ForMember(dest => dest.IsSubscriptionOnly, opt => opt.MapFrom(src => src.m_bSubscriptionOnly))
               .ForMember(dest => dest.PPVDescriptions, opt => opt.MapFrom(src => src.m_oPPVDescription))
               .ForMember(dest => dest.PPVModuleId, opt => opt.MapFrom(src => src.m_sPPVModuleCode))
               .ForMember(dest => dest.PrePaidId, opt => opt.MapFrom(src => src.m_relevantPP))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrice))
               .ForMember(dest => dest.PurchasedMediaFileId, opt => opt.MapFrom(src => src.m_lPurchasedMediaFileID))
               .ForMember(dest => dest.PurchaseStatus, opt => opt.ResolveUsing(src => ConvertPriceReasonToPurchaseStatus(src.m_PriceReason)))
               .ForMember(dest => dest.PurchaseUserId, opt => opt.MapFrom(src => src.m_sPurchasedBySiteGuid))
               .ForMember(dest => dest.RelatedMediaFileIds, opt => opt.MapFrom(src => src.m_lRelatedMediaFileIDs))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.m_dtStartDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtStartDate.Value) : 0))
               .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.m_relevantSub.m_sObjectCode))
               .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_sProductCode));

            // ItemPriceContainer to PPVItemPriceDetails
            cfg.CreateMap<MediaFileItemPricesContainer, KalturaItemPrice>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sProductCode))
               .ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.m_nMediaFileID))
               .ForMember(dest => dest.PPVPriceDetails, opt => opt.MapFrom(src => src.m_oItemPrices));

            // CouponData to CouponDetails
            cfg.CreateMap<CouponData, KalturaCoupon>()
               .ForMember(dest => dest.CouponsGroup, opt => opt.MapFrom(src => src.m_oCouponGroup))
               .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertCouponStatus(src.m_CouponStatus)))
               .ForMember(dest => dest.LeftUses, opt => opt.MapFrom(src => src.leftUses))
               .ForMember(dest => dest.TotalUses, opt => opt.MapFrom(src => src.totalUses))
               .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.id))
               ;

            // PpvModule to KalturaPpvModule
            cfg.CreateMap<PPVModule, KalturaPpv>()
               .ForMember(dest => dest.CouponsGroup, opt => opt.MapFrom(src => src.m_oCouponsGroup))
               .ForMember(dest => dest.CouponsGroupId, opt => opt.MapFrom(src => GetCouponsGroupId(src.m_oCouponsGroup)))
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.DiscountModule, opt => opt.MapFrom(src => src.m_oDiscountModule))
               .ForMember(dest => dest.DiscountId, opt => opt.MapFrom(src => GetDiscountModuleId(src.m_oDiscountModule)))
               .ForMember(dest => dest.FileTypes, opt => opt.MapFrom(src => src.m_relatedFileTypes))
               .ForMember(dest => dest.FileTypesIds, opt => opt.MapFrom(src => src.m_relatedFileTypes != null ? string.Join(",", src.m_relatedFileTypes) : string.Empty))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sObjectCode))
               .ForMember(dest => dest.IsSubscriptionOnly, opt => opt.MapFrom(src => src.m_bSubscriptionOnly))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sObjectVirtualName))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPriceCode))
               .ForMember(dest => dest.PriceDetailsId, opt => opt.MapFrom(src => src.m_oPriceCode.m_nObjectID))
               .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_Product_Code))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
               .ForMember(dest => dest.UsageModule, opt => opt.MapFrom(src => src.m_oUsageModule))
               .ForMember(dest => dest.UsageModuleId, opt => opt.MapFrom(src => src.m_oUsageModule.m_nObjectID))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
               .ForMember(dest => dest.VirtualAssetId, opt => opt.MapFrom(src => src.VirtualAssetId))
               .ForMember(dest => dest.FirstDeviceLimitation, opt => opt.MapFrom(src => src.m_bFirstDeviceLimitation))
               .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId));

            cfg.CreateMap<KalturaPpv, PPVModule>()
                .ForMember(dest => dest.m_sDescription, opt => opt.MapFrom(src => src.Descriptions))
                .ForMember(dest => dest.m_oCouponsGroup, opt => opt.MapFrom(src => src.CouponsGroup))
                .ForMember(dest => dest.m_oDiscountModule, opt => opt.MapFrom(src => src.DiscountModule))
                .ForMember(dest => dest.m_relatedFileTypes, opt => opt.MapFrom(src => src.FileTypes))
                .ForMember(dest => dest.m_relatedFileTypes, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.FileTypesIds) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.FileTypesIds, "KalturaSubscription.FileTypesIds", true) : null))
                .ForMember(dest => dest.m_sObjectCode, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.m_bSubscriptionOnly, opt => opt.MapFrom(src => src.IsSubscriptionOnly))
                .ForMember(dest => dest.m_sObjectVirtualName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.m_oPriceCode, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.m_Product_Code, opt => opt.MapFrom(src => src.ProductCode))
                .ForMember(dest => dest.m_oUsageModule, opt => opt.MapFrom(src => src.UsageModule))
                .ForMember(dest => dest.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.m_bFirstDeviceLimitation, opt => opt.MapFrom(src => src.FirstDeviceLimitation))
                .ForMember(dest => dest.VirtualAssetId, opt => opt.MapFrom(src => src.VirtualAssetId))
                .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId));

            cfg.CreateMap<KalturaPpv, PpvModuleInternal>()
                .ForMember(dest => dest.CouponsGroupId, opt => opt.MapFrom(src => src.CouponsGroupId))
                .ForMember(dest => dest.DiscountId, opt => opt.MapFrom(src => src.DiscountId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SubscriptionOnly, opt => opt.MapFrom(src => src.IsSubscriptionOnly))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PriceId, opt => opt.MapFrom(src => src.PriceDetailsId))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.ProductCode))
                .ForMember(dest => dest.UsageModuleId, opt => opt.MapFrom(src => src.UsageModuleId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
                .ForMember(dest => dest.FirstDeviceLimitation, opt => opt.MapFrom(src => src.FirstDeviceLimitation))
                .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
                .ForMember(dest => dest.VirtualAssetId, opt => opt.MapFrom(src => src.VirtualAssetId))
                .AfterMap((src, dest) =>  dest.RelatedFileTypes = src.GetFileTypesIds())       
                .AfterMap((src, dest) =>  dest.Description = GetDescriptions(src.Descriptions));

            cfg.CreateMap<PpvModuleInternal,KalturaPpv>()
                .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CouponsGroupId, opt => opt.MapFrom(src => src.CouponsGroupId))
                .ForMember(dest => dest.DiscountId, opt => opt.MapFrom(src => src.DiscountId))
                .ForMember(dest => dest.FileTypesIds, opt => opt.ResolveUsing(src => src.RelatedFileTypes != null ? string.Join(",", src.RelatedFileTypes) : string.Empty))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsSubscriptionOnly, opt => opt.MapFrom(src => src.SubscriptionOnly))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PriceDetailsId, opt => opt.MapFrom(src => src.PriceId))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.ProductCode))
                .ForMember(dest => dest.UsageModuleId, opt => opt.MapFrom(src => src.UsageModuleId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
                .ForMember(dest => dest.VirtualAssetId, opt => opt.MapFrom(src => src.VirtualAssetId))
                .ForMember(dest => dest.FirstDeviceLimitation, opt => opt.MapFrom(src => src.FirstDeviceLimitation))
                .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId));

            //KalturaPpvPrice
            cfg.CreateMap<ItemPriceContainer, KalturaPpvPrice>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sProductCode))
               .ForMember(dest => dest.CollectionId, opt => opt.MapFrom(src => src.m_relevantCol))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.m_dtEndDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtEndDate.Value) : 0))
               .ForMember(dest => dest.DiscountEndDate, opt => opt.MapFrom(src => src.m_dtDiscountEndDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtDiscountEndDate.Value) : 0))
               .ForMember(dest => dest.FirstDeviceName, opt => opt.MapFrom(src => src.m_sFirstDeviceNameFound))
               .ForMember(dest => dest.FullPrice, opt => opt.MapFrom(src => src.m_oFullPrice))
               .ForMember(dest => dest.IsInCancelationPeriod, opt => opt.MapFrom(src => src.m_bCancelWindow))
               .ForMember(dest => dest.IsSubscriptionOnly, opt => opt.MapFrom(src => src.m_bSubscriptionOnly))
               .ForMember(dest => dest.PPVDescriptions, opt => opt.MapFrom(src => src.m_oPPVDescription))
               .ForMember(dest => dest.PPVModuleId, opt => opt.MapFrom(src => src.m_sPPVModuleCode))
               .ForMember(dest => dest.PrePaidId, opt => opt.MapFrom(src => src.m_relevantPP))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrice))
               .ForMember(dest => dest.PurchasedMediaFileId, opt => opt.MapFrom(src => src.m_lPurchasedMediaFileID))
               .ForMember(dest => dest.PurchaseStatus, opt => opt.ResolveUsing(src => ConvertPriceReasonToPurchaseStatus(src.m_PriceReason)))
               .ForMember(dest => dest.PurchaseUserId, opt => opt.MapFrom(src => src.m_sPurchasedBySiteGuid))
               .ForMember(dest => dest.RelatedMediaFileIds, opt => opt.MapFrom(src => src.m_lRelatedMediaFileIDs))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.m_dtStartDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtStartDate.Value) : 0))
               .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.m_relevantSub.m_sObjectCode))
               .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_sProductCode))
               .ForMember(dest => dest.PromotionInfo, opt => opt.MapFrom(src => src.PromotionInfo)); ;

            //SubscriptionSet to KalturaSubscriptionSet
            cfg.CreateMap<SubscriptionSet, KalturaSubscriptionSet>()
                .Include<SwitchSet, KalturaSubscriptionSwitchSet>()
                .Include<DependencySet, KalturaSubscriptionDependencySet>()
                ;

            // KalturaSubscriptionSet
            cfg.CreateMap<SwitchSet, KalturaSubscriptionSwitchSet>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.SubscriptionIds, opt => opt.MapFrom(src => src.SubscriptionIds != null ? string.Join(",", src.SubscriptionIds) : string.Empty))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertSetType(src.Type)));

            cfg.CreateMap<DependencySet, KalturaSubscriptionDependencySet>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.BaseSubscriptionId, opt => opt.MapFrom(src => src.BaseSubscriptionId))
                .ForMember(dest => dest.SubscriptionIds, opt => opt.MapFrom(src => src.AddOnIds != null ? string.Join(",", src.AddOnIds) : string.Empty))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertSetType(src.Type)));
            ;

            cfg.CreateMap<PriceDetails, KalturaPriceDetails>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.MultiCurrencyPrice, opt => opt.MapFrom(src => src.Prices))
                ;

            cfg.CreateMap<KalturaPriceDetails, PriceDetails>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Prices, opt => opt.MapFrom(src => src.MultiCurrencyPrice))
               ;

            // KalturaPricePlan
            cfg.CreateMap<KalturaPricePlan, UsageModule>()
               .ForMember(dest => dest.m_nObjectID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.m_coupon_id, opt => opt.MapFrom(src => src.CouponId))
               .ForMember(dest => dest.m_tsMaxUsageModuleLifeCycle, opt => opt.MapFrom(src => src.FullLifeCycle))
               .ForMember(dest => dest.m_bIsOfflinePlayBack, opt => opt.MapFrom(src => src.IsOfflinePlayback))
               .ForMember(dest => dest.m_is_renew, opt => opt.MapFrom(src => src.IsRenewable))
               .ForMember(dest => dest.m_bWaiver, opt => opt.MapFrom(src => src.IsWaiverEnabled))
               .ForMember(dest => dest.m_nMaxNumberOfViews, opt => opt.MapFrom(src => src.MaxViewsNumber))
               .ForMember(dest => dest.m_sVirtualName, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.m_num_of_rec_periods, opt => opt.MapFrom(src => src.RenewalsNumber))
               .ForMember(dest => dest.m_tsViewLifeCycle, opt => opt.MapFrom(src => src.ViewLifeCycle))
               .ForMember(dest => dest.m_nWaiverPeriod, opt => opt.MapFrom(src => src.WaiverPeriod))
               .ForMember(dest => dest.m_ext_discount_id, opt => opt.MapFrom(src => src.DiscountId))
               .ForMember(dest => dest.m_pricing_id, opt => opt.MapFrom(src => src.PriceDetailsId.HasValue ? src.PriceDetailsId : src.PriceId))
               ;

            // Collection
            cfg.CreateMap<Collection, KalturaCollection>()
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dStartDate)))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dEndDate)))
               .ForMember(dest => dest.DiscountModule, opt => opt.MapFrom(src => src.m_oDiscountModule))
               .ForMember(dest => dest.Channels, opt => opt.MapFrom(src => src.m_sCodes))
               .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.m_sDescription)))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.m_sName)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_CollectionCode))
               .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_ProductCode))
               .ForMember(dest => dest.UsageModule, opt => opt.MapFrom(src => src.m_oCollectionUsageModule))
               .ForMember(dest => dest.PriceDetailsId, opt => opt.MapFrom(src => src.m_oCollectionPriceCode != null ? src.m_oCollectionPriceCode.m_nObjectID : 0))
               .ForMember(dest => dest.ProductCodes, opt => opt.ResolveUsing(src => ConvertProductCodes(src.ExternalProductCodes)))
               .ForMember(dest => dest.CouponGroups, opt => opt.ResolveUsing(src => ConvertCouponsGroup(src.CouponsGroups)))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.ChannelsIds, opt => opt.MapFrom(src =>
                   src.m_sCodes != null && src.m_sCodes.Length > 0 ?
                   string.Join(",", src.m_sCodes.Select(um => um.m_sCode).ToArray()) :
                   string.Empty))
               .ForMember(dest => dest.CollectionCouponGroup, opt => opt.MapFrom(src => ConvertCollCouponsGroup(src.CouponsGroups)))
               .ForMember(dest => dest.DiscountModuleId, opt => opt.MapFrom(src =>
                   src.m_oDiscountModule != null ? (long?)src.m_oDiscountModule.m_nObjectID : null))
               .ForMember(dest => dest.UsageModuleId, opt => opt.MapFrom(src =>
                   src.m_oCollectionUsageModule != null ? (long?)src.m_oCollectionUsageModule.m_nObjectID : null))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
               .ForMember(dest => dest.VirtualAssetId, opt => opt.MapFrom(src => src.VirtualAssetId))
               .ForMember(dest => dest.FileTypes, opt => opt.MapFrom(src => src.m_sFileTypes))
               .ForMember(dest => dest.FileTypesIds, opt => opt.MapFrom(src =>
                   src.m_sFileTypes != null && src.m_sFileTypes.Length > 0 ?
                   string.Join(",", src.m_sFileTypes) :
                   string.Empty))
               .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
               ;

            cfg.CreateMap<KalturaCollection, CollectionInternal>()
                .ForMember(dest => dest.ChannelsIds, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.ChannelsIds) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.ChannelsIds, "KalturaCollection.ChannelsIds", true) : null))
                .ForMember(dest => dest.CouponGroups, opt => opt.MapFrom(src => ConvertCollCouponGroup(src.CollectionCouponGroup)))
                .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => ConvertLanguageContainer(src.Description)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.EndDate.Value).DateTime : (DateTime?)null))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.ExternalProductCodes, opt => opt.ResolveUsing(src => ConvertProductCodes(src.ProductCodes)))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DiscountModuleId, opt => opt.MapFrom(src => src.DiscountModuleId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Names, opt => opt.MapFrom(src => ConvertLanguageContainer(src.Name)))
                .ForMember(dest => dest.PriceDetailsId, opt => opt.MapFrom(src => src.PriceDetailsId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.StartDate.Value).DateTime : (DateTime?)null))
                .ForMember(dest => dest.UsageModuleId, opt => opt.MapFrom(src => src.UsageModuleId))
                .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
                .ForMember(dest => dest.NullableProperties, opt => opt.MapFrom(src => src.NullableProperties))
                .ForMember(dest => dest.FileTypesIds, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.FileTypesIds) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.FileTypesIds, "KalturaCollection.FileTypesIds", true) : null))
                .AfterMap((src, dest) => dest.ChannelsIds = src.ChannelsIds != null ? dest.ChannelsIds : null)
                .AfterMap((src, dest) => dest.CouponGroups = src.CollectionCouponGroup != null ? dest.CouponGroups : null)
                .AfterMap((src, dest) => dest.Names = src.Name != null ? dest.Names : null)
                .AfterMap((src, dest) => dest.Descriptions = src.Description != null ? dest.Descriptions : null)
                .AfterMap((src, dest) => dest.ExternalProductCodes = src.ProductCodes != null ? dest.ExternalProductCodes : null)
                .AfterMap((src, dest) => dest.IsActive = src.IsActive != null ? dest.IsActive : null)
                .AfterMap((src, dest) => dest.FileTypesIds = src.FileTypesIds != null ? dest.FileTypesIds : null)
                ;

            cfg.CreateMap<CollectionInternal, KalturaCollection>()
                .ForMember(dest => dest.ChannelsIds, opt => opt.MapFrom(src => src.ChannelsIds != null ? string.Join(",", src.ChannelsIds) : string.Empty))
                .ForMember(dest => dest.CollectionCouponGroup, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Descriptions)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.EndDate)))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DiscountModuleId, opt => opt.MapFrom(src => src.DiscountModuleId))
                .ForMember(dest => dest.UsageModuleId, opt => opt.MapFrom(src => src.UsageModuleId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Names)))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.StartDate)))
                .ForMember(dest => dest.FileTypesIds, opt => opt.MapFrom(src => src.FileTypesIds != null ? string.Join(",", src.FileTypesIds) : string.Empty))
                .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
                ;

            cfg.CreateMap<KalturaCollectionOrderBy, CollectionOrderBy>()
               .ConvertUsing(type =>
               {
                   switch (type)
                   {
                       case KalturaCollectionOrderBy.NONE:
                           return CollectionOrderBy.None;

                       case KalturaCollectionOrderBy.NAME_ASC:
                           return CollectionOrderBy.NameAsc;
                       
                       case KalturaCollectionOrderBy.NAME_DESC:
                           return CollectionOrderBy.NameDesc;
                       
                       case KalturaCollectionOrderBy.UPDATE_DATE_ASC:
                           return CollectionOrderBy.UpdateDataAsc;
                       
                       case KalturaCollectionOrderBy.UPDATE_DATE_DESC:
                           return CollectionOrderBy.UpdateDataDesc;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown CollectionOrderBy value : {type.ToString()}");
                   }
               });

            // DiscountDetails
            cfg.CreateMap<DiscountDetails, KalturaDiscountDetails>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.WhenAlgoTimes, opt => opt.MapFrom(src => src.WhenAlgoTimes))
               .ForMember(dest => dest.WhenAlgoType, opt => opt.MapFrom(src => (WhenAlgoType)src.WhenAlgoType))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.StartDate)))
               .ForMember(dest => dest.EndtDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.EndDate)))
               .ForMember(dest => dest.MultiCurrencyDiscount, opt => opt.MapFrom(src => src.MultiCurrencyDiscounts))
               ;

            // KalturaDiscountDetails
            cfg.CreateMap<KalturaDiscountDetails, DiscountDetails>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
               .ForMember(dest => dest.WhenAlgoTimes, opt => opt.MapFrom(src => src.WhenAlgoTimes))
               .ForMember(dest => dest.WhenAlgoType, opt => opt.MapFrom(src => (int)src.WhenAlgoType))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(src.StartDate)))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(src.EndtDate)))
               .ForMember(dest => dest.MultiCurrencyDiscounts, opt => opt.MapFrom(src => src.MultiCurrencyDiscount))
               ;

            // Discount
            cfg.CreateMap<Discount, KalturaDiscount>()
               .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.m_dPrice))
               .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3))
               .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.m_oCurrency.m_nCurrencyID))
               .ForMember(dest => dest.CurrencySign, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencySign))
               .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.countryId != 0 ? (long?)src.countryId : null))
               .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.Percentage))
               ;

            // KalturaDiscount
            cfg.CreateMap<KalturaDiscount, Discount>()
               .ForMember(dest => dest.m_dPrice, opt => opt.MapFrom(src => src.Amount))
               .ForMember(dest => dest.m_oCurrency, opt => opt.MapFrom(src => ConvertPriceCurrency(src)))
               .ForMember(dest => dest.countryId, opt => opt.MapFrom(src => src.CountryId != 0 ? (long?)src.CountryId : null))
               .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.Percentage ?? 0))
               ;

            #region AssetFilePpv
            cfg.CreateMap<AssetFilePpv, KalturaAssetFilePpv>()
               .ForMember(dest => dest.AssetFileId, opt => opt.MapFrom(src => src.AssetFileId))
               .ForMember(dest => dest.PpvModuleId, opt => opt.MapFrom(src => src.PpvModuleId))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.StartDate)))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.EndDate)))
               ;

            cfg.CreateMap<KalturaAssetFilePpv, AssetFilePpv>()
               .ForMember(dest => dest.AssetFileId, opt => opt.MapFrom(src => src.AssetFileId))
               .ForMember(dest => dest.PpvModuleId, opt => opt.MapFrom(src => src.PpvModuleId))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampSecondsToDateTime(src.StartDate)))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampSecondsToDateTime(src.EndDate)))
               .ForMember(dest => dest.NullableProperties, opt => opt.MapFrom(src => src.NullableProperties))
               ;

            #endregion

            // PartnerPremiumServices to PremiumService
            cfg.CreateMap<PartnerPremiumServices, KalturaPartnerPremiumServices>()
               .ForMember(dest => dest.PremiumServices, opt => opt.MapFrom(src => src.Services));

            cfg.CreateMap<KalturaPartnerPremiumServices, PartnerPremiumServices>()
               .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.PremiumServices));

            // PartnerPremiumService to KalturaPartnerPremiumService
            cfg.CreateMap<PartnerPremiumService, KalturaPartnerPremiumService>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.IsApplied, opt => opt.MapFrom(src => src.IsApplied));

            // KalturaPartnerPremiumService to PartnerPremiumService
            cfg.CreateMap<KalturaPartnerPremiumService, PartnerPremiumService>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.IsApplied, opt => opt.MapFrom(src => src.IsApplied));

            cfg.CreateMap<KalturaProgramAssetGroupOffer, ProgramAssetGroupOffer>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => ConvertLanguagedictionary(src.Description)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.EndDate.Value).DateTime : (DateTime?)null))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.ExpiryDate.Value).DateTime : (DateTime?)null))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.ExternalOfferId, opt => opt.MapFrom(src => src.ExternalOfferId))
                .ForMember(dest => dest.FileTypeIds, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.FileTypesIds) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.FileTypesIds, "KalturaProgramAssetGroupOffer.FileTypesIds", true) : null))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => ConvertLanguagedictionary(src.Name)))
                .ForMember(dest => dest.PriceDetailsId, opt => opt.MapFrom(src => src.PriceDetailsId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(src.StartDate.Value).DateTime : (DateTime?)null))
                .ForMember(dest => dest.VirtualAssetId, opt => opt.MapFrom(src => src.VirtualAssetId))
                .ForMember(dest => dest.NullableProperties, opt => opt.MapFrom(src => src.NullableProperties))
                .ForMember(dest => dest.__updated, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdaterId, opt => opt.Ignore())
                .AfterMap((src, dest) => dest.FileTypeIds = src.FileTypesIds != null ? dest.FileTypeIds : null)
                .AfterMap((src, dest) => dest.Name = src.Name != null ? dest.Name : null)
                .AfterMap((src, dest) => dest.Description = src.Description != null ? dest.Description : null)
                .AfterMap((src, dest) => dest.IsActive = src.IsActive != null ? dest.IsActive : null)
               ;

            cfg.CreateMap<ProgramAssetGroupOffer, KalturaProgramAssetGroupOffer>()
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Description)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.EndDate)))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.ExpiryDate)))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.ExternalOfferId, opt => opt.MapFrom(src => src.ExternalOfferId))
                .ForMember(dest => dest.FileTypesIds, opt => opt.MapFrom(src => src.FileTypeIds != null ? string.Join(",", src.FileTypeIds) : string.Empty))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Name)))
                .ForMember(dest => dest.PriceDetailsId, opt => opt.MapFrom(src => src.PriceDetailsId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.StartDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
                .ForMember(dest => dest.VirtualAssetId, opt => opt.MapFrom(src => src.VirtualAssetId))                
                ;

            cfg.CreateMap<KalturaProgramAssetGroupOfferOrderBy, ProgramAssetGroupOfferOrderBy>()
               .ConvertUsing(type =>
               {
                   switch (type)
                   {
                       case KalturaProgramAssetGroupOfferOrderBy.NAME_ASC:
                           return ProgramAssetGroupOfferOrderBy.NameAsc;
                       case KalturaProgramAssetGroupOfferOrderBy.NAME_DESC:
                           return ProgramAssetGroupOfferOrderBy.NameDesc;
                       case KalturaProgramAssetGroupOfferOrderBy.UPDATE_DATE_ASC:
                           return ProgramAssetGroupOfferOrderBy.UpdateDateAsc;
                       case KalturaProgramAssetGroupOfferOrderBy.UPDATE_DATE_DESC:
                           return ProgramAssetGroupOfferOrderBy.UpdateDateDesc;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown ProgramAssetGroupOfferOrderBy value : {type}");
                   }
               });

            cfg.CreateMap<PricesContainer, KalturaProductPrice>()
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrice))
               .ForMember(dest => dest.PurchaseStatus, opt => opt.MapFrom(src => ConvertPriceReasonToPurchaseStatus(src.m_PriceReason)))
               .ForMember(dest => dest.FullPrice, opt => opt.MapFrom(src => src.OriginalPrice))
               .ForMember(dest => dest.PromotionInfo, opt => opt.MapFrom(src => src.PromotionInfo))
               ;

            cfg.CreateMap<PagoPricesContainer, KalturaProgramAssetGroupOfferPrice>()
                .IncludeBase<PricesContainer, KalturaProductPrice>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.PagoId))
               .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => KalturaTransactionType.programAssetGroupOffer))
               ;
        }

        private static KalturaSubscriptionSetType ConvertSetType(SubscriptionSetType subscriptionSetType)
        {
            KalturaSubscriptionSetType result = KalturaSubscriptionSetType.SWITCH;
            switch (subscriptionSetType)
            {
                case SubscriptionSetType.Dependency:
                    {
                        result = KalturaSubscriptionSetType.DEPENDENCY;
                        break;
                    }
                case SubscriptionSetType.Switch:
                    {
                        result = KalturaSubscriptionSetType.SWITCH;
                        break;
                    }
                default:
                    break;
            }
            return result;
        }

        private static List<KalturaProductCode> ConvertProductCodes(List<KeyValuePair<VerificationPaymentGateway, string>> list)
        {
            List<KalturaProductCode> res = new List<KalturaProductCode>();
            if (list != null && list.Count > 0)
            {
                KalturaProductCode kpc = null;
                foreach (KeyValuePair<VerificationPaymentGateway, string> item in list)
                {
                    kpc = new KalturaProductCode();
                    switch (item.Key)
                    {
                        case VerificationPaymentGateway.Apple:
                            kpc.InappProvider = VerificationPaymentGateway.Apple.ToString();
                            break;
                        case VerificationPaymentGateway.Google:
                            kpc.InappProvider = VerificationPaymentGateway.Google.ToString();
                            break;
                        case VerificationPaymentGateway.Roku:
                            kpc.InappProvider = VerificationPaymentGateway.Roku.ToString();
                            break;
                        default:
                            throw new ClientException((int)StatusCode.Error, "Unknown verification payment gateway");
                    }

                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        kpc.Code = item.Value;
                        res.Add(kpc);
                    }
                }
            }
            return res;
        }
        
        private static LanguageContainer[] GetDescriptions(List<KalturaTranslationToken> descriptions)
        {
            if (descriptions != null)
            {
                return AutoMapper.Mapper.Map<List<KalturaTranslationToken>, LanguageContainer[]>(descriptions);
            }
            return null;
        }
        
        private static long? GetCouponsGroupId(CouponsGroup couponsGroup)
        {
            if (couponsGroup != null && long.TryParse(couponsGroup.m_sGroupCode, out long couponGroupIdLong))
            {
                return couponGroupIdLong;
            }
            return null;
        }
        
        private static int? GetDiscountModuleId(DiscountModule discountModule)
        {
            if (discountModule != null)
            {
                return discountModule.m_nObjectID;
            }
            return null;
        }
        private static List<KeyValuePair<VerificationPaymentGateway, string>> ConvertProductCodes(List<KalturaProductCode> list)
        {
            List<KeyValuePair<VerificationPaymentGateway, string>> res = new List<KeyValuePair<VerificationPaymentGateway, string>>();

            if (list != null && list.Count > 0)
            {
                list.ForEach(productCode =>
                {
                    res.Add(new KeyValuePair<VerificationPaymentGateway, string>((VerificationPaymentGateway)Enum.Parse(typeof(VerificationPaymentGateway), productCode.InappProvider), productCode.Code));

                });
            }

            return res;
        }

        private static List<KalturaCouponsGroup> ConvertCouponsGroup(List<SubscriptionCouponGroup> list)
        {
            try
            {
                List<KalturaCouponsGroup> res = new List<KalturaCouponsGroup>();
                if (list != null && list.Count > 0)
                {
                    KalturaCouponsGroup item = null;
                    foreach (SubscriptionCouponGroup scg in list)
                    {
                        item = AutoMapper.Mapper.Map<KalturaCouponsGroup>(scg);
                        res.Add(item);
                    }
                }
                return res;
            }
            catch
            {
                return null;
            }
        }

        private static List<SubscriptionCouponGroup> ConvertCouponsGroup(List<KalturaCouponsGroup> list)
        {
            try
            {
                List<SubscriptionCouponGroup> res = new List<SubscriptionCouponGroup>();
                if (list != null && list.Count > 0)
                {
                    SubscriptionCouponGroup item = null;

                    foreach (KalturaCouponsGroup scg in list)
                    {
                        item = AutoMapper.Mapper.Map<SubscriptionCouponGroup>(scg);
                        res.Add(item);
                    }
                }
                return res;
            }
            catch
            {
                return new List<SubscriptionCouponGroup>();
            }
        }

        public static List<int> ConvertToIntList(List<int> list)
        {
            List<int> result = null;

            if (list != null && list.Count() > 0)
            {
                result = list.ToList();
            }
            return result;
        }

        public static CouponGroupType? ConvertCouponGroupType(KalturaCouponGroupType? couponGroupType)
        {
            CouponGroupType? result = null;
            if (couponGroupType.HasValue)
            {
                switch (couponGroupType.Value)
                {
                    case KalturaCouponGroupType.COUPON:
                        {
                            result = CouponGroupType.Coupon;
                            break;
                        }
                    case KalturaCouponGroupType.GIFT_CARD:
                        {
                            result = CouponGroupType.GiftCard;
                            break;
                        }
                    default:
                        break;
                }
            }

            return result;
        }

        public static KalturaCouponGroupType? ConvertCouponGroupType(CouponGroupType? couponGroupType)
        {
            KalturaCouponGroupType? result = null;
            if (couponGroupType.HasValue)
            {
                switch (couponGroupType.Value)
                {
                    case CouponGroupType.Coupon:
                        {
                            result = KalturaCouponGroupType.COUPON;
                            break;
                        }
                    case CouponGroupType.GiftCard:
                        {
                            result = KalturaCouponGroupType.GIFT_CARD;
                            break;
                        }
                    default:
                        break;
                }
            }

            return result;
        }

        private static KalturaCouponGroupType ConvertCouponGroupType(CouponGroupType couponType)
        {
            KalturaCouponGroupType result = KalturaCouponGroupType.COUPON;

            switch (couponType)
            {
                case CouponGroupType.Coupon:
                    {
                        result = KalturaCouponGroupType.COUPON;
                        break;
                    }
                case CouponGroupType.GiftCard:
                    {
                        result = KalturaCouponGroupType.GIFT_CARD;
                        break;
                    }
                default:
                    break;
            }

            return result;
        }
        private static KalturaPurchaseStatus ConvertPriceReasonToPurchaseStatus(PriceReason priceReason)
        {
            KalturaPurchaseStatus result;
            switch (priceReason)
            {
                case PriceReason.PPVPurchased:
                    result = KalturaPurchaseStatus.ppv_purchased;
                    break;
                case PriceReason.Free:
                    result = KalturaPurchaseStatus.free;
                    break;
                case PriceReason.ForPurchaseSubscriptionOnly:
                    result = KalturaPurchaseStatus.for_purchase_subscription_only;
                    break;
                case PriceReason.SubscriptionPurchased:
                    result = KalturaPurchaseStatus.subscription_purchased;
                    break;
                case PriceReason.ForPurchase:
                    result = KalturaPurchaseStatus.for_purchase;
                    break;
                case PriceReason.SubscriptionPurchasedWrongCurrency:
                    result = KalturaPurchaseStatus.subscription_purchased_wrong_currency;
                    break;
                case PriceReason.PrePaidPurchased:
                    result = KalturaPurchaseStatus.pre_paid_purchased;
                    break;
                case PriceReason.GeoCommerceBlocked:
                    result = KalturaPurchaseStatus.geo_commerce_blocked;
                    break;
                case PriceReason.EntitledToPreviewModule:
                    result = KalturaPurchaseStatus.entitled_to_preview_module;
                    break;
                case PriceReason.FirstDeviceLimitation:
                    result = KalturaPurchaseStatus.first_device_limitation;
                    break;
                case PriceReason.CollectionPurchased:
                    result = KalturaPurchaseStatus.collection_purchased;
                    break;
                case PriceReason.UserSuspended:
                    result = KalturaPurchaseStatus.user_suspended;
                    break;
                case PriceReason.NotForPurchase:
                    result = KalturaPurchaseStatus.not_for_purchase;
                    break;
                case PriceReason.InvalidCurrency:
                    result = KalturaPurchaseStatus.invalid_currency;
                    break;
                case PriceReason.CurrencyNotDefinedOnPriceCode:
                    result = KalturaPurchaseStatus.currency_not_defined_on_price_code;
                    break;
                case PriceReason.PendingEntitlement:
                    result = KalturaPurchaseStatus.pending_entitlement;
                    break;
                case PriceReason.PagoPurchased:
                    result = KalturaPurchaseStatus.program_asset_group_offer_purchased;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown purchase status");
            }

            return result;
        }

        private static KalturaCouponStatus ConvertCouponStatus(CouponsStatus couponStatus)
        {
            KalturaCouponStatus result;

            switch (couponStatus)
            {
                case CouponsStatus.Valid:
                    result = KalturaCouponStatus.VALID;
                    break;
                case CouponsStatus.NotExists:
                    result = KalturaCouponStatus.NOT_EXISTS;
                    break;
                case CouponsStatus.AllreadyUsed:
                    result = KalturaCouponStatus.ALREADY_USED;
                    break;
                case CouponsStatus.Expired:
                    result = KalturaCouponStatus.EXPIRED;
                    break;
                case CouponsStatus.NotActive:
                    result = KalturaCouponStatus.INACTIVE;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown coupon status");
            }

            return result;
        }

        public static SubscriptionOrderBy ConvertSubscriptionOrderBy(KalturaSubscriptionOrderBy orderBy)
        {
            SubscriptionOrderBy result;

            switch (orderBy)
            {
                case KalturaSubscriptionOrderBy.START_DATE_ASC:
                    result = SubscriptionOrderBy.StartDateAsc;
                    break;
                case KalturaSubscriptionOrderBy.START_DATE_DESC:
                    result = SubscriptionOrderBy.StartDateDesc;
                    break;
                case KalturaSubscriptionOrderBy.CREATE_DATE_ASC:
                    result = SubscriptionOrderBy.CreateDateAsc;
                    break;
                case KalturaSubscriptionOrderBy.CREATE_DATE_DESC:
                    result = SubscriptionOrderBy.CreateDateDesc;
                    break;
                case KalturaSubscriptionOrderBy.UPDATE_DATE_ASC:
                    result = SubscriptionOrderBy.UpdateDateAsc;
                    break;
                case KalturaSubscriptionOrderBy.UPDATE_DATE_DESC:
                    result = SubscriptionOrderBy.UpdateDateDesc;
                    break;
                case KalturaSubscriptionOrderBy.NAME_ASC:
                    result = SubscriptionOrderBy.NameAsc;
                    break;
                case KalturaSubscriptionOrderBy.NAME_DESC:
                    result = SubscriptionOrderBy.NameDesc;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown export task order by");
            }

            return result;
        }

        public static List<KalturaPpvPrice> ConvertPpvPrice(MediaFileItemPricesContainer[] itemPrices)
        {
            List<KalturaPpvPrice> prices = null;
            if (itemPrices != null)
            {
                prices = new List<KalturaPpvPrice>();

                foreach (var item in itemPrices)
                {
                    if (item.m_oItemPrices != null)
                    {
                        foreach (var ppvPrice in item.m_oItemPrices)
                        {
                            prices.Add(new KalturaPpvPrice()
                            {
                                CollectionId = ppvPrice.m_relevantCol != null ? ppvPrice.m_relevantCol.m_CollectionCode : null,
                                DiscountEndDate = ppvPrice.m_dtDiscountEndDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(ppvPrice.m_dtDiscountEndDate.Value) : 0,
                                EndDate = ppvPrice.m_dtEndDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(ppvPrice.m_dtEndDate.Value) : 0,
                                FileId = item.m_nMediaFileID,
                                FirstDeviceName = ppvPrice.m_sFirstDeviceNameFound,
                                FullPrice = AutoMapper.Mapper.Map<KalturaPrice>(ppvPrice.m_oFullPrice),
                                IsInCancelationPeriod = ppvPrice.m_bCancelWindow,
                                IsSubscriptionOnly = ppvPrice.m_bSubscriptionOnly,
                                PPVDescriptions = AutoMapper.Mapper.Map<List<KalturaTranslationToken>>(ppvPrice.m_oPPVDescription),
                                PPVModuleId = ppvPrice.m_sPPVModuleCode,
                                PrePaidId = ppvPrice.m_relevantPP != null ? ppvPrice.m_relevantPP.m_ObjectCode.ToString() : null,
                                Price = AutoMapper.Mapper.Map<KalturaPrice>(ppvPrice.m_oPrice),
                                ProductCode = ppvPrice.m_sProductCode,
                                ProductId = item.m_sProductCode,
                                ProductType = KalturaTransactionType.ppv,
                                PurchasedMediaFileId = ppvPrice.m_lPurchasedMediaFileID,
                                PurchaseStatus = ConvertPriceReasonToPurchaseStatus(ppvPrice.m_PriceReason),
                                PurchaseUserId = ppvPrice.m_sPurchasedBySiteGuid,
                                RelatedMediaFileIds = AutoMapper.Mapper.Map<List<KalturaIntegerValue>>(ppvPrice.m_lRelatedMediaFileIDs),
                                StartDate = ppvPrice.m_dtStartDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(ppvPrice.m_dtStartDate.Value) : 0,
                                SubscriptionId = ppvPrice.m_relevantSub != null ? ppvPrice.m_relevantSub.m_sObjectCode : null,
                                PromotionInfo = AutoMapper.Mapper.Map<KalturaPromotionInfo>(ppvPrice.PromotionInfo),
                            });
                        }
                    }
                }

            }
            return prices;
        }

        private static List<KalturaPremiumService> ConvertServices(ServiceObject[] services)
        {
            try
            {
                List<KalturaPremiumService> result = null;

                if (services != null && services.Count() > 0)
                {
                    result = new List<KalturaPremiumService>();

                    KalturaPremiumService item;

                    foreach (var service in services)
                    {
                        if (service is NpvrServiceObject)
                        {
                            item = AutoMapper.Mapper.Map<KalturaNpvrPremiumService>((NpvrServiceObject)service);
                        }
                        else
                        {
                            item = AutoMapper.Mapper.Map<KalturaPremiumService>(service);
                        }
                        result.Add(item);
                    }
                }

                return result;
            }
            catch
            {
                return null;
            }

            return new List<KalturaPremiumService>();
        }

        private static KalturaSubscriptionDependencyType? ConvertSubscriptionType(SubscriptionType? type)
        {
            KalturaSubscriptionDependencyType? result = null;

            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case SubscriptionType.AddOn:
                        {
                            result = KalturaSubscriptionDependencyType.ADDON;
                            break;
                        }
                    case SubscriptionType.Base:
                        {
                            result = KalturaSubscriptionDependencyType.BASE;
                            break;
                        }
                    case SubscriptionType.NotApplicable:
                        {
                            result = KalturaSubscriptionDependencyType.NOTAPPLICABLE;
                            break;
                        }
                    default:
                        break;
                }
            }
            return result;
        }

        public static SubscriptionType? ConvertSubscriptionType(KalturaSubscriptionDependencyType? type)
        {
            SubscriptionType? result = null;

            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case KalturaSubscriptionDependencyType.ADDON:
                        {
                            result = SubscriptionType.AddOn;
                            break;
                        }
                    case KalturaSubscriptionDependencyType.BASE:
                        {
                            result = SubscriptionType.Base;
                            break;
                        }
                    case KalturaSubscriptionDependencyType.NOTAPPLICABLE:
                        {
                            result = SubscriptionType.NotApplicable;
                            break;
                        }
                    default:
                        break;
                }
            }
            return result;
        }


        public static KalturaSubscriptionSetType? ConvertSubscriptionSetType(SubscriptionSetType? type)
        {
            KalturaSubscriptionSetType? result = null;

            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case SubscriptionSetType.Switch:
                        {
                            result = KalturaSubscriptionSetType.SWITCH;
                            break;
                        }
                    case SubscriptionSetType.Dependency:
                        {
                            result = KalturaSubscriptionSetType.DEPENDENCY;
                            break;
                        }
                    default:
                        break;
                }
            }

            return result;
        }

        public static SubscriptionSetType? ConvertSubscriptionSetType(KalturaSubscriptionSetType? type)
        {
            SubscriptionSetType? result = null;
            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case KalturaSubscriptionSetType.SWITCH:
                        {
                            result = SubscriptionSetType.Switch;
                            break;
                        }
                    case KalturaSubscriptionSetType.DEPENDENCY:
                        {
                            result = SubscriptionSetType.Dependency;
                            break;
                        }
                    default:
                        break;
                }
            }

            return result;
        }

        internal static List<KalturaCollectionPrice> ConvertCollectionPrice(CollectionsPricesContainer[] collectionsPrices)
        {
            List<KalturaCollectionPrice> prices = null;
            if (collectionsPrices != null)
            {
                prices = new List<KalturaCollectionPrice>();

                foreach (var collectionPrice in collectionsPrices)
                {
                    prices.Add(new KalturaCollectionPrice()
                    {
                        Price = AutoMapper.Mapper.Map<KalturaPrice>(collectionPrice.m_oPrice),
                        ProductId = collectionPrice.m_sCollectionCode,
                        ProductType = KalturaTransactionType.collection,
                        PurchaseStatus = ConvertPriceReasonToPurchaseStatus(collectionPrice.m_PriceReason),
                        FullPrice = AutoMapper.Mapper.Map<KalturaPrice>(collectionPrice.OriginalPrice),
                        PromotionInfo = AutoMapper.Mapper.Map<KalturaPromotionInfo>(collectionPrice.PromotionInfo)
                    });
                }
            }

            return prices;
        }

        public static KalturaStringValueArray BuildCouponCodeList(List<string> list)
        {
            if (list == null)
            {
                return null;
            }

            KalturaStringValueArray stringValueArray = new KalturaStringValueArray();

            foreach (var stringValue in list)
            {
                stringValueArray.Objects.Add(new KalturaStringValue() { value = stringValue });
            }

            return stringValueArray;
        }

        public static Currency ConvertPriceCurrency(KalturaPrice price)
        {
            Currency result = null;

            if (price != null)
            {
                result = new Currency()
                {
                    m_sCurrencyCD3 = price.Currency,
                    m_sCurrencySign = price.CurrencySign
                };
            }

            return result;
        }

        public static LanguageContainer[] ConvertLanguageContainer(KalturaMultilingualString multilingualString)
        {
            List<LanguageContainer> languageContainerList = new List<LanguageContainer>();

            multilingualString.Values.ForEach(val =>
            {
                languageContainerList.Add(new LanguageContainer(val.Language, val.Value));
            });

            return languageContainerList.ToArray();
        }

        private static ServiceObject[] ConvertServices(List<KalturaPremiumService> services)
        {
            List<ServiceObject> result = null;

            try
            {
                if (services?.Count > 0)
                {
                    result = new List<ServiceObject>();

                    ServiceObject item;

                    foreach (var service in services)
                    {
                        if (service is KalturaNpvrPremiumService)
                        {
                            item = AutoMapper.Mapper.Map<NpvrServiceObject>((KalturaNpvrPremiumService)service);
                        }
                        else
                        {
                            item = AutoMapper.Mapper.Map<ServiceObject>(service);
                        }
                        result.Add(item);
                    }
                }

                return result != null ? result.ToArray() : null;
            }
            catch
            {
                return null;
            }
        }
        private static List<KalturaSubscriptionCouponGroup> ConvertSubCouponsGroup(List<SubscriptionCouponGroup> couponsGroups)
        {
            List<KalturaSubscriptionCouponGroup> list = null;

            if (couponsGroups?.Count > 0)
            {
                list = new List<KalturaSubscriptionCouponGroup>();
                foreach (var item in couponsGroups)
                {
                    long.TryParse(item.m_sGroupCode, out long id);
                    list.Add(new KalturaSubscriptionCouponGroup()
                    {
                        CouponGroupId = id,
                        EndDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(item.endDate),
                        StartDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(item.startDate)

                    });
                }
            }

            return list;
        }

        private static List<SubscriptionCouponGroupDTO> ConvertSubCouponGroup(List<KalturaSubscriptionCouponGroup> subscriptionCouponGroup)
        {
            List<SubscriptionCouponGroupDTO> list = null;

            if (subscriptionCouponGroup == null)
                return null;

            if (subscriptionCouponGroup.Count > 0)
            {
                list = new List<SubscriptionCouponGroupDTO>();
                foreach (var item in subscriptionCouponGroup)
                {
                    list.Add(new SubscriptionCouponGroupDTO(item.CouponGroupId.ToString(),
                        DateUtils.UtcUnixTimestampSecondsToDateTime(item.StartDate), DateUtils.UtcUnixTimestampSecondsToDateTime(item.EndDate)));
                }
            }

            return list;
        }

        private static List<KalturaCollectionCouponGroup> ConvertCollCouponsGroup(List<SubscriptionCouponGroup> couponsGroups)
        {
            List<KalturaCollectionCouponGroup> list = null;

            if (couponsGroups?.Count > 0)
            {
                list = new List<KalturaCollectionCouponGroup>();
                foreach (var item in couponsGroups)
                {
                    long.TryParse(item.m_sGroupCode, out long id);
                    list.Add(new KalturaCollectionCouponGroup()
                    {
                        CouponGroupId = id,
                        EndDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(item.endDate),
                        StartDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(item.startDate)

                    });
                }
            }

            return list;
        }

        private static List<SubscriptionCouponGroupDTO> ConvertCollCouponGroup(List<KalturaCollectionCouponGroup> subscriptionCouponGroup)
        {
            List<SubscriptionCouponGroupDTO> list = null;

            if (subscriptionCouponGroup == null)
                return null;

            if (subscriptionCouponGroup.Count > 0)
            {
                list = new List<SubscriptionCouponGroupDTO>();
                foreach (var item in subscriptionCouponGroup)
                {
                    list.Add(new SubscriptionCouponGroupDTO(item.CouponGroupId.ToString(),
                        DateUtils.UtcUnixTimestampSecondsToDateTime(item.StartDate), DateUtils.UtcUnixTimestampSecondsToDateTime(item.EndDate)));
                }
            }

            return list;
        }

        private static Dictionary<string, string> ConvertLanguagedictionary(KalturaMultilingualString multilingualString)
        {
            Dictionary<string, string> languages = new Dictionary<string, string>();

            multilingualString.Values.ForEach(val =>
            {
                languages.Add(val.Language, val.Value);
            });

            return languages;
        }
    }
}