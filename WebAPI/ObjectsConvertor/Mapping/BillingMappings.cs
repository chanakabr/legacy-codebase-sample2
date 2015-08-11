using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Billing;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class BillingMappings
    {
        public static void RegisterMappings()
        {
            //PaymentGWConfigResponse to PaymentGWConfigResponse
            Mapper.CreateMap<PaymentGatewaySettingsResponse, WebAPI.Models.Billing.KalturaPaymentGatewaySettingsResponse>()
                .ForMember(dest => dest.pgw, opt => opt.MapFrom(src => src.pgw));

            //PaymentGWConfigResponse to PaymentGWConfigResponse
            Mapper.CreateMap<PaymentGateway, WebAPI.Models.Billing.KalturaPaymentGatewayProfile>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
                .ForMember(dest => dest.TransactUrl, opt => opt.MapFrom(src => src.TransactUrl))
                .ForMember(dest => dest.StatusUrl, opt => opt.MapFrom(src => src.StatusUrl))
                .ForMember(dest => dest.RenewUrl, opt => opt.MapFrom(src => src.RenewUrl))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertPaymentGatewaySettings(src.Settings)))
                .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));


            Mapper.CreateMap<PaymentGatewayResponse, WebAPI.Models.Billing.KalturaPaymentGatewayResponse>()
                 .ForMember(dest => dest.pgw, opt => opt.MapFrom(src => src.pgw));

            Mapper.CreateMap<PaymentGatewayBase, WebAPI.Models.Billing.KalturaPaymentGatewayBaseProfile>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));

            ////from local object to WS object            
            //Mapper.CreateMap<WebAPI.Models.Billing.KalturaPaymentGatewayData, PaymentGateway>()
            //    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            //    .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
            //    .ForMember(dest => dest.TransactUrl, opt => opt.MapFrom(src => src.TransactUrl))
            //    .ForMember(dest => dest.StatusUrl, opt => opt.MapFrom(src => src.StatusUrl))
            //    .ForMember(dest => dest.RenewUrl, opt => opt.MapFrom(src => src.RenewUrl))
            //    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Convert.ToInt32(src.IsActive)))
            //    .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => Convert.ToInt32(src.IsDefault)))
            //    .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
            //    .ForMember(dest => dest.PendingInterval, opt => opt.MapFrom(src => src.PendingInterval))
            //    .ForMember(dest => dest.PendingRetries, opt => opt.MapFrom(src => src.PendingRetries))
            //    .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret))
            //    .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertPaymentGatewaySettings(src.Settings)));


            ////from local object to WS object            
            //Mapper.CreateMap<WebAPI.Models.Billing.KalturaPaymentGatewayData, PaymentGateway>()
            //    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            //    .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
            //    .ForMember(dest => dest.TransactUrl, opt => opt.MapFrom(src => src.TransactUrl))
            //    .ForMember(dest => dest.StatusUrl, opt => opt.MapFrom(src => src.StatusUrl))
            //    .ForMember(dest => dest.RenewUrl, opt => opt.MapFrom(src => src.RenewUrl))
            //    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Convert.ToInt32(src.IsActive)))
            //    .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => Convert.ToInt32(src.IsDefault)))
            //    .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
            //    .ForMember(dest => dest.PendingInterval, opt => opt.MapFrom(src => src.PendingInterval))
            //    .ForMember(dest => dest.PendingRetries, opt => opt.MapFrom(src => src.PendingRetries))
            //    .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret))
            //    .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertPaymentGatewaySettings(src.Settings)));

            //from local object to WS object            
            Mapper.CreateMap<HouseholdPaymentGatewayResponse, WebAPI.Models.Billing.KalturaHouseholdPaymentGatewayResponse>()
                .ForMember(dest => dest.paymentGateway, opt => opt.MapFrom(src => src.PaymentGateway))
                .ForMember(dest => dest.selectedBy, opt => opt.MapFrom(src => ConvertHouseholdPaymentGatewaySelectedBy(src.SelectedBy)));

        }


        public static Billing.PaymentGatewaySettings[] ConvertPaymentGatewaySettings(Dictionary<string, string> settings)
        {
            List<Billing.PaymentGatewaySettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<PaymentGatewaySettings>();
                Billing.PaymentGatewaySettings pc;
                foreach (KeyValuePair<string, string> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new Billing.PaymentGatewaySettings();
                        pc.key = data.Key;
                        pc.value = data.Value;
                        result.Add(pc);
                    }
                }
            }
            if (result != null && result.Count > 0)
            {
                return result.ToArray();
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, string> ConvertPaymentGatewaySettings(Billing.PaymentGatewaySettings[] settings)
        {
            Dictionary<string, string> result = null;

            if (settings != null && settings.Count() > 0)
            {
                result = new Dictionary<string, string>();
                foreach (var data in settings)
                {
                    if (!string.IsNullOrEmpty(data.key))
                    {
                        result.Add(data.key, data.value);
                    }
                }
            }
            return result;
        }

        private static Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy ConvertHouseholdPaymentGatewaySelectedBy(WebAPI.Billing.eHouseholdPaymentGatewaySelectedBy? type)
        {
            WebAPI.Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy result;

            switch (type)
            {
                case WebAPI.Billing.eHouseholdPaymentGatewaySelectedBy.Account:
                    result = Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy.account;
                    break;
                case WebAPI.Billing.eHouseholdPaymentGatewaySelectedBy.Household:
                    result = Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy.household;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "unknown household payment gateway selected by");

            }

            return result;
        }
    }
}