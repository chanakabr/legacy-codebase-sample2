using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using WebAPI.Managers.Models;


namespace WebAPI.Mapping.ObjectsConvertor
{
    public class PricingMappings
    {
        public static void RegisterMappings()
        {
            //WebAPI.ConditionalAccess.BillingTransactions(WS) to  Models.ConditionalAccess.BillingTransactions(REST)
            Mapper.CreateMap<WebAPI.ConditionalAccess.Price, Models.Pricing.KalturaPrice>()
               .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.m_dPrice))
               .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3));

            // CouponsGroup
            Mapper.CreateMap<WebAPI.Pricing.CouponsGroup, Models.Pricing.KalturaCouponsGroup>()
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.m_dEndDate))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sGroupCode))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sGroupName))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.m_dStartDate))
               .ForMember(dest => dest.MaxUsesNumber, opt => opt.MapFrom(src => src.m_nMaxUseCountForCoupon))
               .ForMember(dest => dest.MaxUsesNumberOnRenewableSub, opt => opt.MapFrom(src => src.m_nMaxRecurringUsesCountForCoupon));

            // Price
            Mapper.CreateMap<WebAPI.Pricing.Price, Models.Pricing.KalturaPrice>()
               .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.m_dPrice))
               .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3));

            // PriceCode
            Mapper.CreateMap<WebAPI.Pricing.PriceCode, Models.Pricing.KalturaPriceDetails>()
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => src.m_sDescription))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjectID))
               .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.m_sCode))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrise));

            // DiscountModule
            Mapper.CreateMap<WebAPI.Pricing.DiscountModule, Models.Pricing.KalturaDiscountModule>()
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.m_dEndDate))
               .ForMember(dest => dest.Percent, opt => opt.MapFrom(src => src.m_dPercent))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.m_dStartDate));

            // UsageModule
            Mapper.CreateMap<WebAPI.Pricing.UsageModule, Models.Pricing.KalturaUsageModule>()
               .ForMember(dest => dest.CouponId, opt => opt.MapFrom(src => src.m_coupon_id))
               .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.m_tsMaxUsageModuleLifeCycle))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjectID))
               .ForMember(dest => dest.IsOfflinePlayback, opt => opt.MapFrom(src => src.m_bIsOfflinePlayBack))
               .ForMember(dest => dest.IsSubscriptionOnly, opt => opt.MapFrom(src => src.m_subscription_only))
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
            Mapper.CreateMap<Pricing.PreviewModule, Models.Pricing.KalturaPreviewModule>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nID))
               .ForMember(dest => dest.LifeCycle, opt => opt.MapFrom(src => src.m_tsFullLifeCycle))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
               .ForMember(dest => dest.NonRenewablePeriod, opt => opt.MapFrom(src => src.m_tsNonRenewPeriod));

            // ServiceObject to PremiumService
            Mapper.CreateMap<Pricing.ServiceObject, Models.ConditionalAccess.KalturaPremiumService>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            // LanguageContainer to TranslationContainer
            Mapper.CreateMap<Pricing.LanguageContainer, Models.General.KalturaTranslationContainer>()
               .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.m_sLanguageCode3))
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.m_sValue));

            // LanguageContainer to TranslationContainer
            Mapper.CreateMap<ConditionalAccess.LanguageContainer, Models.General.KalturaTranslationContainer>()
               .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.m_sLanguageCode3))
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.m_sValue));

            // BundleCodeContainer to SlimChannel
            Mapper.CreateMap<Pricing.BundleCodeContainer, Models.Catalog.KalturaBaseChannel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sCode))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName));

            // BundleCodeContainer to SlimChannel
            Mapper.CreateMap<ConditionalAccess.SubscriptionsPricesContainer, Models.Pricing.KalturaSubscriptionPrice>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sSubscriptionCode))
               .ForMember(dest => dest.PurchaseStatus, opt => opt.MapFrom(src => ConvertPriceReasonToPurchaseStatus(src.m_PriceReason)))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrice));

            // Subscription
            Mapper.CreateMap<WebAPI.Pricing.Subscription, Models.Pricing.KalturaSubscription>()
               .ForMember(dest => dest.IsInfiniteRenewal, opt => opt.MapFrom(src => src.m_bIsInfiniteRecurring))
               .ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.m_bIsRecurring))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.m_dStartDate))
               .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.m_fictivicMediaID))
               .ForMember(dest => dest.PremiumServices, opt => opt.MapFrom(src => src.m_lServices))
               .ForMember(dest => dest.PricePlans, opt => opt.MapFrom(src => src.m_MultiSubscriptionUsageModule))
               .ForMember(dest => dest.DlmId, opt => opt.MapFrom(src => src.m_nDomainLimitationModule))
               .ForMember(dest => dest.RenewalsNumber, opt => opt.MapFrom(src => src.m_nNumberOfRecPeriods))
               .ForMember(dest => dest.CouponsGroup, opt => opt.MapFrom(src => src.m_oCouponsGroup))
               .ForMember(dest => dest.DiscountModule, opt => opt.MapFrom(src => src.m_oExtDisountModule))
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
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_SubscriptionCode))
               .ForMember(dest => dest.UserTypes, opt => opt.MapFrom(src => src.m_UserTypes));

            // ItemPriceContainer to PPVItemPriceDetails
            Mapper.CreateMap<ConditionalAccess.ItemPriceContainer, Models.Pricing.KalturaPPVItemPriceDetails>()
               .ForMember(dest => dest.CollectionId, opt => opt.MapFrom(src => src.m_relevantCol))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.m_dtEndDate))
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
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.m_dtStartDate))
               .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.m_relevantSub));

            // ItemPriceContainer to PPVItemPriceDetails
            Mapper.CreateMap<ConditionalAccess.MediaFileItemPricesContainer, Models.Pricing.KalturaItemPrice>()
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.m_sProductCode))
               .ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.m_nMediaFileID))
               .ForMember(dest => dest.PPVPriceDetails, opt => opt.MapFrom(src => src.m_oItemPrices));

            // CouponData to CouponDetails
            Mapper.CreateMap<Pricing.CouponData, Models.Pricing.KalturaCoupon>()
               .ForMember(dest => dest.CouponsGroup, opt => opt.MapFrom(src => src.m_oCouponGroup))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ConvertCouponStatus(src.m_CouponStatus)));
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

        private static WebAPI.Models.Pricing.KalturaPurchaseStatus ConvertPriceReasonToPurchaseStatus(ConditionalAccess.PriceReason priceReason)
        {
            WebAPI.Models.Pricing.KalturaPurchaseStatus result;
            switch (priceReason)
            {
                case WebAPI.ConditionalAccess.PriceReason.PPVPurchased:
                    result = Models.Pricing.KalturaPurchaseStatus.ppv_purchased;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.Free:
                    result = Models.Pricing.KalturaPurchaseStatus.free;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.ForPurchaseSubscriptionOnly:
                    result = Models.Pricing.KalturaPurchaseStatus.for_purchase_subscription_only;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.SubscriptionPurchased:
                    result = Models.Pricing.KalturaPurchaseStatus.subscription_purchased;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.ForPurchase:
                    result = Models.Pricing.KalturaPurchaseStatus.for_purchase;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.SubscriptionPurchasedWrongCurrency:
                    result = Models.Pricing.KalturaPurchaseStatus.subscription_purchased_wrong_currency;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.PrePaidPurchased:
                    result = Models.Pricing.KalturaPurchaseStatus.pre_paid_purchased;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.GeoCommerceBlocked:
                    result = Models.Pricing.KalturaPurchaseStatus.geo_commerce_blocked;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.EntitledToPreviewModule:
                    result = Models.Pricing.KalturaPurchaseStatus.entitled_to_preview_module;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.FirstDeviceLimitation:
                    result = Models.Pricing.KalturaPurchaseStatus.first_device_limitation;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.CollectionPurchased:
                    result = Models.Pricing.KalturaPurchaseStatus.collection_purchased;
                    break;
                case WebAPI.ConditionalAccess.PriceReason.UserSuspended:
                    result = Models.Pricing.KalturaPurchaseStatus.user_suspended;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown purchase status");
            }

            return result;
        }

        private static WebAPI.Models.Pricing.KalturaCouponStatus ConvertCouponStatus(WebAPI.Pricing.CouponsStatus couponStatus)
        {
            WebAPI.Models.Pricing.KalturaCouponStatus result;

            switch (couponStatus)
            {
                case WebAPI.Pricing.CouponsStatus.Valid:
                    result = Models.Pricing.KalturaCouponStatus.valid;
                    break;
                case WebAPI.Pricing.CouponsStatus.NotExists:
                    result = Models.Pricing.KalturaCouponStatus.not_exists;
                    break;
                case WebAPI.Pricing.CouponsStatus.AllreadyUsed:
                    result = Models.Pricing.KalturaCouponStatus.already_used;
                    break;
                case WebAPI.Pricing.CouponsStatus.Expired:
                    result = Models.Pricing.KalturaCouponStatus.expired;
                    break;
                case WebAPI.Pricing.CouponsStatus.NotActive:
                    result = Models.Pricing.KalturaCouponStatus.not_active;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown coupon status");
            }

            return result;
        }
    }
}