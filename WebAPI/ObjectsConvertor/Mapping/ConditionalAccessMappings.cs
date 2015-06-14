using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Mapping.ObjectsConvertor
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
                        break;
                }
                return result;
            });

            //WebAPI.ConditionalAccess.Entitlements(WS) to  WebAPI.Models.ConditionalAccess.Entitlement(REST)
            Mapper.CreateMap<ConditionalAccess.Entitlements, Entitlement>()
               .ForMember(dest => dest.entitlementsId, opt => opt.MapFrom(src => src.entitlementsId))
               .ForMember(dest => dest.currentUses, opt => opt.MapFrom(src => src.currentUses))
               .ForMember(dest => dest.currentDate, opt => opt.MapFrom(src => src.currentDate))
               .ForMember(dest => dest.lastViewDate, opt => opt.MapFrom(src => src.lastViewDate))
               .ForMember(dest => dest.purchaseDate, opt => opt.MapFrom(src => src.purchaseDate))
               .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.purchaseID))
               .ForMember(dest => dest.deviceUDID, opt => opt.MapFrom(src => src.deviceUDID))
               .ForMember(dest => dest.deviceName, opt => opt.MapFrom(src => src.deviceName))
               .ForMember(dest => dest.cancelWindow, opt => opt.MapFrom(src => src.cancelWindow))
               .ForMember(dest => dest.maxUses, opt => opt.MapFrom(src => src.maxUses))
               .ForMember(dest => dest.nextRenewalDate, opt => opt.MapFrom(src => src.nextRenewalDate))
               .ForMember(dest => dest.recurringStatus, opt => opt.MapFrom(src => src.recurringStatus))
               .ForMember(dest => dest.isRenewable, opt => opt.MapFrom(src => src.isRenewable))
               .ForMember(dest => dest.mediaFileID, opt => opt.MapFrom(src => src.mediaFileID))
               .ForMember(dest => dest.type, opt => opt.MapFrom(src => src.type))
               .ForMember(dest => dest.paymentMethod, opt => opt.MapFrom(src => src.paymentMethod))
               ;


            //eTransactionType to TransactionType
            //Mapper.CreateMap<WebAPI.ConditionalAccess.eTransactionType, TransactionType>().ConstructUsing((WebAPI.ConditionalAccess.eTransactionType transactionType) =>
            //{
            //    //TransactionType result;
            //    //switch (transactionType)
            //    //{
            //    //    case WebAPI.ConditionalAccess.eTransactionType.PPV:
            //    //        result = TransactionType.ppv;
            //    //        break;
            //    //    case WebAPI.ConditionalAccess.eTransactionType.Subscription:
            //    //        result = TransactionType.subscription;
            //    //        break;
            //    //    case WebAPI.ConditionalAccess.eTransactionType.Collection:
            //    //        result = TransactionType.collection;
            //    //        break;
            //    //    default:
            //    //        throw new ClientException((int)StatusCode.Error, "Unknown transaction type");
            //    //        break;
            //    //}
            //    //return result;
            //});

            ////eTransactionType to TransactionType
            //Mapper.CreateMap<WebAPI.ConditionalAccess.PaymentMethod, PaymentMethod>().ConstructUsing((WebAPI.ConditionalAccess.PaymentMethod paymentMethod) =>
            //{
            //    PaymentMethod result;
            //    switch (paymentMethod)
            //    {
            //        case WebAPI.ConditionalAccess.PaymentMethod.Unknown:
            //            result = PaymentMethod.Unknown;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.CreditCard:
            //            result = PaymentMethod.CreditCard;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.SMS:
            //            result = PaymentMethod.SMS;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.PayPal:
            //            result = PaymentMethod.PayPal;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.DebitCard:
            //            result = PaymentMethod.DebitCard;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.Ideal:
            //            result = PaymentMethod.Ideal;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.Incaso:
            //            result = PaymentMethod.Incaso;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.Gift:
            //            result = PaymentMethod.Gift;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.Visa:
            //            result = PaymentMethod.Visa;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.MasterCard:
            //            result = PaymentMethod.MasterCard;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.InApp:
            //            result = PaymentMethod.InApp;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.M1:
            //            result = PaymentMethod.M1;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.ChangeSubscription:
            //            result = PaymentMethod.ChangeSubscription;
            //            break;
            //        case WebAPI.ConditionalAccess.PaymentMethod.Offline:
            //            result = PaymentMethod.Offline;
            //            break;
            //        default:
            //            throw new ClientException((int)StatusCode.Error, "Unknown transaction type");
            //            break;
            //    }
            //    return result;
            //});

        }
    }
}