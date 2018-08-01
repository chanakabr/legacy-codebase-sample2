using ApiObjects;
using ApiObjects.Billing;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Billing;
using WebAPI.Models.General;
using AutoMapper.Configuration;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class BillingMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {

            //PaymentGWConfigResponse to PaymentGWConfigResponse
            cfg.CreateMap<PaymentGateway, WebAPI.Models.Billing.KalturaPaymentGatewayProfile>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
                .ForMember(dest => dest.AdapterUrl, opt => opt.ResolveUsing(src => src.AdapterUrl))
                .ForMember(dest => dest.TransactUrl, opt => opt.ResolveUsing(src => src.TransactUrl))
                .ForMember(dest => dest.StatusUrl, opt => opt.ResolveUsing(src => src.StatusUrl))
                .ForMember(dest => dest.RenewUrl, opt => opt.ResolveUsing(src => src.RenewUrl))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.IsActive))
                .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.IsDefault))
                .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertPaymentGatewaySettings(src.Settings)))
                .ForMember(dest => dest.PendingRetries, opt => opt.ResolveUsing(src => src.PendingRetries))
                .ForMember(dest => dest.PendingInterval, opt => opt.ResolveUsing(src => src.PendingInterval))
                .ForMember(dest => dest.RenewIntervalMinutes, opt => opt.ResolveUsing(src => src.RenewalIntervalMinutes))
                .ForMember(dest => dest.RenewStartMinutes, opt => opt.ResolveUsing(src => src.RenewalStartMinutes))
                .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertPaymentGatewaySettings(src.Settings)))
                .ForMember(dest => dest.ExternalIdentifier, opt => opt.ResolveUsing(src => src.ExternalIdentifier))
                 .ForMember(dest => dest.ExternalVerification, opt => opt.ResolveUsing(src => src.ExternalVerification));

            //KalturaPaymentGatewayProfile to PaymentGateway
            cfg.CreateMap<WebAPI.Models.Billing.KalturaPaymentGatewayProfile, PaymentGateway>()
               .ForMember(dest => dest.ID, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.ResolveUsing(src => src.AdapterUrl))
               .ForMember(dest => dest.TransactUrl, opt => opt.ResolveUsing(src => src.TransactUrl))
               .ForMember(dest => dest.StatusUrl, opt => opt.ResolveUsing(src => src.StatusUrl))
               .ForMember(dest => dest.PendingRetries, opt => opt.ResolveUsing(src => src.PendingRetries))
               .ForMember(dest => dest.PendingInterval, opt => opt.ResolveUsing(src => src.PendingInterval))
               .ForMember(dest => dest.RenewalIntervalMinutes, opt => opt.ResolveUsing(src => src.RenewIntervalMinutes))
               .ForMember(dest => dest.RenewalStartMinutes, opt => opt.ResolveUsing(src => src.RenewStartMinutes))
               .ForMember(dest => dest.RenewUrl, opt => opt.ResolveUsing(src => src.RenewUrl))
               .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.IsActive))
               .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.IsDefault))
               .ForMember(dest => dest.SkipSettings, opt => opt.ResolveUsing(src => src.Settings == null))
               .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertPaymentGatewaySettings(src.Settings)))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.ResolveUsing(src => src.ExternalIdentifier))
               .ForMember(dest => dest.ExternalVerification, opt => opt.ResolveUsing(src => src.ExternalVerification));

            cfg.CreateMap<PaymentGatewayBase, WebAPI.Models.Billing.KalturaPaymentGatewayBaseProfile>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
                .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.IsDefault));

            //from local object to WS object            
            cfg.CreateMap<HouseholdPaymentGatewayResponse, WebAPI.Models.Billing.KalturaPaymentGateway>()
                .ForMember(dest => dest.paymentGateway, opt => opt.ResolveUsing(src => src.PaymentGateway))
                .ForMember(dest => dest.selectedBy, opt => opt.ResolveUsing(src => ConvertHouseholdPaymentGatewaySelectedBy(src.SelectedBy)));

            //from local object to WS object            
            cfg.CreateMap<PaymentGatewayConfigurationResponse, WebAPI.Models.Billing.KalturaPaymentGatewayConfiguration>()
                .ForMember(dest => dest.Configuration, opt => opt.ResolveUsing(src => src.Configuration.Select(x => new KalturaKeyValue(null) { key = x.key, value = x.value }).ToList()));

            //from local object to WS object            
            cfg.CreateMap<KalturaKeyValue, KeyValuePair>()
                .ForMember(dest => dest.key, opt => opt.ResolveUsing(src => src.key))
                .ForMember(dest => dest.value, opt => opt.ResolveUsing(src => src.value));

            //PaymentGatewayItemResponse to KalturaPaymentGatewayProfile
            cfg.CreateMap<PaymentGatewayItemResponse, WebAPI.Models.Billing.KalturaPaymentGatewayProfile>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.PaymentGateway.ID))
                .ForMember(dest => dest.AdapterUrl, opt => opt.ResolveUsing(src => src.PaymentGateway.AdapterUrl))
                .ForMember(dest => dest.ExternalIdentifier, opt => opt.ResolveUsing(src => src.PaymentGateway.ExternalIdentifier))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.PaymentGateway.IsActive))
                .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.PaymentGateway.IsDefault))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.PaymentGateway.Name))
                .ForMember(dest => dest.PendingInterval, opt => opt.ResolveUsing(src => src.PaymentGateway.PendingInterval))
                .ForMember(dest => dest.PendingRetries, opt => opt.ResolveUsing(src => src.PaymentGateway.PendingRetries))
                .ForMember(dest => dest.RenewIntervalMinutes, opt => opt.ResolveUsing(src => src.PaymentGateway.RenewalIntervalMinutes))
                .ForMember(dest => dest.RenewStartMinutes, opt => opt.ResolveUsing(src => src.PaymentGateway.RenewalStartMinutes))
                .ForMember(dest => dest.RenewUrl, opt => opt.ResolveUsing(src => src.PaymentGateway.RenewUrl))
                .ForMember(dest => dest.SharedSecret, opt => opt.ResolveUsing(src => src.PaymentGateway.SharedSecret))
                .ForMember(dest => dest.StatusUrl, opt => opt.ResolveUsing(src => src.PaymentGateway.StatusUrl))
                .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertPaymentGatewaySettings(src.PaymentGateway.Settings)))
                .ForMember(dest => dest.TransactUrl, opt => opt.ResolveUsing(src => src.PaymentGateway.TransactUrl))
                .ForMember(dest => dest.ExternalVerification, opt => opt.ResolveUsing(src => src.PaymentGateway.ExternalVerification)); 

            cfg.CreateMap<PaymentGatewaySelectedBy, WebAPI.Models.Billing.KalturaHouseholdPaymentGateway>()
             .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
             .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
             .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.IsDefault))
             .ForMember(dest => dest.selectedBy, opt => opt.ResolveUsing(src => ConvertHouseholdPaymentGatewaySelectedBy(src.By)));

            cfg.CreateMap<PaymentGatewaySelectedBy, WebAPI.Models.Billing.KalturaPaymentGatewayBaseProfile>()
             .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
             .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
             .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.IsDefault))
             .ForMember(dest => dest.PaymentMethods, opt => opt.ResolveUsing(src => src.PaymentMethods))
             .ForMember(dest => dest.selectedBy, opt => opt.ResolveUsing(src => ConvertHouseholdPaymentGatewaySelectedBy(src.By)));

            cfg.CreateMap<PaymentMethod, WebAPI.Models.Billing.KalturaPaymentMethodProfile>()
             .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
             .ForMember(dest => dest.PaymentGatewayId, opt => opt.ResolveUsing(src => src.PaymentGatewayId))
             .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
             .ForMember(dest => dest.AllowMultiInstance, opt => opt.ResolveUsing(src => src.AllowMultiInstance));

            cfg.CreateMap<PaymentGatewaySelectedBy, WebAPI.Models.Billing.KalturaPaymentGatewayBaseProfile>()
           .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
           .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
           .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.IsDefault))
           .ForMember(dest => dest.PaymentMethods, opt => opt.ResolveUsing(src => src.PaymentMethods));

            cfg.CreateMap<PaymentGatwayPaymentMethods, WebAPI.Models.Billing.KalturaPaymentMethod>()
            .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.PaymentMethod.ID))
            .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.PaymentMethod.Name))
            .ForMember(dest => dest.AllowMultiInstance, opt => opt.ResolveUsing(src => src.PaymentMethod.AllowMultiInstance))
            .ForMember(dest => dest.HouseholdPaymentMethods, opt => opt.ResolveUsing(src => src.HouseHoldPaymentMethods));

            cfg.CreateMap<PaymentGatwayPaymentMethods, WebAPI.Models.Billing.KalturaHouseholdPaymentMethod>()
            .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.PaymentMethod.ID))
            .ForMember(dest => dest.PaymentGatewayId, opt => opt.ResolveUsing(src => src.PaymentMethod.PaymentGatewayId))
            .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.PaymentMethod.Name))
            .ForMember(dest => dest.AllowMultiInstance, opt => opt.ResolveUsing(src => src.PaymentMethod.AllowMultiInstance));

            cfg.CreateMap<HouseholdPaymentMethod, WebAPI.Models.Billing.KalturaHouseholdPaymentMethod>()
            .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
            .ForMember(dest => dest.Details, opt => opt.ResolveUsing(src => src.Details))
            .ForMember(dest => dest.Selected, opt => opt.ResolveUsing(src => src.Selected))
            .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.Selected))
            .ForMember(dest => dest.PaymentGatewayId, opt => opt.ResolveUsing(src => src.PaymentGatewayId))
            .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.ExternalId))
            .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.Selected))
            .ForMember(dest => dest.PaymentGatewayId, opt => opt.ResolveUsing(src => src.PaymentGatewayId))
            .ForMember(dest => dest.PaymentMethodProfileId, opt => opt.ResolveUsing(src => src.PaymentMethodId));

            cfg.CreateMap<KalturaHouseholdPaymentMethod, HouseholdPaymentMethod>()
            .ForMember(dest => dest.ID, opt => opt.ResolveUsing(src => src.Id))
            .ForMember(dest => dest.Details, opt => opt.ResolveUsing(src => src.Details))
            .ForMember(dest => dest.Selected, opt => opt.ResolveUsing(src => src.Selected))
            .ForMember(dest => dest.PaymentMethodId, opt => opt.ResolveUsing(src => src.PaymentMethodProfileId))
            .ForMember(dest => dest.PaymentGatewayId, opt => opt.ResolveUsing(src => src.PaymentGatewayId))
            .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.ExternalId));

            cfg.CreateMap<KalturaPaymentMethod, PaymentMethod>()
           .ForMember(dest => dest.ID, opt => opt.ResolveUsing(src => src.Id))
           .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
           .ForMember(dest => dest.AllowMultiInstance, opt => opt.ResolveUsing(src => src.AllowMultiInstance));
        }


        public static List<PaymentGatewaySettings> ConvertPaymentGatewaySettings(Dictionary<string, KalturaStringValue> settings)
        {
            List<PaymentGatewaySettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<PaymentGatewaySettings>();
                PaymentGatewaySettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new PaymentGatewaySettings();
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


        public static List<PaymentGatewaySettings> ConvertPaymentGatewaySettings(SerializableDictionary<string, KalturaStringValue> settings)
        {
            List<PaymentGatewaySettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<PaymentGatewaySettings>();
                PaymentGatewaySettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new PaymentGatewaySettings();
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

        public static Dictionary<string, KalturaStringValue> ConvertPaymentGatewaySettings(List<PaymentGatewaySettings> settings)
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

        private static WebAPI.Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy ConvertHouseholdPaymentGatewaySelectedBy(eHouseholdPaymentGatewaySelectedBy? type)
        {
            WebAPI.Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy result;

            switch (type)
            {
                case eHouseholdPaymentGatewaySelectedBy.Account:
                    result = WebAPI.Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy.account;
                    break;
                case eHouseholdPaymentGatewaySelectedBy.Household:
                    result = WebAPI.Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy.household;
                    break;
                case eHouseholdPaymentGatewaySelectedBy.None:
                    result = WebAPI.Models.Billing.KalturaHouseholdPaymentGatewaySelectedBy.none;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "unknown household payment gateway selected by");
            }

            return result;
        }

        //private static List<KalturaPaymentMethod> ConvertHouseholdPaymentMethod(HouseholdPaymentMethod[] householdPaymentMethods)
        //{
        //    List<KalturaPaymentMethod> result = null;

        //    if (householdPaymentMethods != null && householdPaymentMethods.Length > 0)
        //    {
        //        result = new List<KalturaPaymentMethod>();

        //        KalturaPaymentMethod item;

        //        foreach (var householdPaymentMethod in householdPaymentMethods)
        //        {
        //            item = AutoMapper.Mapper.Map<KalturaPaymentMethod>(householdPaymentMethod);
        //            result.Add(item);
        //        }
        //    }

        //    return result;
        //}
    }
}