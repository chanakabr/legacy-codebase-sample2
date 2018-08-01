using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Utils;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using ApiObjects;
using ApiObjects.SearchObjects;
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

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class ConditionalAccessMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            // Entitlements(WS) to  WebAPI.Entitlement(REST)
            #region Entitlement

            cfg.CreateMap<Entitlement, KalturaSubscriptionEntitlement>()
               .ForMember(dest => dest.EntitlementId, opt => opt.ResolveUsing(src => src.entitlementId))
               .ForMember(dest => dest.CurrentUses, opt => opt.ResolveUsing(src => src.currentUses))
               .ForMember(dest => dest.CurrentDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.currentDate)))
               .ForMember(dest => dest.LastViewDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.lastViewDate)))
               .ForMember(dest => dest.PurchaseDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.purchaseDate)))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.purchaseID))
               .ForMember(dest => dest.DeviceUDID, opt => opt.ResolveUsing(src => src.deviceUDID))
               .ForMember(dest => dest.DeviceName, opt => opt.ResolveUsing(src => src.deviceName))
               .ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.ResolveUsing(src => src.cancelWindow))
               .ForMember(dest => dest.MaxUses, opt => opt.ResolveUsing(src => src.maxUses))
               .ForMember(dest => dest.NextRenewalDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.nextRenewalDate)))
               .ForMember(dest => dest.IsRenewableForPurchase, opt => opt.ResolveUsing(src => src.recurringStatus))
               .ForMember(dest => dest.IsRenewable, opt => opt.ResolveUsing(src => src.isRenewable))
               .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.type))
               .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.endDate)))
               .ForMember(dest => dest.PaymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.paymentMethod)))
               .ForMember(dest => dest.IsInGracePeriod, opt => opt.ResolveUsing(src => src.IsInGracePeriod))
               .ForMember(dest => dest.PaymentGatewayId, opt => opt.ResolveUsing(src => GetNullableInt(src.paymentGatewayId)))
               .ForMember(dest => dest.PaymentMethodId, opt => opt.ResolveUsing(src => GetNullableInt(src.paymentMethodId)))
               .ForMember(dest => dest.ScheduledSubscriptionId, opt => opt.ResolveUsing(src => src.ScheduledSubscriptionId))
               .ForMember(dest => dest.IsSuspended, opt => opt.ResolveUsing(src => src.IsSuspended))
               .ForMember(dest => dest.ProductId, opt => opt.ResolveUsing(src => src.entitlementId))
               .ForMember(dest => dest.UnifiedPaymentId, opt => opt.ResolveUsing(src => src.UnifiedPaymentId))
               ;

            cfg.CreateMap<SubscriptionPurchase, KalturaSubscriptionEntitlement>()
              .ForMember(dest => dest.EntitlementId, opt => opt.ResolveUsing(src => src.purchaseId))
              .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => KalturaTransactionType.subscription))
              .ForMember(dest => dest.CurrentDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow)))
              .ForMember(dest => dest.PurchaseDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.entitlementDate)))
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => (int)src.purchaseId))
              .ForMember(dest => dest.PurchaseId, opt => opt.ResolveUsing(src => (int)src.purchaseId))
              .ForMember(dest => dest.DeviceName, opt => opt.ResolveUsing(src => src.deviceName))
              .ForMember(dest => dest.MaxUses, opt => opt.ResolveUsing(src => src.maxNumberOfViews))
              .ForMember(dest => dest.IsRenewable, opt => opt.ResolveUsing(src => src.isRecurring))
              .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.endDate)))
              .ForMember(dest => dest.UserId, opt => opt.ResolveUsing(src => src.siteGuid))
              .ForMember(dest => dest.HouseholdId, opt => opt.ResolveUsing(src => src.houseHoldId))
              .ForMember(dest => dest.ProductId, opt => opt.ResolveUsing(src => src.productId));
            
            cfg.CreateMap<Entitlement, KalturaPpvEntitlement>()
               .ForMember(dest => dest.EntitlementId, opt => opt.ResolveUsing(src => src.entitlementId))
               .ForMember(dest => dest.CurrentUses, opt => opt.ResolveUsing(src => src.currentUses))
               .ForMember(dest => dest.CurrentDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.currentDate)))
               .ForMember(dest => dest.LastViewDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.lastViewDate)))
               .ForMember(dest => dest.PurchaseDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.purchaseDate)))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.purchaseID))
               .ForMember(dest => dest.DeviceUDID, opt => opt.ResolveUsing(src => src.deviceUDID))
               .ForMember(dest => dest.DeviceName, opt => opt.ResolveUsing(src => src.deviceName))
               .ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.ResolveUsing(src => src.cancelWindow))
               .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.type))
               .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.endDate)))
               .ForMember(dest => dest.PaymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.paymentMethod)))
               .ForMember(dest => dest.MediaFileId, opt => opt.ResolveUsing(src => GetNullableInt(src.mediaFileID)))
               .ForMember(dest => dest.MediaId, opt => opt.ResolveUsing(src => GetNullableInt(src.mediaID)))
               .ForMember(dest => dest.MaxUses, opt => opt.ResolveUsing(src => src.maxUses))
               .ForMember(dest => dest.NextRenewalDate, opt => opt.ResolveUsing(src => GetNullableInt(0)))
               .ForMember(dest => dest.ProductId, opt => opt.ResolveUsing(src => src.entitlementId))
               ;
            cfg.CreateMap<PpvPurchase, KalturaPpvEntitlement>()
                //.ForMember(dest => dest.EntitlementId, opt => opt.ResolveUsing(src => src.ppv))
              .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => KalturaTransactionType.ppv))
                //.ForMember(dest => dest.CurrentUses, opt => opt.ResolveUsing(src => src.currentUses))
                //.ForMember(dest => dest.CurrentDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.currentDate)))
                //.ForMember(dest => dest.LastViewDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.lastViewDate)))
               .ForMember(dest => dest.PurchaseDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.entitlementDate)))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => (int)src.purchaseId))
                //.ForMember(dest => dest.DeviceUDID, opt => opt.ResolveUsing(src => src.deviceUDID))
               .ForMember(dest => dest.DeviceName, opt => opt.ResolveUsing(src => src.deviceName))
                //.ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.ResolveUsing(src => src.cancelWindow))
                //.ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.type))
               .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.endDate)))
                //.ForMember(dest => dest.PaymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.paymentMethod)))
                //.ForMember(dest => dest.MediaFileId, opt => opt.ResolveUsing(src => GetNullableInt(src.mediaFileID)))
               .ForMember(dest => dest.MediaFileId, opt => opt.ResolveUsing(src => GetNullableInt(src.contentId)))
                //.ForMember(dest => dest.MediaId, opt => opt.ResolveUsing(src => GetNullableInt(src.)))
               .ForMember(dest => dest.MaxUses, opt => opt.ResolveUsing(src => src.maxNumOfViews))
               .ForMember(dest => dest.NextRenewalDate, opt => opt.ResolveUsing(src => GetNullableInt(0)))
               .ForMember(dest => dest.UserId, opt => opt.ResolveUsing(src => src.siteGuid))
               .ForMember(dest => dest.HouseholdId, opt => opt.ResolveUsing(src => src.houseHoldId))
               ;

            cfg.CreateMap<Entitlement, KalturaCollectionEntitlement>()
              .ForMember(dest => dest.EntitlementId, opt => opt.ResolveUsing(src => src.entitlementId))
              .ForMember(dest => dest.CurrentUses, opt => opt.ResolveUsing(src => src.currentUses))
              .ForMember(dest => dest.CurrentDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.currentDate)))
              .ForMember(dest => dest.LastViewDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.lastViewDate)))
              .ForMember(dest => dest.PurchaseDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.purchaseDate)))
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.purchaseID))
              .ForMember(dest => dest.DeviceUDID, opt => opt.ResolveUsing(src => src.deviceUDID))
              .ForMember(dest => dest.DeviceName, opt => opt.ResolveUsing(src => src.deviceName))
              .ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.ResolveUsing(src => src.cancelWindow))
              .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.type))
              .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.endDate)))
              .ForMember(dest => dest.PaymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.paymentMethod)))
              .ForMember(dest => dest.MediaFileId, opt => opt.ResolveUsing(src => GetNullableInt(src.mediaFileID)))
              .ForMember(dest => dest.MediaId, opt => opt.ResolveUsing(src => GetNullableInt(src.mediaID)))
              .ForMember(dest => dest.MaxUses, opt => opt.ResolveUsing(src => src.maxUses))
              .ForMember(dest => dest.NextRenewalDate, opt => opt.ResolveUsing(src => GetNullableInt(0)))
              .ForMember(dest => dest.PurchaseId, opt => opt.ResolveUsing(src => src.purchaseID))
              .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => KalturaTransactionType.collection))
              .ForMember(dest => dest.ProductId, opt => opt.ResolveUsing(src => src.entitlementId))
              ;

            cfg.CreateMap<PpvPurchase, KalturaEntitlementCancellation>()
              .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => KalturaTransactionType.ppv))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => (int)src.purchaseId))
               .ForMember(dest => dest.ProductId, opt => opt.ResolveUsing(src => src.ppvCode))
               .ForMember(dest => dest.UserId, opt => opt.ResolveUsing(src => src.siteGuid))
               .ForMember(dest => dest.HouseholdId, opt => opt.ResolveUsing(src => src.houseHoldId))
               ;

            cfg.CreateMap<SubscriptionPurchase, KalturaEntitlementCancellation>()
              .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => KalturaTransactionType.subscription))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => (int)src.purchaseId))
               .ForMember(dest => dest.ProductId, opt => opt.ResolveUsing(src => src.productId))
               .ForMember(dest => dest.UserId, opt => opt.ResolveUsing(src => src.siteGuid))
               .ForMember(dest => dest.HouseholdId, opt => opt.ResolveUsing(src => src.houseHoldId))
               ;

            cfg.CreateMap<KalturaSubscriptionEntitlement, Entitlement>()
              .ForMember(dest => dest.purchaseID, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.paymentGatewayId, opt => opt.ResolveUsing(src => src.PaymentGatewayId))
              .ForMember(dest => dest.paymentMethodId, opt => opt.ResolveUsing(src => src.PaymentMethodId))
              .ForMember(dest => dest.type, opt => opt.ResolveUsing(src => eTransactionType.Subscription))
              ;

            cfg.CreateMap<Entitlement, KalturaEntitlement>().ConstructUsing(ConvertToKalturaEntitlement);

            // cfg.CreateMap<Entitlement, KalturaEntitlement>()
            //.ForMember(dest => dest.EntitlementId, opt => opt.ResolveUsing(src => src.entitlementId))
            //.ForMember(dest => dest.CurrentUses, opt => opt.ResolveUsing(src => src.currentUses))
            //.ForMember(dest => dest.CurrentDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.currentDate)))
            //.ForMember(dest => dest.LastViewDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.lastViewDate)))
            //.ForMember(dest => dest.PurchaseDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.purchaseDate)))
            //.ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.purchaseID))
            //.ForMember(dest => dest.DeviceUDID, opt => opt.ResolveUsing(src => src.deviceUDID))
            //.ForMember(dest => dest.DeviceName, opt => opt.ResolveUsing(src => src.deviceName))
            //.ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.ResolveUsing(src => src.cancelWindow))
            //.ForMember(dest => dest.MaxUses, opt => opt.ResolveUsing(src => src.maxUses))
            //.ForMember(dest => dest.NextRenewalDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.nextRenewalDate)))
            //.ForMember(dest => dest.IsRenewableForPurchase, opt => opt.ResolveUsing(src => src.recurringStatus))
            //.ForMember(dest => dest.IsRenewable, opt => opt.ResolveUsing(src => src.isRenewable))
            //.ForMember(dest => dest.MediaFileId, opt => opt.ResolveUsing(src => src.mediaFileID))
            //.ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.type))
            //.ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.endDate)))
            //.ForMember(dest => dest.PaymentMethod, opt => opt.ResolveUsing(src => src.paymentMethod))
            //.ForMember(dest => dest.IsInGracePeriod, opt => opt.ResolveUsing(src => src.IsInGracePeriod))
            //.ForMember(dest => dest.MediaId, opt => opt.ResolveUsing(src => src.mediaID));

            #endregion

            // BillingTransactions(WS) to  BillingTransactions(REST)
            #region Billing Transaction Container
            cfg.CreateMap<BillingTransactionContainer, KalturaBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtEndDate)))

               .ForMember(dest => dest.billingAction, opt => opt.ResolveUsing(src => src.m_eBillingAction))
               .ForMember(dest => dest.itemType, opt => opt.ResolveUsing(src => src.m_eItemType))
               .ForMember(dest => dest.paymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.m_ePaymentMethod)))
               .ForMember(dest => dest.isRecurring, opt => opt.ResolveUsing(src => src.m_bIsRecurring))
               .ForMember(dest => dest.billingProviderRef, opt => opt.ResolveUsing(src => src.m_nBillingProviderRef))
               .ForMember(dest => dest.purchaseID, opt => opt.ResolveUsing(src => src.m_nPurchaseID))
               .ForMember(dest => dest.purchasedItemName, opt => opt.ResolveUsing(src => src.m_sPurchasedItemName))
               .ForMember(dest => dest.purchasedItemCode, opt => opt.ResolveUsing(src => src.m_sPurchasedItemCode))
               .ForMember(dest => dest.recieptCode, opt => opt.ResolveUsing(src => src.m_sRecieptCode))
               .ForMember(dest => dest.remarks, opt => opt.ResolveUsing(src => src.m_sRemarks))
               .ForMember(dest => dest.paymentMethodExtraDetails, opt => opt.ResolveUsing(src => src.m_sPaymentMethodExtraDetails))
               .ForMember(dest => dest.price, opt => opt.ResolveUsing(src => src.m_Price))
               ;

            #endregion

            // BillingTransactions(WS) to  BillingTransactions(REST)
            #region Billing Transaction List
            cfg.CreateMap<BillingTransactionsResponse, KalturaBillingTransactionListResponse>()
               .ForMember(dest => dest.TotalCount, opt => opt.ResolveUsing(src => src.m_nTransactionsCount))
               .ForMember(dest => dest.transactions, opt => opt.ResolveUsing(src => src.m_Transactions));
            #endregion

            #region Price
            cfg.CreateMap<Price, KalturaPrice>()
              .ForMember(dest => dest.Amount, opt => opt.ResolveUsing(src => src.m_dPrice))
              .ForMember(dest => dest.Currency, opt => opt.ResolveUsing(src => src.m_oCurrency.m_sCurrencyCD3))
              .ForMember(dest => dest.CurrencySign, opt => opt.ResolveUsing(src => src.m_oCurrency.m_sCurrencySign));
            #endregion

            // BillingResponse
            #region Billing
            cfg.CreateMap<BillingResponse, KalturaBillingResponse>()
               .ForMember(dest => dest.ReceiptCode, opt => opt.ResolveUsing(src => src.m_sRecieptCode))
               .ForMember(dest => dest.ExternalReceiptCode, opt => opt.ResolveUsing(src => src.m_sExternalReceiptCode));
            #endregion

            // TransactionResponse to KalturaTransactionResponse
            #region Transaction
            cfg.CreateMap<TransactionResponse, KalturaTransaction>()
               .ForMember(dest => dest.PGReferenceID, opt => opt.ResolveUsing(src => src.PGReferenceID))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.TransactionID))
               .ForMember(dest => dest.State, opt => opt.ResolveUsing(src => src.State.ToString()))
               .ForMember(dest => dest.PGResponseID, opt => opt.ResolveUsing(src => src.PGResponseCode))
               .ForMember(dest => dest.FailReasonCode, opt => opt.ResolveUsing(src => src.FailReasonCode))
               .ForMember(dest => dest.CreatedAt, opt => opt.ResolveUsing(src => src.CreatedAt));
            #endregion

            #region Billing Transaction Container to User Billing Transaciton
            cfg.CreateMap<BillingTransactionContainer, KalturaUserBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtEndDate)))

               .ForMember(dest => dest.billingAction, opt => opt.ResolveUsing(src => src.m_eBillingAction))
               .ForMember(dest => dest.itemType, opt => opt.ResolveUsing(src => src.m_eItemType))
               .ForMember(dest => dest.paymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.m_ePaymentMethod)))
               .ForMember(dest => dest.isRecurring, opt => opt.ResolveUsing(src => src.m_bIsRecurring))
               .ForMember(dest => dest.billingProviderRef, opt => opt.ResolveUsing(src => src.m_nBillingProviderRef))
               .ForMember(dest => dest.purchaseID, opt => opt.ResolveUsing(src => src.m_nPurchaseID))
               .ForMember(dest => dest.purchasedItemName, opt => opt.ResolveUsing(src => src.m_sPurchasedItemName))
               .ForMember(dest => dest.purchasedItemCode, opt => opt.ResolveUsing(src => src.m_sPurchasedItemCode))
               .ForMember(dest => dest.recieptCode, opt => opt.ResolveUsing(src => src.m_sRecieptCode))
               .ForMember(dest => dest.remarks, opt => opt.ResolveUsing(src => src.m_sRemarks))
               .ForMember(dest => dest.paymentMethodExtraDetails, opt => opt.ResolveUsing(src => src.m_sPaymentMethodExtraDetails))
               .ForMember(dest => dest.price, opt => opt.ResolveUsing(src => src.m_Price))
               ;

            #endregion

            #region TransactionHistoryContainer to KalturaUserBillingTransaction
            cfg.CreateMap<TransactionHistoryContainer, KalturaUserBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtEndDate)))
               .ForMember(dest => dest.billingAction, opt => opt.ResolveUsing(src => src.m_eBillingAction))
               .ForMember(dest => dest.itemType, opt => opt.ResolveUsing(src => src.m_eItemType))
               .ForMember(dest => dest.paymentMethod, opt => opt.ResolveUsing(src => ConvertPaymentMethod(src.m_ePaymentMethod)))
               .ForMember(dest => dest.isRecurring, opt => opt.ResolveUsing(src => src.m_bIsRecurring))
               .ForMember(dest => dest.billingProviderRef, opt => opt.ResolveUsing(src => src.m_nBillingProviderRef))
               .ForMember(dest => dest.purchaseID, opt => opt.ResolveUsing(src => src.m_nPurchaseID))
               .ForMember(dest => dest.purchasedItemName, opt => opt.ResolveUsing(src => src.m_sPurchasedItemName))
               .ForMember(dest => dest.purchasedItemCode, opt => opt.ResolveUsing(src => src.m_sPurchasedItemCode))
               .ForMember(dest => dest.recieptCode, opt => opt.ResolveUsing(src => src.m_sRecieptCode))
               .ForMember(dest => dest.remarks, opt => opt.ResolveUsing(src => src.m_sRemarks))
               .ForMember(dest => dest.paymentMethodExtraDetails, opt => opt.ResolveUsing(src => src.m_sPaymentMethodExtraDetails))
               .ForMember(dest => dest.price, opt => opt.ResolveUsing(src => src.m_Price))
               .ForMember(dest => dest.UserID, opt => opt.ResolveUsing(src => src.SiteGuid))
               .ForMember(dest => dest.UserFullName, opt => opt.ResolveUsing(src => src.UserFullName))
               .ForMember(dest => dest.billingPriceType, opt => opt.ResolveUsing(src => src.billingPriceType))
               ;


            #endregion

            //#region Domains Billing Transactions
            //cfg.CreateMap<DomainsBillingTransactionsResponse, KalturaHouseholdsBillingTransactions>()
            //    .ForMember(dest => dest.DomainsBillingTransactions, opt => opt.ResolveUsing(src => src.billingTransactions));

            //#endregion

            //#region Domain Billing Transactions
            //cfg.CreateMap<DomainBillingTransactionsResponse, >()
            //    .ForMember(dest => dest.UsersBillingTransactions, opt => opt.ResolveUsing(src => src.m_BillingTransactionResponses))
            //    .ForMember(dest => dest.HouseholdId, opt => opt.ResolveUsing(src => src.m_nDomainID));
            //#endregion

            //#region User Billing Transactions
            //cfg.CreateMap<UserBillingTransactionsResponse, KalturaUserBillingTransactions>()
            //    .ForMember(dest => dest.SiteGuid, opt => opt.ResolveUsing(src => src.m_sSiteGUID))
            //    .ForMember(dest => dest.BillingTransactions, opt => opt.ResolveUsing(src => src.m_BillingTransactionResponse));
            //#endregion

            #region Asset Item Prices
            cfg.CreateMap<AssetItemPrices, KalturaAssetPrice>()
              .ForMember(dest => dest.AssetId, opt => opt.ResolveUsing(src => src.AssetId))
              .ForMember(dest => dest.AssetType, opt => opt.ResolveUsing(src => src.AssetType))
              .ForMember(dest => dest.FilePrices, opt => opt.ResolveUsing(src => src.PriceContainers))
              ;
            #endregion

            #region Asset Files
            cfg.CreateMap<KalturaPersonalAssetRequest, AssetFiles>()
              .ForMember(dest => dest.AssetId, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.AssetType, opt => opt.ResolveUsing(src => src.Type))
              .ForMember(dest => dest.FileIds, opt => opt.ResolveUsing(src =>
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
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name));

            cfg.CreateMap<KalturaCDVRAdapterProfile, CDVRAdapter>()
               .ForMember(dest => dest.ID, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.ResolveUsing(src => src.AdapterUrl))
               .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.IsActive))
               .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertCDVRAdapterSettings(src.Settings)))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.ResolveUsing(src => src.ExternalIdentifier))
               .ForMember(dest => dest.DynamicLinksSupport, opt => opt.ResolveUsing(src => src.DynamicLinksSupport))
               .ForMember(dest => dest.SharedSecret, opt => opt.ResolveUsing(src => src.SharedSecret));

            cfg.CreateMap<CDVRAdapter, KalturaCDVRAdapterProfile>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.ResolveUsing(src => src.AdapterUrl))
              .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertCDVRAdapterSettings(src.Settings)))
              .ForMember(dest => dest.ExternalIdentifier, opt => opt.ResolveUsing(src => src.ExternalIdentifier))
              .ForMember(dest => dest.DynamicLinksSupport, opt => opt.ResolveUsing(src => src.DynamicLinksSupport))
              .ForMember(dest => dest.SharedSecret, opt => opt.ResolveUsing(src => src.SharedSecret));


            // LicensedLinkResponse to KalturaLicensedUrls
            cfg.CreateMap<LicensedLinkResponse, KalturaLicensedUrl>()
               .ForMember(dest => dest.MainUrl, opt => opt.ResolveUsing(src => src.mainUrl))
               .ForMember(dest => dest.AltUrl, opt => opt.ResolveUsing(src => src.altUrl));

            #region Recordings

            // KalturaRecording to Recording
            cfg.CreateMap<KalturaRecording, Recording>()
               .ForMember(dest => dest.EpgId, opt => opt.ResolveUsing(src => src.AssetId))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.RecordingStatus, opt => opt.ResolveUsing(src => ConvertKalturaRecordingStatus(src.Status)))
               .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertKalturaRecordingType(src.Type)))
               .ForMember(dest => dest.ViewableUntilDate, opt => opt.ResolveUsing(src => src.ViewableUntilDate))
               .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertFromUnixTimestamp(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertFromUnixTimestamp(src.UpdateDate)));

            // Recording to KalturaRecording
            cfg.CreateMap<Recording, KalturaRecording>()
               .ForMember(dest => dest.AssetId, opt => opt.ResolveUsing(src => src.EpgId))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertTstvRecordingStatus(src.RecordingStatus)))
               .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertRecordingType(src.Type)))
               .ForMember(dest => dest.IsProtected, opt => opt.ResolveUsing(src => IsRecordingProtected(src.ProtectedUntilDate)))
               .ForMember(dest => dest.ViewableUntilDate, opt => opt.ResolveUsing(src => src.ViewableUntilDate))
               .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.UpdateDate)));

            // KalturaExternalRecording to ExternalRecording
            cfg.CreateMap<KalturaExternalRecording, ExternalRecording>()
               .ForMember(dest => dest.EpgId, opt => opt.ResolveUsing(src => src.AssetId))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.RecordingStatus, opt => opt.ResolveUsing(src => ConvertKalturaRecordingStatus(src.Status)))
               .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertKalturaRecordingType(src.Type)))
               .ForMember(dest => dest.ViewableUntilDate, opt => opt.ResolveUsing(src => src.ViewableUntilDate))
               .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertFromUnixTimestamp(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertFromUnixTimestamp(src.UpdateDate)))
               .ForMember(dest => dest.ExternalDomainRecordingId, opt => opt.ResolveUsing(src => src.ExternalId))
               .ForMember(dest => dest.Status, opt => opt.Ignore());

            // ExternalRecording to KalturaExternalRecording
            cfg.CreateMap<ExternalRecording, KalturaExternalRecording>()
               .ForMember(dest => dest.AssetId, opt => opt.ResolveUsing(src => src.EpgId))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertTstvRecordingStatus(src.RecordingStatus)))
               .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertRecordingType(src.Type)))
               .ForMember(dest => dest.IsProtected, opt => opt.ResolveUsing(src => IsRecordingProtected(src.ProtectedUntilDate)))
               .ForMember(dest => dest.ViewableUntilDate, opt => opt.ResolveUsing(src => src.ViewableUntilDate))
               .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.UpdateDate)))
               .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.ExternalDomainRecordingId));

            // KalturaSeriesRecording to SeriesRecording
            cfg.CreateMap<KalturaSeriesRecording, SeriesRecording>()
               .ForMember(dest => dest.EpgId, opt => opt.ResolveUsing(src => src.EpgId))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.EpgChannelId, opt => opt.ResolveUsing(src => src.ChannelId))
               .ForMember(dest => dest.SeasonNumber, opt => opt.ResolveUsing(src => src.SeasonNumber))
               .ForMember(dest => dest.SeriesId, opt => opt.ResolveUsing(src => src.SeriesId))
               .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertKalturaRecordingType(src.Type)))
               .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertFromUnixTimestamp(src.CreateDate)));

            // SeriesRecording to KalturaSeriesRecording
            cfg.CreateMap<SeriesRecording, KalturaSeriesRecording>()
               .ForMember(dest => dest.EpgId, opt => opt.ResolveUsing(src => src.EpgId))
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.ChannelId, opt => opt.ResolveUsing(src => src.EpgChannelId))
               .ForMember(dest => dest.SeasonNumber, opt => opt.ResolveUsing(src => src.SeasonNumber > 0 ? (int?)src.SeasonNumber : null))
               .ForMember(dest => dest.SeriesId, opt => opt.ResolveUsing(src => src.SeriesId))
               .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertRecordingType(src.Type)))
               .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.UpdateDate)))
               .ForMember(dest => dest.ExcludedSeasons, opt => opt.ResolveUsing(src => src.ExcludedSeasons));
            #endregion

            #region Household Quota
            cfg.CreateMap<DomainQuotaResponse, KalturaHouseholdQuota>()
               .ForMember(dest => dest.AvailableQuota, opt => opt.ResolveUsing(src => src.AvailableQuota))
               .ForMember(dest => dest.TotalQuota, opt => opt.ResolveUsing(src => src.TotalQuota));

            #endregion

            //KalturaHouseholdPremiumService
            cfg.CreateMap<ServiceObject, KalturaHouseholdPremiumService>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name));

            // KalturaAssetFileContext to EntitlementResponse
            cfg.CreateMap<KalturaAssetFileContext, EntitlementResponse>()
              .ForMember(dest => dest.ViewLifeCycle, opt => opt.ResolveUsing(src => src.ViewLifeCycle))
              .ForMember(dest => dest.FullLifeCycle, opt => opt.ResolveUsing(src => src.FullLifeCycle))
            .ForMember(dest => dest.IsOfflinePlayBack, opt => opt.ResolveUsing(src => src.IsOfflinePlayBack));

            // EntitlementResponse to KalturaAssetFileContext
            cfg.CreateMap<EntitlementResponse, KalturaAssetFileContext>()
              .ForMember(dest => dest.ViewLifeCycle, opt => opt.ResolveUsing(src => src.ViewLifeCycle))
              .ForMember(dest => dest.FullLifeCycle, opt => opt.ResolveUsing(src => src.FullLifeCycle))
            .ForMember(dest => dest.IsOfflinePlayBack, opt => opt.ResolveUsing(src => src.IsOfflinePlayBack));

            cfg.CreateMap<MediaFile, KalturaPlaybackSource>()
              .ForMember(dest => dest.AssetId, opt => opt.ResolveUsing(src => src.MediaId.ToString()))
              .ForMember(dest => dest.Duration, opt => opt.ResolveUsing(src => src.Duration))
              .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.ExternalId))
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.Type))
              .ForMember(dest => dest.Url, opt => opt.ResolveUsing(src => src.DirectUrl))
              .ForMember(dest => dest.DrmId, opt => opt.ResolveUsing(src => src.DrmId))
              .ForMember(dest => dest.FileExtention, opt => opt.ResolveUsing(src => src.Url.Substring(src.Url.LastIndexOf('.'))))
              .ForMember(dest => dest.Protocols, opt => opt.ResolveUsing(src => src.Url.StartsWith("https") ? "https" : src.Url.StartsWith("http") ? "http" : string.Empty))                          
              .ForMember(dest => dest.Format, opt => opt.ResolveUsing(src => src.StreamerType.HasValue ? src.StreamerType.ToString(): string.Empty))
              .ForMember(dest => dest.AdsParams, opt => opt.ResolveUsing(src => src.AdsParam))
              .ForMember(dest => dest.AdsPolicy, opt => opt.ResolveUsing(src => ConvertAdsPolicy(src.AdsPolicy)));

            cfg.CreateMap<PlaybackContextResponse, KalturaPlaybackContext>()
              .ForMember(dest => dest.Sources, opt => opt.ResolveUsing(src => src.Files))
              .ForMember(dest => dest.Messages, opt => opt.ResolveUsing(src => new List<ApiObjects.Response.Status>()));

            cfg.CreateMap<ApiObjects.Response.Status, KalturaAccessControlMessage>()
              .ForMember(dest => dest.Code, opt => opt.ResolveUsing(src => ((ApiObjects.Response.eResponseStatus)src.Code).ToString()))
              .ForMember(dest => dest.Message, opt => opt.ResolveUsing(src => src.Message));

            cfg.CreateMap<KalturaCompensation, Compensation>()
              .ForMember(dest => dest.Amount, opt => opt.ResolveUsing(src => src.Amount))
              .ForMember(dest => dest.TotalRenewals, opt => opt.ResolveUsing(src => src.TotalRenewalIterations))
              .ForMember(dest => dest.CompensationType, opt => opt.ResolveUsing(src => ConvertCompensationType(src.CompensationType)))
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.SubscriptionId, opt => opt.ResolveUsing(src => src.SubscriptionId))
              .ForMember(dest => dest.PurchaseId, opt => opt.ResolveUsing(src => src.PurchaseId));

            cfg.CreateMap<Compensation, KalturaCompensation>()
              .ForMember(dest => dest.Amount, opt => opt.ResolveUsing(src => src.Amount))
              .ForMember(dest => dest.TotalRenewalIterations, opt => opt.ResolveUsing(src => src.TotalRenewals))
              .ForMember(dest => dest.CompensationType, opt => opt.ResolveUsing(src => ConvertCompensationType(src.CompensationType)))
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.SubscriptionId, opt => opt.ResolveUsing(src => src.SubscriptionId))
              .ForMember(dest => dest.PurchaseId, opt => opt.ResolveUsing(src => src.PurchaseId))
              .ForMember(dest => dest.AppliedRenewalIterations, opt => opt.ResolveUsing(src => src.Renewals));

            cfg.CreateMap<APILogic.ConditionalAccess.AdsControlData, KalturaAdsSource>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.FileId))
              .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.FileType))
              .ForMember(dest => dest.AdsParams, opt => opt.ResolveUsing(src => src.AdsParam))
              .ForMember(dest => dest.AdsPolicy, opt => opt.ResolveUsing(src => ConvertAdsPolicy(src.AdsPolicy)));

            cfg.CreateMap<APILogic.ConditionalAccess.Modules.EntitlementRenewal, KalturaEntitlementRenewal>()
             .ForMember(dest => dest.PurchaseId, opt => opt.ResolveUsing(src => src.PurchaseId))
             .ForMember(dest => dest.SubscriptionId, opt => opt.ResolveUsing(src => src.SubscriptionId))
             .ForMember(dest => dest.Price, opt => opt.ResolveUsing(src => src.Price))
             .ForMember(dest => dest.Date, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.Date)));

            cfg.CreateMap<APILogic.ConditionalAccess.Modules.EntitlementRenewalBase, KalturaEntitlementRenewalBase>()
             .ForMember(dest => dest.PurchaseId, opt => opt.ResolveUsing(src => src.PurchaseId))
             .ForMember(dest => dest.SubscriptionId, opt => opt.ResolveUsing(src => src.SubscriptionId))
             .ForMember(dest => dest.Price, opt => opt.ResolveUsing(src => src.PriceAmount));

            cfg.CreateMap<APILogic.ConditionalAccess.Modules.UnifiedPaymentRenewal, KalturaUnifiedPaymentRenewal>()
             .ForMember(dest => dest.UnifiedPaymentId, opt => opt.ResolveUsing(src => src.UnifiedPaymentId))
             .ForMember(dest => dest.Entitlements, opt => opt.ResolveUsing(src => src.Entitlements))
             .ForMember(dest => dest.Price, opt => opt.ResolveUsing(src => src.Price))
             .ForMember(dest => dest.Date, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.Date)));
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
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown entitlement type");
                    break;
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

            long currentUtcTime = SerializationUtils.GetCurrentUtcTimeInUnixTimestamp();
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
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown recordingType type");
            }
            return result;
        }

        public static KalturaRecordingType? ConvertNullableRecordingType(RecordingType? recordingType)
        {
            KalturaRecordingType? result = null;
            if (recordingType.HasValue)
            {
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
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown recordingType type");
                }
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

        // TransactionType to eTransactionType
        public static eTransactionType ConvertTransactionType(KalturaTransactionType clientTransactionType)
        {
            eTransactionType result;
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
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown transaction type");
            }
            return result;
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