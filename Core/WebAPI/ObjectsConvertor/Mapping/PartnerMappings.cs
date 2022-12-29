using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Rules;
using AutoMapper.Configuration;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Partner;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using TVinciShared;
using DAL.DTO;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.ModelsValidators;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class PartnerMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            // map KalturaBillingPartnerConfig to PartnerConfiguration
            cfg.CreateMap<KalturaBillingPartnerConfig, PartnerConfiguration>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertPartnerConfigurationType(src.GetConfigurationType())));

            // map DeviceConcurrencyPriority to KalturaConcurrencyPartnerConfig
            cfg.CreateMap<DeviceConcurrencyPriority, KalturaConcurrencyPartnerConfig>()
                .ForMember(dest => dest.DeviceFamilyIds, opt => opt.MapFrom(src => src.GetDeviceFamilyIds()))
                .ForMember(dest => dest.EvictionPolicy, opt => opt.ResolveUsing(src => ConvertDowngradePolicyToEvictionPolicy(src.PriorityOrder)))
                .ForMember(dest => dest.ConcurrencyThresholdInSeconds, opt => opt.MapFrom(src => src.ConcurrencyThresholdInSeconds))
                .ForMember(dest => dest.RevokeOnDeviceDelete, opt => opt.MapFrom(src => src.RevokeOnDeviceDelete))
                .ForMember(dest => dest.ExcludeFreeContentFromConcurrency, opt => opt.MapFrom(src => src.ExcludeFreeContentFromConcurrency))
                ;

            // map KalturaConcurrencyPartnerConfig to DeviceConcurrencyPriority
            cfg.CreateMap<KalturaConcurrencyPartnerConfig, DeviceConcurrencyPriority>()
                .ForMember(dest => dest.DeviceFamilyIds, opt => opt.MapFrom(src => src.GetDeviceFamilyIds()))
                .AfterMap((src, dest) => dest.DeviceFamilyIds = src.DeviceFamilyIds != null ? dest.DeviceFamilyIds : null)
                .ForMember(dest => dest.PriorityOrder, opt => opt.ResolveUsing(src => ConvertEvictionPolicyToDowngradePolicy(src.EvictionPolicy)))
                .ForMember(dest => dest.ConcurrencyThresholdInSeconds, opt => opt.MapFrom(src => src.ConcurrencyThresholdInSeconds))
                .ForMember(dest => dest.RevokeOnDeviceDelete, opt => opt.MapFrom(src => src.RevokeOnDeviceDelete))
                .ForMember(dest => dest.ExcludeFreeContentFromConcurrency, opt => opt.MapFrom(src => src.ExcludeFreeContentFromConcurrency))
                ;
            cfg.CreateMap<CustomFieldsPartnerConfig, KalturaCustomFieldsPartnerConfiguration>()
                .ForMember(dest => dest.MetaSystemNameInsteadOfAliasList, opt => opt.MapFrom(src => src.ClientTagsToIgnoreMetaAlias != null ?
                    string.Join(",", src.ClientTagsToIgnoreMetaAlias) : string.Empty))
            ;

            cfg.CreateMap<KalturaCustomFieldsPartnerConfiguration, CustomFieldsPartnerConfig>()
                .ForMember(dest => dest.ClientTagsToIgnoreMetaAlias, opt => opt.MapFrom(src => src.GetMetaSystemNameInsteadOfAliasList()))
            ;

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

            // map BEO-9373 ResetPasswordPartnerConfigTemplate
            cfg.CreateMap<ResetPasswordPartnerConfigTemplate, KalturaResetPasswordPartnerConfigTemplate>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                ;

            cfg.CreateMap<ResetPasswordPartnerConfig, KalturaResetPasswordPartnerConfig>()
                .ForMember(dest => dest.TemplateListLabel, opt => opt.MapFrom(src => src.TemplateListLabel))
                .ForMember(dest => dest.Templates, opt => opt.MapFrom(src => src.Templates))
                .AfterMap((src, dest) => dest.Templates = src.Templates != null ? dest.Templates : null)
                ;

            cfg.CreateMap<KalturaResetPasswordPartnerConfigTemplate, ResetPasswordPartnerConfigTemplate>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                ;

            cfg.CreateMap<KalturaResetPasswordPartnerConfig, ResetPasswordPartnerConfig>()
                .ForMember(dest => dest.TemplateListLabel, opt => opt.MapFrom(src => src.TemplateListLabel))
                .ForMember(dest => dest.Templates, opt => opt.MapFrom(src => src.Templates))
                .AfterMap((src, dest) => dest.Templates = src.Templates != null ? dest.Templates : null)
                ;

            cfg.CreateMap<OpcPartnerConfig, KalturaOpcPartnerConfiguration>()
                .ForMember(dest => dest.ResetPassword, opt => opt.MapFrom(src => src.ResetPassword))
                ;

            cfg.CreateMap<KalturaOpcPartnerConfiguration, OpcPartnerConfig>()
                .ForMember(dest => dest.ResetPassword, opt => opt.MapFrom(src => src.ResetPassword))
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
                .ForMember(dest => dest.LinearWatchHistoryThreshold, opt => opt.MapFrom(src => src.LinearWatchHistoryThreshold))
                .ForMember(dest => dest.FinishedPercentThreshold, opt => opt.MapFrom(src => src.FinishedPercentThreshold))
                .ForMember(dest => dest.SuspensionProfileInheritanceType, opt => opt.ResolveUsing(src => 
                                    ConvertSuspensionProfileInheritanceType(src.SuspensionProfileInheritanceType)))
                .ForMember(dest => dest.AllowDeviceMobility, opt => opt.MapFrom(src => src.AllowDeviceMobility))
                .ForMember(dest => dest.DowngradePriorityFamilyIds, opt => opt.MapFrom(src => src.DowngradePriorityFamilyIds != null ?
                    string.Join(",", src.DowngradePriorityFamilyIds) : string.Empty))
                .ForMember(dest => dest.EnableMultiLcns, opt => opt.MapFrom(src => src.EnableMultiLcns))
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
                .ForMember(dest => dest.LinearWatchHistoryThreshold, opt => opt.MapFrom(src => src.LinearWatchHistoryThreshold))
                .ForMember(dest => dest.FinishedPercentThreshold, opt => opt.MapFrom(src => src.FinishedPercentThreshold))
                .AfterMap((src, dest) => dest.SecondaryLanguages = src.SecondaryLanguages == null ? null : dest.SecondaryLanguages)
                .AfterMap((src, dest) => dest.SecondaryCurrencies = src.SecondaryCurrencies == null ? null : dest.SecondaryCurrencies)
                .ForMember(dest => dest.SuspensionProfileInheritanceType, opt => opt.ResolveUsing(src => 
                                    ConvertSuspensionProfileInheritanceType(src.SuspensionProfileInheritanceType)))
                .ForMember(dest => dest.AllowDeviceMobility, opt => opt.MapFrom(src => src.AllowDeviceMobility))
                .ForMember(dest => dest.DowngradePriorityFamilyIds, opt => opt.MapFrom(src => src.GetDowngradePriorityFamilyIds()))
                .ForMember(dest => dest.EnableMultiLcns, opt => opt.MapFrom(src => src.EnableMultiLcns))
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
                .ForMember(dest => dest.ExtendedTypes, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ConvertSerializeableDictionary(src.ExtendedTypes, true)))
                .AfterMap((src, dest) => dest.ExtendedTypes = src.ExtendedTypes != null ? dest.ExtendedTypes : null)
                ;

            cfg.CreateMap<ObjectVirtualAssetInfo, KalturaObjectVirtualAssetInfo>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.AssetStructId, opt => opt.MapFrom(src => src.AssetStructId))
                .ForMember(dest => dest.MetaId, opt => opt.MapFrom(src => src.MetaId))
                .ForMember(dest => dest.ExtendedTypes, opt => opt.MapFrom(src => src.ExtendedTypes != null ? src.ExtendedTypes.ToDictionary(k => k.Key, v => v.Value) : null))
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
                        case KalturaObjectVirtualAssetInfoType.Tvod:
                            return ObjectVirtualAssetInfoType.Tvod;
                        case KalturaObjectVirtualAssetInfoType.Boxset:
                            return ObjectVirtualAssetInfoType.Boxset;
                        case KalturaObjectVirtualAssetInfoType.PAGO:
                            return ObjectVirtualAssetInfoType.PAGO;

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
                        case ObjectVirtualAssetInfoType.Tvod:
                            return KalturaObjectVirtualAssetInfoType.Tvod;
                        case ObjectVirtualAssetInfoType.Boxset:
                            return KalturaObjectVirtualAssetInfoType.Boxset;
                        case ObjectVirtualAssetInfoType.PAGO:
                            return KalturaObjectVirtualAssetInfoType.PAGO;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown ObjectVirtualAssetInfoType value : {0}", type.ToString()));
                    }
                });

            #endregion KalturaObjectVirtualAssetPartnerConfig

            cfg.CreateMap<KalturaCommercePartnerConfig, CommercePartnerConfig>()
                .ForMember(dest => dest.BookmarkEventThresholds, opt => opt.MapFrom(src => src.GetBookmarkEventThresholds()))
                .ForMember(dest => dest.KeepSubscriptionAddOns, opt => opt.MapFrom(src => src.KeepSubscriptionAddOns))
                .ForMember(dest => dest.ProgramAssetEntitlementPaddingStart, opt => opt.MapFrom(src => src.ProgramAssetEntitlementPaddingStart))
                .ForMember(dest => dest.ProgramAssetEntitlementPaddingEnd, opt => opt.MapFrom(src => src.ProgramAssetEntitlementPaddingEnd))
                ;

            cfg.CreateMap<CommercePartnerConfig, KalturaCommercePartnerConfig>()
                .ForMember(dest => dest.BookmarkEventThresholds, opt => opt.MapFrom(src => src.BookmarkEventThresholds))
                .ForMember(dest => dest.KeepSubscriptionAddOns, opt => opt.MapFrom(src => src.KeepSubscriptionAddOns))
                .ForMember(dest => dest.ProgramAssetEntitlementPaddingStart, opt => opt.MapFrom(src => src.ProgramAssetEntitlementPaddingStart))
                .ForMember(dest => dest.ProgramAssetEntitlementPaddingEnd, opt => opt.MapFrom(src => src.ProgramAssetEntitlementPaddingEnd))
                ;

            cfg.CreateMap<KeyValuePair<eTransactionType, int>, KalturaBookmarkEventThreshold>()
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Threshold, opt => opt.MapFrom(src => src.Value));

            cfg.CreateMap<KalturaPlaybackPartnerConfig, PlaybackPartnerConfig>()
               .ForMember(dest => dest.DefaultAdapters, opt => opt.MapFrom(src => src.DefaultAdapters))
               ;

            cfg.CreateMap<PlaybackPartnerConfig, KalturaPlaybackPartnerConfig>()
               .ForMember(dest => dest.DefaultAdapters, opt => opt.MapFrom(src => src.DefaultAdapters))
               ;


            cfg.CreateMap<KalturaEncryptionType, EncryptionType>().ConvertUsing(value =>
                {
                    switch (value)
                    {
                        case KalturaEncryptionType.AES256: return EncryptionType.aes256;
                        default: throw new ClientException((int)StatusCode.Error, "Unknown encryption type");
                    }
                });
            cfg.CreateMap<EncryptionType, KalturaEncryptionType>().ConvertUsing(value =>
            {
                switch (value)
                {
                    case EncryptionType.aes256: return KalturaEncryptionType.AES256;
                    default: throw new ClientException((int)StatusCode.Error, "Unknown encryption type");
                }
            });
            cfg.CreateMap<KalturaSecurityPartnerConfig, SecurityPartnerConfig>();
            cfg.CreateMap<SecurityPartnerConfig, KalturaSecurityPartnerConfig>();
            cfg.CreateMap<KalturaDataEncryption, DataEncryption>();
            cfg.CreateMap<DataEncryption, KalturaDataEncryption>();
            cfg.CreateMap<KalturaEncryption, Encryption>();
            cfg.CreateMap<Encryption, KalturaEncryption>();

            cfg.CreateMap<KalturaDefaultPlaybackAdapters, DefaultPlaybackAdapters>()
               .ForMember(dest => dest.EpgAdapterId, opt => opt.MapFrom(src => src.EpgAdapterId))
               .ForMember(dest => dest.MediaAdapterId, opt => opt.MapFrom(src => src.MediaAdapterId))
               .ForMember(dest => dest.RecordingAdapterId, opt => opt.MapFrom(src => src.RecordingAdapterId));

            cfg.CreateMap<DefaultPlaybackAdapters, KalturaDefaultPlaybackAdapters>()
               .ForMember(dest => dest.EpgAdapterId, opt => opt.MapFrom(src => src.EpgAdapterId))
               .ForMember(dest => dest.MediaAdapterId, opt => opt.MapFrom(src => src.MediaAdapterId))
               .ForMember(dest => dest.RecordingAdapterId, opt => opt.MapFrom(src => src.RecordingAdapterId));

            cfg.CreateMap<KalturaPaymentPartnerConfig, PaymentPartnerConfig>()
               .ForMember(dest => dest.UnifiedBillingCycles, opt => opt.MapFrom(src => src.UnifiedBillingCycles));

            cfg.CreateMap<PaymentPartnerConfig, KalturaPaymentPartnerConfig>()
               .ForMember(dest => dest.UnifiedBillingCycles, opt => opt.MapFrom(src => src.UnifiedBillingCycles));

            cfg.CreateMap<KalturaUnifiedBillingCycle, UnifiedBillingCycleObject>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
               .ForMember(dest => dest.PaymentGatewayId, opt => opt.MapFrom(src => src.PaymentGatewayId))
               .ForMember(dest => dest.IgnorePartialBilling, opt => opt.MapFrom(src => src.IgnorePartialBilling));

            cfg.CreateMap<UnifiedBillingCycleObject, KalturaUnifiedBillingCycle>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
               .ForMember(dest => dest.PaymentGatewayId, opt => opt.MapFrom(src => src.PaymentGatewayId))
               .ForMember(dest => dest.IgnorePartialBilling, opt => opt.MapFrom(src => src.IgnorePartialBilling)); 

            cfg.CreateMap<KalturaDuration, Duration>()
              .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit))
              .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            cfg.CreateMap<Duration, KalturaDuration>()
              .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit))
              .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.TvmCode))
              .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            cfg.CreateMap<KalturaDurationUnit, DurationUnit>()
               .ConvertUsing(kalturaDurationUnit =>
               {
                   switch (kalturaDurationUnit)
                   {
                       case KalturaDurationUnit.Minutes:
                           return DurationUnit.Minutes;
                       case KalturaDurationUnit.Hours:
                           return DurationUnit.Hours;
                       case KalturaDurationUnit.Days:
                           return DurationUnit.Days;
                       case KalturaDurationUnit.Months:
                           return DurationUnit.Months;
                       case KalturaDurationUnit.Years:
                           return DurationUnit.Years;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown KalturaDurationUnit value : {kalturaDurationUnit.ToString()}");
                   }
               });

            cfg.CreateMap<DurationUnit, KalturaDurationUnit>()
               .ConvertUsing(durationUnit =>
               {
                   switch (durationUnit)
                   {
                       case DurationUnit.Minutes:
                           return KalturaDurationUnit.Minutes;
                       case DurationUnit.Hours:
                           return KalturaDurationUnit.Hours;
                       case DurationUnit.Days:
                           return KalturaDurationUnit.Days;
                       case DurationUnit.Months:
                           return KalturaDurationUnit.Months;
                       case DurationUnit.Years:
                           return KalturaDurationUnit.Years;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown DurationUnit value : {durationUnit.ToString()}");
                   }
               });

            cfg.CreateMap<KalturaCatalogPartnerConfig, CatalogPartnerConfig>()
               .ForMember(dest => dest.SingleMultilingualMode, opt => opt.MapFrom(src => src.SingleMultilingualMode))
               .ForMember(dest => dest.EpgMultilingualFallbackSupport, opt => opt.MapFrom(src => src.EpgMultilingualFallbackSupport))
               .ForMember(dest => dest.CategoryManagement, opt => opt.MapFrom(src => src.CategoryManagement))
               .ForMember(dest => dest.UploadExportDatalake, opt => opt.MapFrom(src => src.UploadExportDatalake))
               .ForMember(dest => dest.ShopMarkerMetaId, opt => opt.MapFrom(src => src.ShopMarkerMetaId));               

            cfg.CreateMap<CatalogPartnerConfig, KalturaCatalogPartnerConfig>()
               .ForMember(dest => dest.SingleMultilingualMode, opt => opt.MapFrom(src => src.SingleMultilingualMode))
               .ForMember(dest => dest.EpgMultilingualFallbackSupport, opt => opt.MapFrom(src => src.EpgMultilingualFallbackSupport))
               .ForMember(dest => dest.CategoryManagement, opt => opt.MapFrom(src => src.CategoryManagement))
               .ForMember(dest => dest.UploadExportDatalake, opt => opt.MapFrom(src => src.UploadExportDatalake))
               .ForMember(dest => dest.ShopMarkerMetaId, opt => opt.MapFrom(src => src.ShopMarkerMetaId));

            cfg.CreateMap<KalturaCategoryManagement, ApiObjects.CategoryManagement>()
                .ForMember(dest => dest.DefaultCategoryTree, opt => opt.MapFrom(src => src.DefaultCategoryTreeId))
                .ForMember(dest => dest.DeviceFamilyToCategoryTree, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ConvertSerializeableDictionary(src.DeviceFamilyToCategoryTree, true)))
                .AfterMap((src, dest) => dest.DeviceFamilyToCategoryTree = src.DeviceFamilyToCategoryTree != null ? dest.DeviceFamilyToCategoryTree : null);

            cfg.CreateMap<ApiObjects.CategoryManagement, KalturaCategoryManagement>()
                .ForMember(dest => dest.DefaultCategoryTreeId, opt => opt.MapFrom(src => src.DefaultCategoryTree))
                .ForMember(dest => dest.DeviceFamilyToCategoryTree, opt => opt.MapFrom(src => src.DeviceFamilyToCategoryTree != null ? src.DeviceFamilyToCategoryTree.ToDictionary(k => k.Key, v => v.Value) : null));

            cfg.CreateMap<KalturaBasePartnerConfiguration, Group>()
              .ForMember(dest => dest.KSExpirationSeconds, opt => opt.MapFrom(src => src.KsExpirationSeconds))
              .ForMember(dest => dest.AppTokenSessionMaxDurationSeconds, opt => opt.MapFrom(src => src.AppTokenSessionMaxDurationSeconds))
              .ForMember(dest => dest.AnonymousKSExpirationSeconds, opt => opt.MapFrom(src => src.AnonymousKSExpirationSeconds))
              .ForMember(dest => dest.RefreshExpirationForPinLoginSeconds, opt => opt.MapFrom(src => src.RefreshExpirationForPinLoginSeconds))
              .ForMember(dest => dest.AppTokenMaxExpirySeconds, opt => opt.MapFrom(src => src.AppTokenMaxExpirySeconds))
              .ForMember(dest => dest.AutoRefreshAppToken, opt => opt.MapFrom(src => src.AutoRefreshAppToken))
              .ForMember(dest => dest.UploadTokenExpirySeconds, opt => opt.MapFrom(src => src.UploadTokenExpirySeconds))
              .ForMember(dest => dest.ApptokenUserValidationDisabled, opt => opt.MapFrom(src => src.ApptokenUserValidationDisabled));

            cfg.CreateMap<Group, KalturaBasePartnerConfiguration>()
              .ForMember(dest => dest.KsExpirationSeconds, opt => opt.MapFrom(src => src.KSExpirationSeconds))
              .ForMember(dest => dest.AppTokenSessionMaxDurationSeconds, opt => opt.MapFrom(src => src.AppTokenSessionMaxDurationSeconds))
              .ForMember(dest => dest.AnonymousKSExpirationSeconds, opt => opt.MapFrom(src => src.AnonymousKSExpirationSeconds))
              .ForMember(dest => dest.RefreshExpirationForPinLoginSeconds, opt => opt.MapFrom(src => src.RefreshExpirationForPinLoginSeconds))
              .ForMember(dest => dest.AppTokenMaxExpirySeconds, opt => opt.MapFrom(src => src.AppTokenMaxExpirySeconds))
              .ForMember(dest => dest.AutoRefreshAppToken, opt => opt.MapFrom(src => src.AutoRefreshAppToken))
              .ForMember(dest => dest.UploadTokenExpirySeconds, opt => opt.MapFrom(src => src.UploadTokenExpirySeconds))
              .ForMember(dest => dest.ApptokenUserValidationDisabled, opt => opt.MapFrom(src => src.ApptokenUserValidationDisabled));

            // PartnerSetup to KalturaPartnerSetup
            cfg.CreateMap<Partner, KalturaPartner>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate.ToUtcUnixTimestampSeconds()))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate.ToUtcUnixTimestampSeconds()));

            cfg.CreateMap<KalturaPartner, Partner>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<KalturaPartnerSetup, PartnerSetup>()
                .ForMember(dest => dest.AdminUsername, opt => opt.MapFrom(src => src.AdminUsername))
                .ForMember(dest => dest.AdminPassword, opt => opt.MapFrom(src => src.AdminPassword));

            cfg.CreateMap<DefaultParentalSettingsPartnerConfig, KalturaDefaultParentalSettingsPartnerConfig>()
                  .ForMember(dest => dest.DefaultPurchaseSettings, opt => opt.MapFrom(src => src.DefaultPurchaseSettings))
                  .ForMember(dest => dest.DefaultTvSeriesParentalRuleId, opt => opt.MapFrom(src => src.DefaultTvSeriesParentalRuleId))
                  .ForMember(dest => dest.DefaultPurchasePin, opt => opt.MapFrom(src => src.DefaultPurchasePin))
                  .ForMember(dest => dest.DefaultParentalPin, opt => opt.MapFrom(src => src.DefaultParentalPin))
                  .ForMember(dest => dest.DefaultMoviesParentalRuleId, opt => opt.MapFrom(src => src.DefaultMoviesParentalRuleId));

            cfg.CreateMap<KalturaDefaultParentalSettingsPartnerConfig, DefaultParentalSettingsPartnerConfig>()
                  .ForMember(dest => dest.DefaultPurchaseSettings, opt => opt.MapFrom(src => src.DefaultPurchaseSettings))
                  .ForMember(dest => dest.DefaultTvSeriesParentalRuleId, opt => opt.MapFrom(src => src.DefaultTvSeriesParentalRuleId))
                  .ForMember(dest => dest.DefaultPurchasePin, opt => opt.MapFrom(src => src.DefaultPurchasePin))
                  .ForMember(dest => dest.DefaultParentalPin, opt => opt.MapFrom(src => src.DefaultParentalPin))
                  .ForMember(dest => dest.DefaultMoviesParentalRuleId, opt => opt.MapFrom(src => src.DefaultMoviesParentalRuleId));

            RegisterDTOMappings(cfg);
        }

        // We tried making a DTO Automapper but because Group.cs is in a different assembly than the ApiLogic we ran into issues
        private static void RegisterDTOMappings(MapperConfigurationExpression cfg) 
        {
            cfg.CreateMap<Group, GroupDTO>();
            cfg.CreateMap<GroupDTO, Group>();
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
                    throw new ClientException((int)StatusCode.Error, "Unknown partner configuration type");
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
                    throw new ClientException((int)StatusCode.Error, "Unknown partner configuration type");
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

        private static KalturaEvictionPolicyType? ConvertDowngradePolicyToEvictionPolicy(DowngradePolicy? priorityOrder)
        {
            KalturaEvictionPolicyType? result = null;

            if (priorityOrder.HasValue)
            {
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
            }

            return result;
        }

        private static DowngradePolicy? ConvertEvictionPolicyToDowngradePolicy(KalturaEvictionPolicyType? evictionPolicy)
        {
            DowngradePolicy? result = null;

            if (evictionPolicy.HasValue)
            {
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

        private static KalturaSuspensionProfileInheritanceType? ConvertSuspensionProfileInheritanceType(SuspensionProfileInheritanceType? type)
        {
            KalturaSuspensionProfileInheritanceType? result = null;

            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case SuspensionProfileInheritanceType.Always:
                        return KalturaSuspensionProfileInheritanceType.ALWAYS;
                    case SuspensionProfileInheritanceType.Never:
                        return KalturaSuspensionProfileInheritanceType.NEVER;
                    default:
                        return KalturaSuspensionProfileInheritanceType.DEFAULT;
                }
            }

            return result;
        }

        private static SuspensionProfileInheritanceType? ConvertSuspensionProfileInheritanceType(KalturaSuspensionProfileInheritanceType? type)
        {
            SuspensionProfileInheritanceType? result = null;

            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case KalturaSuspensionProfileInheritanceType.ALWAYS:
                        return SuspensionProfileInheritanceType.Always;
                    case KalturaSuspensionProfileInheritanceType.NEVER:
                        return SuspensionProfileInheritanceType.Never;
                    default:
                        return SuspensionProfileInheritanceType.Default;
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
                    case DowngradePolicy.ACTIVE_DATE:
                        result = KalturaDowngradePolicy.ACTIVE_DATE;
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
                    case KalturaDowngradePolicy.ACTIVE_DATE:
                        result = DowngradePolicy.ACTIVE_DATE;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown DowngradePolicy");
                }
            }

            return result;
        }
    }
}
