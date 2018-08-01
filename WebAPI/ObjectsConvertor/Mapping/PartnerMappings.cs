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
using AutoMapper.Configuration;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class PartnerMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            // map KalturaBillingPartnerConfig to PartnerConfiguration
            cfg.CreateMap<KalturaBillingPartnerConfig, PartnerConfiguration>()
                .ForMember(dest => dest.Value, opt => opt.ResolveUsing(src => src.Value))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertPartnerConfigurationType(src.getType())));

            // map DeviceConcurrencyPriority to KalturaConcurrencyPartnerConfig
            cfg.CreateMap<DeviceConcurrencyPriority, KalturaConcurrencyPartnerConfig>()
                .ForMember(dest => dest.DeviceFamilyIds, opt => opt.ResolveUsing(src => string.Join(",", src.DeviceFamilyIds)))
                .ForMember(dest => dest.EvictionPolicy, opt => opt.ResolveUsing(src => ConvertDowngradePolicyToEvictionPolicy(src.PriorityOrder)));

            // map KalturaConcurrencyPartnerConfig to DeviceConcurrencyPriority
            cfg.CreateMap<KalturaConcurrencyPartnerConfig, DeviceConcurrencyPriority>()
                .ForMember(dest => dest.DeviceFamilyIds, opt => opt.ResolveUsing(src => src.GetDeviceFamilyIds()))
                .ForMember(dest => dest.PriorityOrder, opt => opt.ResolveUsing(src => ConvertEvictionPolicyToDowngradePolicy(src.EvictionPolicy)));
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
        
        private static KalturaEvictionPolicyType ConvertDowngradePolicyToEvictionPolicy(DowngradePolicy priorityOrder)
        {
            KalturaEvictionPolicyType result;

            switch (priorityOrder)
            {
                case DowngradePolicy.FIFO:
                    result = KalturaEvictionPolicyType.FIFO;
                    break;
                case DowngradePolicy.LIFO:
                    result = KalturaEvictionPolicyType.LIFO;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Downgrade Policy type");
            }

            return result;
        }

        private static DowngradePolicy ConvertEvictionPolicyToDowngradePolicy(KalturaEvictionPolicyType evictionPolicy)
        {
            DowngradePolicy result;

            switch (evictionPolicy)
            {
                case KalturaEvictionPolicyType.FIFO:
                    result = DowngradePolicy.FIFO;
                    break;
                case KalturaEvictionPolicyType.LIFO:
                    result = DowngradePolicy.LIFO;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Eviction Policy type");
            }

            return result;
        }
    }
}