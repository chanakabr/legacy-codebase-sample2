using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IUsersRepository
    {
        UserResponseObject GetUserData(InitializationObject initObj, string siteGuid);

        UserResponseObject SetUserData(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData);

        PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj, string siteGuid);

        PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(InitializationObject initObj, string siteGuid, int iTotalItems);

        PermittedMediaContainer[] GetUserPermittedItems(InitializationObject initObj, string siteGuid);

        PermittedMediaContainer[] GetUserExpiredItems(InitializationObject initObj, string siteGuid, int iTotalItems);

        UserResponseObject SignUp(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode);

        FavoritObject[] GetUserFavorites(InitializationObject initObj, string siteGuid);

        bool AddUserFavorite(InitializationObject initObj, string siteGuid, int mediaID, int mediaType, int extraVal);

        bool RemoveUserFavorite(InitializationObject initObj, string siteGuid, int mediaID);

        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetUserGroupRules(InitializationObject initObj, string siteGuid);

        bool SetUserGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN, int isActive);

        bool CheckGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN);

        UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass);

        UserResponseObject RenewUserPassword(InitializationObject initObj, string sUN, string sPass);

        UserResponseObject ActivateAccount(InitializationObject initObj, string sUserName, string sToken);

        bool ResendActivationMail(InitializationObject initObj, string sUserName, string sNewPassword);

        TVPPro.SiteManager.TvinciPlatform.Users.UserType[] GetGroupUserTypes(InitializationObject initObj);

        string RenewUserPIN(InitializationObject initObj, string sSiteGUID, int ruleID);

        bool SendPasswordMail(InitializationObject initObj, string userName);

        ResponseStatus SetUserTypeByUserID(InitializationObject initObj, string sSiteGUID, int nUserTypeID);

        UserResponseObject ActivateAccountByDomainMaster(InitializationObject initObj, string masterUserName, string userName, string token);

        bool AddItemToList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        UserItemList[] GetItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        KeyValuePair[] IsItemExistsInList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        bool RemoveItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        bool UpdateItemInList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        string[] GetPrepaidBalance(InitializationObject initObj, string currencyCode);

        UserResponseObject GetUserDataByCoGuid(InitializationObject initObj, string coGuid, int operatorID);
    }
}
