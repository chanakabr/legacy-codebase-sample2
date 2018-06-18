using ApiObjects.Billing;
using ApiObjects.Rules;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Partner;
using ApiObjects;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class PartnerMappings
    {
        public static void RegisterMappings()
        {
            // map KalturaBillingPartnerConfig to PartnerConfiguration
            Mapper.CreateMap<KalturaBillingPartnerConfig, PartnerConfiguration>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertPartnerConfigurationType(src.getType())));

            // map DeviceConcurrencyPriority to KalturaConcurrencyPartnerConfig
            Mapper.CreateMap<DeviceConcurrencyPriority, KalturaConcurrencyPartnerConfig>()
                .ForMember(dest => dest.DeviceFamilyIds, opt => opt.MapFrom(src => string.Join(",", src.DeviceFamilyIds)))
                .ForMember(dest => dest.PriorityByFIFO, opt => opt.MapFrom(src => IsPriorityOrderByFIFO(src.PriorityOrder)));

            // map KalturaConcurrencyPartnerConfig to DeviceConcurrencyPriority
            Mapper.CreateMap<KalturaConcurrencyPartnerConfig, DeviceConcurrencyPriority>()
                .ForMember(dest => dest.DeviceFamilyIds, opt => opt.MapFrom(src => src.GetDeviceFamilyIds()))
                .ForMember(dest => dest.PriorityOrder, opt => opt.MapFrom(src => ConvertToDowngradePolicy(src.PriorityByFIFO)));
        }
        
        private static PartnerConfigurationType ConvertPartnerConfigurationType(KalturaPartnerConfigurationType type)
        {
            PartnerConfigurationType result;

            switch (type)
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
        
        private static bool IsPriorityOrderByFIFO(DowngradePolicy priorityOrder)
        {
            bool result;

            switch (priorityOrder)
            {
                case DowngradePolicy.FIFO:
                    result = true;
                    break;
                case DowngradePolicy.LIFO:
                    result = false;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Downgrade Policy type");
            }

            return result;
        }

        private static DowngradePolicy ConvertToDowngradePolicy(bool isPriorityByFIFO)
        {
            if (isPriorityByFIFO)
            {
                return DowngradePolicy.FIFO;
            }
            else
            {
                return DowngradePolicy.LIFO;
            }
        }
    }
}