using System;
using System.Collections.Generic;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.TvinciPlatform.Billing;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPPro.SiteManager.TvinciPlatform.Social;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IUsersRepository
    {
        TVPApiModule.Objects.Responses.UserResponseObject[] GetUsersData(InitializationObject initObj, string siteGuids);

        TVPApiModule.Objects.Responses.UserResponseObject SetUserData(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData);

        TVPApiModule.Objects.Responses.PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj, string siteGuid);

        TVPApiModule.Objects.Responses.PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(InitializationObject initObj, string siteGuid, int iTotalItems);

        TVPApiModule.Objects.Responses.PermittedMediaContainer[] GetUserPermittedItems(InitializationObject initObj, string siteGuid);

        TVPApiModule.Objects.Responses.PermittedMediaContainer[] GetUserExpiredItems(InitializationObject initObj, string siteGuid, int iTotalItems);

        TVPApiModule.Objects.Responses.UserResponseObject SignUp(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode);

        TVPApiModule.Objects.Responses.FavoriteObject[] GetUserFavorites(InitializationObject initObj, string siteGuid);

        GroupRule[] GetUserGroupRules(InitializationObject initObj, string siteGuid);

        bool SetUserGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN, int isActive);

        bool CheckGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN);

        TVPApiModule.Objects.Responses.UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass);

        TVPApiModule.Objects.Responses.UserResponseObject RenewUserPassword(InitializationObject initObj, string sUN, string sPass);

        TVPApiModule.Objects.Responses.UserResponseObject ActivateAccount(InitializationObject initObj, string sUserName, string sToken);

        bool ResendActivationMail(InitializationObject initObj, string sUserName, string sNewPassword);

        TVPApiModule.Objects.Responses.UserType[] GetGroupUserTypes(InitializationObject initObj);

        TVPApiModule.Objects.Responses.ResponseStatus RenewUserPIN(InitializationObject initObj, string sSiteGUID, int ruleID);

        TVPApiModule.Objects.Responses.ResponseStatus SetUserTypeByUserID(InitializationObject initObj, string sSiteGUID, int nUserTypeID);

        TVPApiModule.Objects.Responses.UserResponseObject ActivateAccountByDomainMaster(InitializationObject initObj, string masterUserName, string userName, string token);

        bool AddItemToList(InitializationObject initObj, string sSiteGUID, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        UserItemList[] GetItemFromList(InitializationObject initObj, string sSiteGUID, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        TVPPro.SiteManager.TvinciPlatform.Users.KeyValuePair[] IsItemExistsInList(InitializationObject initObj, string sSiteGUID, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        bool RemoveItemFromList(InitializationObject initObj, string sSiteGUID, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        bool UpdateItemInList(InitializationObject initObj, string sSiteGUID, ItemObj[] itemObjects, ItemType itemType, ListType listType);

        string[] GetPrepaidBalance(InitializationObject initObj, string sSiteGUID, string currencyCode);

        List<Media> GetLastWatchedMediasByPeriod(InitializationObject initObj, string sSiteGUID, string picSize, int periodBefore, MediaHelper.ePeriod byPeriod);

        List<Media> GetUserSocialMedias(InitializationObject initObj, string sSiteGUID, int socialPlatform, int socialAction, string picSize, int pageSize, int pageIndex);

        TVPApiModule.Objects.Responses.BillingTransactionsResponse GetUserTransactionHistory(InitializationObject initObj, string sSiteGUID, int start_index, int pageSize);

        TVPApiModule.Objects.Responses.BillingResponse CC_ChargeUserForPrePaid(InitializationObject initObj, string sSiteGUID, double price, string currency, string productCode, string ppvModuleCode);

        TVPApiModule.Objects.Responses.UserBillingTransactionsResponse[] GetUsersBillingHistory(InitializationObject initObj, string[] siteGuids, DateTime startDate, DateTime endDate);

        List<Media> GetUserItems(InitializationObject initObj, string sSiteGUID, UserItemType itemType, string picSize, int pageSize, int start_index);

        TVPApiModule.Objects.Responses.AdyenBillingDetail GetLastBillingUserInfo(InitializationObject initObj, string siteGuid, int billingMethod);

        string GetClientMerchantSig(InitializationObject initObj, string sParamaters);

        List<KeyValuePair<int, bool>> AreMediasFavorite(InitializationObject initObj, string sSiteGUID, List<int> mediaIds);

        List<Media> GetRecommendedMediasByTypes(InitializationObject initObj, string sSiteGUID, string picSize, int pageSize, int pageIndex, int[] reqMediaTypes);

        bool CancelSubscription(InitializationObject initObj, string sSiteGUID, string sSubscriptionID, int sSubscriptionPurchaseID);

        List<Notification> GetDeviceNotifications(InitializationObject initObj, string sSiteGUID, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, Nullable<int> messageCount);

        bool SetNotificationMessageViewStatus(InitializationObject initObj, string sSiteGUID, Nullable<long> notificationRequestID, Nullable<long> notificationMessageID, NotificationMessageViewStatus viewStatus);

        List<TVPApi.TagMetaPairArray> GetUserStatusSubscriptions(InitializationObject initObj, string sSiteGUID);

        bool CleanUserHistory(InitializationObject initObj, string siteGuid, int[] mediaIDs);

        string[] GetUserStartedWatchingMedias(InitializationObject initObj, string siteGuid, int numOfItems);

        bool SendNewPassword(InitializationObject initObj, string sUserName);

        bool IsUserSignedIn(InitializationObject initObj, string siteGuid);

        bool SetUserDynamicData(InitializationObject initObj, string siteGuid, string sKey, string sValue);

        TVPApiModule.Services.ApiUsersService.LogInResponseData SignIn(InitializationObject initObj, string userName, string password);

        void SignOut(InitializationObject initObj, string siteGuid);

        FriendWatchedObject[] GetAllFriendsWatched(InitializationObject initObj, string siteGuid, int maxResult);

        SocialActionResponseStatus DoUserAction(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID);

        TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionObject[] GetFriendsActions(InitializationObject initObj, string siteGuid, string[] userActions, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionObject[] GetUserActions(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        eSocialPrivacy[] GetUserAllowedSocialPrivacyList(InitializationObject initObj, string siteGuid);

        eSocialActionPrivacy GetUserExternalActionShare(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        eSocialActionPrivacy GetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        string[] GetUserFriends(InitializationObject initObj, string siteGuid);

        eSocialPrivacy GetUserSocialPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction);

        bool SetUserExternalActionShare(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy);

        bool SetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, eSocialActionPrivacy actionPrivacy);

        int AD_GetCustomDataID(InitializationObject initObj, string siteGuid, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string countryCd2, string languageCode3, string deviceName, int assetType);

        int GetCustomDataID(InitializationObject initObj, string siteGuid, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string countryCd2, string languageCode3, string deviceName, int assetType, string overrideEndDate);
    }
}
