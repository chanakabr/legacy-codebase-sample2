using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using WebAPI.Managers.Models;
using WebAPI.Utils;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Pricing;
using WebAPI.Pricing;


namespace WebAPI.Mapping.ObjectsConvertor
{
    public class PricingMappings
    {
        public static void RegisterMappings()
        {
            // CouponsGroup
            Mapper.CreateMap<WebAPI.Pricing.CouponsGroup, KalturaCouponsGroup>()
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sGroupCode))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sGroupName))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)))
               .ForMember(dest => dest.MaxUsesNumber, opt => opt.MapFrom(src => src.m_nMaxUseCountForCoupon))
               .ForMember(dest => dest.MaxUsesNumberOnRenewableSub, opt => opt.MapFrom(src => src.m_nMaxRecurringUsesCountForCoupon));

            // Price
            Mapper.CreateMap<WebAPI.Pricing.Price, KalturaPrice>()
               .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.m_dPrice))
               .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3))
               .ForMember(dest => dest.CurrencySign, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencySign)); ;

            // PriceCode
            Mapper.CreateMap<WebAPI.Pricing.PriceCode, KalturaPriceDetails>()
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjectID))
               .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.m_sCode))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrise));

            // DiscountModule
            Mapper.CreateMap<WebAPI.Pricing.DiscountModule, KalturaDiscountModule>()
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
               .ForMember(dest => dest.Percent, opt => opt.MapFrom(src => src.m_dPercent))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)));

            // UsageModule
            Mapper.CreateMap<WebAPI.Pricing.UsageModule, KalturaUsageModule>()
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
            Mapper.CreateMap<Pricing.UserType, Models.Users.KalturaOTTUserType>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // PreviewModule
            Mapper.CreateMap<Pricing.PreviewModule, KalturaPreviewModule>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nID))
               .ForMember(dest => dest.LifeCycle, opt => opt.MapFrom(src => src.m_tsFullLifeCycle))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
               .ForMember(dest => dest.NonRenewablePeriod, opt => opt.MapFrom(src => src.m_tsNonRenewPeriod));

            // ServiceObject to PremiumService
            Mapper.CreateMap<Pricing.ServiceObject, Models.ConditionalAccess.KalturaPremiumService>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            // LanguageContainer to TranslationContainer
            Mapper.CreateMap<Pricing.LanguageContainer, Models.General.KalturaTranslationToken>()
               .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.m_sLanguageCode3))
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.m_sValue));

            // LanguageContainer to TranslationContainer
            Mapper.CreateMap<ConditionalAccess.LanguageContainer, Models.General.KalturaTranslationToken>()
               .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.m_sLanguageCode3))
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.m_sValue));

            // BundleCodeContainer to SlimChannel
            Mapper.CreateMap<Pricing.BundleCodeContainer, Models.Catalog.KalturaBaseChannel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sCode))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName));

            // BundleCodeContainer to SlimChannel
            Mapper.CreateMap<ConditionalAccess.SubscriptionsPricesContainer, KalturaSubscriptionPrice>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sSubscriptionCode))
               .ForMember(dest => dest.PurchaseStatus, opt => opt.MapFrom(src => ConvertPriceReasonToPurchaseStatus(src.m_PriceReason)))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrice))
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sSubscriptionCode))
               .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => KalturaTransactionType.SUBSCRIPTION));


            // Subscription
            Mapper.CreateMap<WebAPI.Pricing.Subscription, KalturaSubscription>()
               .ForMember(dest => dest.IsInfiniteRenewal, opt => opt.MapFrom(src => src.m_bIsInfiniteRecurring))
               .ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.m_bIsRecurring))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
               .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.m_fictivicMediaID))
               .ForMember(dest => dest.PremiumServices, opt => opt.MapFrom(src => src.m_lServices))
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
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.FileTypes, opt => opt.MapFrom(src => src.m_sFileTypes))
               .ForMember(dest => dest.Names, opt => opt.MapFrom(src => src.m_sName))
               .ForMember(dest => dest.GracePeriodMinutes, opt => opt.MapFrom(src => src.m_GracePeriodMinutes))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_SubscriptionCode))
               .ForMember(dest => dest.UserTypes, opt => opt.MapFrom(src => src.m_UserTypes));

            // KalturaPricePlan
            Mapper.CreateMap<WebAPI.Pricing.UsageModule, KalturaPricePlan>()
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
            Mapper.CreateMap<ConditionalAccess.ItemPriceContainer, KalturaPPVItemPriceDetails>()
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
            Mapper.CreateMap<ConditionalAccess.MediaFileItemPricesContainer, KalturaItemPrice>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sProductCode))
               .ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.m_nMediaFileID))
               .ForMember(dest => dest.PPVPriceDetails, opt => opt.MapFrom(src => src.m_oItemPrices));

            // CouponData to CouponDetails
            Mapper.CreateMap<Pricing.CouponData, KalturaCoupon>()
               .ForMember(dest => dest.CouponsGroup, opt => opt.MapFrom(src => src.m_oCouponGroup))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ConvertCouponStatus(src.m_CouponStatus)));

            // PpvModule to KalturaPpvModule
            Mapper.CreateMap<WebAPI.Pricing.PPVModule, KalturaPpv>()
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
        }

        public static List<int> ConvertToIntList(int[] list)
        {
            List<int> result = null;

            if (list != null && list.Count() > 0)
            {
                result = list.ToList();
            }
            return result;
        }

        private static KalturaPurchaseStatus ConvertPriceReasonToPurchaseStatus(ConditionalAccess.PriceReason priceReason)
        {
            KalturaPurchaseStatus result;
            switch (priceReason)
            {
                case WebAPI.ConditionalAccess.PriceReason.PPVPurchased:
                    result = KalturaPurchaseStatus.PPV_PURCHASED;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.Free:
                    result = KalturaPurchaseStatus.FREE;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.ForPurchaseSubscriptionOnly:
                    result = KalturaPurchaseStatus.FOR_PURCHASE_SUBSCRIPTION_ONLY;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.SubscriptionPurchased:
                    result = KalturaPurchaseStatus.SUBSCRIPTION_PURCHASED;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.ForPurchase:
                    result = KalturaPurchaseStatus.FOR_PURCHASE;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.SubscriptionPurchasedWrongCurrency:
                    result = KalturaPurchaseStatus.SUBSCRIPTION_PURCHASED_WRONG_CURRENCY;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.PrePaidPurchased:
                    result = KalturaPurchaseStatus.PRE_PAID_PURCHASED;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.GeoCommerceBlocked:
                    result = KalturaPurchaseStatus.GEO_COMMERCE_BLOCKED;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.EntitledToPreviewModule:
                    result = KalturaPurchaseStatus.ENTITLED_TO_PREVIEW_MODULE;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.FirstDeviceLimitation:
                    result = KalturaPurchaseStatus.FIRST_DEVICE_LIMITATION;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.CollectionPurchased:
                    result = KalturaPurchaseStatus.COLLECTION_PURCHASED;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.UserSuspended:
                    result = KalturaPurchaseStatus.USER_SUSPENDED;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.NotForPurchase:
                    result = KalturaPurchaseStatus.NOT_FOR_PURCHASE;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown purchase status");
            }

            return result;
        }

        private static KalturaCouponStatus ConvertCouponStatus(WebAPI.Pricing.CouponsStatus couponStatus)
        {
            KalturaCouponStatus result;

            switch (couponStatus)
            {
                case WebAPI.Pricing.CouponsStatus.Valid:
                    result = KalturaCouponStatus.VALID;
                    break;
                case WebAPI.Pricing.CouponsStatus.NotExists:
                    result = KalturaCouponStatus.NOT_EXISTS;
                    break;
                case WebAPI.Pricing.CouponsStatus.AllreadyUsed:
                    result = KalturaCouponStatus.ALREADY_USED;
                    break;
                case WebAPI.Pricing.CouponsStatus.Expired:
                    result = KalturaCouponStatus.EXPIRED;
                    break;
                case WebAPI.Pricing.CouponsStatus.NotActive:
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
    }
}