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
        UserResponseObject[] GetUsersData(InitializationObject initObj, string siteGuids);

        UserResponseObject SetUserData(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData);

        PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj, string siteGuid);

        PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(InitializationObject initObj, string siteGuid, int iTotalItems);

        PermittedMediaContainer[] GetUserPermittedItems(InitializationObject initObj, string siteGuid);

        PermittedMediaContainer[] GetUserExpiredItems(InitializationObject initObj, string siteGuid, int iTotalItems);

        UserResponseObject SignUp(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode);

        FavoritObject[] GetUserFavorites(InitializationObject initObj, string siteGuid);

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

        List<Media> GetLastWatchedMedias(InitializationObject initObj, string picSize, int pageSize, int pageIndex);

        List<Media> GetUserSocialMedias(InitializationObject initObj, int socialPlatform, int socialAction, string picSize, int pageSize, int pageIndex);

        BillingTransactionsResponse GetUserTransactionHistory(InitializationObject initObj, int start_index, int pageSize);

        Country[] GetCountriesList(InitializationObject initObj);

        BillingResponse CC_ChargeUserForPrePaid(InitializationObject initObj, double price, string currency, string productCode, string ppvModuleCode);

        string GetGoogleSignature(InitializationObject initObj, int customerId);

        MediaFileItemPricesContainer[] GetItemsPricesWithCoupons(InitializationObject initObj, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName);

        SubscriptionsPricesContainer[] GetSubscriptionsPricesWithCoupon(InitializationObject initObj, string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);

        UserBillingTransactionsResponse[] GetUsersBillingHistory(InitializationObject initObj, string[] siteGuids, DateTime startDate, DateTime endDate);

        BillingResponse InApp_ChargeUserForMediaFile(InitializationObject initObj, double price, string currency, string productCode, string ppvModuleCode, string receipt);

        List<Media> GetUserItems(InitializationObject initObj, UserItemType itemType, string picSize, int pageSize, int start_index);
    }
}
