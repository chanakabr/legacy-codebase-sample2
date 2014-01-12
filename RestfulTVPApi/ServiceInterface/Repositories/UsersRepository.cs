using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace RestfulTVPApi.ServiceInterface
{
    public class UsersRepository : IUsersRepository
    {
        public UserResponseObject[] GetUsersData(InitializationObject initObj, string siteGuids)
        {
            UserResponseObject[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUsersData(siteGuids);

                XmlDocument a = WSExtension.XmlResponse;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        //Ofir - Moved from Site
        public UserResponseObject SetUserData(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData)
        {
            UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetUserData(siteGuid, userBasicData, userDynamicData);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        //Ofir - Moved from Media
        public PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj, string siteGuid)
        {
            PermittedSubscriptionContainer[] permitedSubscriptions = new PermittedSubscriptionContainer[] { };

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermitedSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                var permitted = new TVPApiModule.Services.ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermitedSubscriptions(siteGuid);

                if (permitted != null)
                    permitedSubscriptions = permitted.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return permitedSubscriptions;
        }

        //Ofir - Moved from Media
        public PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(InitializationObject initObj, string siteGuid, int totalItems)
        {
            PermittedSubscriptionContainer[] items = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                items = new TVPApiModule.Services.ApiConditionalAccessService(groupId, initObj.Platform).GetUserExpiredSubscriptions(siteGuid, totalItems);

                if (items != null)
                    items = items.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            if (items == null)
            {
                items = new PermittedSubscriptionContainer[0];
            }

            return items;
        }

        //Ofir - Moved from Media
        public PermittedMediaContainer[] GetUserPermittedItems(InitializationObject initObj, string siteGuid)
        {
            PermittedMediaContainer[] permittedMediaContainer = { };

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermittedItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                var permitted = new TVPApiModule.Services.ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermittedItems(siteGuid);

                if (permitted != null)
                    permittedMediaContainer = permitted.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return permittedMediaContainer;
        }

        //Ofir - Moved from Media
        public PermittedMediaContainer[] GetUserExpiredItems(InitializationObject initObj, string siteGuid, int totalItems)
        {
            PermittedMediaContainer[] permittedMediaContainer = { };

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                var permitted = new TVPApiModule.Services.ApiConditionalAccessService(groupId, initObj.Platform).GetUserExpiredItems(siteGuid, totalItems);

                if (permitted != null)
                    permittedMediaContainer = permitted.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return permittedMediaContainer;
        }

        //Ofir - Moved from Site
        public UserResponseObject SignUp(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignUp", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SignUp(userBasicData, userDynamicData, sPassword, sAffiliateCode);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        //Ofir - Moved from Media
        //Ofir - Should DomainID passed as param?
        public FavoritObject[] GetUserFavorites(InitializationObject initObj, string siteGuid)
        {
            FavoritObject[] favoritesObj = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserFavorites", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                favoritesObj = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserFavorites(siteGuid, string.Empty, initObj.DomainID, string.Empty);

                favoritesObj = favoritesObj.OrderByDescending(r => r.m_dUpdateDate.Date).ThenByDescending(r => r.m_dUpdateDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return favoritesObj;
        }

        public TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetUserGroupRules(InitializationObject initObj, string siteGuid)
        {
            TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetUserGroupRules(siteGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public bool SetUserGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN, int isActive)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetUserGroupRule(siteGuid, ruleID, PIN, isActive);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public bool CheckGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CheckGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).CheckParentalPIN(siteGuid, ruleID, PIN);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeUserPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ChangeUserPassword(sUN, sOldPass, sPass);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public UserResponseObject RenewUserPassword(InitializationObject initObj, string sUN, string sPass)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RenewUserPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).RenewUserPassword(sUN, sPass);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public UserResponseObject ActivateAccount(InitializationObject initObj, string sUserName, string sToken)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ActivateAccount", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ActivateAccount(sUserName, sToken);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public bool ResendActivationMail(InitializationObject initObj, string sUserName, string sNewPassword)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ResendActivationMail", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ResendActivationMail(sUserName, sNewPassword);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public TVPPro.SiteManager.TvinciPlatform.Users.UserType[] GetGroupUserTypes(InitializationObject initObj)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserType[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupUserTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetGroupUserTypes();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public string RenewUserPIN(InitializationObject initObj, string sSiteGUID, int ruleID)
        {
            ResponseStatus response = ResponseStatus.OK;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RenewUserPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).RenewUserPIN(sSiteGUID, ruleID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response.ToString();
        }

        public bool SendPasswordMail(InitializationObject initObj, string userName)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SendPasswordMail", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SendPasswordMail(userName);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public ResponseStatus SetUserTypeByUserID(InitializationObject initObj, string sSiteGUID, int nUserTypeID)
        {
            ResponseStatus response = ResponseStatus.OK;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserTypeByUserID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetUserTypeByUserID(sSiteGUID, nUserTypeID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public UserResponseObject ActivateAccountByDomainMaster(InitializationObject initObj, string masterUserName, string userName, string token)
        {
            UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ActivateAccountByDomainMaster", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ActivateAccountByDomainMaster(masterUserName, userName, token);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public bool AddItemToList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddItemToList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).AddItemToList(initObj.SiteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public UserItemList[] GetItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            UserItemList[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetItemFromList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetItemFromList(initObj.SiteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public KeyValuePair[] IsItemExistsInList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            KeyValuePair[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsItemExistsInList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IsItemExistsInList(initObj.SiteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public bool RemoveItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveItemFromList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).RemoveItemFromList(initObj.SiteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public bool UpdateItemInList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "UpdateItemInList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).UpdateItemInList(initObj.SiteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public string[] GetPrepaidBalance(InitializationObject initObj, string currencyCode)
        {
            string[] fResponse = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetPrepaidBalance", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                fResponse = new ApiConditionalAccessService(groupId, initObj.Platform).GetPrepaidBalance(initObj.SiteGuid, currencyCode);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return fResponse;
        }

        public UserResponseObject GetUserDataByCoGuid(InitializationObject initObj, string coGuid, int operatorID)
        {
            UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserDataByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserDataByCoGuid(coGuid, operatorID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public List<Media> GetLastWatchedMedias(InitializationObject initObj, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetLastWatchedMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = new APIPersonalLastWatchedLoader(initObj.SiteGuid, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize)
                {
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public List<Media> GetUserSocialMedias(InitializationObject initObj, int socialPlatform, int socialAction, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = new List<Media>();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserSocialMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = new APIUserSocialMediaLoader(initObj.SiteGuid, socialAction, socialPlatform, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize)
                {
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public BillingTransactionsResponse GetUserTransactionHistory(InitializationObject initObj, int start_index, int pageSize)
        {
            BillingTransactionsResponse transactions = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserTransactionHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                transactions = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserTransactionHistory(initObj.SiteGuid, start_index, pageSize);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return transactions;
        }

        public Country[] GetCountriesList(InitializationObject initObj)
        {
            Country[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetCountriesList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetCountriesList();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        //Ofir - moved from ca
        public BillingResponse CC_ChargeUserForPrePaid(InitializationObject initObj, double price, string currency, string productCode, string ppvModuleCode)
        {
            BillingResponse res = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CC_ChargeUserForPrePaid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                res = new ApiConditionalAccessService(groupId, initObj.Platform).CC_ChargeUserForPrePaid(initObj.SiteGuid, price, currency, productCode, ppvModuleCode, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        //Ofir - moved from ca
        public string GetGoogleSignature(InitializationObject initObj, int customerId)
        {
            string res = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetGoogleSignature", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetGoogleSignature(initObj.SiteGuid, customerId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        //Ofir - moved from ca
        public MediaFileItemPricesContainer[] GetItemsPricesWithCoupons(InitializationObject initObj, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            MediaFileItemPricesContainer[] res = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetItemsPricesWithCoupons", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                res = new ApiConditionalAccessService(groupId, initObj.Platform).GetItemsPricesWithCoupons(initObj.SiteGuid, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        //Ofir - moved from ca
        public SubscriptionsPricesContainer[] GetSubscriptionsPricesWithCoupon(InitializationObject initObj, string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            SubscriptionsPricesContainer[] res = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsPricesWithCoupon", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                res = new ApiConditionalAccessService(groupId, initObj.Platform).GetSubscriptionsPricesWithCoupon(initObj.SiteGuid, sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        //Ofir - moved from ca
        public UserBillingTransactionsResponse[] GetUsersBillingHistory(InitializationObject initObj, string[] siteGuids, DateTime startDate, DateTime endDate)
        {
            UserBillingTransactionsResponse[] res = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUsersBillingHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                res = new ApiConditionalAccessService(groupId, initObj.Platform).GetUsersBillingHistory(siteGuids, startDate, endDate);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        //Ofir - moved from ca
        public BillingResponse InApp_ChargeUserForMediaFile(InitializationObject initObj, double price, string currency, string productCode, string ppvModuleCode, string receipt)
        {
            BillingResponse res = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "InApp_ChargeUserForMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                res = new ApiConditionalAccessService(groupId, initObj.Platform).InApp_ChargeUserForMediaFile(initObj.SiteGuid, price, currency, productCode, ppvModuleCode, initObj.UDID, receipt);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        //Ofir - moved from Media
        //Ofir - Should SiteGuid, DomainID, UDID, LocalLangauge be a param
        public List<Media> GetUserItems(InitializationObject initObj, UserItemType itemType, string picSize, int pageSize, int start_index)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = MediaHelper.GetUserItems(initObj.Platform, initObj.SiteGuid, initObj.DomainID, initObj.UDID, initObj.Locale.LocaleLanguage, itemType, picSize, pageSize, start_index, groupID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }
    }
}