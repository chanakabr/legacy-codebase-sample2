using System.Net;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using RestfulTVPApi.ServiceModel;
using System;
using System.Linq;
using ServiceStack.ServiceHost;
using System.Collections.Generic;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class UsersService : Service
    {
        public IUsersRepository _repository { get; set; }  //Injected by IOC

        #region GET

        public object Get(GetUsersDataRequest request)
        {
            return _repository.GetUsersData(request.InitObj, request.site_guids);
        }

        public object Get(GetUserPermitedSubscriptionsRequest request)
        {
            return _repository.GetUserPermitedSubscriptions(request.InitObj, request.site_guid);
        }

        public object Get(GetUserExpiredSubscriptionsRequest request)
        {
            return _repository.GetUserExpiredSubscriptions(request.InitObj, request.site_guid, request.page_size);
        }

        public object Get(GetUserPermittedItemsRequest request)
        {
            return _repository.GetUserPermittedItems(request.InitObj, request.site_guid);
        }

        public object Get(GetUserExpiredItemsRequest request)
        {
            return _repository.GetUserExpiredItems(request.InitObj, request.site_guid, request.page_size);
        }

        public object Get(GetUserFavoritesRequest request)
        {
            return _repository.GetUserFavorites(request.InitObj, request.site_guid);
        }

        public object Get(GetUserGroupRulesRequest request)
        {
            return _repository.GetUserGroupRules(request.InitObj, request.site_guid);
        }

        public object Get(CheckGroupRuleRequest request)
        {
            return _repository.CheckGroupRule(request.InitObj, request.site_guid, request.rule_id, request.pin);
        }

        public object Get(RenewUserPINRequest request)
        {
            return _repository.RenewUserPIN(request.InitObj, request.site_guid, request.rule_id);
        }

        public object Get(GetItemFromListRequest request)
        {
            return _repository.GetItemFromList(request.InitObj, request.site_guid, request.item_objects, request.item_type, request.list_type);
        }

        public object Get(IsItemExistsInListRequest request)
        {
            return _repository.IsItemExistsInList(request.InitObj, request.site_guid, request.item_objects, request.item_type, request.list_type);
        }

        public object Get(GetPrepaidBalanceRequest request)
        {
            return _repository.GetPrepaidBalance(request.InitObj, request.site_guid, request.currency_code);
        }

        public object Get(ResendActivationMailRequest request)
        {
            return _repository.ResendActivationMail(request.InitObj, request.user_name, request.password);
        }

        public object Get(GetLastWatchedMediasByPeriodRequest request)
        {
            return _repository.GetLastWatchedMediasByPeriod(request.InitObj, request.site_guid, request.pic_size, request.period_before, request.by_period);
        }

        public object Get(GetUserSocialMediasRequest request)
        {
            return _repository.GetUserSocialMedias(request.InitObj, request.site_guid, request.social_platform, request.social_action, request.pic_size, request.page_size, request.page_number);
        }

        public object Get(GetUserTransactionHistoryRequest request)
        {
            return _repository.GetUserTransactionHistory(request.InitObj, request.site_guid, request.page_size, request.page_number);
        }

        public object Get(GetUsersBillingHistoryRequest request)
        {
            return _repository.GetUsersBillingHistory(request.InitObj, request.site_guids, request.start_date, request.end_date);
        }

        public object Get(GetUserItemsRequest request)
        {
            return _repository.GetUserItems(request.InitObj, request.site_guid, request.item_type, request.pic_size, request.page_size, request.page_number);
        }

        public object Get(GetLastBillingUserInfoRequest request)
        {
            return _repository.GetLastBillingUserInfo(request.InitObj, request.site_guid, request.billing_method);
        }

        public object Get(GetClientMerchantSigRequest request)
        {
            return _repository.GetClientMerchantSig(request.InitObj, request.paramaters);
        }

        public object Get(AreMediasFavoriteRequest request)
        {
            return _repository.AreMediasFavorite(request.InitObj, request.site_guid, request.media_ids);
        }

        public object Get(GetRecommendedMediasByTypesRequest request)
        {
            return _repository.GetRecommendedMediasByTypes(request.InitObj, request.site_guid, request.pic_size, request.page_size, request.page_number, request.media_types);
        }

        public object Get(GetDeviceNotificationsRequest request)
        {
            int? message_count = request.page_size > 0 ? new Nullable<int>(request.page_size) : null;

            return _repository.GetDeviceNotifications(request.InitObj, request.site_guid, request.notification_type, request.view_status, message_count);
        }

        public object Get(GetUserStatusSubscriptionsRequest request)
        {
            return _repository.GetUserStatusSubscriptions(request.InitObj, request.site_guid);
        }

        public object Get(GetUserStartedWatchingMediasRequest request)
        {
            return _repository.GetUserStartedWatchingMedias(request.InitObj, request.site_guid, request.page_size);
        }

        public object Get(IsUserSignedInRequest request)
        {
            return _repository.IsUserSignedIn(request.InitObj, request.site_guid);
        }

        public object Get(GetAllFriendsWatchedRequest request)
        {
            return _repository.GetAllFriendsWatched(request.InitObj, request.site_guid, request.page_size);
        }

        public object Get(GetFriendsActionsRequest request)
        {
            return _repository.GetFriendsActions(request.InitObj, request.site_guid, request.user_actions, request.asset_type, request.asset_id, request.page_number, request.page_size, request.social_platform);
        }

        public object Get(GetUserAllowedSocialPrivacyListRequest request)
        {
            return _repository.GetUserAllowedSocialPrivacyList(request.InitObj, request.site_guid);
        }

        public object Get(GetUserExternalActionShareRequest request)
        {
            return _repository.GetUserExternalActionShare(request.InitObj, request.site_guid, request.user_action, request.social_platform);
        }

        public object Get(GetUserInternalActionPrivacyRequest request)
        {
            return _repository.GetUserInternalActionPrivacy(request.InitObj, request.site_guid, request.user_action, request.social_platform);
        }

        public object Get(GetUserFriendsRequest request)
        {
            return _repository.GetUserFriends(request.InitObj, request.site_guid);
        }

        public object Get(AD_GetCustomDataIDRequest request)
        {
            return _repository.AD_GetCustomDataID(request.InitObj, request.site_guid, request.price, request.currency_code, request.asset_id, request.ppv_module_code, request.campaign_code, request.coupon_code, request.payment_method, request.country_code, request.language_code, request.device_name, request.asset_type);
        }

        public object Get(GetCustomDataIDRequest request)
        {
            return _repository.GetCustomDataID(request.InitObj, request.site_guid, request.price, request.currency_code, request.asset_id, request.ppv_module_code, request.campaign_code, request.coupon_code, request.payment_method, request.country_code, request.language_code, request.device_name, request.asset_type, request.override_end_date);
        }

        public object Get(SendNewPasswordRequest request)
        {
            return _repository.SendNewPassword(request.InitObj, request.user_name);
        }

        #endregion

        #region PUT

        public object Put(SetUserDataRequest request)
        {
            return _repository.SetUserData(request.InitObj, request.site_guid, request.user_basic_data, request.user_dynamic_data);
        }

        public object Put(SetUserDynamicDataRequest request)
        {
            return _repository.SetUserDynamicData(request.InitObj, request.site_guid, request.key, request.value);
        }

        public object Put(SetUserGroupRuleRequest request)
        {
            return _repository.SetUserGroupRule(request.InitObj, request.site_guid, request.rule_id, request.pin, request.is_active);
        }

        public object Put(ChangeUserPasswordRequest request)
        {
            return _repository.ChangeUserPassword(request.InitObj, request.user_name, request.old_password, request.new_password);
        }

        public object Put(RenewUserPasswordRequest request)
        {
            return _repository.RenewUserPassword(request.InitObj, request.user_name, request.password);
        }

        public object Put(ActivateAccountRequest request)
        {
            return _repository.ActivateAccount(request.InitObj, request.user_name, request.token);
        }

        public object Put(ActivateAccountByDomainMasterRequest request)
        {
            return _repository.ActivateAccountByDomainMaster(request.InitObj, request.master_user_name, request.user_name, request.token);
        }

        public object Put(UpdateItemInListRequest request)
        {
            return _repository.UpdateItemInList(request.InitObj, request.site_guid, request.item_objects, request.item_type, request.list_type);
        }

        public object Put(SetNotificationMessageViewStatusRequest request)
        {
            return _repository.SetNotificationMessageViewStatus(request.InitObj, request.site_guid, request.notification_request_id, request.notification_message_id, request.view_status);
        }

        public object Put(CC_ChargeUserForPrePaidRequest request)
        {
            return _repository.CC_ChargeUserForPrePaid(request.InitObj, request.site_guid, request.price, request.currency, request.product_code, request.ppv_module_code);;
        }

        public object Put(SetUserExternalActionShareRequest request)
        {
            return _repository.SetUserExternalActionShare(request.InitObj, request.site_guid, request.user_action, request.social_platform, request.social_action_privacy);
        }

        public object Put(SetUserInternalActionPrivacyRequest request)
        {
            return _repository.SetUserInternalActionPrivacy(request.InitObj, request.site_guid, request.user_action, request.social_platform, request.social_action_privacy);
        }
        
        #endregion

        #region POST

        public object Post(SignUpRequest request)
        {
            return _repository.SignUp(request.InitObj, request.user_basic_data, request.user_dynamic_data, request.password, request.affiliate_code);
        }

        public object Post(AddItemToListRequest request)
        {
            return _repository.AddItemToList(request.InitObj, request.site_guid, request.item_objects, request.item_type, request.list_type);
        }

        public object Post(SignInRequest request)
        {
            return _repository.SignIn(request.InitObj, request.user_name, request.password);
        }

        public object Post(DoUserActionRequest request)
        {
            return _repository.DoUserAction(request.InitObj, request.site_guid, request.user_action, request.extra_params, request.social_platform, request.asset_type, request.asset_id);
        }

        public object Post(InApp_ChargeUserForMediaFileRequest request)
        {
            return _repository.InApp_ChargeUserForMediaFile(request.InitObj, request.site_guid, request.price, request.currency, request.product_code, request.ppv_module_code, request.receipt);
        }

        #endregion

        #region DELETE

        public object Delete(RemoveItemFromListRequest request)
        {
            return _repository.RemoveItemFromList(request.InitObj, request.site_guid, request.item_objects, request.item_type, request.list_type);
        }

        public object Delete(ClearUserHistoryRequest request)
        {
            return _repository.CleanUserHistory(request.InitObj, request.site_guid, request.media_ids);
        }

        public object Delete(CancelSubscriptionRequest request)
        {
            return _repository.CancelSubscription(request.InitObj, request.site_guid, request.subscription_id, request.subscription_purchase_id);
        }

        public void Delete(SignOutRequest request)
        {
            _repository.SignOut(request.InitObj, request.user_name);
        }

        #endregion

    }
}
