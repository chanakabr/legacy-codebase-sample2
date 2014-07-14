using System;
using System.Collections.Generic;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPApiModule.Helper;
using TVPApiModule.Context;
using RestfulTVPApi.ServiceModel;


namespace RestfulTVPApi.ServiceInterface
{
    public interface IUsersRepository
    {
        List<UserResponseObject> GetUsersData(GetUsersDataRequest request);

        UserResponseObject SetUserData(SetUserDataRequest request);

        List<PermittedSubscriptionContainer> GetUserPermitedSubscriptions(GetUserPermitedSubscriptionsRequest request);

        List<PermittedSubscriptionContainer> GetUserExpiredSubscriptions(GetUserExpiredSubscriptionsRequest request);

        List<PermittedMediaContainer> GetUserPermittedItems(GetUserPermittedItemsRequest request);

        List<PermittedMediaContainer> GetUserExpiredItems(GetUserExpiredItemsRequest request);

        UserResponseObject SignUp(SignUpRequest request);

        List<FavoriteObject> GetUserFavorites(GetUserFavoritesRequest request);

        List<GroupRule> GetUserGroupRules(GetUserGroupRulesRequest request);

        bool SetUserGroupRule(SetUserGroupRuleRequest request);

        bool CheckGroupRule(CheckGroupRuleRequest request);

        UserResponseObject ChangeUserPassword(ChangeUserPasswordRequest request);

        UserResponseObject RenewUserPassword(RenewUserPasswordRequest request);

        UserResponseObject ActivateAccount(ActivateAccountRequest request);

        bool ResendActivationMail(ResendActivationMailRequest request);

        eResponseStatus RenewUserPIN(RenewUserPINRequest request);

        //eResponseStatus SetUserTypeByUserID(InitializationObject initObj, string sSiteGUID, int nUserTypeID);

        UserResponseObject ActivateAccountByDomainMaster(ActivateAccountByDomainMasterRequest request);

        bool AddItemToList(AddItemToListRequest request);

        List<UserItemList> GetItemFromList(GetItemFromListRequest request);

        List<KeyValuePair> IsItemExistsInList(IsItemExistsInListRequest request);

        bool RemoveItemFromList(RemoveItemFromListRequest request);

        bool UpdateItemInList(UpdateItemInListRequest request);

        List<string> GetPrepaidBalance(GetPrepaidBalanceRequest request);

        List<Media> GetLastWatchedMediasByPeriod(GetLastWatchedMediasByPeriodRequest request);

        List<Media> GetUserSocialMedias(GetUserSocialMediasRequest request);

        BillingTransactionsResponse GetUserTransactionHistory(GetUserTransactionHistoryRequest request);

        BillingResponse CC_ChargeUserForPrePaid(CC_ChargeUserForPrePaidRequest request);

        List<UserBillingTransactionsResponse> GetUsersBillingHistory(GetUsersBillingHistoryRequest request);

        List<Media> GetUserItems(GetUserItemsRequest request);

        AdyenBillingDetail GetLastBillingUserInfo(GetLastBillingUserInfoRequest request);

        string GetClientMerchantSig(GetClientMerchantSigRequest request);

        List<KeyValuePair<int, bool>> AreMediasFavorite(AreMediasFavoriteRequest request);

        List<Media> GetRecommendedMediasByTypes(GetRecommendedMediasByTypesRequest request);

        bool CancelSubscription(CancelSubscriptionRequest request);

        List<Notification> GetDeviceNotifications(GetDeviceNotificationsRequest request);

        bool SetNotificationMessageViewStatus(SetNotificationMessageViewStatusRequest request);

        List<TagMetaPairArray> GetUserStatusSubscriptions(GetUserStatusSubscriptionsRequest request);

        bool CleanUserHistory(ClearUserHistoryRequest request);

        List<string> GetUserStartedWatchingMedias(GetUserStartedWatchingMediasRequest request);

        bool SendNewPassword(SendNewPasswordRequest request);

        bool IsUserSignedIn(IsUserSignedInRequest request);

        bool SetUserDynamicData(SetUserDynamicDataRequest request);

        TVPApiModule.Services.ApiUsersService.LogInResponseData SignIn(SignInRequest request);

        void SignOut(SignOutRequest request);

        List<FriendWatchedObject> GetAllFriendsWatched(GetAllFriendsWatchedRequest request);

        SocialActionResponseStatus DoUserAction(DoUserActionRequest request);

        List<UserSocialActionObject> GetFriendsActions(GetFriendsActionsRequest request);

        List<UserSocialActionObject> GetUserActions(GetUserActionsRequest request);

        List<eSocialPrivacy> GetUserAllowedSocialPrivacyList(GetUserAllowedSocialPrivacyListRequest request);

        eSocialActionPrivacy GetUserExternalActionShare(GetUserExternalActionShareRequest request);

        eSocialActionPrivacy GetUserInternalActionPrivacy(GetUserInternalActionPrivacyRequest request);

        List<string> GetUserFriends(GetUserFriendsRequest request);

        eSocialPrivacy GetUserSocialPrivacy(GetUserSocialPrivacyRequest request);

        bool SetUserExternalActionShare(SetUserExternalActionShareRequest request);

        bool SetUserInternalActionPrivacy(SetUserInternalActionPrivacyRequest request);

        int AD_GetCustomDataID(AD_GetCustomDataIDRequest request);

        int GetCustomDataID(GetCustomDataIDRequest request);

        BillingResponse InApp_ChargeUserForMediaFile(InApp_ChargeUserForMediaFileRequest request);
    }
}
