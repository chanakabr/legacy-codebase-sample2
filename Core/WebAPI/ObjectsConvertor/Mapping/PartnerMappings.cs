using System;
using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Rules;
using AutoMapper.Configuration;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Partner;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class PartnerMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            // map KalturaBillingPartnerConfig to PartnerConfiguration
            cfg.CreateMap<KalturaBillingPartnerConfig, PartnerConfiguration>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertPartnerConfigurationType(src.getType())));

            // map DeviceConcurrencyPriority to KalturaConcurrencyPartnerConfig
            cfg.CreateMap<DeviceConcurrencyPriority, KalturaConcurrencyPartnerConfig>()
                .ForMember(dest => dest.DeviceFamilyIds, opt => opt.MapFrom(src => string.Join(",", src.DeviceFamilyIds)))
                .ForMember(dest => dest.EvictionPolicy, opt => opt.ResolveUsing(src => ConvertDowngradePolicyToEvictionPolicy(src.PriorityOrder)))
                .ForMember(dest => dest.ConcurrencyThresholdInSeconds, opt => opt.MapFrom(src => src.ConcurrencyThresholdInSeconds)); 

            // map KalturaConcurrencyPartnerConfig to DeviceConcurrencyPriority
            cfg.CreateMap<KalturaConcurrencyPartnerConfig, DeviceConcurrencyPriority>()
                .ForMember(dest => dest.DeviceFamilyIds, opt => opt.MapFrom(src => src.GetDeviceFamilyIds()))
                .ForMember(dest => dest.PriorityOrder, opt => opt.ResolveUsing(src => ConvertEvictionPolicyToDowngradePolicy(src.EvictionPolicy)))
                .ForMember(dest => dest.ConcurrencyThresholdInSeconds, opt => opt.MapFrom(src => src.ConcurrencyThresholdInSeconds));

            // map RollingDeviceRemovalData to KalturaRollingDeviceRemovalData
            cfg.CreateMap<RollingDeviceRemovalData, KalturaRollingDeviceRemovalData>()
                .ForMember(dest => dest.RollingDeviceRemovalFamilyIds, opt => opt.MapFrom(src => src.RollingDeviceRemovalFamilyIds != null ? 
                    string.Join(",", src.RollingDeviceRemovalFamilyIds) : string.Empty))
                .ForMember(dest => dest.RollingDeviceRemovalPolicy, opt => opt.ResolveUsing(src => ConvertRollingDevicePolicy(src.RollingDeviceRemovalPolicy)))
                
                ;

            // map KalturaRollingDeviceRemovalData to KalturaRollingDeviceRemovalData
            cfg.CreateMap<KalturaRollingDeviceRemovalData, RollingDeviceRemovalData>()
                .ForMember(dest => dest.RollingDeviceRemovalFamilyIds, opt => opt.MapFrom(src => src.GetRollingDeviceRemovalFamilyIds()))
                .ForMember(dest => dest.RollingDeviceRemovalPolicy, opt => opt.ResolveUsing(src => ConvertRollingDevicePolicy(src.RollingDeviceRemovalPolicy)))
                ;


            // map GeneralPartnerConfig to KalturaGeneralPartnerConfig
            cfg.CreateMap<GeneralPartnerConfig, KalturaGeneralPartnerConfig>()
                .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => src.PartnerName))
                .ForMember(dest => dest.MainLanguage, opt => opt.MapFrom(src => src.MainLanguage))
                .ForMember(dest => dest.SecondaryLanguages, opt => opt.MapFrom(src => src.SecondaryLanguages != null ? string.Join(",", src.SecondaryLanguages) : string.Empty))
                .ForMember(dest => dest.DeleteMediaPolicy, opt => opt.ResolveUsing(src => ConvertDeleteMediaPolicy(src.DeleteMediaPolicy)))
                .ForMember(dest => dest.MainCurrency, opt => opt.MapFrom(src => src.MainCurrency))
                .ForMember(dest => dest.SecondaryCurrencies, opt => opt.MapFrom(src => src.SecondaryCurrencies != null ? string.Join(",", src.SecondaryCurrencies) : string.Empty))
                .ForMember(dest => dest.DowngradePolicy, opt => opt.ResolveUsing(src => ConvertDowngradePolicy(src.DowngradePolicy)))
                .ForMember(dest => dest.MailSettings, opt => opt.MapFrom(src => src.MailSettings))
                .ForMember(dest => dest.DateFormat, opt => opt.MapFrom(src => src.DateFormat))
                .ForMember(dest => dest.HouseholdLimitationModule, opt => opt.MapFrom(src => src.HouseholdLimitationModule))
                .ForMember(dest => dest.EnableRegionFiltering, opt => opt.MapFrom(src => src.EnableRegionFiltering))
                .ForMember(dest => dest.DefaultRegion, opt => opt.MapFrom(src => src.DefaultRegion))
                .ForMember(dest => dest.RollingDeviceRemovalData, opt => opt.MapFrom(src => src.RollingDeviceRemovalData))
                ;

            // map KalturaGeneralPartnerConfig to GeneralPartnerConfig
            cfg.CreateMap<KalturaGeneralPartnerConfig, GeneralPartnerConfig>()
                .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => src.PartnerName))
                .ForMember(dest => dest.MainLanguage, opt => opt.MapFrom(src => src.MainLanguage))
                .ForMember(dest => dest.SecondaryLanguages, opt => opt.MapFrom(src => src.GetSecondaryLanguagesIds()))
                .ForMember(dest => dest.DeleteMediaPolicy, opt => opt.ResolveUsing(src => ConvertDeleteMediaPolicy(src.DeleteMediaPolicy)))
                .ForMember(dest => dest.MainCurrency, opt => opt.MapFrom(src => src.MainCurrency))
                .ForMember(dest => dest.SecondaryCurrencies, opt => opt.MapFrom(src => src.GetSecondaryCurrenciesIds()))
                .ForMember(dest => dest.DowngradePolicy, opt => opt.ResolveUsing(src => ConvertDowngradePolicy(src.DowngradePolicy)))
                .ForMember(dest => dest.MailSettings, opt => opt.MapFrom(src => src.MailSettings))
                .ForMember(dest => dest.DateFormat, opt => opt.MapFrom(src => src.DateFormat))
                .ForMember(dest => dest.HouseholdLimitationModule, opt => opt.MapFrom(src => src.HouseholdLimitationModule))
                .ForMember(dest => dest.EnableRegionFiltering, opt => opt.MapFrom(src => src.EnableRegionFiltering))
                .ForMember(dest => dest.DefaultRegion, opt => opt.MapFrom(src => src.DefaultRegion))
                .ForMember(dest => dest.RollingDeviceRemovalData, opt => opt.MapFrom(src => src.RollingDeviceRemovalData))
                .AfterMap((src, dest) => dest.SecondaryLanguages = src.SecondaryLanguages == null ? null : dest.SecondaryLanguages)
                .AfterMap((src, dest) => dest.SecondaryCurrencies = src.SecondaryCurrencies == null ? null : dest.SecondaryCurrencies)
                ;

            #region KalturaObjectVirtualAssetPartnerConfig
            // map ObjectVirtualAssetPartnerConfig to KalturaObjectVirtualAssetPartnerConfig
            cfg.CreateMap<ObjectVirtualAssetPartnerConfig, KalturaObjectVirtualAssetPartnerConfig>()
                .ForMember(dest => dest.ObjectVirtualAssets, opt => opt.MapFrom(src => src.ObjectVirtualAssets))
                ;

            // map KalturaObjectVirtualAssetPartnerConfig to ObjectVirtualAssetPartnerConfig
            cfg.CreateMap<KalturaObjectVirtualAssetPartnerConfig, ObjectVirtualAssetPartnerConfig>()
                .ForMember(dest => dest.ObjectVirtualAssets, opt => opt.MapFrom(src => src.ObjectVirtualAssets))
               ;

            cfg.CreateMap<KalturaObjectVirtualAssetInfo, ObjectVirtualAssetInfo>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.AssetStructId, opt => opt.MapFrom(src => src.AssetStructId))
                .ForMember(dest => dest.MetaId, opt => opt.MapFrom(src => src.MetaId))
                ;

            cfg.CreateMap<ObjectVirtualAssetInfo, KalturaObjectVirtualAssetInfo>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.AssetStructId, opt => opt.MapFrom(src => src.AssetStructId))
                .ForMember(dest => dest.MetaId, opt => opt.MapFrom(src => src.MetaId))
                ;

            cfg.CreateMap<KalturaObjectVirtualAssetInfoType, ObjectVirtualAssetInfoType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case KalturaObjectVirtualAssetInfoType.Segment:
                            return ObjectVirtualAssetInfoType.Segment;
                        case KalturaObjectVirtualAssetInfoType.Subscription:
                            return ObjectVirtualAssetInfoType.Subscription;
                        case KalturaObjectVirtualAssetInfoType.Category:
                            return ObjectVirtualAssetInfoType.Category;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown KalturaObjectVirtualAssetInfoType value : {0}", type.ToString()));
                    }                   
                });

            cfg.CreateMap<ObjectVirtualAssetInfoType, KalturaObjectVirtualAssetInfoType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case ObjectVirtualAssetInfoType.Segment:
                            return KalturaObjectVirtualAssetInfoType.Segment;
                        case ObjectVirtualAssetInfoType.Subscription:
                            return KalturaObjectVirtualAssetInfoType.Subscription;
                        case ObjectVirtualAssetInfoType.Category:
                            return KalturaObjectVirtualAssetInfoType.Category;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown ObjectVirtualAssetInfoType value : {0}", type.ToString()));
                    }
                });

            #endregion KalturaObjectVirtualAssetPartnerConfig

            cfg.CreateMap<KalturaCommercePartnerConfig, CommercePartnerConfig>()
                .ForMember(dest => dest.BookmarkEventThresholds, opt => opt.MapFrom(src => src.GetBookmarkEventThresholds()));

            cfg.CreateMap<CommercePartnerConfig, KalturaCommercePartnerConfig>()
                .ForMember(dest => dest.BookmarkEventThresholds, opt => opt.MapFrom(src => src.BookmarkEventThresholds));

            cfg.CreateMap<KeyValuePair<eTransactionType, int>, KalturaBookmarkEventThreshold>()
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Threshold, opt => opt.MapFrom(src => src.Value));

            //cfg.CreateMap<IGrouping<eTransactionType, DbCustomProperty>, DTOCustomProperty>()
            //.ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
            //.ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value)));

            cfg.CreateMap<KalturaPlaybackPartnerConfig, PlaybackPartnerConfig>()
               .ForMember(dest => dest.DefaultAdapters, opt => opt.MapFrom(src => src.DefaultAdapters));

            cfg.CreateMap<PlaybackPartnerConfig, KalturaPlaybackPartnerConfig>()
               .ForMember(dest => dest.DefaultAdapters, opt => opt.MapFrom(src => src.DefaultAdapters));

            cfg.CreateMap<KalturaDefaultPlaybackAdapters, DefaultPlaybackAdapters>()
               .ForMember(dest => dest.EpgAdapterId, opt => opt.MapFrom(src => src.EpgAdapterId))
               .ForMember(dest => dest.MediaAdapterId, opt => opt.MapFrom(src => src.MediaAdapterId))
               .ForMember(dest => dest.RecordingAdapterId, opt => opt.MapFrom(src => src.RecordingAdapterId));

            cfg.CreateMap<DefaultPlaybackAdapters, KalturaDefaultPlaybackAdapters>()
               .ForMember(dest => dest.EpgAdapterId, opt => opt.MapFrom(src => src.EpgAdapterId))
               .ForMember(dest => dest.MediaAdapterId, opt => opt.MapFrom(src => src.MediaAdapterId))
               .ForMember(dest => dest.RecordingAdapterId, opt => opt.MapFrom(src => src.RecordingAdapterId));
        }

        private static KalturaRollingDevicePolicy ConvertRollingDevicePolicy(
            RollingDevicePolicy? srcRollingDeviceRemovalPolicy)
        {
            KalturaRollingDevicePolicy res;
            switch (srcRollingDeviceRemovalPolicy)
            {
                case RollingDevicePolicy.NONE:
                    res = KalturaRollingDevicePolicy.NONE;
                    break;
                case RollingDevicePolicy.LIFO:
                    res = KalturaRollingDevicePolicy.LIFO;
                    break;
                case RollingDevicePolicy.FIFO:
                    res = KalturaRollingDevicePolicy.FIFO;
                    break;
                case RollingDevicePolicy.ACTIVE_DEVICE_ASCENDING:
                    res = KalturaRollingDevicePolicy.ACTIVE_DEVICE_ASCENDING;
                    break;
                default:
                    throw new ClientException((int) StatusCode.Error, "Unknown partner configuration type");
            }

            return res;
        }
        private static RollingDevicePolicy ConvertRollingDevicePolicy(
            KalturaRollingDevicePolicy? srcRollingDeviceRemovalPolicy)
        {
            RollingDevicePolicy res;
            switch (srcRollingDeviceRemovalPolicy)
            {
                case KalturaRollingDevicePolicy.NONE:
                    res = RollingDevicePolicy.NONE;
                    break;
                case KalturaRollingDevicePolicy.LIFO:
                    res = RollingDevicePolicy.LIFO;
                    break;
                case KalturaRollingDevicePolicy.FIFO:
                    res = RollingDevicePolicy.FIFO;
                    break;
                case KalturaRollingDevicePolicy.ACTIVE_DEVICE_ASCENDING:
                    res = RollingDevicePolicy.ACTIVE_DEVICE_ASCENDING;
                    break;
                default:
                    throw new ClientException((int) StatusCode.Error, "Unknown partner configuration type");
            }

            return res;
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

        private static KalturaDeleteMediaPolicy? ConvertDeleteMediaPolicy(DeleteMediaPolicy? deleteMediaPolicy)
        {
            KalturaDeleteMediaPolicy? result = null;

            if (deleteMediaPolicy.HasValue)
            {
                switch (deleteMediaPolicy)
                {
                    case DeleteMediaPolicy.Delete:
                        result = KalturaDeleteMediaPolicy.Delete;
                        break;
                    case DeleteMediaPolicy.Disable:
                        result = KalturaDeleteMediaPolicy.Disable;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown DeleteMediaPolicy");
                }
            }

            return result;
        }

        private static DeleteMediaPolicy? ConvertDeleteMediaPolicy(KalturaDeleteMediaPolicy? deleteMediaPolicy)
        {
            DeleteMediaPolicy? result = null;

            if (deleteMediaPolicy.HasValue)
            {
                switch (deleteMediaPolicy)
                {
                    case KalturaDeleteMediaPolicy.Delete:
                        result = DeleteMediaPolicy.Delete;
                        break;
                    case KalturaDeleteMediaPolicy.Disable:
                        result = DeleteMediaPolicy.Disable;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown DeleteMediaPolicy");
                }
            }

            return result;
        }

        private static KalturaDowngradePolicy? ConvertDowngradePolicy(DowngradePolicy? downgradePolicy)
        {
            KalturaDowngradePolicy? result = null;

            if (downgradePolicy.HasValue)
            {
                switch (downgradePolicy)
                {
                    case DowngradePolicy.FIFO:
                        result = KalturaDowngradePolicy.FIFO;
                        break;
                    case DowngradePolicy.LIFO:
                        result = KalturaDowngradePolicy.LIFO;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown DowngradePolicy");
                }
            }

            return result;
        }

        private static DowngradePolicy? ConvertDowngradePolicy(KalturaDowngradePolicy? downgradePolicy)
        {
            DowngradePolicy? result = null;

            if (downgradePolicy.HasValue)
            {
                switch (downgradePolicy)
                {
                    case KalturaDowngradePolicy.FIFO:
                        result = DowngradePolicy.FIFO;
                        break;
                    case KalturaDowngradePolicy.LIFO:
                        result = DowngradePolicy.LIFO;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown DowngradePolicy");
                }
            }

            return result;
        }
    }
}