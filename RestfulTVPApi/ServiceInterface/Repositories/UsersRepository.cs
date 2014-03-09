using System;
using System.Collections.Generic;
using System.Linq;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Interfaces;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPApiModule.Manager;
using TVPApiModule.Context;
using TVPApiModule.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class UsersRepository : IUsersRepository
    {
        public List<UserResponseObject> GetUsersData(InitializationObject initObj, string siteGuids)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).GetUsersData(siteGuids);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public UserResponseObject SetUserData(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).SetUserData(siteGuid, userBasicData, userDynamicData);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<PermittedSubscriptionContainer> GetUserPermitedSubscriptions(InitializationObject initObj, string siteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermitedSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ConditionalAccessService(groupID, initObj.Platform).GetUserPermitedSubscriptions(siteGuid);             
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<PermittedSubscriptionContainer> GetUserExpiredSubscriptions(InitializationObject initObj, string siteGuid, int totalItems)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ConditionalAccessService(groupID, initObj.Platform).GetUserExpiredSubscriptions(siteGuid, totalItems);                
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<PermittedMediaContainer> GetUserPermittedItems(InitializationObject initObj, string siteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermittedItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ConditionalAccessService(groupID, initObj.Platform).GetUserPermittedItems(siteGuid);              
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<PermittedMediaContainer> GetUserExpiredItems(InitializationObject initObj, string siteGuid, int totalItems)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ConditionalAccessService(groupID, initObj.Platform).GetUserExpiredItems(siteGuid, totalItems);               
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public UserResponseObject SignUp(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignUp", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).SignUp(userBasicData, userDynamicData, sPassword, sAffiliateCode);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        //Ofir - Should DomainID passed as param?
        public List<FavoriteObject> GetUserFavorites(InitializationObject initObj, string siteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserFavorites", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).GetUserFavorites(siteGuid, string.Empty, initObj.DomainID, string.Empty);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<GroupRule> GetUserGroupRules(InitializationObject initObj, string siteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ApiApiService(groupID, initObj.Platform).GetUserGroupRules(siteGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SetUserGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN, int isActive)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ApiApiService(groupID, initObj.Platform).SetUserGroupRule(siteGuid, ruleID, PIN, isActive);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool CheckGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CheckGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ApiApiService(groupID, initObj.Platform).CheckParentalPIN(siteGuid, ruleID, PIN);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeUserPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).ChangeUserPassword(sUN, sOldPass, sPass);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public UserResponseObject RenewUserPassword(InitializationObject initObj, string sUN, string sPass)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RenewUserPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).RenewUserPassword(sUN, sPass);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public UserResponseObject ActivateAccount(InitializationObject initObj, string sUserName, string sToken)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ActivateAccount", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).ActivateAccount(sUserName, sToken);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool ResendActivationMail(InitializationObject initObj, string sUserName, string sNewPassword)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ResendActivationMail", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).ResendActivationMail(sUserName, sNewPassword);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public eResponseStatus RenewUserPIN(InitializationObject initObj, string siteGuid, int ruleID)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RenewUserPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).RenewUserPIN(siteGuid, ruleID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public eResponseStatus SetUserTypeByUserID(InitializationObject initObj, string siteGuid, int nUserTypeID)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserTypeByUserID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).SetUserTypeByUserID(siteGuid, nUserTypeID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public UserResponseObject ActivateAccountByDomainMaster(InitializationObject initObj, string masterUserName, string userName, string token)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ActivateAccountByDomainMaster", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.UsersService(groupId, initObj.Platform).ActivateAccountByDomainMaster(masterUserName, userName, token);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool AddItemToList(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "AddItemToList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.UsersService(groupId, initObj.Platform).AddItemToList(siteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<UserItemList> GetItemFromList(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetItemFromList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.UsersService(groupId, initObj.Platform).GetItemFromList(siteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            };
        }

        public List<KeyValuePair> IsItemExistsInList(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsItemExistsInList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.UsersService(groupId, initObj.Platform).IsItemExistsInList(siteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool RemoveItemFromList(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "RemoveItemFromList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.UsersService(groupId, initObj.Platform).RemoveItemFromList(siteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool UpdateItemInList(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "UpdateItemInList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.UsersService(groupId, initObj.Platform).UpdateItemInList(siteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<string> GetPrepaidBalance(InitializationObject initObj, string siteGuid, string currencyCode)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetPrepaidBalance", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.ConditionalAccessService(groupId, initObj.Platform).GetPrepaidBalance(siteGuid, currencyCode);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<Media> GetLastWatchedMediasByPeriod(InitializationObject initObj, string siteGuid, string picSize, int periodBefore, MediaHelper.ePeriod byPeriod)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetLastWatchedMediasByPeriod", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                List<Media> lstAllMedias = new TVPApiModule.CatalogLoaders.APIPersonalLastWatchedLoader(siteGuid, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, 100, 0, picSize).Execute() as List<Media>;

                if (lstAllMedias != null)
                {
                    lstMedia = (from media in lstAllMedias
                                where
                                    (DateTime.Now.AddDays((double)byPeriod * periodBefore * -1) - (DateTime)media.last_watch_date).TotalDays >= 0 &&
                                    (DateTime.Now.AddDays((double)byPeriod * periodBefore * -1) - (DateTime)media.last_watch_date).TotalDays <= (periodBefore + 1) * (int)byPeriod
                                select media).ToList<Media>();
                }
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public List<Media> GetUserSocialMedias(InitializationObject initObj, string siteGuid, int socialPlatform, int socialAction, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserSocialMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = new APIUserSocialMediaLoader(siteGuid, socialAction, socialPlatform, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize)
                {
                    //UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public BillingTransactionsResponse GetUserTransactionHistory(InitializationObject initObj, string siteGuid, int start_index, int pageSize)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserTransactionHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ConditionalAccessService(groupID, initObj.Platform).GetUserTransactionHistory(siteGuid, start_index, pageSize);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public BillingResponse CC_ChargeUserForPrePaid(InitializationObject initObj, string siteGuid, double price, string currency, string productCode, string ppvModuleCode)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CC_ChargeUserForPrePaid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ConditionalAccessService(groupID, initObj.Platform).CC_ChargeUserForPrePaid(siteGuid, price, currency, productCode, ppvModuleCode, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<UserBillingTransactionsResponse> GetUsersBillingHistory(InitializationObject initObj, string[] siteGuids, DateTime startDate, DateTime endDate)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUsersBillingHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ConditionalAccessService(groupID, initObj.Platform).GetUsersBillingHistory(siteGuids, startDate, endDate);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<Media> GetUserItems(InitializationObject initObj, string siteGuid, UserItemType itemType, string picSize, int pageSize, int start_index)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return MediaHelper.GetUserItems(initObj.Platform, siteGuid, initObj.DomainID, initObj.UDID, initObj.Locale.LocaleLanguage, itemType, picSize, pageSize, start_index, groupID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public AdyenBillingDetail GetLastBillingUserInfo(InitializationObject initObj, string siteGuid, int billingMethod)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetLastBillingUserInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.BillingService(groupID, initObj.Platform).GetLastBillingUserInfo(siteGuid, billingMethod);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public string GetClientMerchantSig(InitializationObject initObj, string sParamaters)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetClientMerchantSig", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.BillingService(groupID, initObj.Platform).GetClientMerchantSig(sParamaters);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<KeyValuePair<int, bool>> AreMediasFavorite(InitializationObject initObj, string siteGuid, List<int> mediaIds)
        {
            List<KeyValuePair<int, bool>> result = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AreMediasFavorite", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                List<FavoriteObject> favoriteObjects = ServicesManager.UsersService(groupID, initObj.Platform).GetUserFavorites(siteGuid, string.Empty, initObj.DomainID, string.Empty);

                if (favoriteObjects != null)
                    result = mediaIds.Select(y => new KeyValuePair<int, bool>(y, favoriteObjects.Where(x => x.item_code == y.ToString()).Count() > 0)).ToList();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return result;
        }

        public List<Media> GetRecommendedMediasByTypes(InitializationObject initObj, string siteGuid, string picSize, int pageSize, int pageIndex, int[] reqMediaTypes)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRecommendedMediasByTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = new TVPApiModule.CatalogLoaders.APIPersonalRecommendedLoader(siteGuid, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize)
                {
                    //UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public bool CancelSubscription(InitializationObject initObj, string siteGuid, string sSubscriptionID, int sSubscriptionPurchaseID)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CancelSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ConditionalAccessService(groupID, initObj.Platform).CancelSubscription(siteGuid, sSubscriptionID, sSubscriptionPurchaseID); 
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<Notification> GetDeviceNotifications(InitializationObject initObj, string siteGuid, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, Nullable<int> messageCount)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceNotifications", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.NotificationService(groupId, initObj.Platform).GetDeviceNotifications(siteGuid, initObj.UDID, notificationType == NotificationMessageType.All ? NotificationMessageType.Pull : notificationType, viewStatus, messageCount);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SetNotificationMessageViewStatus(InitializationObject initObj, string siteGuid, Nullable<long> notificationRequestID, Nullable<long> notificationMessageID, NotificationMessageViewStatus viewStatus)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetNotificationMessageViewStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.NotificationService(groupId, initObj.Platform).SetNotificationMessageViewStatus(siteGuid, notificationRequestID, notificationMessageID, viewStatus);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<TagMetaPairArray> GetUserStatusSubscriptions(InitializationObject initObj, string siteGuid)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserStatusSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.NotificationService(groupId, initObj.Platform).GetUserStatusSubscriptions(siteGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool CleanUserHistory(InitializationObject initObj, string siteGuid, int[] mediaIDs)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CleanUserHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ApiApiService(groupID, initObj.Platform).CleanUserHistory(siteGuid, mediaIDs);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<string> GetUserStartedWatchingMedias(InitializationObject initObj, string siteGuid, int numOfItems)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserStartedWatchingMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ApiApiService(groupID, initObj.Platform).GetUserStartedWatchingMedias(siteGuid, numOfItems);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SendNewPassword(InitializationObject initObj, string sUserName)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SendNewPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).SentNewPasswordToUser(sUserName);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool IsUserSignedIn(InitializationObject initObj, string siteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsUserSignedIn", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                bool isSingleLogin = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;

                return ServicesManager.UsersService(groupID, initObj.Platform).IsUserLoggedIn(siteGuid, initObj.UDID, string.Empty, SiteHelper.GetClientIP(), isSingleLogin);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SetUserDynamicData(InitializationObject initObj, string siteGuid, string sKey, string sValue)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserDynamicData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.UsersService(groupID, initObj.Platform).SetUserDynamicData(siteGuid, sKey, sValue);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public ApiUsersService.LogInResponseData SignIn(InitializationObject initObj, string userName, string password)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                IImplementation impl = WSUtils.GetImplementation(groupID, initObj);
                
                return impl.SignIn(userName, password);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public void SignOut(InitializationObject initObj, string siteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                bool isSingleLogin = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                
                ServicesManager.UsersService(groupID, initObj.Platform).SignOut(siteGuid, initObj.UDID, string.Empty, isSingleLogin);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<FriendWatchedObject> GetAllFriendsWatched(InitializationObject initObj, string siteGuid, int maxResult)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetAllFriendsWatched", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.SocialService(groupId, initObj.Platform).GetAllFriendsWatched(siteGuid, maxResult);                
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public SocialActionResponseStatus DoUserAction(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DoUserAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
           
            if (groupId > 0)
            {
                return ServicesManager.SocialService(groupId, initObj.Platform).DoUserAction(siteGuid, initObj.UDID, userAction, extraParams, socialPlatform, assetType, assetID);                
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<UserSocialActionObject> GetFriendsActions(InitializationObject initObj, string siteGuid, string[] userActions, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFriendsActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                return ServicesManager.SocialService(groupId, initObj.Platform).GetFriendsActions(siteGuid, userActions, assetType, assetID, startIndex, numOfRecords, socialPlatform);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<UserSocialActionObject> GetUserActions(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                return ServicesManager.SocialService(groupId, initObj.Platform).GetUserActions(siteGuid, userAction, assetType, assetID, startIndex, numOfRecords, socialPlatform);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<eSocialPrivacy> GetUserAllowedSocialPrivacyList(InitializationObject initObj, string siteGuid)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserAllowedSocialPrivacyList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                return ServicesManager.SocialService(groupId, initObj.Platform).GetUserAllowedSocialPrivacyList(siteGuid);                
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public eSocialActionPrivacy GetUserExternalActionShare(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserExternalActionShare", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                return ServicesManager.SocialService(groupId, initObj.Platform).GetUserExternalActionShare(siteGuid, userAction, socialPlatform);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<string> GetUserFriends(InitializationObject initObj, string siteGuid)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserFriends", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.SocialService(groupId, initObj.Platform).GetUserFriends(siteGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public eSocialActionPrivacy GetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserInternalActionPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                return ServicesManager.SocialService(groupId, initObj.Platform).GetUserInternalActionPrivacy(siteGuid, userAction, socialPlatform);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public eSocialPrivacy GetUserSocialPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserSocialPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                return ServicesManager.SocialService(groupId, initObj.Platform).GetUserSocialPrivacy(siteGuid, socialPlatform, userAction);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SetUserExternalActionShare(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetUserExternalActionShare", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                return ServicesManager.SocialService(groupId, initObj.Platform).SetUserExternalActionShare(siteGuid, userAction, socialPlatform, actionPrivacy);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetUserInternalActionPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                return ServicesManager.SocialService(groupId, initObj.Platform).SetUserInternalActionPrivacy(siteGuid, userAction, socialPlatform, actionPrivacy);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public int AD_GetCustomDataID(InitializationObject initObj, string siteGuid, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string countryCd2, string languageCode3, string deviceName, int assetType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "AD_GetCustomDataID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                return ServicesManager.ConditionalAccessService(groupId, initObj.Platform).AD_GetCustomDataID(siteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, SiteHelper.GetClientIP(), countryCd2, languageCode3, deviceName, assetType);}
            else
            {
                throw new UnknownGroupException();
            }
        }

        public int GetCustomDataID(InitializationObject initObj, string siteGuid, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string countryCd2, string languageCode3, string deviceName, int assetType, string overrideEndDate)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCustomDataID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                return ServicesManager.ConditionalAccessService(groupId, initObj.Platform).GetCustomDataID(siteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, SiteHelper.GetClientIP(), countryCd2, languageCode3, deviceName, assetType, overrideEndDate);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public BillingResponse InApp_ChargeUserForMediaFile(InitializationObject initObj, string sSiteGUID, double price, string currency, string productCode, string ppvModuleCode, string receipt)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "InApp_ChargeUserForMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.ConditionalAccessService(groupId, initObj.Platform).InApp_ChargeUserForMediaFile(sSiteGUID, price, currency, productCode, ppvModuleCode, initObj.UDID, receipt);                
            }
            else
            {
                throw new UnknownGroupException();
            }
        }
    }
}