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
        }
    }
}