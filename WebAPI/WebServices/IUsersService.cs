using System.Collections.Generic;
using System.ServiceModel;
using ApiObjects;
using ApiObjects.Response;
using Core.Users;

namespace WebAPI.WebServices
{
    [ServiceContract(Namespace="http://user.tvinci.com/")]
    public interface IUsersService
    {
        [OperationContract]
        UserResponse ActivateAccount(string sWSUserName, string sWSPassword, string sUserName, string sToken);
        [OperationContract]
        UserResponseObject ActivateAccountByDomainMaster(string sWSUserName, string sWSPassword, string sMasterUsername, string sUserName, string sToken);
        [OperationContract]
        bool AddChannelMediaToFavorites(string sWSUserName, string sWSPassword, string sUserGUID, int domainID, string sDeviceUDID, string sItemType, string sChannelID, string sExtraData);
        [OperationContract]
        void AddInitiateNotificationAction(string sWSUserName, string sWSPassword, eUserMessageAction userAction, int userId, string udid, string pushToken = "");
        [OperationContract]
        bool AddItemToList(string sWSUserName, string sWSPassword, UserItemList userItemList);
        [OperationContract]
        UsersListItemResponse AddItemToUsersList(string sWSUserName, string sWSPassword, string userId, Item item);
        [OperationContract]
        UserResponseObject AddNewKalturaUser(string sWSUserName, string sWSPassword, UserBasicData oBasicData, UserDynamicData sDynamicData, string sPassword, string sAffiliateCode, List<KeyValuePair> keyValueList);
        [OperationContract]
        UserResponseObject AddNewUser(string sWSUserName, string sWSPassword, UserBasicData oBasicData, UserDynamicData sDynamicData, string sPassword, string sAffiliateCode);
        [OperationContract]
        ApiObjects.Response.Status AddRoleToUser(string sWSUserName, string sWSPassword, string userId, long roleId);
        [OperationContract]
        ApiObjects.Response.Status AddUserFavorit(string sWSUserName, string sWSPassword, string sUserGUID, int domainID, string sDeviceUDID, string sItemType, string sItemCode, string sExtraData);
        [OperationContract]
        bool AddUserOfflineAsset(string sWSUserName, string sWSPassword, string sSiteGuid, string sMediaID);
        [OperationContract]
        UserResponseObject AutoSignIn(string sWSUserName, string sWSPassword, string sSiteGUID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins);
        [OperationContract]
        UserGroupRuleResponse ChangeParentalPInCodeByToken(string sWSUserName, string sWSPassword, string sSiteGuid, int nUserRuleID, string sChangePinToken, string sCode);
        [OperationContract]
        UserResponseObject ChangePassword(string sWSUserName, string sWSPassword, string sUN);
        [OperationContract]
        UserResponseObject ChangeUserPassword(string sWSUserName, string sWSPassword, string sUN, string sOldPass, string sPass);
        [OperationContract]
        ApiObjects.Response.Status ChangeUsers(string sWSUserName, string sWSPassword, string userId, string userIdToChange, string udid);
        [OperationContract]
        UserGroupRuleResponse CheckParentalPINToken(string sWSUserName, string sWSPassword, string sToken);
        [OperationContract]
        UserResponse CheckPasswordToken(string sWSUserName, string sWSPassword, string token);
        [OperationContract]
        UserResponseObject CheckTemporaryToken(string sWSUserName, string sWSPassword, string sToken);
        [OperationContract]
        UserResponseObject CheckUserPassword(string sWSUserName, string sWSPassword, string sUserName, string sPassword, bool bPreventDoubleLogins);
        [OperationContract]
        ApiObjects.Response.Status ClearLoginPIN(string sWSUserName, string sWSPassword, string siteGuid, string pin);
        [OperationContract]
        bool ClearUserOfflineAssets(string sWSUserName, string sWSPassword, string sSiteGuid);
        [OperationContract]
        ApiObjects.Response.Status DeleteItemFromUsersList(string sWSUserName, string sWSPassword, string userId, Item item);
        [OperationContract]
        ApiObjects.Response.Status DeleteUser(string sWSUserName, string sWSPassword, int userId);
        [OperationContract]
        bool DoesUserNameExists(string sWSUserName, string sWSPassword, string sUserName);
        [OperationContract]
        FavoriteResponse FilterFavoriteMediaIds(string sWSUserName, string sWSPassword, string userId, List<int> mediaIds, string udid, string mediaType, FavoriteOrderBy orderBy);
        [OperationContract]
        UserResponseObject ForgotPassword(string sWSUserName, string sWSPassword, string sUN);
        [OperationContract]
        PinCodeResponse GenerateLoginPIN(string sWSUserName, string sWSPassword, string siteGuid, string secret);
        [OperationContract]
        UserOfflineObject[] GetAllUserOfflineAssets(string sWSUserName, string sWSPassword, string sSiteGuid);
        [OperationContract]
        Core.Users.Country[] GetCountryList(string sWSUserName, string sWSPassword);
        [OperationContract]
        UserType[] GetGroupUserTypes(string sWSUserName, string sWSPassword);
        [OperationContract]
        Core.Users.Country GetIPToCountry(string sWSUserName, string sWSPassword, string sUserIP);
        [OperationContract]
        UserItemListsResponse GetItemFromList(string sWSUserName, string sWSPassword, UserItemList userItemList);
        [OperationContract]
        UsersListItemResponse GetItemFromUsersList(string sWSUserName, string sWSPassword, string userId, Item item);
        [OperationContract]
        UsersItemsListsResponse GetItemsFromUsersLists(string sWSUserName, string sWSPassword, List<string> userIds, ListType listType, ListItemType itemType);
        [OperationContract]
        State[] GetStateList(string sWSUserName, string sWSPassword, int nCountryID);
        [OperationContract]
        UserResponse GetUserByExternalID(string sWSUserName, string sWSPassword, string externalId, int operatorID);
        [OperationContract]
        UserResponseObject GetUserByFacebookID(string sWSUserName, string sWSPassword, string sFacebookID);
        [OperationContract]
        UserResponse GetUserByName(string sWSUserName, string sWSPassword, string username);
        [OperationContract]
        UserResponseObject GetUserByUsername(string sWSUserName, string sWSPassword, string sUsername);
        [OperationContract]
        UserResponseObject GetUserData(string sWSUserName, string sWSPassword, string sSiteGUID, string sUserIp);
        [OperationContract]
        UserResponseObject GetUserDataByCoGuid(string sWSUserName, string sWSPassword, string sCoGuid, int operatorID);
        [OperationContract]
        FavoriteResponse GetUserFavorites(string sWSUserName, string sWSPassword, string sUserGUID, int domainID, string sDeviceUDID, string sItemType, FavoriteOrderBy orderBy);
        [OperationContract]
        UserState GetUserInstanceState(string sWSUserName, string sWSPassword, string sSiteGUID, string sessionID, string deviceID, string sIP);
        [OperationContract]
        LongIdsResponse GetUserRoleIds(string sWSUserName, string sWSPassword, string userId);
        [OperationContract]
        UsersResponse GetUsers(string sWSUserName, string sWSPassword, string[] sSiteGUIDs, string userIP);
        [OperationContract]
        List<UserResponseObject> GetUsersData(string sWSUserName, string sWSPassword, string[] sSiteGUIDs, string userIp);
        [OperationContract]
        User[] GetUsersLikedMedia(string sWSUserName, string sWSPassword, int nUserGuid, int nMediaID, int nPlatform, bool bOnlyFriends, int nStartIndex, int nNumberOfItems);
        [OperationContract]
        UserState GetUserState(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        string GetUserToken(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        int GetUserType(string sWSUserName, string sWSPassword, string sSiteGuid);
        [OperationContract]
        void Hit(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        List<KeyValuePair> IsItemExistsInList(string sWSUserName, string sWSPassword, UserItemList userItemList);
        [OperationContract]
        ApiObjects.Response.Status IsUserActivated(string sWSUserName, string sWSPassword, int userId);
        [OperationContract]
        UserResponseObject KalturaSignIn(string sWSUserName, string sWSPassword, string sUserName, string sPassword, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, List<KeyValuePair> keyValueList);
        [OperationContract]
        UserResponseObject KalturaSignOut(string sWSUserName, string sWSPassword, string sSiteGUID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, List<KeyValuePair> keyValueList);
        [OperationContract]
        UserResponse LogIn(string sWSUserName, string sWSPassword, string userName, string password, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, List<KeyValuePair> keyValueList);
        [OperationContract]
        UserResponse LoginWithPIN(string sWSUserName, string sWSPassword, string PIN, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, List<KeyValuePair> keyValueList, string secret);
        [OperationContract]
        void Logout(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        bool Purge();
        [OperationContract]
        void RemoveChannelMediaUserFavorit(string sWSUserName, string sWSPassword, string sUserGUID, int[] nChannelIDs);
        [OperationContract]
        bool RemoveItemFromList(string sWSUserName, string sWSPassword, UserItemList userItemList);
        [OperationContract]
        ApiObjects.Response.Status RemoveUserFavorit(string sWSUserName, string sWSPassword, string sUserGUID, long[] nMediaIDs);
        [OperationContract]
        bool RemoveUserOfflineAsset(string sWSUserName, string sWSPassword, string sSiteGuid, string sMediaID);
        [OperationContract]
        ApiObjects.Response.Status RenewPassword(string sWSUserName, string sWSPassword, string userName, string newPassword);
        [OperationContract]
        UserResponseObject RenewUserPassword(string sWSUserName, string sWSPassword, string sUserName, string sNewPassword);
        [OperationContract]
        ApiObjects.Response.Status ReplacePassword(string sWSUserName, string sWSPassword, string userName, string oldPassword, string newPassword);
        [OperationContract]
        ApiObjects.Response.Status ResendActivationMail(string sWSUserName, string sWSPassword, string sUserName, string sNewPassword);
        [OperationContract]
        ApiObjects.Response.Status ResendActivationToken(string sWSUserName, string sWSPassword, string username);
        [OperationContract]
        bool ResendWelcomeMail(string sWSUserName, string sWSPassword, string sUserName, string sNewPassword);
        [OperationContract]
        List<UserBasicData> SearchUsers(string sWSUserName, string sWSPassword, string[] sTerms, string[] sFields, bool bIsExact);
        [OperationContract]
        List<UserBasicData> SearchUsers_MT(string sWSUserName, string sWSPassword, string sTerms, string sFields, bool bIsExact);
        [OperationContract]
        ResponseStatus SendChangedPinMail(string sWSUserName, string sWSPassword, string sSiteGuid, int nUserRuleID);
        [OperationContract]
        bool SendPasswordMail(string sWSUserName, string sWSPassword, string sUserName);
        [OperationContract]
        ApiObjects.Response.Status SendRenewalPasswordMail(string sWSUserName, string sWSPassword, string userName);
        [OperationContract]
        PinCodeResponse SetLoginPIN(string sWSUserName, string sWSPassword, string siteGuid, string PIN, string secret);
        [OperationContract]
        UserResponse SetUser(string sWSUserName, string sWSPassword, string siteGUID, UserBasicData basicData, UserDynamicData dynamicData);
        [OperationContract]
        UserResponseObject SetUserData(string sWSUserName, string sWSPassword, string sSiteGUID, UserBasicData oBasicData, UserDynamicData sDynamicData);
        [OperationContract]
        bool SetUserDynamicData(string sWSUserName, string sWSPassword, string sSiteGuid, string sType, string sValue);
        [OperationContract]
        ResponseStatus SetUserTypeByUserID(string sWSUserName, string sWSPassword, string sSiteGuid, int nUserTypeID);
        [OperationContract]
        UserResponseObject SignIn(string sWSUserName, string sWSPassword, string sUserName, string sPassword, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, KeyValuePair[] KeyValuePairs);
        [OperationContract]
        UserResponseObject SignInWithToken(string sWSUserName, string sWSPassword, string sToken, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins);
        [OperationContract]
        UserResponseObject SignOut(string sWSUserName, string sWSPassword, string sSiteGUID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins);
        [OperationContract]
        UserResponse SignUp(string sWSUserName, string sWSPassword, UserBasicData oBasicData, UserDynamicData dynamicData, string password, string affiliateCode);
        [OperationContract]
        UserResponseObject SSOCheckLogin(string sWSUserName, string sWSPassword, string sUserName, int nSSOProviderID);
        [OperationContract]
        UserResponseObject SSOSignIn(string sWSUserName, string sWSPassword, string sUserName, string sPassword, int nSSOProviderID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins);
        [OperationContract]
        bool UpdateItemInList(string sWSUserName, string sWSPassword, UserItemList userItemList);
        [OperationContract]
        ApiObjects.Response.Status UpdateUserPassword(string sWSUserName, string sWSPassword, int userId, string password);
        [OperationContract]
        bool WriteLog(string sWSUserName, string sWSPassword, string sUserGUID, string sLogMessage, string sWriter);
    }
}