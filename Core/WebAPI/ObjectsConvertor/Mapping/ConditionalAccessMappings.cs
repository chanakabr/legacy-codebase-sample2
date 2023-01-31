using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using ApiObjects;
using ApiObjects.TimeShiftedTv;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Models.Catalog;
using Core.ConditionalAccess;
using Core.ConditionalAccess.Response;
using Core.Pricing;
using ApiObjects.Billing;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using Core.ConditionalAccess.Modules;
using AutoMapper.Configuration;
using TVinciShared;
using ApiLogic.ConditionalAccess.Modules;
using WebAPI.Models.Notification;
using ApiObjects.Notification;
using ApiObjects.Response;
using ApiLogic;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class ConditionalAccessMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            cfg.CreateMap<KalturaTransactionType, eTransactionType>()
               .ConvertUsing(kalturaTransactionType =>
               {
                   switch (kalturaTransactionType)
                   {
                       case KalturaTransactionType.ppv:
                           return eTransactionType.PPV;
                       case KalturaTransactionType.subscription:
                           return eTransactionType.Subscription;
                       case KalturaTransactionType.collection:
                           return eTransactionType.Collection;
                       case KalturaTransactionType.programAssetGroupOffer:
                           return eTransactionType.ProgramAssetGroupOffer;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown kalturaTransactionType value : {kalturaTransactionType}");
                   }
               });

            cfg.CreateMap<KalturaTransactionType?, eTransactionType>()
               .ConvertUsing(kalturaTransactionType =>
               {
                   if (kalturaTransactionType.HasValue)
                   {
                       switch (kalturaTransactionType.Value)
                       {
                           case KalturaTransactionType.ppv:
                               return eTransactionType.PPV;
                           case KalturaTransactionType.subscription:
                               return eTransactionType.Subscription;
                           case KalturaTransactionType.collection:
                               return eTransactionType.Collection;
                           case KalturaTransactionType.programAssetGroupOffer:
                               return eTransactionType.ProgramAssetGroupOffer;
                           default:
                               throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown kalturaTransactionType value : {kalturaTransactionType}");
                       }
                   }

                   throw new ClientException((int)StatusCode.ArgumentCannotBeEmpty, $"Argument KalturaTransactionType cannot be empty");
               });

            cfg.CreateMap<KalturaTransactionType?, eTransactionType?>()
                .ConvertUsing(clientTransactionType =>
                {
                    eTransactionType? result = null;
                    if (clientTransactionType.HasValue)
                    {
                        switch (clientTransactionType)
                        {
                            case KalturaTransactionType.ppv:
                                result = eTransactionType.PPV;
                                break;
                            case KalturaTransactionType.subscription:
                                result = eTransactionType.Subscription;
                                break;
                            case KalturaTransactionType.collection:
                                result = eTransactionType.Collection;
                                break;
                            case KalturaTransactionType.programAssetGroupOffer:
                                result = eTransactionType.ProgramAssetGroupOffer;
                                break;
                            default:
                                throw new ClientException((int)StatusCode.Error, "Unknown transaction type");
                        }
                    }
                    return result;
                });

            cfg.CreateMap<eTransactionType, KalturaTransactionType>()
                .ConvertUsing(transactionType =>
                {
                    switch (transactionType)
                    {
                        case eTransactionType.PPV:
                            return KalturaTransactionType.ppv;
                        case eTransactionType.Subscription:
                            return KalturaTransactionType.subscription;
                        case eTransactionType.Collection:
                            return KalturaTransactionType.collection;
                        case eTransactionType.ProgramAssetGroupOffer:
                            return KalturaTransactionType.programAssetGroupOffer;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown eTransactionType value : {transactionType}");
                    }
                });

            // Entitlements(WS) to  WebAPI.Entitlement(REST)
            #region Entitlement

            cfg.CreateMap<Entitlement, KalturaSubscriptionEntitlement>()
               .ForMember(dest => dest.EntitlementId, opt => opt.MapFrom(src => src.entitlementId))
               .ForMember(dest => dest.CurrentUses, opt => opt.MapFrom(src => src.currentUses))
               .ForMember(dest => dest.CurrentDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.currentDate)))
               .ForMember(dest => dest.LastViewDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.lastViewDate)))
               .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.purchaseDate)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.purchaseID))
               .ForMember(dest => dest.DeviceUDID, opt => opt.MapFrom(src => src.deviceUDID))
               .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.deviceName))
               .ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.MapFrom(src => src.cancelWindow))
               .ForMember(dest => dest.MaxUses, opt => opt.MapFrom(src => src.maxUses))
               .ForMember(dest => dest.NextRenewalDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.nextRenewalDate)))
               .ForMember(dest => dest.IsRenewableForPurchase, opt => opt.MapFrom(src => src.recurringStatus))
               .ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.isRenewable))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.endDate)))
               .ForMember(dest => dest.PaymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.paymentMethod)))
               .ForMember(dest => dest.IsInGracePeriod, opt => opt.MapFrom(src => src.IsInGracePeriod))
               .ForMember(dest => dest.PaymentGatewayId, opt => opt.MapFrom(src => GetNullableInt(src.paymentGatewayId)))
               .ForMember(dest => dest.PaymentMethodId, opt => opt.MapFrom(src => GetNullableInt(src.paymentMethodId)))
               .ForMember(dest => dest.ScheduledSubscriptionId, opt => opt.MapFrom(src => src.ScheduledSubscriptionId))
               .ForMember(dest => dest.IsSuspended, opt => opt.MapFrom(src => src.IsSuspended))
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.entitlementId))
               .ForMember(dest => dest.UnifiedPaymentId, opt => opt.MapFrom(src => src.UnifiedPaymentId))
               .ForMember(dest => dest.PriceDetails, opt => opt.MapFrom(src => src.PriceDetails))
               ;

            cfg.CreateMap<SubscriptionPurchase, KalturaSubscriptionEntitlement>()
              .ForMember(dest => dest.EntitlementId, opt => opt.MapFrom(src => src.purchaseId))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaTransactionType.subscription))
              .ForMember(dest => dest.CurrentDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)))
              .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.entitlementDate)))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int)src.purchaseId))
              .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => (int)src.purchaseId))
              .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.deviceName))
              .ForMember(dest => dest.MaxUses, opt => opt.MapFrom(src => src.maxNumberOfViews))
              .ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.isRecurring))
              .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.endDate)))
              .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid))
              .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.houseHoldId))
              .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.productId))
              .ForMember(dest => dest.IsPending, opt => opt.MapFrom(src => src.IsPending))
              ;

            cfg.CreateMap<CollectionPurchase, KalturaCollectionEntitlement>()
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaTransactionType.collection))
              .ForMember(dest => dest.EntitlementId, opt => opt.MapFrom(src => src.purchaseId))
              .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.deviceName))
              .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int)src.purchaseId))
              .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.productId))
              .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.houseHoldId))
              .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)))
              .ForMember(dest => dest.CurrentDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)))
              .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.endDate)))
              .ForMember(dest => dest.MaxUses, opt => opt.MapFrom(src => src.maxNumberOfUses))
              ;

            cfg.CreateMap<ProgramAssetGroupOfferPurchase, KalturaProgramAssetGroupOfferEntitlement>()
             .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaTransactionType.programAssetGroupOffer))
             .ForMember(dest => dest.EntitlementId, opt => opt.MapFrom(src => src.purchaseId))
             .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.deviceName))
             .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid))
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int)src.purchaseId))
             .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
             .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.houseHoldId))
             .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)))
             .ForMember(dest => dest.CurrentDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)))
             .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.endDate)))             
             ;

            cfg.CreateMap<Entitlement, KalturaPpvEntitlement>()
               .ForMember(dest => dest.EntitlementId, opt => opt.MapFrom(src => src.entitlementId))
               .ForMember(dest => dest.CurrentUses, opt => opt.MapFrom(src => src.currentUses))
               .ForMember(dest => dest.CurrentDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.currentDate)))
               .ForMember(dest => dest.LastViewDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.lastViewDate)))
               .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.purchaseDate)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.purchaseID))
               .ForMember(dest => dest.DeviceUDID, opt => opt.MapFrom(src => src.deviceUDID))
               .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.deviceName))
               .ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.MapFrom(src => src.cancelWindow))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.endDate)))
               .ForMember(dest => dest.PaymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.paymentMethod)))
               .ForMember(dest => dest.MediaFileId, opt => opt.MapFrom(src => GetNullableInt(src.mediaFileID)))
               .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => GetNullableInt(src.mediaID)))
               .ForMember(dest => dest.MaxUses, opt => opt.MapFrom(src => src.maxUses))
               .ForMember(dest => dest.NextRenewalDate, opt => opt.MapFrom(src => GetNullableInt(0)))
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.entitlementId))
               .ForMember(dest => dest.IsPending, opt => opt.MapFrom(src => src.IsPending))
               ;
            cfg.CreateMap<PpvPurchase, KalturaPpvEntitlement>()
              //.ForMember(dest => dest.EntitlementId, opt => opt.MapFrom(src => src.ppv))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaTransactionType.ppv))
               //.ForMember(dest => dest.CurrentUses, opt => opt.MapFrom(src => src.currentUses))
               //.ForMember(dest => dest.CurrentDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.currentDate)))
               //.ForMember(dest => dest.LastViewDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.lastViewDate)))
               .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.entitlementDate)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int)src.purchaseId))
               //.ForMember(dest => dest.DeviceUDID, opt => opt.MapFrom(src => src.deviceUDID))
               .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.deviceName))
               //.ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.MapFrom(src => src.cancelWindow))
               //.ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.endDate)))
               //.ForMember(dest => dest.PaymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.paymentMethod)))
               //.ForMember(dest => dest.MediaFileId, opt => opt.MapFrom(src => GetNullableInt(src.mediaFileID)))
               .ForMember(dest => dest.MediaFileId, opt => opt.MapFrom(src => GetNullableInt(src.contentId)))
               //.ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => GetNullableInt(src.)))
               .ForMember(dest => dest.MaxUses, opt => opt.MapFrom(src => src.maxNumOfViews))
               .ForMember(dest => dest.NextRenewalDate, opt => opt.MapFrom(src => GetNullableInt(0)))
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid))
               .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.houseHoldId))
               .ForMember(dest => dest.IsPending, opt => opt.MapFrom(src => src.IsPending))
               ;

            cfg.CreateMap<Entitlement, KalturaCollectionEntitlement>()
              .ForMember(dest => dest.EntitlementId, opt => opt.MapFrom(src => src.entitlementId))
              .ForMember(dest => dest.CurrentUses, opt => opt.MapFrom(src => src.currentUses))
              .ForMember(dest => dest.CurrentDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.currentDate)))
              .ForMember(dest => dest.LastViewDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.lastViewDate)))
              .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.purchaseDate)))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.purchaseID))
              .ForMember(dest => dest.DeviceUDID, opt => opt.MapFrom(src => src.deviceUDID))
              .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.deviceName))
              .ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.MapFrom(src => src.cancelWindow))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
              .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.endDate)))
              .ForMember(dest => dest.PaymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.paymentMethod)))
              .ForMember(dest => dest.MediaFileId, opt => opt.MapFrom(src => GetNullableInt(src.mediaFileID)))
              .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => GetNullableInt(src.mediaID)))
              .ForMember(dest => dest.MaxUses, opt => opt.MapFrom(src => src.maxUses))
              .ForMember(dest => dest.NextRenewalDate, opt => opt.MapFrom(src => GetNullableInt(0)))
              .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => src.purchaseID))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaTransactionType.collection))
              .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.entitlementId))
              .ForMember(dest => dest.IsPending, opt => opt.MapFrom(src => src.IsPending))
              ;

            cfg.CreateMap<Entitlement, KalturaProgramAssetGroupOfferEntitlement>()
              .ForMember(dest => dest.EntitlementId, opt => opt.MapFrom(src => src.entitlementId))
              .ForMember(dest => dest.CurrentUses, opt => opt.MapFrom(src => src.currentUses))
              .ForMember(dest => dest.CurrentDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.currentDate)))
              .ForMember(dest => dest.LastViewDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.lastViewDate)))
              .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.purchaseDate)))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.purchaseID))
              .ForMember(dest => dest.DeviceUDID, opt => opt.MapFrom(src => src.deviceUDID))
              .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.deviceName))
              .ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.MapFrom(src => src.cancelWindow))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
              .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.endDate)))
              .ForMember(dest => dest.PaymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.paymentMethod)))
              .ForMember(dest => dest.MediaFileId, opt => opt.MapFrom(src => GetNullableInt(src.mediaFileID)))
              .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => GetNullableInt(src.mediaID)))
              .ForMember(dest => dest.MaxUses, opt => opt.MapFrom(src => src.maxUses))
              .ForMember(dest => dest.NextRenewalDate, opt => opt.MapFrom(src => GetNullableInt(0)))
              .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => src.purchaseID))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaTransactionType.programAssetGroupOffer))
              .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.entitlementId))
              .ForMember(dest => dest.IsPending, opt => opt.MapFrom(src => src.IsPending))
              ;

            cfg.CreateMap<PpvPurchase, KalturaEntitlementCancellation>()
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaTransactionType.ppv))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int)src.purchaseId))
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ppvCode))
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid))
               .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.houseHoldId))
               ;

            cfg.CreateMap<SubscriptionPurchase, KalturaEntitlementCancellation>()
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaTransactionType.subscription))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int)src.purchaseId))
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.productId))
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid))
               .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.houseHoldId))
               ;

            cfg.CreateMap<CollectionPurchase, KalturaEntitlementCancellation>()
             .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaTransactionType.collection))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int)src.purchaseId))
              .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.productId))
              .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid))
              .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.houseHoldId))
              ;

            cfg.CreateMap<KalturaSubscriptionEntitlement, Entitlement>()
                  .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.paymentGatewayId, opt => opt.MapFrom(src => src.PaymentGatewayId))
                  .ForMember(dest => dest.paymentMethodId, opt => opt.MapFrom(src => src.PaymentMethodId))
                  .ForMember(dest => dest.type, opt => opt.MapFrom(src => eTransactionType.Subscription))
                  .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampAbsSecondsToDateTime(src.EndDate)))
                  ;

            cfg.CreateMap<Entitlement, KalturaEntitlement>().ConstructUsing(ConvertToKalturaEntitlement);

            cfg.CreateMap<KalturaPpvEntitlement, Entitlement>()
                  .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.type, opt => opt.MapFrom(src => eTransactionType.PPV))
                  .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampAbsSecondsToDateTime(src.EndDate)))
                  ;

            cfg.CreateMap<KalturaCollectionEntitlement, Entitlement>()
                  .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.type, opt => opt.MapFrom(src => eTransactionType.Collection))
                  .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampAbsSecondsToDateTime(src.EndDate)))
                  ;
            
            cfg.CreateMap<KalturaProgramAssetGroupOfferEntitlement, Entitlement>()
                  .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.type, opt => opt.MapFrom(src => eTransactionType.ProgramAssetGroupOffer))
                  .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampAbsSecondsToDateTime(src.EndDate)))
                  ;

            // cfg.CreateMap<Entitlement, KalturaEntitlement>()
            //.ForMember(dest => dest.EntitlementId, opt => opt.MapFrom(src => src.entitlementId))
            //.ForMember(dest => dest.CurrentUses, opt => opt.MapFrom(src => src.currentUses))
            //.ForMember(dest => dest.CurrentDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.currentDate)))
            //.ForMember(dest => dest.LastViewDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.lastViewDate)))
            //.ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.purchaseDate)))
            //.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.purchaseID))
            //.ForMember(dest => dest.DeviceUDID, opt => opt.MapFrom(src => src.deviceUDID))
            //.ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.deviceName))
            //.ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.MapFrom(src => src.cancelWindow))
            //.ForMember(dest => dest.MaxUses, opt => opt.MapFrom(src => src.maxUses))
            //.ForMember(dest => dest.NextRenewalDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.nextRenewalDate)))
            //.ForMember(dest => dest.IsRenewableForPurchase, opt => opt.MapFrom(src => src.recurringStatus))
            //.ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.isRenewable))
            //.ForMember(dest => dest.MediaFileId, opt => opt.MapFrom(src => src.mediaFileID))
            //.ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
            //.ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.endDate)))
            //.ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.paymentMethod))
            //.ForMember(dest => dest.IsInGracePeriod, opt => opt.MapFrom(src => src.IsInGracePeriod))
            //.ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.mediaID));

            cfg.CreateMap<EntitlementPriceDetails, KalturaEntitlementPriceDetails>()
              .ForMember(dest => dest.FullPrice, opt => opt.MapFrom(src => src.FullPrice))
              .ForMember(dest => dest.DiscountDetails, opt => opt.MapFrom(src => src.DiscountDetails));

            cfg.CreateMap<EntitlementDiscountDetails, KalturaEntitlementDiscountDetails>()
              .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
              .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
              .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));

            cfg.CreateMap<CouponEntitlementDiscountDetails, KalturaCouponEntitlementDiscountDetails>()
                .IncludeBase<EntitlementDiscountDetails, KalturaEntitlementDiscountDetails>()
                .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.EndlessCoupon))
                .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.CouponCode));

            cfg.CreateMap<EntitlementDiscountDetailsIdentifier, KalturaEntitlementDiscountDetailsIdentifier>()
                .IncludeBase<EntitlementDiscountDetails, KalturaEntitlementDiscountDetails>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            cfg.CreateMap<CompensationEntitlementDiscountDetails, KalturaCompensationEntitlementDiscountDetails>()
                .IncludeBase<EntitlementDiscountDetailsIdentifier, KalturaEntitlementDiscountDetailsIdentifier>();

            cfg.CreateMap<CampaignEntitlementDiscountDetails, KalturaCampaignEntitlementDiscountDetails>()
                .IncludeBase<EntitlementDiscountDetailsIdentifier, KalturaEntitlementDiscountDetailsIdentifier>();

            cfg.CreateMap<DiscountEntitlementDiscountDetails, KalturaDiscountEntitlementDiscountDetails>()
                .IncludeBase<EntitlementDiscountDetailsIdentifier, KalturaEntitlementDiscountDetailsIdentifier>();

            cfg.CreateMap<TrailEntitlementDiscountDetails, KalturaTrailEntitlementDiscountDetails>()
                .IncludeBase<EntitlementDiscountDetailsIdentifier, KalturaEntitlementDiscountDetailsIdentifier>();

            #endregion

            // BillingTransactions(WS) to  BillingTransactions(REST)
            #region Billing Transaction Container
            cfg.CreateMap<BillingTransactionContainer, KalturaBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtEndDate)))
               .ForMember(dest => dest.billingAction, opt => opt.MapFrom(src => src.m_eBillingAction))
               .ForMember(dest => dest.itemType, opt => opt.MapFrom(src => src.m_eItemType))
               .ForMember(dest => dest.paymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.m_ePaymentMethod)))
               .ForMember(dest => dest.isRecurring, opt => opt.MapFrom(src => src.m_bIsRecurring))
               .ForMember(dest => dest.billingProviderRef, opt => opt.MapFrom(src => src.m_nBillingProviderRef))
               .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.m_nPurchaseID))
               .ForMember(dest => dest.purchasedItemName, opt => opt.MapFrom(src => src.m_sPurchasedItemName))
               .ForMember(dest => dest.purchasedItemCode, opt => opt.MapFrom(src => src.m_sPurchasedItemCode))
               .ForMember(dest => dest.recieptCode, opt => opt.MapFrom(src => src.m_sRecieptCode))
               .ForMember(dest => dest.remarks, opt => opt.MapFrom(src => src.m_sRemarks))
               .ForMember(dest => dest.paymentMethodExtraDetails, opt => opt.MapFrom(src => src.m_sPaymentMethodExtraDetails))
               .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.m_Price))
               .ForMember(dest => dest.ExternalTransactionId, opt => opt.MapFrom(src => src.ExternalTransactionId));

            #endregion

            // BillingTransactions(WS) to  BillingTransactions(REST)
            #region Billing Transaction List
            cfg.CreateMap<BillingTransactionsResponse, KalturaBillingTransactionListResponse>()
               .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.m_nTransactionsCount))
               .ForMember(dest => dest.transactions, opt => opt.MapFrom(src => src.m_Transactions));
            #endregion

            #region Price
            cfg.CreateMap<Price, KalturaPrice>()
              .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.m_dPrice))
              .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3))
              .ForMember(dest => dest.CurrencySign, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencySign))
              .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.m_oCurrency.m_nCurrencyID));
            #endregion

            // BillingResponse
            #region Billing
            cfg.CreateMap<BillingResponse, KalturaBillingResponse>()
               .ForMember(dest => dest.ReceiptCode, opt => opt.MapFrom(src => src.m_sRecieptCode))
               .ForMember(dest => dest.ExternalReceiptCode, opt => opt.MapFrom(src => src.m_sExternalReceiptCode));
            #endregion

            // TransactionResponse to KalturaTransactionResponse
            #region Transaction
            cfg.CreateMap<TransactionResponse, KalturaTransaction>()
               .ForMember(dest => dest.PGReferenceID, opt => opt.MapFrom(src => src.PGReferenceID))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TransactionID))
               .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State.ToString()))
               .ForMember(dest => dest.PGResponseID, opt => opt.MapFrom(src => src.PGResponseCode))
               .ForMember(dest => dest.FailReasonCode, opt => opt.MapFrom(src => src.FailReasonCode))
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
            #endregion

            #region Billing Transaction Container to User Billing Transaciton
            cfg.CreateMap<BillingTransactionContainer, KalturaUserBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtEndDate)))

               .ForMember(dest => dest.billingAction, opt => opt.MapFrom(src => src.m_eBillingAction))
               .ForMember(dest => dest.itemType, opt => opt.MapFrom(src => src.m_eItemType))
               .ForMember(dest => dest.paymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.m_ePaymentMethod)))
               .ForMember(dest => dest.isRecurring, opt => opt.MapFrom(src => src.m_bIsRecurring))
               .ForMember(dest => dest.billingProviderRef, opt => opt.MapFrom(src => src.m_nBillingProviderRef))
               .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.m_nPurchaseID))
               .ForMember(dest => dest.purchasedItemName, opt => opt.MapFrom(src => src.m_sPurchasedItemName))
               .ForMember(dest => dest.purchasedItemCode, opt => opt.MapFrom(src => src.m_sPurchasedItemCode))
               .ForMember(dest => dest.recieptCode, opt => opt.MapFrom(src => src.m_sRecieptCode))
               .ForMember(dest => dest.remarks, opt => opt.MapFrom(src => src.m_sRemarks))
               .ForMember(dest => dest.paymentMethodExtraDetails, opt => opt.MapFrom(src => src.m_sPaymentMethodExtraDetails))
               .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.m_Price))
               ;

            #endregion

            #region TransactionHistoryContainer to KalturaUserBillingTransaction
            cfg.CreateMap<TransactionHistoryContainer, KalturaUserBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dtEndDate)))
               .ForMember(dest => dest.billingAction, opt => opt.MapFrom(src => src.m_eBillingAction))
               .ForMember(dest => dest.itemType, opt => opt.MapFrom(src => src.m_eItemType))
               .ForMember(dest => dest.paymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.m_ePaymentMethod)))
               .ForMember(dest => dest.isRecurring, opt => opt.MapFrom(src => src.m_bIsRecurring))
               .ForMember(dest => dest.billingProviderRef, opt => opt.MapFrom(src => src.m_nBillingProviderRef))
               .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.m_nPurchaseID))
               .ForMember(dest => dest.purchasedItemName, opt => opt.MapFrom(src => src.m_sPurchasedItemName))
               .ForMember(dest => dest.purchasedItemCode, opt => opt.MapFrom(src => src.m_sPurchasedItemCode))
               .ForMember(dest => dest.recieptCode, opt => opt.MapFrom(src => src.m_sRecieptCode))
               .ForMember(dest => dest.remarks, opt => opt.MapFrom(src => src.m_sRemarks))
               .ForMember(dest => dest.paymentMethodExtraDetails, opt => opt.MapFrom(src => src.m_sPaymentMethodExtraDetails))
               .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.m_Price))
               .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.SiteGuid))
               .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.UserFullName))
               .ForMember(dest => dest.billingPriceType, opt => opt.MapFrom(src => src.billingPriceType))
               .ForMember(dest => dest.ExternalTransactionId, opt => opt.MapFrom(src => src.ExternalTransactionId));


            #endregion

            //#region Domains Billing Transactions
            //cfg.CreateMap<DomainsBillingTransactionsResponse, KalturaHouseholdsBillingTransactions>()
            //    .ForMember(dest => dest.DomainsBillingTransactions, opt => opt.MapFrom(src => src.billingTransactions));

            //#endregion

            //#region Domain Billing Transactions
            //cfg.CreateMap<DomainBillingTransactionsResponse, >()
            //    .ForMember(dest => dest.UsersBillingTransactions, opt => opt.MapFrom(src => src.m_BillingTransactionResponses))
            //    .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.m_nDomainID));
            //#endregion

            //#region User Billing Transactions
            //cfg.CreateMap<UserBillingTransactionsResponse, KalturaUserBillingTransactions>()
            //    .ForMember(dest => dest.SiteGuid, opt => opt.MapFrom(src => src.m_sSiteGUID))
            //    .ForMember(dest => dest.BillingTransactions, opt => opt.MapFrom(src => src.m_BillingTransactionResponse));
            //#endregion

            #region Asset Item Prices
            cfg.CreateMap<AssetItemPrices, KalturaAssetPrice>()
              .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
              .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.AssetType))
              .ForMember(dest => dest.FilePrices, opt => opt.MapFrom(src => src.PriceContainers))
              ;
            #endregion

            #region Asset Files
            cfg.CreateMap<KalturaPersonalAssetRequest, AssetFiles>()
              .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.Type))
              .ForMember(dest => dest.FileIds, opt => opt.MapFrom(src =>
                          //{
                          //if (src.FileIds != null)
                          //{
                          //    return null;
                          //}
                          //else
                          //{
                          src.FileIds
                //.Select(i => (long)i).ToList();
                //}
                //}
                ))
              ;
            #endregion

            // ServiceObject to KalturaPremiumService
            cfg.CreateMap<ServiceObject, KalturaPremiumService>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<KalturaCDVRAdapterProfile, CDVRAdapter>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertCDVRAdapterSettings(src.Settings)))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
               .ForMember(dest => dest.DynamicLinksSupport, opt => opt.MapFrom(src => src.DynamicLinksSupport))
               .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));

            cfg.CreateMap<CDVRAdapter, KalturaCDVRAdapterProfile>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertCDVRAdapterSettings(src.Settings)))
              .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
              .ForMember(dest => dest.DynamicLinksSupport, opt => opt.MapFrom(src => src.DynamicLinksSupport))
              .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));


            // LicensedLinkResponse to KalturaLicensedUrls
            cfg.CreateMap<LicensedLinkResponse, KalturaLicensedUrl>()
               .ForMember(dest => dest.MainUrl, opt => opt.MapFrom(src => src.mainUrl))
               .ForMember(dest => dest.AltUrl, opt => opt.MapFrom(src => src.altUrl));

            #region Recordings

            // KalturaRecording to Recording
            cfg.CreateMap<KalturaRecording, Recording>()
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.RecordingStatus, opt => opt.ResolveUsing(src => ConvertKalturaRecordingStatus(src.Status)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertKalturaRecordingType(src.Type)))
                .ForMember(dest => dest.ViewableUntilDate, opt => opt.MapFrom(src => src.ViewableUntilDate))
                .ForMember(dest => dest.IsProtected, opt => opt.MapFrom(src => src.IsProtected))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampSecondsToDateTime(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampSecondsToDateTime(src.UpdateDate)))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Duration, opt => opt.Ignore())
                ;
            
            cfg.CreateMap<KalturaPaddedRecording, Recording>()
                .IncludeBase<KalturaRecording, Recording>()
                .ForMember(dest => dest.StartPadding, opt => opt.MapFrom(src => src.StartPadding))
                .ForMember(dest => dest.EndPadding, opt => opt.MapFrom(src => src.EndPadding))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampSecondsToDateTime(src.Duration)))
                ;
            
              
            cfg.CreateMap<KalturaImmediateRecording, Recording>()
                .IncludeBase<KalturaRecording, Recording>()
                .ForMember(dest => dest.AbsoluteEndTime, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampSecondsToDateTime(src.AbsoluteEndTime)))
                .ForMember(dest => dest.AbsoluteStartTime, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampSecondsToDateTime(src.AbsoluteStartTime)))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampSecondsToDateTime(src.Duration)))
                ;
            
            cfg.CreateMap<Recording, KalturaImmediateRecording>()
                .IncludeBase<Recording, KalturaRecording>()
                .ForMember(dest => dest.AbsoluteEndTime, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.AbsoluteEndTime)))
                .ForMember(dest => dest.AbsoluteStartTime, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.AbsoluteStartTime)))
                ;

            // Recording to KalturaRecording
            cfg.CreateMap<Recording, KalturaRecording>()
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.EpgId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertTstvRecordingStatus(src.RecordingStatus)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertRecordingType(src.Type)))
                .ForMember(dest => dest.IsProtected, opt => opt.MapFrom(src => IsRecordingProtected(src.ProtectedUntilDate)))
                .ForMember(dest => dest.ViewableUntilDate, opt => opt.MapFrom(src => src.ViewableUntilDate))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
                ;

            cfg.CreateMap<Recording, KalturaPaddedRecording>()
                .IncludeBase<Recording, KalturaRecording>()
                .ForMember(dest => dest.StartPadding, opt => opt.MapFrom(src => src.StartPadding))
                .ForMember(dest => dest.EndPadding, opt => opt.MapFrom(src => src.EndPadding))
                ;
            
            // KalturaExternalRecording to ExternalRecording
            cfg.CreateMap<KalturaExternalRecording, ExternalRecording>()
                .IncludeBase<KalturaRecording, Recording>()
                .ForMember(dest => dest.ExternalDomainRecordingId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.MetaData, opt => opt.ResolveUsing(src => ConvertMetaData(src.MetaData)))
                .AfterMap((src, dest) => dest.MetaData = dest.MetaData != null && dest.MetaData.Any() ? dest.MetaData : null);

            // ExternalRecording to KalturaExternalRecording
            cfg.CreateMap<ExternalRecording, KalturaExternalRecording>()
                .IncludeBase<Recording, KalturaRecording>()
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalDomainRecordingId))
                .ForMember(dest => dest.MetaData, opt => opt.ResolveUsing(src => ConvertMetaData(src.MetaData)))
                .AfterMap((src, dest) => dest.MetaData = dest.MetaData != null && dest.MetaData.Any() ? dest.MetaData : null);

            cfg.CreateMap<KalturaSeriesRecordingOption, SeriesRecordingOption>()
                .ForMember(dest => dest.MinEpisodeNumber, opt => opt.MapFrom(src => src.MinEpisodeNumber))
                .ForMember(dest => dest.MinSeasonNumber, opt => opt.MapFrom(src => src.MinSeasonNumber))
                .ForMember(dest => dest.ChronologicalRecordStartTime, opt => opt.ResolveUsing(src => ConvertChronologicalRecordFrom(src.ChronologicalRecordStartTime)))
                ;

            cfg.CreateMap<SeriesRecordingOption, KalturaSeriesRecordingOption>()
                .ForMember(dest => dest.MinEpisodeNumber, opt => opt.MapFrom(src => src.MinEpisodeNumber))
                .ForMember(dest => dest.MinSeasonNumber, opt => opt.MapFrom(src => src.MinSeasonNumber))
                .ForMember(dest => dest.ChronologicalRecordStartTime, opt => opt.ResolveUsing(src => ConvertChronologicalRecordFrom(src.ChronologicalRecordStartTime)))
                ;

            // KalturaSeriesRecording to SeriesRecording
            cfg.CreateMap<KalturaSeriesRecording, SeriesRecording>()
               .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.EpgId))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.ChannelId))
               .ForMember(dest => dest.SeasonNumber, opt => opt.MapFrom(src => src.SeasonNumber))
               .ForMember(dest => dest.SeriesId, opt => opt.MapFrom(src => src.SeriesId))
               .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertKalturaRecordingType(src.Type)))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampSecondsToDateTime(src.CreateDate)))
               .ForMember(dest => dest.SeriesRecordingOption, opt => opt.ResolveUsing(src => src.SeriesRecordingOption))
               ;

            // SeriesRecording to KalturaSeriesRecording
            cfg.CreateMap<SeriesRecording, KalturaSeriesRecording>()
               .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.EpgId))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.ChannelId, opt => opt.MapFrom(src => src.EpgChannelId))
               .ForMember(dest => dest.SeasonNumber, opt => opt.MapFrom(src => src.SeasonNumber > 0 ? (int?)src.SeasonNumber : null))
               .ForMember(dest => dest.SeriesId, opt => opt.MapFrom(src => src.SeriesId))
               .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertRecordingType(src.Type)))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
               .ForMember(dest => dest.ExcludedSeasons, opt => opt.MapFrom(src => src.ExcludedSeasons))
               .ForMember(dest => dest.SeriesRecordingOption, opt => opt.ResolveUsing(src => src.SeriesRecordingOption))
               ;

            cfg.CreateMap<ExternalSeriesRecording, KalturaExternalSeriesRecording>()
               .IncludeBase<SeriesRecording, KalturaSeriesRecording>()
               .ForMember(dest => dest.MetaData, opt => opt.ResolveUsing(src => ConvertMetaData(src.MetaData)))
               .AfterMap((src, dest) => dest.MetaData = dest.MetaData != null && dest.MetaData.Any() ? dest.MetaData : null);
            #endregion

            #region Household Quota
            cfg.CreateMap<DomainQuotaResponse, KalturaHouseholdQuota>()
               .ForMember(dest => dest.AvailableQuota, opt => opt.MapFrom(src => src.AvailableQuota))
               .ForMember(dest => dest.TotalQuota, opt => opt.MapFrom(src => src.TotalQuota));

            #endregion

            //KalturaHouseholdPremiumService
            cfg.CreateMap<ServiceObject, KalturaHouseholdPremiumService>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            // KalturaAssetFileContext to EntitlementResponse
            cfg.CreateMap<KalturaAssetFileContext, EntitlementResponse>()
              .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.ViewLifeCycle))
              .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.FullLifeCycle))
              .ForMember(dest => dest.IsOfflinePlayBack, opt => opt.MapFrom(src => src.IsOfflinePlayBack))
              .ForMember(dest => dest.IsLivePlayBack, opt => opt.MapFrom(src => src.IsLivePlayBack));

            // EntitlementResponse to KalturaAssetFileContext
            cfg.CreateMap<EntitlementResponse, KalturaAssetFileContext>()
              .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.ViewLifeCycle))
              .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.FullLifeCycle))
              .ForMember(dest => dest.IsOfflinePlayBack, opt => opt.MapFrom(src => src.IsOfflinePlayBack))
              .ForMember(dest => dest.IsLivePlayBack, opt => opt.MapFrom(src => src.IsLivePlayBack));

            cfg.CreateMap<BusinessModuleDetails, KalturaBusinessModuleDetails>()
              .ForMember(dest => dest.BusinessModuleId, opt => opt.MapFrom(src => src.BusinessModuleId))
              .ForMember(dest => dest.BusinessModuleType, opt => opt.ResolveUsing(src => src.BusinessModuleType));            

            cfg.CreateMap<KalturaBusinessModuleDetails, BusinessModuleDetails>()
              .ForMember(dest => dest.BusinessModuleId, opt => opt.MapFrom(src => src.BusinessModuleId))
              .ForMember(dest => dest.BusinessModuleType, opt => opt.ResolveUsing(src => src.BusinessModuleType));           

            cfg.CreateMap<MediaFile, KalturaPlaybackSource>()
              .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.MediaId.ToString()))
              .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
              .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
              .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => (int?)src.TypeId))
              .ForMember(dest => dest.Url, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.DirectUrl) ? src.DirectUrl : src.Url))
              .ForMember(dest => dest.AltUrl, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.AltDirectUrl) ? src.AltDirectUrl : src.AltUrl))
              .ForMember(dest => dest.DrmId, opt => opt.MapFrom(src => src.DrmId))
              .ForMember(dest => dest.FileExtention, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Url) || !src.Url.Contains(".") ? string.Empty : src.Url.Substring(src.Url.LastIndexOf('.'))))
              .ForMember(dest => dest.Protocols, opt => opt.MapFrom(src => src.Url.StartsWith("https") ? "https" : src.Url.StartsWith("http") ? "http" : string.Empty))
              .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.StreamerType.HasValue ? src.StreamerType.ToString() : string.Empty))
              .ForMember(dest => dest.AdsParams, opt => opt.MapFrom(src => src.AdsParam))
              .ForMember(dest => dest.AdsPolicy, opt => opt.ResolveUsing(src => ConvertAdsPolicy(src.AdsPolicy)))
              .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.Opl, opt => opt.ResolveUsing(src => src.Opl))
              .ForMember(dest => dest.BusinessModuleDetails, opt => opt.ResolveUsing(src => src.BusinessModuleDetails))
              .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.Labels))
              ;

            cfg.CreateMap<PlaybackContextResponse, KalturaPlaybackContext>()
              .ForMember(dest => dest.Sources, opt => opt.MapFrom(src => src.Files))
              .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => new List<ApiObjects.Response.Status>()));

            cfg.CreateMap<ApiObjects.Response.Status, KalturaAccessControlMessage>()
              .ForMember(dest => dest.Code, opt => opt.MapFrom(src => ((ApiObjects.Response.eResponseStatus)src.Code).ToString()))
              .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message));

            cfg.CreateMap<KalturaCompensation, Compensation>()
              .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
              .ForMember(dest => dest.TotalRenewals, opt => opt.MapFrom(src => src.TotalRenewalIterations))
              .ForMember(dest => dest.CompensationType, opt => opt.ResolveUsing(src => ConvertCompensationType(src.CompensationType)))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId))
              .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => src.PurchaseId));

            cfg.CreateMap<Compensation, KalturaCompensation>()
              .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
              .ForMember(dest => dest.TotalRenewalIterations, opt => opt.MapFrom(src => src.TotalRenewals))
              .ForMember(dest => dest.CompensationType, opt => opt.ResolveUsing(src => ConvertCompensationType(src.CompensationType)))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId))
              .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => src.PurchaseId))
              .ForMember(dest => dest.AppliedRenewalIterations, opt => opt.MapFrom(src => src.Renewals));

            cfg.CreateMap<APILogic.ConditionalAccess.AdsControlData, KalturaAdsSource>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.FileId))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.FileType))
              .ForMember(dest => dest.AdsParams, opt => opt.MapFrom(src => src.AdsParam))
              .ForMember(dest => dest.AdsPolicy, opt => opt.ResolveUsing(src => ConvertAdsPolicy(src.AdsPolicy)));

            cfg.CreateMap<APILogic.ConditionalAccess.Modules.EntitlementRenewal, KalturaEntitlementRenewal>()
             .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => src.PurchaseId))
             .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId))
             .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
             .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
             .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.Date)));

            cfg.CreateMap<APILogic.ConditionalAccess.Modules.EntitlementRenewalBase, KalturaEntitlementRenewalBase>()
             .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => src.PurchaseId))
             .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId))
             .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.PriceAmount));

            cfg.CreateMap<APILogic.ConditionalAccess.Modules.UnifiedPaymentRenewal, KalturaUnifiedPaymentRenewal>()
             .ForMember(dest => dest.UnifiedPaymentId, opt => opt.MapFrom(src => src.UnifiedPaymentId))
             .ForMember(dest => dest.Entitlements, opt => opt.MapFrom(src => src.Entitlements))
             .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
             .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
             .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.Date)));

            cfg.CreateMap<ActionResult, KalturaActionResult>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Result, opt => opt.MapFrom(src => src.Result))
                .AfterMap((src, dest) => dest.Result.Args = dest.Result.Args.Any() ? dest.Result.Args : null);
        }

        private static ChronologicalRecordStartTime ConvertChronologicalRecordFrom(KalturaChronologicalRecordStartTime? chronologicalRecordFrom)
        {
            return GenericExtensionMethods.ConvertEnumsById<KalturaChronologicalRecordStartTime, ChronologicalRecordStartTime>
                (chronologicalRecordFrom, ChronologicalRecordStartTime.None).Value;
        }

        private static KalturaChronologicalRecordStartTime ConvertChronologicalRecordFrom(ChronologicalRecordStartTime? chronologicalRecordFrom)
        {
            return GenericExtensionMethods.ConvertEnumsById<ChronologicalRecordStartTime, KalturaChronologicalRecordStartTime>
                (chronologicalRecordFrom, KalturaChronologicalRecordStartTime.NONE).Value;
        }      

        private static KalturaAdsPolicy? ConvertAdsPolicy(AdsPolicy? adsPolicy)
        {
            if (!adsPolicy.HasValue)
            {
                return null;
            }
            switch (adsPolicy.Value)
            {
                case AdsPolicy.NoAds:
                    return KalturaAdsPolicy.NO_ADS;
                    break;
                case AdsPolicy.KeepAds:
                    return KalturaAdsPolicy.KEEP_ADS;
                    break;
                default:
                    return null;
                    break;
            }
        }

        private static CompensationType ConvertCompensationType(KalturaCompensationType kalturaCompensationType)
        {
            CompensationType result;

            switch (kalturaCompensationType)
            {
                case KalturaCompensationType.PERCENTAGE:
                    result = CompensationType.Percentage;
                    break;
                case KalturaCompensationType.FIXED_AMOUNT:
                    result = CompensationType.FixedAmount;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown compensation type");
                    break;
            }

            return result;
        }

        private static KalturaCompensationType ConvertCompensationType(CompensationType kalturaCompensationType)
        {
            KalturaCompensationType result;

            switch (kalturaCompensationType)
            {
                case CompensationType.Percentage:
                    result = KalturaCompensationType.PERCENTAGE;
                    break;
                case CompensationType.FixedAmount:
                    result = KalturaCompensationType.FIXED_AMOUNT;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown compensation type");
                    break;
            }

            return result;
        }

        private static int? GetNullableInt(int p)
        {
            if (p == 0)
            {
                return null;
            }
            else
            {
                return p;
            }
        }

        internal static KalturaEntitlement ConvertToKalturaEntitlement(Entitlement entitlement)
        {
            KalturaEntitlement result = null;
            switch (entitlement.type)
            {
                case eTransactionType.PPV:
                    result = AutoMapper.Mapper.Map<KalturaPpvEntitlement>(entitlement);
                    break;
                case eTransactionType.Collection:
                    result = AutoMapper.Mapper.Map<KalturaCollectionEntitlement>(entitlement);
                    break;
                case eTransactionType.Subscription:
                    result = AutoMapper.Mapper.Map<KalturaSubscriptionEntitlement>(entitlement);
                    break;
                case eTransactionType.ProgramAssetGroupOffer:
                    result = AutoMapper.Mapper.Map<KalturaProgramAssetGroupOfferEntitlement>(entitlement);
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown entitlement type");
            }
            return result;
        }

        #region Recording Help Methods

        public static bool IsRecordingProtected(long? protectedUntilEpoch)
        {
            if (!protectedUntilEpoch.HasValue)
            {
                return false;
            }

            long currentUtcTime = DateUtils.GetUtcUnixTimestampNow();
            return protectedUntilEpoch.Value > currentUtcTime;
        }

        public static TstvRecordingStatus ConvertKalturaRecordingStatus(KalturaRecordingStatus recordingStatus)
        {
            TstvRecordingStatus result;
            switch (recordingStatus)
            {
                case KalturaRecordingStatus.CANCELED:
                    result = TstvRecordingStatus.Canceled;
                    break;
                case KalturaRecordingStatus.DELETED:
                    result = TstvRecordingStatus.Deleted;
                    break;
                case KalturaRecordingStatus.FAILED:
                    result = TstvRecordingStatus.Failed;
                    break;
                case KalturaRecordingStatus.RECORDED:
                    result = TstvRecordingStatus.Recorded;
                    break;
                case KalturaRecordingStatus.RECORDING:
                    result = TstvRecordingStatus.Recording;
                    break;
                case KalturaRecordingStatus.SCHEDULED:
                    result = TstvRecordingStatus.Scheduled;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown recordingStatus type");
            }
            return result;
        }

        public static KalturaRecordingStatus ConvertTstvRecordingStatus(TstvRecordingStatus recordingStatus)
        {
            KalturaRecordingStatus result;
            switch (recordingStatus)
            {
                case TstvRecordingStatus.SeriesCancel:
                case TstvRecordingStatus.Canceled:
                    result = KalturaRecordingStatus.CANCELED;
                    break;
                case TstvRecordingStatus.SeriesDelete:
                case TstvRecordingStatus.Deleted:
                    result = KalturaRecordingStatus.DELETED;
                    break;
                case TstvRecordingStatus.Failed:
                    result = KalturaRecordingStatus.FAILED;
                    break;
                case TstvRecordingStatus.Recorded:
                    result = KalturaRecordingStatus.RECORDED;
                    break;
                case TstvRecordingStatus.Recording:
                    result = KalturaRecordingStatus.RECORDING;
                    break;
                case TstvRecordingStatus.Scheduled:
                    result = KalturaRecordingStatus.SCHEDULED;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown recordingStatus type");
            }
            return result;
        }

        public static RecordingType ConvertKalturaRecordingType(KalturaRecordingType recordingType)
        {
            RecordingType result;
            switch (recordingType)
            {
                case KalturaRecordingType.SINGLE:
                    result = RecordingType.Single;
                    break;
                case KalturaRecordingType.SEASON:
                    result = RecordingType.Season;
                    break;
                case KalturaRecordingType.SERIES:
                    result = RecordingType.Series;
                    break;
                case KalturaRecordingType.OriginalBroadcast:
                    result = RecordingType.OriginalBroadcast;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown recordingType type");
            }
            return result;
        }

        public static KalturaRecordingType ConvertRecordingType(RecordingType recordingType)
        {
            KalturaRecordingType result;
            switch (recordingType)
            {
                case RecordingType.Single:
                    result = KalturaRecordingType.SINGLE;
                    break;
                case RecordingType.Season:
                    result = KalturaRecordingType.SEASON;
                    break;
                case RecordingType.Series:
                    result = KalturaRecordingType.SERIES;
                    break;
                case RecordingType.OriginalBroadcast:
                    result = KalturaRecordingType.OriginalBroadcast;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown recordingType type");
            }
            return result;
        }


        public static ApiObjects.SearchObjects.OrderObj ConvertOrderToOrderObj(KalturaRecordingOrderBy order)
        {
            ApiObjects.SearchObjects.OrderObj result = new ApiObjects.SearchObjects.OrderObj();

            switch (order)
            {
                case KalturaRecordingOrderBy.TITLE_ASC:
                    result.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaRecordingOrderBy.TITLE_DESC:
                    result.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaRecordingOrderBy.START_DATE_ASC:
                    result.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaRecordingOrderBy.START_DATE_DESC:
                    result.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
            }
            return result;
        }
        public static SeriesRecordingOrderObj ConvertOrderToSeriesOrderObj(KalturaSeriesRecordingOrderBy order)
        {
            SeriesRecordingOrderObj result = new SeriesRecordingOrderObj();

            switch (order)
            {
                case KalturaSeriesRecordingOrderBy.START_DATE_ASC:
                    result.OrderBy = SeriesOrderBy.START_DATE;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaSeriesRecordingOrderBy.START_DATE_DESC:
                    result.OrderBy = SeriesOrderBy.START_DATE;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaSeriesRecordingOrderBy.ID_ASC:
                    result.OrderBy = SeriesOrderBy.ID;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaSeriesRecordingOrderBy.ID_DESC:
                    result.OrderBy = SeriesOrderBy.ID;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaSeriesRecordingOrderBy.SERIES_ID_ASC:
                    result.OrderBy = SeriesOrderBy.SERIES_ID;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaSeriesRecordingOrderBy.SERIES_ID_DESC:
                    result.OrderBy = SeriesOrderBy.SERIES_ID;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
            }
            return result;
        }

        #endregion

        internal static Dictionary<string, string> ConvertMetaData(SerializableDictionary<string, KalturaStringValue> metaData)
        {
            Dictionary<string, string> res = null;

            if (metaData != null && metaData.Any())
            {
                res = new Dictionary<string, string>();

                foreach (var item in metaData)
                {
                    res.Add(item.Key, item.Value.value);
                }
            }

            return res;
        }

        internal static SerializableDictionary<string, KalturaStringValue> ConvertMetaData(Dictionary<string, string> metaData)
        {
            SerializableDictionary<string, KalturaStringValue> res = null;

            if (metaData != null && metaData.Any())
            {
                res = new SerializableDictionary<string, KalturaStringValue>();

                foreach (var item in metaData)
                {
                    res.Add(item.Key, new KalturaStringValue() { value = item.Value });
                }
            }

            return res;
        }

        internal static List<CDVRAdapterSettings> ConvertCDVRAdapterSettings(SerializableDictionary<string, KalturaStringValue> settings)
        {
            List<CDVRAdapterSettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<CDVRAdapterSettings>();
                CDVRAdapterSettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new CDVRAdapterSettings();
                        pc.key = data.Key;
                        pc.value = data.Value.value;
                        result.Add(pc);
                    }
                }
            }
            if (result != null && result.Count > 0)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, KalturaStringValue> ConvertCDVRAdapterSettings(List<CDVRAdapterSettings> settings)
        {
            Dictionary<string, KalturaStringValue> result = null;

            if (settings != null && settings.Count() > 0)
            {
                result = new Dictionary<string, KalturaStringValue>();
                foreach (var data in settings)
                {
                    if (!string.IsNullOrEmpty(data.key))
                    {
                        result.Add(data.key, new KalturaStringValue() { value = data.value });
                    }
                }
            }
            return result;
        }

        public static TransactionHistoryOrderBy ConvertTransactionHistoryOrderBy(KalturaTransactionHistoryOrderBy orderBy)
        {
            TransactionHistoryOrderBy result;

            switch (orderBy)
            {
                case KalturaTransactionHistoryOrderBy.CREATE_DATE_DESC:
                    result = TransactionHistoryOrderBy.CreateDateDesc;
                    break;
                case KalturaTransactionHistoryOrderBy.CREATE_DATE_ASC:
                    result = TransactionHistoryOrderBy.CreateDateAsc;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown transaction history order by");
            }

            return result;
        }

        public static EntitlementOrderBy ConvertEntitlementOrderBy(KalturaEntitlementOrderBy orderBy)
        {
            EntitlementOrderBy result;

            switch (orderBy)
            {
                case KalturaEntitlementOrderBy.PURCHASE_DATE_ASC:
                    result = EntitlementOrderBy.PurchaseDateAsc;
                    break;
                case KalturaEntitlementOrderBy.PURCHASE_DATE_DESC:
                    result = EntitlementOrderBy.PurchaseDateDesc;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown entitlement order by");
            }

            return result;
        }

        public static KalturaPaymentMethodType ConvertPaymentMethod(ePaymentMethod paymentMethod)
        {
            KalturaPaymentMethodType result;
            switch (paymentMethod)
            {
                case ePaymentMethod.Unknown:
                    result = KalturaPaymentMethodType.unknown;
                    break;
                case ePaymentMethod.CreditCard:
                    result = KalturaPaymentMethodType.credit_card;
                    break;
                case ePaymentMethod.SMS:
                    result = KalturaPaymentMethodType.sms;
                    break;
                case ePaymentMethod.PayPal:
                    result = KalturaPaymentMethodType.pay_pal;
                    break;
                case ePaymentMethod.DebitCard:
                    result = KalturaPaymentMethodType.debit_card;
                    break;
                case ePaymentMethod.Ideal:
                    result = KalturaPaymentMethodType.ideal;
                    break;
                case ePaymentMethod.Incaso:
                    result = KalturaPaymentMethodType.incaso;
                    break;
                case ePaymentMethod.Gift:
                    result = KalturaPaymentMethodType.gift;
                    break;
                case ePaymentMethod.Visa:
                    result = KalturaPaymentMethodType.visa;
                    break;
                case ePaymentMethod.MasterCard:
                    result = KalturaPaymentMethodType.master_card;
                    break;
                case ePaymentMethod.InApp:
                    result = KalturaPaymentMethodType.in_app;
                    break;
                case ePaymentMethod.M1:
                    result = KalturaPaymentMethodType.m1;
                    break;
                case ePaymentMethod.ChangeSubscription:
                    result = KalturaPaymentMethodType.change_subscription;
                    break;
                case ePaymentMethod.Offline:
                    result = KalturaPaymentMethodType.offline;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown ePaymentMethod type");
            }

            return result;
        }

        public static PlayContextType ConvertPlayContextType(KalturaPlaybackContextType type)
        {
            PlayContextType result;
            switch (type)
            {
                case KalturaPlaybackContextType.TRAILER:
                    result = PlayContextType.Trailer;
                    break;
                case KalturaPlaybackContextType.CATCHUP:
                    result = PlayContextType.CatchUp;
                    break;
                case KalturaPlaybackContextType.START_OVER:
                    result = PlayContextType.StartOver;
                    break;
                case KalturaPlaybackContextType.PLAYBACK:
                    result = PlayContextType.Playback;
                    break;
                case KalturaPlaybackContextType.DOWNLOAD:
                    result = PlayContextType.Download;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown KalturaContextType type");
                    break;
            }

            return result;
        }

        internal static UrlType ConvertUrlType(KalturaUrlType urlType)
        {
            UrlType result;
            switch (urlType)
            {
                case KalturaUrlType.DIRECT:
                    result = UrlType.direct;
                    break;
                case KalturaUrlType.PLAYMANIFEST:
                    result = UrlType.playmanifest;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown KalturaUrlType type");
                    break;
            }

            return result;
        }
    }
}