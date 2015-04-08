using System;
using System.Collections.Generic;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPApiModule.Helper;
using TVPApiModule.Context;
using RestfulTVPApi.ServiceModel;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Objects.Responses.Enums;
using RestfulTVPApi.Clients;


namespace RestfulTVPApi.ServiceInterface
{
    public interface IUsersRepository
    {
        List<UserResponseObject> GetUsersData(GetUsersDataRequest request);

        UserResponseObject SetUserData(SetUserDataRequest request);

        List<PermittedSubscriptionContainer> GetUserPermitedSubscriptions(GetUserPermitedSubscriptionsRequest request);

        List<PermittedSubscriptionContainer> GetUserExpiredSubscriptions(GetUserExpiredSubscriptionsRequest request);

        List<PermittedCollectionContainer> GetUserExpiredCollections(GetUserExpiredCollectionsRequest request);

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

        List<RestfulTVPApi.Objects.Responses.KeyValuePair> IsItemExistsInList(IsItemExistsInListRequest request);

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

        Status CancelSubscription(CancelSubscriptionRequest request);

        List<RestfulTVPApi.Objects.Responses.Notification> GetDeviceNotifications(GetDeviceNotificationsRequest request);

        bool SetNotificationMessageViewStatus(SetNotificationMessageViewStatusRequest request);

        List<RestfulTVPApi.Objects.Responses.TagMetaPairArray> GetUserStatusSubscriptions(GetUserStatusSubscriptionsRequest request);

        bool CleanUserHistory(ClearUserHistoryRequest request);

        List<string> GetUserStartedWatchingMedias(GetUserStartedWatchingMediasRequest request);

        Status SendNewPassword(SendNewPasswordRequest request);

        bool IsUserSignedIn(IsUserSignedInRequest request);

        bool SetUserDynamicData(SetUserDynamicDataRequest request);

        UsersClient.LogInResponseData SignIn(SignInRequest request);

        FBSignIn FBUserSignin(FBUserSigninRequest request);

        void SignOut(SignOutRequest request);

        List<FriendWatchedObject> GetAllFriendsWatched(GetAllFriendsWatchedRequest request);

        DoSocialActionResponse DoUserAction(DoUserActionRequest request);

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

        AdyenBillingDetail GetLastBillingTypeUserInfo(GetLastBillingTypeUserInfoRequest request);

        List<PermittedCollectionContainer> GetUserPermittedCollections(GetUserPermittedCollectionsRequest request);

        ChangeSubscriptionStatus ChangeSubscription(ChangeSubscriptionRequest request);

        int CreatePurchaseToken(CreatePurchaseTokenRequest request);

        string DummyChargeUserForCollection(DummyChargeUserForCollectionRequest request);

        BillingResponse DummyChargeUserForSubscription(DummyChargeUserForSubscriptionRequest request);

        BillingResponse ChargeUserForCollection(ChargeUserForCollectionRequest request);

        BillingResponse CellularChargeUserForSubscription(CellularChargeUserForSubscriptionRequest request);

        string ChargeUserForSubscriptionByPaymentMethod(ChargeUserForSubscriptionByPaymentMethodRequest request);

        string ChargeUserForMediaFileByPaymentMethod(ChargeUserForMediaFileByPaymentMethodRequest request);

        string CellularChargeUserForMediaFile(CellularChargeUserForMediaFileRequest request);

        string ChargeUserForMediaFileUsingCC(ChargeUserForMediaFileUsingCCRequest request);

        string ChargeUserForMediaSubscriptionUsingCC(ChargeUserForMediaSubscriptionUsingCCRequest request);

        UsersClient.LogInResponseData SignInWithToken(SignInWithTokenRequest request);

        bool CancelTransaction(CancelTransactionRequest request);

        bool WaiverTransaction(WaiverTransactionRequest request);

        UserResponseObject CheckTemporaryToken(CheckTemporaryTokenRequest request);

        //Status CancelSubscriptionRenewal(CancelSubscriptionRenewalRequest request);
    }
}
