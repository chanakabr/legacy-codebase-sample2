using System.Net;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using RestfulTVPApi.ServiceModel;
using System;
using System.Linq;
using ServiceStack.ServiceHost;
using System.Collections.Generic;

namespace RestfulTVPApi.ServiceInterface
{
    [RequiresInitializationObject]
    public class UsersService : Service
    {
        public IUsersRepository _repository { get; set; }  //Injected by IOC

        #region GET        
        
        public object Get(GetUserPermitedSubscriptionsRequest request)
        {
            return _repository.GetUserPermitedSubscriptions(request);
        }

        public object Get(GetUserExpiredSubscriptionsRequest request)
        {
            return _repository.GetUserExpiredSubscriptions(request);
        }

        public object Get(GetUserPermittedItemsRequest request)
        {
            return _repository.GetUserPermittedItems(request);
        }

        public object Get(GetUserExpiredItemsRequest request)
        {
            return _repository.GetUserExpiredItems(request);
        }

        public object Get(GetUserFavoritesRequest request)
        {
            return _repository.GetUserFavorites(request);
        }

        public object Get(GetUserGroupRulesRequest request)
        {
            return _repository.GetUserGroupRules(request);
        }

        public object Get(CheckGroupRuleRequest request)
        {
            return _repository.CheckGroupRule(request);
        }

        public object Get(RenewUserPINRequest request)
        {
            return _repository.RenewUserPIN(request);
        }

        public object Get(GetItemFromListRequest request)
        {
            return _repository.GetItemFromList(request);
        }

        public object Get(IsItemExistsInListRequest request)
        {
            return _repository.IsItemExistsInList(request);
        }

        public object Get(GetPrepaidBalanceRequest request)
        {
            return _repository.GetPrepaidBalance(request);
        }

        public object Get(ResendActivationMailRequest request)
        {
            return _repository.ResendActivationMail(request);
        }

        public object Get(GetLastWatchedMediasByPeriodRequest request)
        {
            return _repository.GetLastWatchedMediasByPeriod(request);
        }

        public object Get(GetUserSocialMediasRequest request)
        {
            return _repository.GetUserSocialMedias(request);
        }

        public object Get(GetUserTransactionHistoryRequest request)
        {
            return _repository.GetUserTransactionHistory(request);
        }

        public object Get(GetUsersBillingHistoryRequest request)
        {
            return _repository.GetUsersBillingHistory(request);
        }

        public object Get(GetUserItemsRequest request)
        {
            return _repository.GetUserItems(request);
        }

        public object Get(GetLastBillingUserInfoRequest request)
        {
            return _repository.GetLastBillingUserInfo(request);
        }

        public object Get(GetClientMerchantSigRequest request)
        {
            return _repository.GetClientMerchantSig(request);
        }

        public object Get(AreMediasFavoriteRequest request)
        {
            return _repository.AreMediasFavorite(request);
        }

        public object Get(GetRecommendedMediasByTypesRequest request)
        {
            return _repository.GetRecommendedMediasByTypes(request);
        }

        public object Get(GetDeviceNotificationsRequest request)
        {
            return _repository.GetDeviceNotifications(request);
        }

        public object Get(GetUserStatusSubscriptionsRequest request)
        {
            return _repository.GetUserStatusSubscriptions(request);
        }

        public object Get(GetUserStartedWatchingMediasRequest request)
        {
            return _repository.GetUserStartedWatchingMedias(request);
        }

        public object Get(IsUserSignedInRequest request)
        {
            return _repository.IsUserSignedIn(request);
        }

        public object Get(GetAllFriendsWatchedRequest request)
        {
            return _repository.GetAllFriendsWatched(request);
        }

        public object Get(GetFriendsActionsRequest request)
        {
            return _repository.GetFriendsActions(request);
        }

        public object Get(GetUserAllowedSocialPrivacyListRequest request)
        {
            return _repository.GetUserAllowedSocialPrivacyList(request);
        }

        public object Get(GetUserExternalActionShareRequest request)
        {
            return _repository.GetUserExternalActionShare(request);
        }

        public object Get(GetUserInternalActionPrivacyRequest request)
        {
            return _repository.GetUserInternalActionPrivacy(request);
        }

        public object Get(GetUserFriendsRequest request)
        {
            return _repository.GetUserFriends(request);
        }

        public object Get(AD_GetCustomDataIDRequest request)
        {
            return _repository.AD_GetCustomDataID(request);
        }

        public object Get(GetCustomDataIDRequest request)
        {
            return _repository.GetCustomDataID(request);
        }        

        public object Get(GetLastBillingTypeUserInfoRequest request)
        {
            return _repository.GetLastBillingTypeUserInfo(request);
        }

        public object Get(GetUserPermittedCollectionsRequest request)
        {
            return _repository.GetUserPermittedCollections(request);
        }

        public object Get(GetUserExpiredCollectionsRequest request)
        {
            return _repository.GetUserExpiredCollections(request);
        }

        public object Get(CheckTemporaryTokenRequest request)
        {
            return _repository.CheckTemporaryToken(request);
        }

        #endregion

        #region PUT

        public object Put(SetUserDataRequest request)
        {
            return _repository.SetUserData(request);
        }

        public object Put(SetUserDynamicDataRequest request)
        {
            return _repository.SetUserDynamicData(request);
        }

        public object Put(SetUserGroupRuleRequest request)
        {
            return _repository.SetUserGroupRule(request);
        }

        public object Put(ChangeUserPasswordRequest request)
        {
            return _repository.ChangeUserPassword(request);
        }

        public object Post(RenewUserPasswordRequest request)
        {
            return _repository.RenewUserPassword(request);
        }

        public object Put(ActivateAccountRequest request)
        {
            return _repository.ActivateAccount(request);
        }

        public object Put(ActivateAccountByDomainMasterRequest request)
        {
            return _repository.ActivateAccountByDomainMaster(request);
        }

        public object Put(UpdateItemInListRequest request)
        {
            return _repository.UpdateItemInList(request);
        }

        public object Put(SetNotificationMessageViewStatusRequest request)
        {
            return _repository.SetNotificationMessageViewStatus(request);
        }

        public object Put(CC_ChargeUserForPrePaidRequest request)
        {
            return _repository.CC_ChargeUserForPrePaid(request);
        }

        public object Put(SetUserExternalActionShareRequest request)
        {
            return _repository.SetUserExternalActionShare(request);
        }

        public object Put(SetUserInternalActionPrivacyRequest request)
        {
            return _repository.SetUserInternalActionPrivacy(request);
        }

        public object Put(ChangeSubscriptionRequest request)
        {
            return _repository.ChangeSubscription(request);
        }

        public object Put(CancelTransactionRequest request)
        {
            return _repository.CancelTransaction(request);
        }

        public object Put(WaiverTransactionRequest request)
        {
            return _repository.WaiverTransaction(request);
        }
        
        #endregion

        #region POST

        public object Post(GetUsersDataRequest request)
        {
            return _repository.GetUsersData(request);
        }

        public object Post(FBUserSigninRequest request)
        {
            return _repository.FBUserSignin(request);
        }

        public object Post(SignUpRequest request)
        {
            return _repository.SignUp(request);
        }

        public object Post(AddItemToListRequest request)
        {
            return _repository.AddItemToList(request);
        }

        public object Post(SignInRequest request)
        {
            return _repository.SignIn(request);
        }

        public object Post(DoUserActionRequest request)
        {
            return _repository.DoUserAction(request);
        }

        public object Post(InApp_ChargeUserForMediaFileRequest request)
        {
            return _repository.InApp_ChargeUserForMediaFile(request);
        }
        
        public object Post(SendNewPasswordRequest request)
        {
            return _repository.SendNewPassword(request);
        }

        public object Post(CreatePurchaseTokenRequest request)
        {
            return _repository.CreatePurchaseToken(request);
        }

        public object Post(DummyChargeUserForCollectionRequest request)
        {
            return _repository.DummyChargeUserForCollection(request);
        }

        public object Post(DummyChargeUserForSubscriptionRequest request)
        {
            return _repository.DummyChargeUserForSubscription(request);
        }

        public object Post(ChargeUserForCollectionRequest request)
        {
            return _repository.ChargeUserForCollection(request);
        }

        public object Post(CellularChargeUserForSubscriptionRequest request)
        {
            return _repository.CellularChargeUserForSubscription(request);
        }

        public object Post(ChargeUserForSubscriptionByPaymentMethodRequest request)
        {
            return _repository.ChargeUserForSubscriptionByPaymentMethod(request);
        }

        public object Post(ChargeUserForMediaFileByPaymentMethodRequest request)
        {
            return _repository.ChargeUserForMediaFileByPaymentMethod(request);
        }

        public object Post(CellularChargeUserForMediaFileRequest request)
        {
            return _repository.CellularChargeUserForMediaFile(request);
        }

        public object Post(ChargeUserForMediaFileUsingCCRequest request)
        {
            return _repository.ChargeUserForMediaFileUsingCC(request);
        }

        public object Post(ChargeUserForMediaSubscriptionUsingCCRequest request)
        {
            return _repository.ChargeUserForMediaSubscriptionUsingCC(request);
        }

        public object Post(SignInWithTokenRequest request)
        {
            return _repository.SignInWithToken(request);
        }

        #endregion

        #region DELETE

        public object Delete(RemoveItemFromListRequest request)
        {
            return _repository.RemoveItemFromList(request);
        }

        public object Delete(ClearUserHistoryRequest request)
        {
            return _repository.CleanUserHistory(request);
        }

        public object Delete(CancelSubscriptionRequest request)
        {
            return _repository.CancelSubscription(request);
        }

        /*public object Delete(CancelSubscriptionRenewalRequest request)
        {
            return _repository.CancelSubscriptionRenewal(request);
        }*/

        public void Delete(SignOutRequest request)
        {
            _repository.SignOut(request);
        }

        #endregion

    }
}
