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
               .ForMember(dest => dest.currency, opt => opt.MapFrom(src => src.m_oCurrency));

            Mapper.CreateMap<Pricing.Currency, Models.Pricing.Currency>()
               .ForMember(dest => dest.currencyCD2, opt => opt.MapFrom(src => src.m_sCurrencyCD2))
               .ForMember(dest => dest.currencyCD3, opt => opt.MapFrom(src => src.m_sCurrencyCD3))
               .ForMember(dest => dest.currencySign, opt => opt.MapFrom(src => src.m_sCurrencySign))
               .ForMember(dest => dest.currencyID, opt => opt.MapFrom(src => src.m_nCurrencyID));
        }
    }
}