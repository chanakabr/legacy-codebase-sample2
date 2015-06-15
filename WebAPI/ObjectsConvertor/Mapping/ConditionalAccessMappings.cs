using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;
using WebAPI.Exceptions;

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
                        break;
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
        }
    }
}