using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Social;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IUsersService
    {
        [OperationContract]
        UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass);

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
        Country[] GetCountriesList(InitializationObject initObj);

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
        bool AddItemToList(InitializationObject initObj, string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        [OperationContract]
        bool RemoveItemFromList(InitializationObject initObj, string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        [OperationContract]
        bool UpdateItemInList(InitializationObject initObj, string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        [OperationContract]
        UserItemList[] GetItemFromList(InitializationObject initObj, string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.KeyValuePair[] IsItemExistsInList(InitializationObject initObj, string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType);
    }
}
