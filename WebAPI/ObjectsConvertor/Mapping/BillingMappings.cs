using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Billing;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class BillingMappings
    {
        public static void RegisterMappings()
        {
            //PaymentGWConfigResponse to PaymentGWConfigResponse
            Mapper.CreateMap<PaymentGWSettingsResponse, WebAPI.Models.Billing.KalturaPaymentGWSettingsResponse>()
                .ForMember(dest => dest.pgw, opt => opt.MapFrom(src => src.pgw));

            //PaymentGWConfigResponse to PaymentGWConfigResponse
            Mapper.CreateMap<PaymentGW, WebAPI.Models.Billing.KalturaPaymentGW>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
                .ForMember(dest => dest.TransactUrl, opt => opt.MapFrom(src => src.TransactUrl))
                .ForMember(dest => dest.StatusUrl, opt => opt.MapFrom(src => src.StatusUrl))
                .ForMember(dest => dest.RenewUrl, opt => opt.MapFrom(src => src.RenewUrl))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertPaymentGatewaySettings(src.Settings)))
                .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));


            Mapper.CreateMap<PaymentGWResponse, WebAPI.Models.Billing.KalturaPaymentGWResponse>()
                 .ForMember(dest => dest.pgw, opt => opt.MapFrom(src => src.pgw));

            Mapper.CreateMap<PaymentGWBasic, WebAPI.Models.Billing.KalturaPaymentGWBasic>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name));
          
            //from local object to WS object
            //PaymentGWConfigResponse to PaymentGWConfigResponse
            Mapper.CreateMap<WebAPI.Models.Billing.KalturaPaymentGW, PaymentGW>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
                .ForMember(dest => dest.TransactUrl, opt => opt.MapFrom(src => src.TransactUrl))
                .ForMember(dest => dest.StatusUrl, opt => opt.MapFrom(src => src.StatusUrl))
                .ForMember(dest => dest.RenewUrl, opt => opt.MapFrom(src => src.RenewUrl))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
                .ForMember(dest => dest.PendingInterval, opt => opt.MapFrom(src => src.PendingInterval))
                .ForMember(dest => dest.PendingRetries, opt => opt.MapFrom(src => src.PendingRetries))
                .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret))
                .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertPaymentGatewaySettings(src.Settings)));
            
        }


        public static Billing.PaymentGWSettings[] ConvertPaymentGatewaySettings(Dictionary<string, string> settings)
        {
            List<Billing.PaymentGWSettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<PaymentGWSettings>();
                Billing.PaymentGWSettings pc;
                foreach (KeyValuePair<string, string> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new Billing.PaymentGWSettings();
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

        public static Dictionary<string, string> ConvertPaymentGatewaySettings(Billing.PaymentGWSettings[] settings)
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
    }
}