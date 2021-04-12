using ApiObjects.CanaryDeployment;
using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using WebAPI.Models.CanaryDeployment;
using WebAPI.Models.General;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class CanaryDeploymentMapping
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            cfg.CreateMap<CanaryDeploymentDataOwnership, KalturaCanaryDeploymentDataOwnerShip>()
                .ForMember(dest => dest.AuthenticationMsOwnerShip, opt => opt.MapFrom(src => src.AuthenticationMsOwnership));

            cfg.CreateMap<CanaryDeploymentAuthenticationMsOwnership, KalturaCanaryDeploymentAuthenticationMsOwnerShip>()
                .ForMember(dest => dest.DeviceLoginHistory, opt => opt.MapFrom(src => src.DeviceLoginHistory))
                .ForMember(dest => dest.DeviceLoginPin, opt => opt.MapFrom(src => src.DeviceLoginPin))
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
                .ForMember(dest => dest.SSOAdapterProfiles, opt => opt.MapFrom(src => src.SSOAdapterProfiles))
                .ForMember(dest => dest.UserLoginHistory, opt => opt.MapFrom(src => src.UserLoginHistory))
                .ForMember(dest => dest.SessionRevocation, opt => opt.MapFrom(src => src.SessionRevocation));

            cfg.CreateMap<CanaryDeploymentMigrationEvents, KalturaCanaryDeploymentMigrationEvents>()
                .ForMember(dest => dest.AppToken, opt => opt.MapFrom(src => src.AppToken))
                .ForMember(dest => dest.DeviceLoginHistory, opt => opt.MapFrom(src => src.DeviceLoginHistory))
                .ForMember(dest => dest.DevicePinCode, opt => opt.MapFrom(src => src.DevicePinCode))
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
                .ForMember(dest => dest.SessionRevocation, opt => opt.MapFrom(src => src.SessionRevocation))
                .ForMember(dest => dest.UserLoginHistory, opt => opt.MapFrom(src => src.UserLoginHistory))
                .ForMember(dest => dest.UserPinCode, opt => opt.MapFrom(src => src.UserPinCode));

            cfg.CreateMap<CanaryDeploymentConfiguration, KalturaCanaryDeploymentConfiguration>()
                .ForMember(dest => dest.DataOwnerShip, opt => opt.MapFrom(src => src.DataOwnership))
                .ForMember(dest => dest.MigrationEvents, opt => opt.MapFrom(src => src.MigrationEvents))
                .ForMember(dest => dest.RoutingConfiguration, opt => opt.ResolveUsing(src => ConvertRoutingConfiguration(src.RoutingConfiguration)))
                .ForMember(dest => dest.ShouldProduceInvalidationEventsToKafka, opt => opt.MapFrom(src => src.ShouldProduceInvalidationEventsToKafka));
        }

        private static SerializableDictionary<string, KalturaStringValue> ConvertRoutingConfiguration(Dictionary<string, CanaryDeploymentRoutingService> routingConfiguration)
        {
            SerializableDictionary<string, KalturaStringValue> result = new SerializableDictionary<string, KalturaStringValue>();
            if (routingConfiguration?.Count > 0)
            {
                foreach (KeyValuePair<string, CanaryDeploymentRoutingService> pair in routingConfiguration)
                {
                    result[pair.Key] = new KalturaStringValue() { value = pair.Value.ToString() };
                }
            }

            return result;
        }

        public static CanaryDeploymentMigrationEvent ConvertMigrationEvent(KalturaCanaryDeploymentMigrationEvent migrationEvent)
        {
            CanaryDeploymentMigrationEvent? res = null;
            switch (migrationEvent)
            {
                case KalturaCanaryDeploymentMigrationEvent.APPTOKEN:
                    res = CanaryDeploymentMigrationEvent.AppToken;
                    break;
                case KalturaCanaryDeploymentMigrationEvent.DEVICE_LOGIN_HISTORY:
                    res = CanaryDeploymentMigrationEvent.DeviceLoginHistory;
                    break;
                case KalturaCanaryDeploymentMigrationEvent.DEVICE_PIN_CODE:
                    res = CanaryDeploymentMigrationEvent.DevicePinCode;
                    break;
                case KalturaCanaryDeploymentMigrationEvent.REFRESHTOKEN:
                    res = CanaryDeploymentMigrationEvent.RefreshToken;
                    break;
                case KalturaCanaryDeploymentMigrationEvent.SESSION_REVOCATION:
                    res = CanaryDeploymentMigrationEvent.SessionRevocation;
                    break;
                case KalturaCanaryDeploymentMigrationEvent.USER_LOGIN_HISTORY:
                    res = CanaryDeploymentMigrationEvent.UserLoginHistory;
                    break;
                default:
                    throw new Exception("invalid KalturaCanaryDeploymentMigrationEvent type");
            }

            return res.Value;
        }

        public static CanaryDeploymentRoutingAction ConvertRoutingAction(KalturaCanaryDeploymentRoutingAction routingAction)
        {
            CanaryDeploymentRoutingAction? res = null;
            switch (routingAction)
            {
                case KalturaCanaryDeploymentRoutingAction.ANONYMOUSLOGIN:
                    res = CanaryDeploymentRoutingAction.AnonymousLogin;
                    break;
                case KalturaCanaryDeploymentRoutingAction.APPTOKEN_CONTROLLER:
                    res = CanaryDeploymentRoutingAction.AppTokenController;
                    break;
                case KalturaCanaryDeploymentRoutingAction.HOUSEHOLD_DEVICE_PIN_ACTIONS:
                    res = CanaryDeploymentRoutingAction.HouseHoldDevicePinActions;
                    break;
                case KalturaCanaryDeploymentRoutingAction.LOGIN:
                    res = CanaryDeploymentRoutingAction.Login;
                    break;
                case KalturaCanaryDeploymentRoutingAction.LOGOUT:
                    res = CanaryDeploymentRoutingAction.Logout;
                    break;
                case KalturaCanaryDeploymentRoutingAction.REFRESHTOKEN:
                    res = CanaryDeploymentRoutingAction.RefreshToken;
                    break;
                case KalturaCanaryDeploymentRoutingAction.SESSION_CONTROLLER:
                    res = CanaryDeploymentRoutingAction.SessionController;
                    break;
                case KalturaCanaryDeploymentRoutingAction.SSO_ADAPTER_PROFILE_CONTROLLER:
                    res = CanaryDeploymentRoutingAction.SsoAdapterProfileController;
                    break;
                case KalturaCanaryDeploymentRoutingAction.USER_LOGIN_PIN_CONTROLLER:
                    res = CanaryDeploymentRoutingAction.UserLoginPinController;
                    break;
                default:
                    throw new Exception("invalid KalturaCanaryDeploymentRoutingAction type");
            }

            return res.Value;
        }

        public static CanaryDeploymentRoutingService ConvertRoutingService(KalturaCanaryDeploymentRoutingService routingAction)
        {
            CanaryDeploymentRoutingService? res = null;
            switch (routingAction)
            {
                case KalturaCanaryDeploymentRoutingService.PHOENIX:
                    res = CanaryDeploymentRoutingService.Phoenix;
                    break;
                case KalturaCanaryDeploymentRoutingService.PHOENIX_REST_PROXY:
                    res = CanaryDeploymentRoutingService.PhoenixRestProxy;
                    break;
                default:
                    throw new Exception("invalid KalturaCanaryDeploymentRoutingService type");
            }

            return res.Value;
        }

    }
}
