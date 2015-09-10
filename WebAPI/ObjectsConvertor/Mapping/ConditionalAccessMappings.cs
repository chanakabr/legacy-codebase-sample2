using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class ConditionalAccessMappings
    {
        public static void RegisterMappings()
        {
            // WebAPI.ConditionalAccess.Entitlements(WS) to  WebAPI.Models.ConditionalAccess.Entitlement(REST)
            #region Entitlement
            Mapper.CreateMap<ConditionalAccess.Entitlement, KalturaEntitlement>()
               .ForMember(dest => dest.EntitlementId, opt => opt.MapFrom(src => src.entitlementId))
               .ForMember(dest => dest.CurrentUses, opt => opt.MapFrom(src => src.currentUses))
               .ForMember(dest => dest.CurrentDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.currentDate)))
               .ForMember(dest => dest.LastViewDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.lastViewDate)))
               .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.purchaseDate)))
               .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => src.purchaseID))
               .ForMember(dest => dest.DeviceUDID, opt => opt.MapFrom(src => src.deviceUDID))
               .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.deviceName))
               .ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.MapFrom(src => src.cancelWindow))
               .ForMember(dest => dest.MaxUses, opt => opt.MapFrom(src => src.maxUses))
               .ForMember(dest => dest.NextRenewalDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.nextRenewalDate)))
               .ForMember(dest => dest.IsRenewableForPurchase, opt => opt.MapFrom(src => src.recurringStatus))
               .ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.isRenewable))
               .ForMember(dest => dest.MediaFileId, opt => opt.MapFrom(src => src.mediaFileID))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.endDate)))
               .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.paymentMethod))
               .ForMember(dest => dest.IsInGracePeriod, opt => opt.MapFrom(src => src.IsInGracePeriod))
               .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.mediaID));
            #endregion

            // WebAPI.ConditionalAccess.BillingTransactions(WS) to  Models.ConditionalAccess.BillingTransactions(REST)
            #region Billing Transaction Container
            Mapper.CreateMap<ConditionalAccess.BillingTransactionContainer, KalturaBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtEndDate)))

               .ForMember(dest => dest.billingAction, opt => opt.MapFrom(src => src.m_eBillingAction))
               .ForMember(dest => dest.itemType, opt => opt.MapFrom(src => src.m_eItemType))
               .ForMember(dest => dest.paymentMethod, opt => opt.MapFrom(src => src.m_ePaymentMethod))
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

            // WebAPI.ConditionalAccess.BillingTransactions(WS) to  Models.ConditionalAccess.BillingTransactions(REST)
            #region Billing Transaction List
            Mapper.CreateMap<ConditionalAccess.BillingTransactionsResponse, KalturaBillingTransactionListResponse>()
               .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.m_nTransactionsCount))
               .ForMember(dest => dest.transactions, opt => opt.MapFrom(src => src.m_Transactions));
            #endregion

            #region Price
            Mapper.CreateMap<WebAPI.ConditionalAccess.Price, Models.Pricing.KalturaPrice>()
              .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.m_dPrice))
              .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3))
              .ForMember(dest => dest.CurrencySign, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencySign));
            #endregion

            // BillingResponse
            #region Billing
            Mapper.CreateMap<ConditionalAccess.BillingResponse, KalturaBillingResponse>()
               .ForMember(dest => dest.ReceiptCode, opt => opt.MapFrom(src => src.m_sRecieptCode))
               .ForMember(dest => dest.ExternalReceiptCode, opt => opt.MapFrom(src => src.m_sExternalReceiptCode));
            #endregion

            // TransactionResponse to KalturaTransactionResponse
            #region Transaction
            Mapper.CreateMap<ConditionalAccess.TransactionResponse, KalturaTransaction>()
               .ForMember(dest => dest.PGReferenceID, opt => opt.MapFrom(src => src.PGReferenceID))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TransactionID))
               .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State.ToString()))
               .ForMember(dest => dest.PGResponseID, opt => opt.MapFrom(src => src.PGResponseCode))
               .ForMember(dest => dest.FailReasonCode, opt => opt.MapFrom(src => src.FailReasonCode))
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
            #endregion

            #region Billing Transaction Container to User Billing Transaciton
            Mapper.CreateMap<ConditionalAccess.BillingTransactionContainer, KalturaUserBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtEndDate)))

               .ForMember(dest => dest.billingAction, opt => opt.MapFrom(src => src.m_eBillingAction))
               .ForMember(dest => dest.itemType, opt => opt.MapFrom(src => src.m_eItemType))
               .ForMember(dest => dest.paymentMethod, opt => opt.MapFrom(src => src.m_ePaymentMethod))
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
            //#region Domains Billing Transactions
            //Mapper.CreateMap<ConditionalAccess.DomainsBillingTransactionsResponse, KalturaHouseholdsBillingTransactions>()
            //    .ForMember(dest => dest.DomainsBillingTransactions, opt => opt.MapFrom(src => src.billingTransactions));

            //#endregion

            //#region Domain Billing Transactions
            //Mapper.CreateMap<ConditionalAccess.DomainBillingTransactionsResponse, >()
            //    .ForMember(dest => dest.UsersBillingTransactions, opt => opt.MapFrom(src => src.m_BillingTransactionResponses))
            //    .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.m_nDomainID));
            //#endregion

            //#region User Billing Transactions
            //Mapper.CreateMap<ConditionalAccess.UserBillingTransactionsResponse, KalturaUserBillingTransactions>()
            //    .ForMember(dest => dest.SiteGuid, opt => opt.MapFrom(src => src.m_sSiteGUID))
            //    .ForMember(dest => dest.BillingTransactions, opt => opt.MapFrom(src => src.m_BillingTransactionResponse));
            //#endregion

        }

        // TransactionType to eTransactionType
        public static WebAPI.ConditionalAccess.eTransactionType ConvertTransactionType(KalturaTransactionType clientTransactionType)
        {
            WebAPI.ConditionalAccess.eTransactionType result;
            switch (clientTransactionType)
            {
                case KalturaTransactionType.ppv:
                    result = WebAPI.ConditionalAccess.eTransactionType.PPV;
                    break;
                case KalturaTransactionType.subscription:
                    result = WebAPI.ConditionalAccess.eTransactionType.Subscription;
                    break;
                case KalturaTransactionType.collection:
                    result = WebAPI.ConditionalAccess.eTransactionType.Collection;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown transaction type");
            }
            return result;
        }
    }
}