using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using ApiObjects;
using WebAPI.Managers.Models;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;
using WebAPI.Models.General;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Users;
using WebAPI.Models.Catalog;
using ApiObjects.Pricing;
using Core.Pricing;
using Core.ConditionalAccess;
using ApiObjects.ConditionalAccess;
using WebAPI.Utils;


namespace WebAPI.ObjectsConvertor.Mapping
{
    public class PricingMappings
    {
        public static void RegisterMappings()
        {
            // CouponsGroup
            Mapper.CreateMap<CouponsGroup, KalturaCouponsGroup>()
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sGroupCode))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sGroupName))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)))
               .ForMember(dest => dest.MaxUsesNumber, opt => opt.MapFrom(src => src.m_nMaxUseCountForCoupon))
               .ForMember(dest => dest.MaxUsesNumberOnRenewableSub, opt => opt.MapFrom(src => src.m_nMaxRecurringUsesCountForCoupon))
               .ForMember(dest => dest.CouponGroupType, opt => opt.MapFrom(src => ConvertCouponGroupType(src.couponGroupType)))
               ;

            // Price
            Mapper.CreateMap<Price, KalturaPrice>()
               .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.m_dPrice))
               .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3))
               .ForMember(dest => dest.CurrencySign, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencySign)); ;

            // PriceCode
            Mapper.CreateMap<PriceCode, KalturaPriceDetails>()
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjectID))
               .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.m_sCode))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrise));

            // DiscountModule
            Mapper.CreateMap<DiscountModule, KalturaDiscountModule>()
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
               .ForMember(dest => dest.Percent, opt => opt.MapFrom(src => src.m_dPercent))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)));

            // UsageModule
            Mapper.CreateMap<UsageModule, KalturaUsageModule>()
               .ForMember(dest => dest.CouponId, opt => opt.MapFrom(src => src.m_coupon_id))
               .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.m_tsMaxUsageModuleLifeCycle))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjectID))
               .ForMember(dest => dest.IsOfflinePlayback, opt => opt.MapFrom(src => src.m_bIsOfflinePlayBack))               
               .ForMember(dest => dest.IsWaiverEnabled, opt => opt.MapFrom(src => src.m_bWaiver))
               .ForMember(dest => dest.MaxViewsNumber, opt => opt.MapFrom(src => src.m_nMaxNumberOfViews))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sVirtualName))
               .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.m_tsViewLifeCycle))
               .ForMember(dest => dest.WaiverPeriod, opt => opt.MapFrom(src => src.m_nWaiverPeriod));

            // UserType
            Mapper.CreateMap<UserType, KalturaOTTUserType>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // PreviewModule
            Mapper.CreateMap<PreviewModule, KalturaPreviewModule>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nID))
               .ForMember(dest => dest.LifeCycle, opt => opt.MapFrom(src => src.m_tsFullLifeCycle))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
               .ForMember(dest => dest.NonRenewablePeriod, opt => opt.MapFrom(src => src.m_tsNonRenewPeriod));

            // ServiceObject to PremiumService
            Mapper.CreateMap<ServiceObject, KalturaPremiumService>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            Mapper.CreateMap<NpvrServiceObject, KalturaNpvrPremiumService>()
               .ForMember(dest => dest.QuotaInMinutes, opt => opt.MapFrom(src => src.Quota))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
            // LanguageContainer to TranslationContainer
            Mapper.CreateMap<LanguageContainer, KalturaTranslationToken>()
               .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.m_sLanguageCode3))
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.m_sValue));

            // LanguageContainer to TranslationContainer
            Mapper.CreateMap<LanguageContainer, KalturaTranslationToken>()
               .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.m_sLanguageCode3))
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.m_sValue));

            // BundleCodeContainer to SlimChannel
            Mapper.CreateMap<BundleCodeContainer, KalturaBaseChannel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sCode))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName));

            // BundleCodeContainer to SlimChannel
            Mapper.CreateMap<SubscriptionsPricesContainer, KalturaSubscriptionPrice>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sSubscriptionCode))
               .ForMember(dest => dest.PurchaseStatus, opt => opt.MapFrom(src => ConvertPriceReasonToPurchaseStatus(src.m_PriceReason)))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrice))
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sSubscriptionCode))
               .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => KalturaTransactionType.subscription));


            // Subscription
            Mapper.CreateMap<Subscription, KalturaSubscription>()
               .ForMember(dest => dest.IsInfiniteRenewal, opt => opt.MapFrom(src => src.m_bIsInfiniteRecurring))
               .ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.m_bIsRecurring))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
               .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.m_fictivicMediaID))
               //.ForMember(dest => dest.PremiumServices, opt => opt.MapFrom(src => src.m_lServices))
               .ForMember(dest => dest.PremiumServices, opt => opt.MapFrom(src => ConvertServices(src.m_lServices)))
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
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.m_sDescription)))
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.FileTypes, opt => opt.MapFrom(src => src.m_sFileTypes))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.m_sName)))
               .ForMember(dest => dest.Names, opt => opt.MapFrom(src => src.m_sName))
               .ForMember(dest => dest.GracePeriodMinutes, opt => opt.MapFrom(src => src.m_GracePeriodMinutes))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_SubscriptionCode))
               .ForMember(dest => dest.UserTypes, opt => opt.MapFrom(src => src.m_UserTypes));

            // KalturaPricePlan
            Mapper.CreateMap<UsageModule, KalturaPricePlan>()
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
               .ForMember(dest => dest.DiscountId, opt => opt.MapFrom(src => src.m_ext_discount_id));

            // ItemPriceContainer to PPVItemPriceDetails
            Mapper.CreateMap<ItemPriceContainer, KalturaPPVItemPriceDetails>()
               .ForMember(dest => dest.CollectionId, opt => opt.MapFrom(src => src.m_relevantCol))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.m_dtEndDate.HasValue ? SerializationUtils.ConvertToUnixTimestamp(src.m_dtEndDate.Value) : 0))
               .ForMember(dest => dest.DiscountEndDate, opt => opt.MapFrom(src => src.m_dtDiscountEndDate.HasValue ? SerializationUtils.ConvertToUnixTimestamp(src.m_dtDiscountEndDate.Value) : 0))
               .ForMember(dest => dest.FirstDeviceName, opt => opt.MapFrom(src => src.m_sFirstDeviceNameFound))
               .ForMember(dest => dest.FullPrice, opt => opt.MapFrom(src => src.m_oFullPrice))
               .ForMember(dest => dest.IsInCancelationPeriod, opt => opt.MapFrom(src => src.m_bCancelWindow))
               .ForMember(dest => dest.IsSubscriptionOnly, opt => opt.MapFrom(src => src.m_bSubscriptionOnly))
               .ForMember(dest => dest.PPVDescriptions, opt => opt.MapFrom(src => src.m_oPPVDescription))
               .ForMember(dest => dest.PPVModuleId, opt => opt.MapFrom(src => src.m_sPPVModuleCode))
               .ForMember(dest => dest.PrePaidId, opt => opt.MapFrom(src => src.m_relevantPP))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrice))
               .ForMember(dest => dest.PurchasedMediaFileId, opt => opt.MapFrom(src => src.m_lPurchasedMediaFileID))
               .ForMember(dest => dest.PurchaseStatus, opt => opt.MapFrom(src => ConvertPriceReasonToPurchaseStatus(src.m_PriceReason)))
               .ForMember(dest => dest.PurchaseUserId, opt => opt.MapFrom(src => src.m_sPurchasedBySiteGuid))
               .ForMember(dest => dest.RelatedMediaFileIds, opt => opt.MapFrom(src => src.m_lRelatedMediaFileIDs))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.m_dtStartDate.HasValue ? SerializationUtils.ConvertToUnixTimestamp(src.m_dtStartDate.Value) : 0))
               .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.m_relevantSub.m_sObjectCode))
               .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_sProductCode));

            // ItemPriceContainer to PPVItemPriceDetails
            Mapper.CreateMap<MediaFileItemPricesContainer, KalturaItemPrice>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sProductCode))
               .ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.m_nMediaFileID))
               .ForMember(dest => dest.PPVPriceDetails, opt => opt.MapFrom(src => src.m_oItemPrices));

            // CouponData to CouponDetails
            Mapper.CreateMap<CouponData, KalturaCoupon>()
               .ForMember(dest => dest.CouponsGroup, opt => opt.MapFrom(src => src.m_oCouponGroup))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ConvertCouponStatus(src.m_CouponStatus)));

            // PpvModule to KalturaPpvModule
            Mapper.CreateMap<PPVModule, KalturaPpv>()
               .ForMember(dest => dest.CouponsGroup, opt => opt.MapFrom(src => src.m_oCouponsGroup))
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.DiscountModule, opt => opt.MapFrom(src => src.m_oDiscountModule))
               .ForMember(dest => dest.FileTypes, opt => opt.MapFrom(src => src.m_relatedFileTypes))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sObjectCode))
               .ForMember(dest => dest.IsSubscriptionOnly, opt => opt.MapFrom(src => src.m_bSubscriptionOnly))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sObjectVirtualName))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPriceCode))
               .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_Product_Code))
               .ForMember(dest => dest.UsageModule, opt => opt.MapFrom(src => src.m_oUsageModule))
               .ForMember(dest => dest.FirstDeviceLimitation, opt => opt.MapFrom(src => src.m_bFirstDeviceLimitation));

             //KalturaPpvPrice
            Mapper.CreateMap<ItemPriceContainer, KalturaPpvPrice>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sProductCode))
               .ForMember(dest => dest.CollectionId, opt => opt.MapFrom(src => src.m_relevantCol))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.m_dtEndDate.HasValue ? SerializationUtils.ConvertToUnixTimestamp(src.m_dtEndDate.Value) : 0))
               .ForMember(dest => dest.DiscountEndDate, opt => opt.MapFrom(src => src.m_dtDiscountEndDate.HasValue ? SerializationUtils.ConvertToUnixTimestamp(src.m_dtDiscountEndDate.Value) : 0))
               .ForMember(dest => dest.FirstDeviceName, opt => opt.MapFrom(src => src.m_sFirstDeviceNameFound))
               .ForMember(dest => dest.FullPrice, opt => opt.MapFrom(src => src.m_oFullPrice))
               .ForMember(dest => dest.IsInCancelationPeriod, opt => opt.MapFrom(src => src.m_bCancelWindow))
               .ForMember(dest => dest.IsSubscriptionOnly, opt => opt.MapFrom(src => src.m_bSubscriptionOnly))
               .ForMember(dest => dest.PPVDescriptions, opt => opt.MapFrom(src => src.m_oPPVDescription))
               .ForMember(dest => dest.PPVModuleId, opt => opt.MapFrom(src => src.m_sPPVModuleCode))
               .ForMember(dest => dest.PrePaidId, opt => opt.MapFrom(src => src.m_relevantPP))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrice))
               .ForMember(dest => dest.PurchasedMediaFileId, opt => opt.MapFrom(src => src.m_lPurchasedMediaFileID))
               .ForMember(dest => dest.PurchaseStatus, opt => opt.MapFrom(src => ConvertPriceReasonToPurchaseStatus(src.m_PriceReason)))
               .ForMember(dest => dest.PurchaseUserId, opt => opt.MapFrom(src => src.m_sPurchasedBySiteGuid))
               .ForMember(dest => dest.RelatedMediaFileIds, opt => opt.MapFrom(src => src.m_lRelatedMediaFileIDs))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.m_dtStartDate.HasValue ? SerializationUtils.ConvertToUnixTimestamp(src.m_dtStartDate.Value) : 0))
               .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.m_relevantSub.m_sObjectCode))
               .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_sProductCode));
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
                                    DiscountEndDate = ppvPrice.m_dtDiscountEndDate.HasValue ? SerializationUtils.ConvertToUnixTimestamp(ppvPrice.m_dtDiscountEndDate.Value) : 0,
                                    EndDate = ppvPrice.m_dtEndDate.HasValue ? SerializationUtils.ConvertToUnixTimestamp(ppvPrice.m_dtEndDate.Value) : 0,
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
                                    StartDate = ppvPrice.m_dtStartDate.HasValue ? SerializationUtils.ConvertToUnixTimestamp(ppvPrice.m_dtStartDate.Value) : 0,
                                    SubscriptionId = ppvPrice.m_relevantSub != null ? ppvPrice.m_relevantSub.m_sObjectCode : null
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
    }
}