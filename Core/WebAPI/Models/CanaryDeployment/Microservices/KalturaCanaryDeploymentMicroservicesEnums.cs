namespace WebAPI.Models.CanaryDeployment.Microservices
{
   
    public enum KalturaCanaryDeploymentMicroservicesRoutingService
    {
        PHOENIX = 0,
        PHOENIX_REST_PROXY = 1,
        //MULTIREQUEST = 2,
        HOUSEHOLD = 3,
        PLAYBACK = 4,
        PLAYBACK_V2 = 5
    }

    public enum KalturaCanaryDeploymentMicroservicesRoutingAction
    {
        APPTOKEN_CONTROLLER = 0, // appToken/action/add + appToken/action/delete + appToken/action/get + appToken/action/startSession
        USER_LOGIN_PIN_CONTROLLER = 1, // ottuser/action/loginWithPin + userLoginPin/action/add + userLoginPin/action/delete + userLoginPin/action/deleteAll + userLoginPin/action/update
        SSO_ADAPTER_PROFILE_CONTROLLER = 2, //  ssoAdapterProfile/action/invoke should be moved to invoke MS + ssoAdapterProfile/action/add + ssoAdapterProfile/action/delete + ssoAdapterProfile/action/generateSharedSecret + ssoAdapterProfile/action/update + ssoAdapterProfile/action/list
        SESSION_CONTROLLER = 3, // session/action/get + session/action/revoke + session/action/switchUser 
        HOUSEHOLD_DEVICE_PIN_ACTIONS = 4, // householdDevice/action/generatePin + householdDevice/action/loginWithPin + ownerShip flag for householdDevice/action/addByPin
        REFRESHSESSION = 5, // ottuser/action/refreshSession
        LOGIN = 6, // ottuser/action/login
        LOGOUT = 7, // ottuser/action/logout
        ANONYMOUSLOGIN = 8, // ottuser/action/anonymousLogin
        MULTIREQUEST = 9, // ottuser/action/multirequest
        HOUSEHOLD_USER = 10, // ottuser/action/get
        PLAYBACK = 11, // asset/action/getPlaybackContext => all playback service
        SEGMENTATION = 12 // all segments api's (segmentationType, userSegment, householdSegment)
    }

    public enum KalturaCanaryDeploymentMicroservicesMigrationEvent
    {
        APPTOKEN = 0,
        REFRESHSESSION = 1,
        //GEN-1471- login pin live migration removed due to complexity supporting updates on usage
        //USER_PIN_CODE = 2,
        DEVICE_PIN_CODE = 3,
        SESSION_REVOCATION = 4,
        USER_LOGIN_HISTORY = 5,
        DEVICE_LOGIN_HISTORY = 6
    }
}