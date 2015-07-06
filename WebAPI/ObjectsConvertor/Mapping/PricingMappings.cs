using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;


namespace WebAPI.Mapping.ObjectsConvertor
{
    public class PricingMappings
    {
        public static void RegisterMappings()
        {
            //WebAPI.ConditionalAccess.BillingTransactions(WS) to  Models.ConditionalAccess.BillingTransactions(REST)
            Mapper.CreateMap<Pricing.Price, Models.Pricing.Price>()
               .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.m_dPrice))
               .ForMember(dest => dest.currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3));

            // Subscription
            Mapper.CreateMap<Pricing.Subscription, Models.Pricing.Subscription>()
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
               .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_ProductCode))
               .ForMember(dest => dest.Channels, opt => opt.MapFrom(src => ConvertBundleCodeContainerToDictionary(src.m_sCodes)))
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => ConvertLanguageContainerToDictionary(src.m_sDescription)))
               .ForMember(dest => dest.FileTypes, opt => opt.MapFrom(src => src.m_sFileTypes))
               .ForMember(dest => dest.Names, opt => opt.MapFrom(src => ConvertLanguageContainerToDictionary(src.m_sName)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_SubscriptionCode))
               .ForMember(dest => dest.UserTypes, opt => opt.MapFrom(src => src.m_UserTypes));

            // CouponsGroup
            Mapper.CreateMap<Pricing.CouponsGroup, Models.Pricing.CouponsGroup>()
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => ConvertLanguageContainerToDictionary(src.m_sDescription)))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.m_dEndDate))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sGroupCode))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sGroupName))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.m_dStartDate))
               .ForMember(dest => dest.MaxUsesNumber, opt => opt.MapFrom(src => src.m_nMaxUseCountForCoupon))
               .ForMember(dest => dest.MaxUsesNumberOnRenewableSub, opt => opt.MapFrom(src => src.m_nMaxRecurringUsesCountForCoupon));

            // PriceCode
            Mapper.CreateMap<Pricing.PriceCode, Models.Pricing.PriceCode>()
               .ForMember(dest => dest.Descriptions, opt => opt.MapFrom(src => ConvertLanguageContainerToDictionary(src.m_sDescription)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjectID))
               .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.m_sCode))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.m_oPrise));

            // Price
            Mapper.CreateMap<Pricing.Price, Models.Pricing.Price>()
               .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.m_dPrice))
               .ForMember(dest => dest.currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3));

            // DiscountModule
            Mapper.CreateMap<Pricing.DiscountModule, Models.Pricing.DiscountModule>()
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.m_dEndDate))
               .ForMember(dest => dest.Percent, opt => opt.MapFrom(src => src.m_dPercent))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.m_dStartDate));

            // UsageModule
            Mapper.CreateMap<Pricing.UsageModule, Models.Pricing.UsageModule>()
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
            Mapper.CreateMap<Pricing.UserType, Models.Users.UserType>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // PreviewModule
            Mapper.CreateMap<Pricing.PreviewModule, Models.Pricing.PreviewModule>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nID))
               .ForMember(dest => dest.LifeCycle, opt => opt.MapFrom(src => src.m_tsFullLifeCycle))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
               .ForMember(dest => dest.NonRenewablePeriod, opt => opt.MapFrom(src => src.m_tsNonRenewPeriod));

            // ServiceObject
            Mapper.CreateMap<Pricing.ServiceObject, Models.ConditionalAccess.PremiumService>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }

        private static Dictionary<string, string> ConvertBundleCodeContainerToDictionary(WebAPI.Pricing.BundleCodeContainer[] container)
        {
            Dictionary<string, string> result = null;

            if (container != null)
            {
                result = new Dictionary<string, string>();
                foreach (var item in container)
                {
                    result.Add(item.m_sCode, item.m_sName);
                }
            }
            return result;
        }

        private static Dictionary<string, string> ConvertLanguageContainerToDictionary(WebAPI.Pricing.LanguageContainer[] container)
        {
            Dictionary<string, string> result = null;

            if (container != null)
            {
                result = new Dictionary<string, string>();
                foreach (var item in container)
                {
                    result.Add(item.m_sLanguageCode3, item.m_sValue);
                }
            }
            return result;
        }
    }
}