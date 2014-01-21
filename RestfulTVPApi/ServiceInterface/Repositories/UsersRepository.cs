using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPApiModule.Interfaces;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPPro.SiteManager.TvinciPlatform.Social;

namespace RestfulTVPApi.ServiceInterface
{
    public class UsersRepository : IUsersRepository
    {
        public TVPApiModule.Objects.Responses.UserResponseObject[] GetUsersData(InitializationObject initObj, string siteGuids)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                return _service.GetUsersData(siteGuids);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.UserResponseObject SetUserData(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                return _service.SetUserData(siteGuid, userBasicData, userDynamicData);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj, string siteGuid)
        {
            TVPApiModule.Objects.Responses.PermittedSubscriptionContainer[] permitedSubscriptions = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermitedSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                var permitted = _service.GetUserPermitedSubscriptions(siteGuid);

                if (permitted != null)
                    permitedSubscriptions = permitted.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return permitedSubscriptions;
        }

        public TVPApiModule.Objects.Responses.PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(InitializationObject initObj, string siteGuid, int totalItems)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                TVPApiModule.Objects.Responses.PermittedSubscriptionContainer[] res = _service.GetUserExpiredSubscriptions(siteGuid, totalItems);

                if (res != null)
                    res = res.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();

                return res;
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.PermittedMediaContainer[] GetUserPermittedItems(InitializationObject initObj, string siteGuid)
        {
            TVPApiModule.Objects.Responses.PermittedMediaContainer[] res = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermittedItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                var permitted = _service.GetUserPermittedItems(siteGuid);

                if (permitted != null)
                    res = permitted.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        public TVPApiModule.Objects.Responses.PermittedMediaContainer[] GetUserExpiredItems(InitializationObject initObj, string siteGuid, int totalItems)
        {
            TVPApiModule.Objects.Responses.PermittedMediaContainer[] res = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                var expired = _service.GetUserExpiredItems(siteGuid, totalItems);

                if (expired != null)
                    res = expired.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject SignUp(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignUp", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                return _service.SignUp(userBasicData, userDynamicData, sPassword, sAffiliateCode);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        //Ofir - Should DomainID passed as param?
        public FavoriteObject[] GetUserFavorites(InitializationObject initObj, string siteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserFavorites", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                FavoriteObject[] favoritesObj = _service.GetUserFavorites(siteGuid, string.Empty, initObj.DomainID, string.Empty);

                if (favoritesObj !=null)
                    favoritesObj = favoritesObj.OrderByDescending(r => r.m_dUpdateDate.Date).ThenByDescending(r => r.m_dUpdateDate.TimeOfDay).ToArray();

                return favoritesObj;
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public GroupRule[] GetUserGroupRules(InitializationObject initObj, string siteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiApiService _service = new ApiApiService(groupID, initObj.Platform);

                return _service.GetUserGroupRules(siteGuid);
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
                ApiApiService _service = new ApiApiService(groupID, initObj.Platform);

                return _service.SetUserGroupRule(siteGuid, ruleID, PIN, isActive);
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
                ApiApiService _service = new ApiApiService(groupID, initObj.Platform);

                return _service.CheckParentalPIN(siteGuid, ruleID, PIN);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeUserPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                return _service.ChangeUserPassword(sUN, sOldPass, sPass);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.UserResponseObject RenewUserPassword(InitializationObject initObj, string sUN, string sPass)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RenewUserPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                return _service.RenewUserPassword(sUN, sPass);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.UserResponseObject ActivateAccount(InitializationObject initObj, string sUserName, string sToken)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ActivateAccount", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                return _service.ActivateAccount(sUserName, sToken);
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
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                return _service.ResendActivationMail(sUserName, sNewPassword);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.UserType[] GetGroupUserTypes(InitializationObject initObj)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupUserTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                return _service.GetGroupUserTypes();
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.ResponseStatus RenewUserPIN(InitializationObject initObj, string siteGuid, int ruleID)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RenewUserPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                return _service.RenewUserPIN(siteGuid, ruleID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.ResponseStatus SetUserTypeByUserID(InitializationObject initObj, string siteGuid, int nUserTypeID)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserTypeByUserID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                return _service.SetUserTypeByUserID(siteGuid, nUserTypeID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.UserResponseObject ActivateAccountByDomainMaster(InitializationObject initObj, string masterUserName, string userName, string token)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ActivateAccountByDomainMaster", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupId, initObj.Platform);

                return _service.ActivateAccountByDomainMaster(masterUserName, userName, token);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool AddItemToList(InitializationObject initObj, string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "AddItemToList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupId, initObj.Platform);

                return _service.AddItemToList(siteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public UserItemList[] GetItemFromList(InitializationObject initObj, string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetItemFromList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupId, initObj.Platform);

                return _service.GetItemFromList(siteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            };
        }

        public TVPPro.SiteManager.TvinciPlatform.Users.KeyValuePair[] IsItemExistsInList(InitializationObject initObj, string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsItemExistsInList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupId, initObj.Platform);

                return _service.IsItemExistsInList(siteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool RemoveItemFromList(InitializationObject initObj, string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "RemoveItemFromList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupId, initObj.Platform);

                return _service.RemoveItemFromList(siteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool UpdateItemInList(InitializationObject initObj, string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "UpdateItemInList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiUsersService _service = new ApiUsersService(groupId, initObj.Platform);

                return _service.UpdateItemInList(siteGuid, itemObjects, itemType, listType);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public string[] GetPrepaidBalance(InitializationObject initObj, string siteGuid, string currencyCode)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetPrepaidBalance", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupId, initObj.Platform);

                return _service.GetPrepaidBalance(siteGuid, currencyCode);
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

                lstMedia = (from media in lstAllMedias
                            where
                                (DateTime.Now.AddDays((double)byPeriod * periodBefore * -1) - (DateTime)media.LastWatchDate).TotalDays >= 0 &&
                                (DateTime.Now.AddDays((double)byPeriod * periodBefore * -1) - (DateTime)media.LastWatchDate).TotalDays <= (periodBefore + 1) * (int)byPeriod
                            select media).ToList<Media>();
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
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public TVPApiModule.Objects.Responses.BillingTransactionsResponse GetUserTransactionHistory(InitializationObject initObj, string siteGuid, int start_index, int pageSize)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserTransactionHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                return _service.GetUserTransactionHistory(siteGuid, start_index, pageSize);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.BillingResponse CC_ChargeUserForPrePaid(InitializationObject initObj, string siteGuid, double price, string currency, string productCode, string ppvModuleCode)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CC_ChargeUserForPrePaid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                return _service.CC_ChargeUserForPrePaid(siteGuid, price, currency, productCode, ppvModuleCode, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPApiModule.Objects.Responses.UserBillingTransactionsResponse[] GetUsersBillingHistory(InitializationObject initObj, string[] siteGuids, DateTime startDate, DateTime endDate)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUsersBillingHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                return _service.GetUsersBillingHistory(siteGuids, startDate, endDate);
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

        public TVPApiModule.Objects.Responses.AdyenBillingDetail GetLastBillingUserInfo(InitializationObject initObj, string siteGuid, int billingMethod)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetLastBillingUserInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiBillingService _service = new ApiBillingService(groupID, initObj.Platform);

                return _service.GetLastBillingUserInfo(siteGuid, billingMethod);
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
                ApiBillingService _service = new ApiBillingService(groupID, initObj.Platform);

                return _service.GetClientMerchantSig(sParamaters);
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
                ApiUsersService _service = new ApiUsersService(groupID, initObj.Platform);

                FavoriteObject[] favoriteObjects = _service.GetUserFavorites(siteGuid, string.Empty, initObj.DomainID, string.Empty);

                if (favoriteObjects != null)
                    result = mediaIds.Select(y => new KeyValuePair<int, bool>(y, favoriteObjects.Where(x => x.m_sItemCode == y.ToString()).Count() > 0)).ToList();
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
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
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
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupID, initObj.Platform);

                return _service.CancelSubscription(siteGuid, sSubscriptionID, sSubscriptionPurchaseID);
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
                ApiNotificationService _service = new ApiNotificationService(groupId, initObj.Platform);

                return _service.GetDeviceNotifications(siteGuid, initObj.UDID, notificationType == NotificationMessageType.All ? NotificationMessageType.Pull : notificationType, viewStatus, messageCount);
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
                ApiNotificationService _service = new ApiNotificationService(groupId, initObj.Platform);

                return _service.SetNotificationMessageViewStatus(siteGuid, notificationRequestID, notificationMessageID, viewStatus);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<TVPApi.TagMetaPairArray> GetUserStatusSubscriptions(InitializationObject initObj, string siteGuid)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserStatusSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiNotificationService _service = new ApiNotificationService(groupId, initObj.Platform);

                return _service.GetUserStatusSubscriptions(siteGuid);
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
                return new ApiApiService(groupID, initObj.Platform).CleanUserHistory(siteGuid, mediaIDs);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public string[] GetUserStartedWatchingMedias(InitializationObject initObj, string siteGuid, int numOfItems)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserStartedWatchingMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return new ApiApiService(groupID, initObj.Platform).GetUserStartedWatchingMedias(siteGuid, numOfItems);
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
                return new ApiUsersService(groupID, initObj.Platform).SentNewPasswordToUser(sUserName);
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
                bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                
                return new ApiUsersService(groupID, initObj.Platform).IsUserLoggedIn(siteGuid, initObj.UDID, string.Empty, SiteHelper.GetClientIP(), isSingleLogin);
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
                return new ApiUsersService(groupID, initObj.Platform).SetUserDynamicData(siteGuid, sKey, sValue);
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
                bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                
                new ApiUsersService(groupID, initObj.Platform).SignOut(siteGuid, initObj.UDID, string.Empty, isSingleLogin);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public FriendWatchedObject[] GetAllFriendsWatched(InitializationObject initObj, string siteGuid, int maxResult)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetAllFriendsWatched", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.GetAllFriendsWatched(int.Parse(siteGuid), maxResult);
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
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.DoUserAction(siteGuid, initObj.UDID, userAction, extraParams, socialPlatform, assetType, assetID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionObject[] GetFriendsActions(InitializationObject initObj, string siteGuid, string[] userActions, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFriendsActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.GetFriendsActions(siteGuid, userActions, assetType, assetID, startIndex, numOfRecords, socialPlatform);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionObject[] GetUserActions(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.GetUserActions(siteGuid, userAction, assetType, assetID, startIndex, numOfRecords, socialPlatform);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public eSocialPrivacy[] GetUserAllowedSocialPrivacyList(InitializationObject initObj, string siteGuid)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserAllowedSocialPrivacyList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.GetUserAllowedSocialPrivacyList(int.Parse(siteGuid));
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
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.GetUserExternalActionShare(siteGuid, userAction, socialPlatform);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public string[] GetUserFriends(InitializationObject initObj, string siteGuid)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserFriends", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.GetUserFriends(int.Parse(siteGuid));
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
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.GetUserInternalActionPrivacy(siteGuid, userAction, socialPlatform);
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
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.GetUserSocialPrivacy(int.Parse(siteGuid), socialPlatform, userAction);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SetUserExternalActionShare(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetUserExternalActionShare", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.SetUserExternalActionShare(siteGuid, userAction, socialPlatform, actionPrivacy);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool SetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetUserInternalActionPrivacy", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.SetUserInternalActionPrivacy(siteGuid, userAction, socialPlatform, actionPrivacy);
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
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupId, initObj.Platform);

                return _service.AD_GetCustomDataID(siteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, SiteHelper.GetClientIP(), countryCd2, languageCode3, deviceName, assetType);
            }
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
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupId, initObj.Platform);

                return _service.GetCustomDataID(siteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, SiteHelper.GetClientIP(), countryCd2, languageCode3, deviceName, assetType, overrideEndDate);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

    }
}