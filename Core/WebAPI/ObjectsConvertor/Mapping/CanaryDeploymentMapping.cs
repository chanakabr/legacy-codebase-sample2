using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.CanaryDeployment.Microservices;
using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using WebAPI.Models.CanaryDeployment.Elasticsearch;
using WebAPI.Models.CanaryDeployment.Microservices;
using WebAPI.Models.General;
using KalturaCanaryDeploymentAuthenticationMsOwnerShip = WebAPI.Models.CanaryDeployment.Microservices.KalturaCanaryDeploymentAuthenticationMsOwnerShip;
using KalturaCanaryDeploymentSegmentationMsOwnerShip = WebAPI.Models.CanaryDeployment.Microservices.KalturaCanaryDeploymentSegmentationMsOwnerShip;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class CanaryDeploymentMapping
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            cfg.CreateMap<MicroservicesCanaryDeploymentDataOwnership, KalturaMicroservicesCanaryDeploymentDataOwnerShip>()
                .ForMember(dest => dest.AuthenticationMsOwnerShip, opt => opt.MapFrom(src => src.AuthenticationMsOwnership))
                .ForMember(dest => dest.SegmentationMsOwnerShip, opt => opt.MapFrom(src => src.SegmentationMsOwnership));

            cfg.CreateMap<CanaryDeploymentAuthenticationMsOwnership, KalturaCanaryDeploymentAuthenticationMsOwnerShip>()
                .ForMember(dest => dest.DeviceLoginHistory, opt => opt.MapFrom(src => src.DeviceLoginHistory))
                .ForMember(dest => dest.DeviceLoginPin, opt => opt.MapFrom(src => src.DeviceLoginPin))
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
                .ForMember(dest => dest.SSOAdapterProfiles, opt => opt.MapFrom(src => src.SSOAdapterProfiles))
                .ForMember(dest => dest.UserLoginHistory, opt => opt.MapFrom(src => src.UserLoginHistory))
                .ForMember(dest => dest.SessionRevocation, opt => opt.MapFrom(src => src.SessionRevocation));

            cfg.CreateMap<CanaryDeploymentSegmentationMsOwnership, KalturaCanaryDeploymentSegmentationMsOwnerShip>()
                .ForMember(dest => dest.Segmentation, opt => opt.MapFrom(src => src.Segmentation));

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
            switch (microservicesRoutingAction)
            {
                case KalturaCanaryDeploymentMicroservicesRoutingAction.ANONYMOUSLOGIN:
                    return CanaryDeploymentRoutingAction.AnonymousLogin;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.APPTOKEN_CONTROLLER:
                    return CanaryDeploymentRoutingAction.AppTokenController;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.HOUSEHOLD_DEVICE_PIN_ACTIONS:
                    return CanaryDeploymentRoutingAction.HouseHoldDevicePinActions;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.LOGIN:
                    return CanaryDeploymentRoutingAction.Login;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.LOGOUT:
                    return CanaryDeploymentRoutingAction.Logout;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.REFRESHSESSION:
                    return CanaryDeploymentRoutingAction.RefreshSession;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.SESSION_CONTROLLER:
                    return CanaryDeploymentRoutingAction.SessionController;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.SSO_ADAPTER_PROFILE_CONTROLLER:
                    return CanaryDeploymentRoutingAction.SsoAdapterProfileController;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.USER_LOGIN_PIN_CONTROLLER:
                    return CanaryDeploymentRoutingAction.UserLoginPinController;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.MULTIREQUEST:
                    return CanaryDeploymentRoutingAction.MultiRequestController;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.HOUSEHOLD_USER:
                    return CanaryDeploymentRoutingAction.HouseholdUser;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.PLAYBACK:
                    return CanaryDeploymentRoutingAction.PlaybackController;
                case KalturaCanaryDeploymentMicroservicesRoutingAction.SEGMENTATION:
                    return CanaryDeploymentRoutingAction.Segmentation;
                default:
                    throw new Exception("invalid KalturaCanaryDeploymentRoutingAction type");
            }
        }

        public static MicroservicesCanaryDeploymentRoutingService ConvertRoutingService(KalturaCanaryDeploymentMicroservicesRoutingService microservicesRoutingAction)
        {
            switch (microservicesRoutingAction)
            {
                case KalturaCanaryDeploymentMicroservicesRoutingService.PHOENIX:
                    return MicroservicesCanaryDeploymentRoutingService.Phoenix;
                case KalturaCanaryDeploymentMicroservicesRoutingService.PHOENIX_REST_PROXY:
                    return MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy;
                case KalturaCanaryDeploymentMicroservicesRoutingService.HOUSEHOLD:
                    return MicroservicesCanaryDeploymentRoutingService.HouseholdService;
                case KalturaCanaryDeploymentMicroservicesRoutingService.PLAYBACK:
                    return MicroservicesCanaryDeploymentRoutingService.PlaybackService;
                default:
                    throw new Exception("invalid KalturaCanaryDeploymentRoutingService type");
            }
        }

    }
}
