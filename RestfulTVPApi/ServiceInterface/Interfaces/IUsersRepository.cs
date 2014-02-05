using System;
using System.Collections.Generic;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPApiModule.Helper;
using TVPApiModule.Context;


namespace RestfulTVPApi.ServiceInterface
{
    public interface IUsersRepository
    {
        List<UserResponseObject> GetUsersData(InitializationObject initObj, string siteGuids);

        UserResponseObject SetUserData(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData);

        List<PermittedSubscriptionContainer> GetUserPermitedSubscriptions(InitializationObject initObj, string siteGuid);

        List<PermittedSubscriptionContainer> GetUserExpiredSubscriptions(InitializationObject initObj, string siteGuid, int iTotalItems);

        List<PermittedMediaContainer> GetUserPermittedItems(InitializationObject initObj, string siteGuid);

        List<PermittedMediaContainer> GetUserExpiredItems(InitializationObject initObj, string siteGuid, int iTotalItems);

        UserResponseObject SignUp(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode);

        List<FavoriteObject> GetUserFavorites(InitializationObject initObj, string siteGuid);

        List<GroupRule> GetUserGroupRules(InitializationObject initObj, string siteGuid);

        bool SetUserGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN, int isActive);

        bool CheckGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN);

        UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass);

        UserResponseObject RenewUserPassword(InitializationObject initObj, string sUN, string sPass);

        UserResponseObject ActivateAccount(InitializationObject initObj, string sUserName, string sToken);

        bool ResendActivationMail(InitializationObject initObj, string sUserName, string sNewPassword);

        eResponseStatus RenewUserPIN(InitializationObject initObj, string sSiteGUID, int ruleID);

        eResponseStatus SetUserTypeByUserID(InitializationObject initObj, string sSiteGUID, int nUserTypeID);

        UserResponseObject ActivateAccountByDomainMaster(InitializationObject initObj, string masterUserName, string userName, string token);

        bool AddItemToList(InitializationObject initObj, string sSiteGUID, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType);

        List<UserItemList> GetItemFromList(InitializationObject initObj, string sSiteGUID, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType);

        List<KeyValuePair> IsItemExistsInList(InitializationObject initObj, string sSiteGUID, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType);

        bool RemoveItemFromList(InitializationObject initObj, string sSiteGUID, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType);

        bool UpdateItemInList(InitializationObject initObj, string sSiteGUID, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType);

        List<string> GetPrepaidBalance(InitializationObject initObj, string sSiteGUID, string currencyCode);

        List<Media> GetLastWatchedMediasByPeriod(InitializationObject initObj, string sSiteGUID, string picSize, int periodBefore, MediaHelper.ePeriod byPeriod);

        List<Media> GetUserSocialMedias(InitializationObject initObj, string sSiteGUID, int socialPlatform, int socialAction, string picSize, int pageSize, int pageIndex);

        BillingTransactionsResponse GetUserTransactionHistory(InitializationObject initObj, string sSiteGUID, int start_index, int pageSize);

        BillingResponse CC_ChargeUserForPrePaid(InitializationObject initObj, string sSiteGUID, double price, string currency, string productCode, string ppvModuleCode);

        List<UserBillingTransactionsResponse> GetUsersBillingHistory(InitializationObject initObj, string[] siteGuids, DateTime startDate, DateTime endDate);

        List<Media> GetUserItems(InitializationObject initObj, string sSiteGUID, UserItemType itemType, string picSize, int pageSize, int start_index);

        AdyenBillingDetail GetLastBillingUserInfo(InitializationObject initObj, string siteGuid, int billingMethod);

        string GetClientMerchantSig(InitializationObject initObj, string sParamaters);

        List<KeyValuePair<int, bool>> AreMediasFavorite(InitializationObject initObj, string sSiteGUID, List<int> mediaIds);

        List<Media> GetRecommendedMediasByTypes(InitializationObject initObj, string sSiteGUID, string picSize, int pageSize, int pageIndex, int[] reqMediaTypes);

        bool CancelSubscription(InitializationObject initObj, string sSiteGUID, string sSubscriptionID, int sSubscriptionPurchaseID);

        List<Notification> GetDeviceNotifications(InitializationObject initObj, string sSiteGUID, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, Nullable<int> messageCount);

        bool SetNotificationMessageViewStatus(InitializationObject initObj, string sSiteGUID, Nullable<long> notificationRequestID, Nullable<long> notificationMessageID, NotificationMessageViewStatus viewStatus);

        List<TagMetaPairArray> GetUserStatusSubscriptions(InitializationObject initObj, string sSiteGUID);

        bool CleanUserHistory(InitializationObject initObj, string siteGuid, int[] mediaIDs);

        List<string> GetUserStartedWatchingMedias(InitializationObject initObj, string siteGuid, int numOfItems);

        bool SendNewPassword(InitializationObject initObj, string sUserName);

        bool IsUserSignedIn(InitializationObject initObj, string siteGuid);

        bool SetUserDynamicData(InitializationObject initObj, string siteGuid, string sKey, string sValue);

        TVPApiModule.Services.ApiUsersService.LogInResponseData SignIn(InitializationObject initObj, string userName, string password);

        void SignOut(InitializationObject initObj, string siteGuid);

        List<FriendWatchedObject> GetAllFriendsWatched(InitializationObject initObj, string siteGuid, int maxResult);

        SocialActionResponseStatus DoUserAction(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair[] extraParams, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID);

        List<UserSocialActionObject> GetFriendsActions(InitializationObject initObj, string siteGuid, string[] userActions, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        List<UserSocialActionObject> GetUserActions(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.eAssetType assetType, int assetID, int startIndex, int numOfRecords, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        List<eSocialPrivacy> GetUserAllowedSocialPrivacyList(InitializationObject initObj, string siteGuid);

        eSocialActionPrivacy GetUserExternalActionShare(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        eSocialActionPrivacy GetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform);

        List<string> GetUserFriends(InitializationObject initObj, string siteGuid);

        eSocialPrivacy GetUserSocialPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction);

        bool SetUserExternalActionShare(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy);

        bool SetUserInternalActionPrivacy(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction userAction, TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.Social.eSocialActionPrivacy actionPrivacy);

        int AD_GetCustomDataID(InitializationObject initObj, string siteGuid, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string countryCd2, string languageCode3, string deviceName, int assetType);

        int GetCustomDataID(InitializationObject initObj, string siteGuid, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string countryCd2, string languageCode3, string deviceName, int assetType, string overrideEndDate);

        BillingResponse InApp_ChargeUserForMediaFile(InitializationObject initObj, string sSiteGUID, double price, string currency, string productCode, string ppvModuleCode, string receipt);
    }
}
