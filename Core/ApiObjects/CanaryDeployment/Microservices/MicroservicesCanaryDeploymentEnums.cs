using System.Collections.Generic;

namespace ApiObjects.CanaryDeployment.Microservices
{
    public static class CanaryDeploymentRoutingActionLists
    {
        
        public static readonly Dictionary<CanaryDeploymentRoutingAction, MicroservicesCanaryDeploymentRoutingService>
            RoutingActionsToMsRoutingService =
                new Dictionary<CanaryDeploymentRoutingAction, MicroservicesCanaryDeploymentRoutingService>()
                {
                    {CanaryDeploymentRoutingAction.AppTokenController, MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy},
                    {
                        CanaryDeploymentRoutingAction.UserLoginPinController,
                        MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy
                    },
                    {
                        CanaryDeploymentRoutingAction.SsoAdapterProfileController,
                        MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy
                    },
                    {CanaryDeploymentRoutingAction.SessionController, MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy},
                    {
                        CanaryDeploymentRoutingAction.HouseHoldDevicePinActions,
                        MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy
                    },
                    {CanaryDeploymentRoutingAction.RefreshSession, MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy},
                    {CanaryDeploymentRoutingAction.Login, MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy},
                    {CanaryDeploymentRoutingAction.Logout, MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy},
                    {CanaryDeploymentRoutingAction.AnonymousLogin, MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy},
                    {
                        CanaryDeploymentRoutingAction.MultiRequestController,
                        MicroservicesCanaryDeploymentRoutingService.MultiRequestMicroService
                    }
                };
                
        public static readonly List<string> AppTokenControllerRouting = new List<string>() { "appToken/action/add", "appToken/action/delete", "appToken/action/get", "appToken/action/startSession" };
        public static readonly List<string> UserLoginPinControllerRouting = new List<string>() { "ottuser/action/loginWithPin", "userLoginPin/action/add", "userLoginPin/action/delete", "userLoginPin/action/deleteAll", "userLoginPin/action/update" };
        public static readonly List<string> SsoAdapterProfileControllerRouting = new List<string>() { "ssoAdapterProfile/action/add", "ssoAdapterProfile/action/delete", "ssoAdapterProfile/action/generateSharedSecret", "ssoAdapterProfile/action/update", "ssoAdapterProfile/action/list" };
        public static readonly List<string> SessionControllerRouting = new List<string>() { "session/action/get", "session/action/revoke", "session/action/switchUser" };
        public static readonly List<string> HouseHoldDevicePinActionsRouting = new List<string>() { "householdDevice/action/generatePin", "householdDevice/action/loginWithPin" };
        public static readonly List<string> RefreshSessionRouting = new List<string>() { "ottuser/action/refreshSession" };
        public static readonly List<string> LoginRouting = new List<string>() { "ottuser/action/login" };
        public static readonly List<string> LogoutRouting = new List<string>() { "ottuser/action/logout" };
        public static readonly List<string> AnonymousLoginRouting = new List<string>() { "ottuser/action/anonymousLogin" };        
        public static readonly List<string> MultiRequestController = new List<string>() { "multirequest","multirequest/action/do" };

    }    

    public enum MicroservicesCanaryDeploymentRoutingService
    {
        Phoenix = 0,
        PhoenixRestProxy = 1,       
        MultiRequestMicroService = 2
    }

    public enum CanaryDeploymentRoutingAction
    {
        AppTokenController = 0, // AppTokenControllerRoutings
        UserLoginPinController = 1, // UserLoginPinControllerRoutings
        SsoAdapterProfileController = 2, //  SsoAdapterProfileControllerRoutings
        SessionController = 3, // SessionControllerRoutings
        HouseHoldDevicePinActions = 4, // householdDevice/action/generatePin + householdDevice/action/loginWithPin + ownerShip flag for householdDevice/action/addByPin
        RefreshSession = 5, // ottuser/action/refreshSession
        Login = 6, // ottuser/action/login
        Logout = 7, // ottuser/action/logout
        AnonymousLogin = 8, // ottuser/action/anonymousLogin        
        MultiRequestController = 9, // multirequest/  + multirequest/action/do
    }

    public enum CanaryDeploymentMigrationEvent
    {
        AppToken = 0,
        RefreshSession = 1,

        //GEN-1471: decided to remove live migration support for user pin code
        // UserPinCode = 2,
        DevicePinCode = 3,
        SessionRevocation = 4,
        UserLoginHistory = 5,
        DeviceLoginHistory = 6
    }
    
    public enum CanaryDeploymentDataOwnershipEnum
    {
        AuthenticationUserLoginHistory = 0,
        AuthenticationDeviceLoginHistory = 1,
        AuthenticationSSOAdapterProfiles = 2,
        AuthenticationRefreshToken = 3,
        AuthenticationDeviceLoginPin = 4,
        AuthenticationSessionRevocation = 5,
    }

}
