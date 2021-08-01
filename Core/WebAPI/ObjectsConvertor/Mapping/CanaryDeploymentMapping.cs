using ApiObjects.CanaryDeployment;
using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.CanaryDeployment.Microservices;
using WebAPI.Models.CanaryDeployment;
using WebAPI.Models.CanaryDeployment.Elasticsearch;
using WebAPI.Models.CanaryDeployment.Microservices;
using WebAPI.Models.General;
using KalturaCanaryDeploymentAuthenticationMsOwnerShip = WebAPI.Models.CanaryDeployment.Microservices.KalturaCanaryDeploymentAuthenticationMsOwnerShip;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class CanaryDeploymentMapping
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            cfg.CreateMap<MicroservicesCanaryDeploymentDataOwnership, KalturaMicroservicesCanaryDeploymentDataOwnerShip>()
                .ForMember(dest => dest.AuthenticationMsOwnerShip, opt => opt.MapFrom(src => src.AuthenticationMsOwnership));

            cfg.CreateMap<CanaryDeploymentAuthenticationMsOwnership, KalturaCanaryDeploymentAuthenticationMsOwnerShip>()
                .ForMember(dest => dest.DeviceLoginHistory, opt => opt.MapFrom(src => src.DeviceLoginHistory))
                .ForMember(dest => dest.DeviceLoginPin, opt => opt.MapFrom(src => src.DeviceLoginPin))
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
                .ForMember(dest => dest.SSOAdapterProfiles, opt => opt.MapFrom(src => src.SSOAdapterProfiles))
                .ForMember(dest => dest.UserLoginHistory, opt => opt.MapFrom(src => src.UserLoginHistory))
                .ForMember(dest => dest.SessionRevocation, opt => opt.MapFrom(src => src.SessionRevocation));

            cfg.CreateMap<MicroservicesCanaryDeploymentMigrationEvents, KalturaMicroservicesCanaryDeploymentMigrationEvents>()
                .ForMember(dest => dest.AppToken, opt => opt.MapFrom(src => src.AppToken))
                .ForMember(dest => dest.DeviceLoginHistory, opt => opt.MapFrom(src => src.DeviceLoginHistory))
                .ForMember(dest => dest.DevicePinCode, opt => opt.MapFrom(src => src.DevicePinCode))
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
                .ForMember(dest => dest.SessionRevocation, opt => opt.MapFrom(src => src.SessionRevocation))
                .ForMember(dest => dest.UserLoginHistory, opt => opt.MapFrom(src => src.UserLoginHistory))
                .ForMember(dest => dest.UserPinCode, opt => opt.MapFrom(src => src.UserPinCode));

            cfg.CreateMap<MicroservicesCanaryDeploymentConfiguration, KalturaMicroservicesCanaryDeploymentConfiguration>()
                .ForMember(dest => dest.DataOwnerShip, opt => opt.MapFrom(src => src.DataOwnership))
                .ForMember(dest => dest.MigrationEvents, opt => opt.MapFrom(src => src.MigrationEvents))
                .ForMember(dest => dest.RoutingConfiguration, opt => opt.ResolveUsing(src => ConvertRoutingConfiguration(src.RoutingConfiguration)));

            cfg.CreateMap<ElasticsearchCanaryDeploymentConfiguration, KalturaElasticsearchCanaryDeploymentConfiguration>()
                .ForMember(dest => dest.ElasticsearchActiveVersion, opt => opt.MapFrom(src => src.ElasticsearchActiveVersion))
                .ForMember(dest => dest.EnableMigrationEvents, opt => opt.MapFrom(src => src.EnableMigrationEvents));
        }

        private static SerializableDictionary<string, KalturaStringValue> ConvertRoutingConfiguration(Dictionary<string, MicroservicesCanaryDeploymentRoutingService> routingConfiguration)
        {
            SerializableDictionary<string, KalturaStringValue> result = new SerializableDictionary<string, KalturaStringValue>();
            if (routingConfiguration?.Count > 0)
            {
                foreach (KeyValuePair<string, MicroservicesCanaryDeploymentRoutingService> pair in routingConfiguration)
                {
                    result[pair.Key] = new KalturaStringValue() { value = pair.Value.ToString() };
                }
            }

            return result;
        }

        public static CanaryDeploymentMigrationEvent ConvertMigrationEvent(KalturaCanaryDeploymentMicroservicesMigrationEvent microservicesMigrationEvent)
        {
            CanaryDeploymentMigrationEvent? res = null;
            switch (microservicesMigrationEvent)
            {
                case KalturaCanaryDeploymentMicroservicesMigrationEvent.APPTOKEN:
                    res = CanaryDeploymentMigrationEvent.AppToken;
                    break;
                case KalturaCanaryDeploymentMicroservicesMigrationEvent.DEVICE_LOGIN_HISTORY:
                    res = CanaryDeploymentMigrationEvent.DeviceLoginHistory;
                    break;
                case KalturaCanaryDeploymentMicroservicesMigrationEvent.DEVICE_PIN_CODE:
                    res = CanaryDeploymentMigrationEvent.DevicePinCode;
                    break;
                case KalturaCanaryDeploymentMicroservicesMigrationEvent.REFRESHSESSION:
                    res = CanaryDeploymentMigrationEvent.RefreshSession;
                    break;
                case KalturaCanaryDeploymentMicroservicesMigrationEvent.SESSION_REVOCATION:
                    res = CanaryDeploymentMigrationEvent.SessionRevocation;
                    break;
                case KalturaCanaryDeploymentMicroservicesMigrationEvent.USER_LOGIN_HISTORY:
                    res = CanaryDeploymentMigrationEvent.UserLoginHistory;
                    break;
                default:
                    throw new Exception("invalid KalturaCanaryDeploymentMigrationEvent type");
            }

            return res.Value;
        }

        public static CanaryDeploymentRoutingAction ConvertRoutingAction(KalturaCanaryDeploymentMicroservicesRoutingAction microservicesRoutingAction)
        {
            CanaryDeploymentRoutingAction? res = null;
            switch (microservicesRoutingAction)
            {
                case KalturaCanaryDeploymentMicroservicesRoutingAction.ANONYMOUSLOGIN:
                    res = CanaryDeploymentRoutingAction.AnonymousLogin;
                    break;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.APPTOKEN_CONTROLLER:
                    res = CanaryDeploymentRoutingAction.AppTokenController;
                    break;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.HOUSEHOLD_DEVICE_PIN_ACTIONS:
                    res = CanaryDeploymentRoutingAction.HouseHoldDevicePinActions;
                    break;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.LOGIN:
                    res = CanaryDeploymentRoutingAction.Login;
                    break;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.LOGOUT:
                    res = CanaryDeploymentRoutingAction.Logout;
                    break;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.REFRESHSESSION:
                    res = CanaryDeploymentRoutingAction.RefreshSession;
                    break;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.SESSION_CONTROLLER:
                    res = CanaryDeploymentRoutingAction.SessionController;
                    break;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.SSO_ADAPTER_PROFILE_CONTROLLER:
                    res = CanaryDeploymentRoutingAction.SsoAdapterProfileController;
                    break;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.USER_LOGIN_PIN_CONTROLLER:
                    res = CanaryDeploymentRoutingAction.UserLoginPinController;
                    break;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.MULTIREQUEST:
                    res = CanaryDeploymentRoutingAction.MultiRequestController;
                    break;
                default:
                    throw new Exception("invalid KalturaCanaryDeploymentRoutingAction type");
            }

            return res.Value;
        }

        public static MicroservicesCanaryDeploymentRoutingService ConvertRoutingService(KalturaCanaryDeploymentMicroservicesRoutingService microservicesRoutingAction)
        {
            MicroservicesCanaryDeploymentRoutingService? res = null;
            switch (microservicesRoutingAction)
            {
                case KalturaCanaryDeploymentMicroservicesRoutingService.PHOENIX:
                    res = MicroservicesCanaryDeploymentRoutingService.Phoenix;
                    break;
                case KalturaCanaryDeploymentMicroservicesRoutingService.PHOENIX_REST_PROXY:
                    res = MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy;
                    break;
                default:
                    throw new Exception("invalid KalturaCanaryDeploymentRoutingService type");
            }

            return res.Value;
        }

    }
}
