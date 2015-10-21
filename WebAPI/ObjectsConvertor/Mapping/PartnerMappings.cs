using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Billing;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Partner;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class PartnerMappings
    {
        public static void RegisterMappings()
        {
            Mapper.CreateMap<WebAPI.Models.Partner.KalturaBillingPartnerConfig, PartnerConfiguration >()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertPartnerConfigurationType(src.Type)))
                ;
        }

        private static PartnerConfigurationType ConvertPartnerConfigurationType(KalturaPartnerConfigurationHolder partnerConfigurationHolder)
        {
            PartnerConfigurationType result;

            switch (partnerConfigurationHolder.type)
            {
                case KalturaPartnerConfigurationType.DefaultPaymentGateway:
                    result = PartnerConfigurationType.DefaultPaymentGateway;
                    break;
                case KalturaPartnerConfigurationType.EnablePaymentGatewaySelection:
                    result = PartnerConfigurationType.EnablePaymentGatewaySelection;
                    break;
                case KalturaPartnerConfigurationType.OSSAdapter:
                    result = PartnerConfigurationType.OSSAdapter;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown partner configuration type");
            }

            return result;
        }
    }
}