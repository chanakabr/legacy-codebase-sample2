using System.Collections.Generic;

namespace ApiObjects.CanaryDeployment
{
    public static class CanaryDeploymentRoutingActionLists
    {

        public static readonly Dictionary<CanaryDeploymentRoutingAction, CanaryDeploymentRoutingService>
            RoutingActionsToMsRoutingService =
                new Dictionary<CanaryDeploymentRoutingAction, CanaryDeploymentRoutingService>()
                {
                    {CanaryDeploymentRoutingAction.AppTokenController, CanaryDeploymentRoutingService.PhoenixRestProxy},
                    {
                        CanaryDeploymentRoutingAction.UserLoginPinController,
                        CanaryDeploymentRoutingService.PhoenixRestProxy
                    },
                    {
                        CanaryDeploymentRoutingAction.SsoAdapterProfileController,
                        CanaryDeploymentRoutingService.PhoenixRestProxy
                    },
                    {CanaryDeploymentRoutingAction.SessionController, CanaryDeploymentRoutingService.PhoenixRestProxy},
                    {
                        CanaryDeploymentRoutingAction.HouseHoldDevicePinActions,
                        CanaryDeploymentRoutingService.PhoenixRestProxy
                    },
                    {CanaryDeploymentRoutingAction.RefreshToken, CanaryDeploymentRoutingService.PhoenixRestProxy},
                    {CanaryDeploymentRoutingAction.Login, CanaryDeploymentRoutingService.PhoenixRestProxy},
                    {CanaryDeploymentRoutingAction.Logout, CanaryDeploymentRoutingService.PhoenixRestProxy},
                    {CanaryDeploymentRoutingAction.AnonymousLogin, CanaryDeploymentRoutingService.PhoenixRestProxy},
                    {
                        CanaryDeploymentRoutingAction.MultiRequestController,
                        CanaryDeploymentRoutingService.MultiRequestMicroService
                    }
                };


        public static readonly List<string> AppTokenControllerRouting = new List<string>() { "appToken/action/add", "appToken/action/delete", "appToken/action/get", "appToken/action/startSession" };
        public static readonly List<string> UserLoginPinControllerRouting = new List<string>() { "ottuser/action/loginWithPin", "userLoginPin/action/add", "userLoginPin/action/delete", "userLoginPin/action/deleteAll", "userLoginPin/action/update" };
        public static readonly List<string> SsoAdapterProfileControllerRouting = new List<string>() { "ssoAdapterProfile/action/add", "ssoAdapterProfile/action/delete", "ssoAdapterProfile/action/generateSharedSecret", "ssoAdapterProfile/action/update", "ssoAdapterProfile/action/list" };
        public static readonly List<string> SessionControllerRouting = new List<string>() { "session/action/get", "session/action/revoke", "session/action/switchUser" };
        public static readonly List<string> HouseHoldDevicePinActionsRouting = new List<string>() { "householdDevice/action/generatePin", "householdDevice/action/loginWithPin" };
        public static readonly List<string> RefreshTokenRouting = new List<string>() { "ottuser/action/refreshToken" };
        public static readonly List<string> LoginRouting = new List<string>() { "ottuser/action/login" };
        public static readonly List<string> LogoutRouting = new List<string>() { "ottuser/action/logout" };
        public static readonly List<string> AnonymousLoginRouting = new List<string>() { "ottuser/action/anonymousLogin" };        
        public static readonly List<string> MultiRequestController = new List<string>() { "multirequest","multirequest/action/do" };

    }    

    public enum CanaryDeploymentRoutingService
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
        RefreshToken = 5, // ottuser/action/refreshToken
        Login = 6, // ottuser/action/login
        Logout = 7, // ottuser/action/logout
        AnonymousLogin = 8, // ottuser/action/anonymousLogin        
        MultiRequestController = 9, // multirequest/  + multirequest/action/do
    }

    public enum CanaryDeploymentMigrationEvent
    {
        AppToken = 0,
        RefreshToken = 1,

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
