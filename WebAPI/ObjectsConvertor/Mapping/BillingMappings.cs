using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Billing;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Billing;
using WebAPI.Models.General;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class BillingMappings
    {
        public static void RegisterMappings()
        {

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
                .ForMember(dest => dest.PendingRetries, opt => opt.MapFrom(src => src.PendingRetries))
                .ForMember(dest => dest.PendingInterval, opt => opt.MapFrom(src => src.PendingInterval))
                .ForMember(dest => dest.RenewIntervalMinutes, opt => opt.MapFrom(src => src.RenewalIntervalMinutes))
                .ForMember(dest => dest.RenewStartMinutes, opt => opt.MapFrom(src => src.RenewalStartMinutes))
                .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));

            //KalturaPaymentGatewayProfile to PaymentGateway
            Mapper.CreateMap<WebAPI.Models.Billing.KalturaPaymentGatewayProfile, PaymentGateway>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
               .ForMember(dest => dest.TransactUrl, opt => opt.MapFrom(src => src.TransactUrl))
               .ForMember(dest => dest.StatusUrl, opt => opt.MapFrom(src => src.StatusUrl))
               .ForMember(dest => dest.PendingRetries, opt => opt.MapFrom(src => src.PendingRetries))
               .ForMember(dest => dest.PendingInterval, opt => opt.MapFrom(src => src.PendingInterval))
               .ForMember(dest => dest.RenewalIntervalMinutes, opt => opt.MapFrom(src => src.RenewIntervalMinutes))
               .ForMember(dest => dest.RenewalStartMinutes, opt => opt.MapFrom(src => src.RenewStartMinutes))
               .ForMember(dest => dest.RenewUrl, opt => opt.MapFrom(src => src.RenewUrl))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
               .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertPaymentGatewaySettings(src.Settings)))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));

            Mapper.CreateMap<PaymentGatewayBase, WebAPI.Models.Billing.KalturaPaymentGatewayBaseProfile>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));

            //from local object to WS object            
            Mapper.CreateMap<HouseholdPaymentGatewayResponse, WebAPI.Models.Billing.KalturaPaymentGateway>()
                .ForMember(dest => dest.paymentGateway, opt => opt.MapFrom(src => src.PaymentGateway))
                .ForMember(dest => dest.selectedBy, opt => opt.MapFrom(src => ConvertHouseholdPaymentGatewaySelectedBy(src.SelectedBy)));

            //from local object to WS object            
            Mapper.CreateMap<PaymentGatewayConfigurationResponse, WebAPI.Models.Billing.KalturaPaymentGatewayConfiguration>()
                .ForMember(dest => dest.Configuration, opt => opt.MapFrom(src => src.Configuration.Select(x => new KalturaKeyValue() { key = x.key, value = x.value }).ToList()));

            //PaymentGatewayItemResponse to KalturaPaymentGatewayProfile
            Mapper.CreateMap<PaymentGatewayItemResponse, WebAPI.Models.Billing.KalturaPaymentGatewayProfile>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PaymentGateway.ID))
                .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.PaymentGateway.AdapterUrl))
                .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.PaymentGateway.ExternalIdentifier))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.PaymentGateway.IsActive))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.PaymentGateway.IsDefault))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.PaymentGateway.Name))
                .ForMember(dest => dest.PendingInterval, opt => opt.MapFrom(src => src.PaymentGateway.PendingInterval))
                .ForMember(dest => dest.PendingRetries, opt => opt.MapFrom(src => src.PaymentGateway.PendingRetries))
                .ForMember(dest => dest.RenewIntervalMinutes, opt => opt.MapFrom(src => src.PaymentGateway.RenewalIntervalMinutes))
                .ForMember(dest => dest.RenewStartMinutes, opt => opt.MapFrom(src => src.PaymentGateway.RenewalStartMinutes))
                .ForMember(dest => dest.RenewUrl, opt => opt.MapFrom(src => src.PaymentGateway.RenewUrl))
                .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.PaymentGateway.SharedSecret))
                .ForMember(dest => dest.StatusUrl, opt => opt.MapFrom(src => src.PaymentGateway.StatusUrl))
                .ForMember(dest => dest.TransactUrl, opt => opt.MapFrom(src => src.PaymentGateway.TransactUrl));

            Mapper.CreateMap<PaymentGatewaySelectedBy, WebAPI.Models.Billing.KalturaPaymentGatewayBaseProfile>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
             .ForMember(dest => dest.selectedBy, opt => opt.MapFrom(src => ConvertHouseholdPaymentGatewaySelectedBy(src.By)));

            Mapper.CreateMap<PaymentMethod, WebAPI.Models.Billing.KalturaPaymentMethodProfile>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.PaymentMethodType));

        }


        public static Billing.PaymentGatewaySettings[] ConvertPaymentGatewaySettings(Dictionary<string, KalturaStringValue> settings)
        {
            List<Billing.PaymentGatewaySettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<PaymentGatewaySettings>();
                Billing.PaymentGatewaySettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new Billing.PaymentGatewaySettings();
                        pc.key = data.Key;
                        pc.value = data.Value.value;
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

        public static Dictionary<string, KalturaStringValue> ConvertPaymentGatewaySettings(Billing.PaymentGatewaySettings[] settings)
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

        private static WebAPI.Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy ConvertHouseholdPaymentGatewaySelectedBy(WebAPI.Billing.eHouseholdPaymentGatewaySelectedBy? type)
        {
            WebAPI.Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy result;

            switch (type)
            {
                case WebAPI.Billing.eHouseholdPaymentGatewaySelectedBy.Account:
                    result = WebAPI.Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy.account;
                    break;
                case WebAPI.Billing.eHouseholdPaymentGatewaySelectedBy.Household:
                    result = WebAPI.Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy.household;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "unknown household payment gateway selected by");
            }

            return result;
        }
    }
}