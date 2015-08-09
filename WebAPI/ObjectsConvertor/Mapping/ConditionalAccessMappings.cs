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
            Mapper.CreateMap<ConditionalAccess.Entitlements, KalturaEntitlement>()
               .ForMember(dest => dest.entitlementId, opt => opt.MapFrom(src => src.entitlementsId))
               .ForMember(dest => dest.currentUses, opt => opt.MapFrom(src => src.currentUses))
               .ForMember(dest => dest.currentDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.currentDate)))
               .ForMember(dest => dest.lastViewDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.lastViewDate)))
               .ForMember(dest => dest.purchaseDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.purchaseDate)))
               .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.purchaseID))
               .ForMember(dest => dest.deviceUDID, opt => opt.MapFrom(src => src.deviceUDID))
               .ForMember(dest => dest.deviceName, opt => opt.MapFrom(src => src.deviceName))
               .ForMember(dest => dest.cancelWindow, opt => opt.MapFrom(src => src.cancelWindow))
               .ForMember(dest => dest.maxUses, opt => opt.MapFrom(src => src.maxUses))
               .ForMember(dest => dest.nextRenewalDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.nextRenewalDate)))
               .ForMember(dest => dest.recurringStatus, opt => opt.MapFrom(src => src.recurringStatus))
               .ForMember(dest => dest.isRenewable, opt => opt.MapFrom(src => src.isRenewable))
               .ForMember(dest => dest.mediaFileID, opt => opt.MapFrom(src => src.mediaFileID))
               .ForMember(dest => dest.type, opt => opt.MapFrom(src => src.type))
               .ForMember(dest => dest.paymentMethod, opt => opt.MapFrom(src => src.paymentMethod));

            // WebAPI.ConditionalAccess.BillingTransactions(WS) to  Models.ConditionalAccess.BillingTransactions(REST)
            Mapper.CreateMap<ConditionalAccess.BillingTransactionContainer, KalturaBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.MapFrom(src => Utils.SerializationUtils.ConvertToUnixTimestamp(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.MapFrom(src => Utils.SerializationUtils.ConvertToUnixTimestamp(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => Utils.SerializationUtils.ConvertToUnixTimestamp(src.m_dtEndDate)))

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

            // WebAPI.ConditionalAccess.BillingTransactions(WS) to  Models.ConditionalAccess.BillingTransactions(REST)
            Mapper.CreateMap<ConditionalAccess.BillingTransactionsResponse, KalturaBillingTransactions>()
               .ForMember(dest => dest.transactionsCount, opt => opt.MapFrom(src => src.m_nTransactionsCount))
               .ForMember(dest => dest.transactions, opt => opt.MapFrom(src => src.m_Transactions));


            Mapper.CreateMap<ConditionalAccess.Price, Models.Pricing.KalturaPrice>()
               .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.m_dPrice))
               .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3));

            // BillingResponse
            Mapper.CreateMap<ConditionalAccess.BillingResponse, KalturaBillingResponse>()
               .ForMember(dest => dest.ReceiptCode, opt => opt.MapFrom(src => src.m_sRecieptCode))
               .ForMember(dest => dest.ExternalReceiptCode, opt => opt.MapFrom(src => src.m_sExternalReceiptCode));

            // TransactionResponse to KalturaTransactionResponse
            Mapper.CreateMap<ConditionalAccess.TransactionResponse, KalturaTransactionResponse>()
               .ForMember(dest => dest.PGReferenceID, opt => opt.MapFrom(src => src.PGReferenceID))
               .ForMember(dest => dest.TransactionID, opt => opt.MapFrom(src => src.TransactionID))
               .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State.ToString()))
               .ForMember(dest => dest.PGResponseID, opt => opt.MapFrom(src => src.PGResponseCode))
               .ForMember(dest => dest.FailReasonCode, opt => opt.MapFrom(src => src.FailReasonCode))
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            // PermittedMediaContainer to KalturaPermittedMedia
            Mapper.CreateMap<ConditionalAccess.PermittedMediaContainer, KalturaPermittedMedia>()
               .ForMember(dest => dest.CurrentViews, opt => opt.MapFrom(src => src.m_nCurrentUses))
               .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.m_sDeviceName))
               .ForMember(dest => dest.DeviceUDID, opt => opt.MapFrom(src => src.m_sDeviceUDID))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.m_dEndDate))
               .ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.m_nMediaFileID))
               .ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.MapFrom(src => src.m_bCancelWindow))
               .ForMember(dest => dest.LastViewDate, opt => opt.MapFrom(src => src.m_dLastViewDate))
               .ForMember(dest => dest.MaxViews, opt => opt.MapFrom(src => src.m_nMaxUses))
               .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.m_nMediaID))
               .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.m_purchaseMethod))
               .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => src.m_dPurchaseDate));
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