using System.Collections.Generic;
using System.Linq;
using static ApiObjects.CanaryDeployment.Microservices.CanaryDeploymentRoutingAction;
using static ApiObjects.CanaryDeployment.Microservices.MicroservicesCanaryDeploymentRoutingService;

namespace ApiObjects.CanaryDeployment.Microservices
{
    public static class CanaryDeploymentRoutingActionLists
    {
        public static readonly Dictionary<CanaryDeploymentRoutingAction, HashSet<MicroservicesCanaryDeploymentRoutingService>> RoutingActionsToMsRoutingService =
                new Dictionary<CanaryDeploymentRoutingAction, HashSet<MicroservicesCanaryDeploymentRoutingService>>()
                {
                    {AppTokenController, new HashSet<MicroservicesCanaryDeploymentRoutingService> { PhoenixRestProxy }},
                    {UserLoginPinController, new HashSet<MicroservicesCanaryDeploymentRoutingService> { PhoenixRestProxy }},
                    {SsoAdapterProfileController, new HashSet<MicroservicesCanaryDeploymentRoutingService> { PhoenixRestProxy }},
                    {SessionController, new HashSet<MicroservicesCanaryDeploymentRoutingService> { PhoenixRestProxy }},
                    {HouseHoldDevicePinActions, new HashSet<MicroservicesCanaryDeploymentRoutingService> { PhoenixRestProxy } },
                    {RefreshSession, new HashSet<MicroservicesCanaryDeploymentRoutingService> { PhoenixRestProxy }},
                    {Login, new HashSet<MicroservicesCanaryDeploymentRoutingService> { PhoenixRestProxy }},
                    {Logout, new HashSet<MicroservicesCanaryDeploymentRoutingService> { PhoenixRestProxy }},
                    {AnonymousLogin, new HashSet<MicroservicesCanaryDeploymentRoutingService> { PhoenixRestProxy }},
                    {MultiRequestController, new HashSet<MicroservicesCanaryDeploymentRoutingService> { MultiRequestMicroService }},
                    {HouseholdUser, new HashSet<MicroservicesCanaryDeploymentRoutingService> { HouseholdService }},
                    {PlaybackController, new HashSet<MicroservicesCanaryDeploymentRoutingService> { PlaybackService, PlaybackV2Service }},
                    {CanaryDeploymentRoutingAction.Segmentation, new HashSet<MicroservicesCanaryDeploymentRoutingService> { PhoenixRestProxy }}
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
        public static readonly List<string> MultiRequestControllerRouting = new List<string>() { "multirequest","multirequest/action/do" };
        public static readonly List<string> HouseholdUserRouting = new List<string>() { "ottuser/action/get", "ottuser/action/register", "ottuser/action/update", "ottuser/action/upsertDynamicData", "ottuser/action/deleteDynamicData", "ottuser/action/updateDynamicData", "ottuser/action/resetPassword", "ottuser/action/setInitialPassword", "ottuser/action/updatePassword", "ottuser/action/updateLoginData", "ottuser/action/activate", "ottuser/action/resendActivationToken", "ottuser/action/list", "ottuser/action/getEncryptedUserId", "ottuser/action/delete", "ottuser/action/addRole" };
        public static readonly List<string> PlaybackControllerRouting = new List<string>()
        {
            "asset/action/getPlaybackContext", "assetfile/action/playmanifest", "asset/action/getPlaybackManifest", "asset/action/getAdsContext",
            "playbackProfile/action/add", "playbackProfile/action/delete", "playbackProfile/action/update","playbackProfile/action/list", "playbackProfile/action/generateSharedSecret",
            "cdnAdapterProfile/action/add","cdnAdapterProfile/action/delete","cdnAdapterProfile/action/update" , "cdnAdapterProfile/action/list","cdnAdapterProfile/action/generateSharedSecret",
            "cdnPartnerSettings/action/get","cdnPartnerSettings/action/update",
            "drmProfile/action/add", "drmProfile/action/delete", "drmProfile/action/list",
            "streamingDevice/action/bookPlaybackSession"
        };
        
        public static readonly List<string> SegmentationRouting = new List<string>()
        {
            "segmentationType/action/add",
            "segmentationType/action/update",
            "segmentationType/action/delete",
            "segmentationType/action/list",
            "segmentationType/action/getPartnerConfiguration",
            "segmentationType/action/updatePartnerConfiguration",
            "userSegment/action/add",
            "userSegment/action/delete",
            "userSegment/action/list",
            "householdSegment/action/add",
            "householdSegment/action/delete",
            "householdSegment/action/list",
        };
        
        public static readonly List<string> PlaybackV2ControllerRouting = new List<string>(PlaybackControllerRouting.Concat(new List<string> {
            "streamingDevice/action/list",
            "mediaConcurrencyRule/action/list",
            "bookmark/action/add"
        }));
    }    

    public enum MicroservicesCanaryDeploymentRoutingService
    {
        Phoenix = 0,
        PhoenixRestProxy = 1,       
        MultiRequestMicroService = 2,
        HouseholdService = 3,
        PlaybackService = 4,
        PlaybackV2Service = 5
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
        HouseholdUser = 10, // ottuser/action/get
        PlaybackController = 11, // playback requests
        Segmentation = 12
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
        DeviceLoginHistory = 6,
        SegmentationType = 7,
        HouseholdSegment = 8,
        UserSegment = 9,
    }
    
    public enum CanaryDeploymentDataOwnershipEnum
    {
        AuthenticationUserLoginHistory = 0,
        AuthenticationDeviceLoginHistory = 1,
        AuthenticationSSOAdapterProfiles = 2,
        AuthenticationRefreshToken = 3,
        AuthenticationDeviceLoginPin = 4,
        AuthenticationSessionRevocation = 5,
        Segmentation = 6,
    }
}