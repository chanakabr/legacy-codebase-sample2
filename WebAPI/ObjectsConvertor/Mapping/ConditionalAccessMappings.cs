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
            //TransactionType to eTransactionType
            Mapper.CreateMap<TransactionType, WebAPI.ConditionalAccess.eTransactionType>().ConstructUsing((TransactionType transactionType) =>
            {
                WebAPI.ConditionalAccess.eTransactionType result;
                switch (transactionType)
                {
                    case TransactionType.ppv:
                        result = WebAPI.ConditionalAccess.eTransactionType.PPV;
                        break;
                    case TransactionType.subscription:
                        result = WebAPI.ConditionalAccess.eTransactionType.Subscription;
                        break;
                    case TransactionType.collection:
                        result = WebAPI.ConditionalAccess.eTransactionType.Collection;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown transaction type");                        
                }
                return result;
            });

            //WebAPI.ConditionalAccess.Entitlements(WS) to  WebAPI.Models.ConditionalAccess.Entitlement(REST)
            Mapper.CreateMap<ConditionalAccess.Entitlements, Entitlement>()
               .ForMember(dest => dest.entitlementsId, opt => opt.MapFrom(src => src.entitlementsId))
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


            //WebAPI.ConditionalAccess.BillingTransactions(WS) to  Models.ConditionalAccess.BillingTransactions(REST)
            Mapper.CreateMap<ConditionalAccess.BillingTransactionContainer, BillingTransaction>()
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

            //WebAPI.ConditionalAccess.BillingTransactions(WS) to  Models.ConditionalAccess.BillingTransactions(REST)
            Mapper.CreateMap<ConditionalAccess.BillingTransactionsResponse, BillingTransactions>()
               .ForMember(dest => dest.transactionsCount, opt => opt.MapFrom(src => src.m_nTransactionsCount))
               .ForMember(dest => dest.transactions, opt => opt.MapFrom(src => src.m_Transactions));

            
            Mapper.CreateMap<ConditionalAccess.Price, Models.Pricing.Price>()
               .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.m_dPrice))
               .ForMember(dest => dest.currency, opt => opt.MapFrom(src => src.m_oCurrency));

            Mapper.CreateMap<ConditionalAccess.Currency, Models.Pricing.Currency>()
               .ForMember(dest => dest.currencyCD2, opt => opt.MapFrom(src => src.m_sCurrencyCD2))
               .ForMember(dest => dest.currencyCD3, opt => opt.MapFrom(src => src.m_sCurrencyCD3))
               .ForMember(dest => dest.currencySign, opt => opt.MapFrom(src => src.m_sCurrencySign))
               .ForMember(dest => dest.currencyID, opt => opt.MapFrom(src => src.m_nCurrencyID));

            // BillingResponse
            Mapper.CreateMap<ConditionalAccess.BillingResponse, BillingResponse>()
               .ForMember(dest => dest.ReceiptCode, opt => opt.MapFrom(src => src.m_sRecieptCode))
               .ForMember(dest => dest.ExternalReceiptCode, opt => opt.MapFrom(src => src.m_sExternalReceiptCode));
        }
    }
}