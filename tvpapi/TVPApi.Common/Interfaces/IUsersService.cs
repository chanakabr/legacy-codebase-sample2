using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using TVPApi;
using TVPApiModule.Objects;
using Core.Users;
using ApiObjects;
using InitializationObject = TVPApi.InitializationObject;
using ApiObjects.Billing;
using UserType = ApiObjects.UserType;
using TVPApiModule.Objects.CRM;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IUsersService
    {
        [OperationContract]
        UserResponseObjectDTO ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass);

        [OperationContract]
        UserResponseObject GetUserByFacebookID(InitializationObject initObj, string facebookId);

        [OperationContract]
        UserResponseObject GetUserByUsername(InitializationObject initObj, string userName);

        [OperationContract]
        void Logout(InitializationObject initObj, string sSiteGuid);

        [OperationContract]
        UserResponseObject ActivateAccount(InitializationObject initObj, string sUserName, string sToken);

        [OperationContract]
        bool ResendActivationMail(InitializationObject initObj, string sUserName, string sNewPassword);

        [OperationContract]
        Core.Users.Country[] GetCountriesList(InitializationObject initObj);

        [OperationContract]
        UserType[] GetGroupUserTypes(InitializationObject initObj);

        [OperationContract]
        string CheckTemporaryToken(InitializationObject initObj, string sToken);

        [OperationContract]
        string RenewUserPIN(InitializationObject initObj, string sSiteGUID, int ruleID);

        [OperationContract]
        UserResponseObject ActivateAccountByDomainMaster(InitializationObject initObj, string masterUserName, string userName, string token);

        [OperationContract]
        bool SendPasswordMail(InitializationObject initObj, string userName);

        [OperationContract]
        bool AddItemToList(InitializationObject initObj, ItemObj[] itemObjects, ListItemType itemType, ListType listType);

        [OperationContract]
        bool RemoveItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ListItemType itemType, ListType listType);

        [OperationContract]
        bool UpdateItemInList(InitializationObject initObj, ItemObj[] itemObjects, ListItemType itemType, ListType listType);

        [OperationContract]
        UserItemList[] GetItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ListItemType itemType, ListType listType);

        [OperationContract]
        KeyValuePair[] IsItemExistsInList(InitializationObject initObj, ItemObj[] itemObjects, ListItemType itemType, ListType listType);

        [OperationContract]
        TVPApiModule.Objects.UserResponse SetUserDynamicDataEx(InitializationObject initObj, string key, string value);

        [OperationContract]
        TVPApiModule.Objects.Responses.PinCodeResponse GenerateLoginPIN(InitializationObject initObj, string secret);
        
        [OperationContract]
        TVPApiModule.Objects.Responses.UserResponse LoginWithPIN(InitializationObject initObj, string PIN, string secret);
        
        [OperationContract]
        TVPApiModule.Objects.Responses.PinCodeResponse SetLoginPIN(InitializationObject initObj, string PIN, string secret);
        
        [OperationContract]
        TVPApiModule.Objects.Responses.ClientResponseStatus ClearLoginPIN(InitializationObject initObj, string pinCode);

        [OperationContract]
        TVPApiModule.Objects.Responses.ClientResponseStatus ClearLoginPINs(InitializationObject initObj);

        [OperationContract]
        TVPApiModule.Objects.Responses.ClientResponseStatus DeleteUser(InitializationObject initObj);
        
    }
}
