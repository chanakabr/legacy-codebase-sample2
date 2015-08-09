using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Social;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPApiModule.Objects;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IUsersService
    {
        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject GetUserByFacebookID(InitializationObject initObj, string facebookId);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject GetUserByUsername(InitializationObject initObj, string userName);

        [OperationContract]
        void Logout(InitializationObject initObj, string sSiteGuid);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject ActivateAccount(InitializationObject initObj, string sUserName, string sToken);

        [OperationContract]
        bool ResendActivationMail(InitializationObject initObj, string sUserName, string sNewPassword);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.Country[] GetCountriesList(InitializationObject initObj);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.UserType[] GetGroupUserTypes(InitializationObject initObj);

        [OperationContract]
        string CheckTemporaryToken(InitializationObject initObj, string sToken);

        [OperationContract]
        string RenewUserPIN(InitializationObject initObj, string sSiteGUID, int ruleID);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject ActivateAccountByDomainMaster(InitializationObject initObj, string masterUserName, string userName, string token);

        [OperationContract]
        bool SendPasswordMail(InitializationObject initObj, string userName);

        [OperationContract]
        bool AddItemToList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        [OperationContract]
        bool RemoveItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        [OperationContract]
        bool UpdateItemInList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        [OperationContract]
        UserItemList[] GetItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.KeyValuePair[] IsItemExistsInList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        [OperationContract]
        TVPApiModule.Objects.UserResponse SetUserDynamicDataEx(InitializationObject initObj, string key, string value);

        [OperationContract]
        TVPApiModule.Objects.Responses.PinCodeResponse GenerateLoginPIN(InitializationObject initObj, string secret);
        
        [OperationContract]
        TVPApiModule.Objects.Responses.UserResponse LoginWithPIN(InitializationObject initObj, string PIN, string secret);
        
        [OperationContract]
        TVPApiModule.Objects.Responses.ClientResponseStatus SetLoginPIN(InitializationObject initObj, string PIN, string secret);
        
        [OperationContract]
        TVPApiModule.Objects.Responses.ClientResponseStatus ClearLoginPIN(InitializationObject initObj, string pinCode);

        [OperationContract]
        TVPApiModule.Objects.Responses.ClientResponseStatus DeleteUserLoginPinCodes(InitializationObject initObj);
    }
}
