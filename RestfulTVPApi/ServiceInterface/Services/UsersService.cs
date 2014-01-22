using System.Net;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using RestfulTVPApi.ServiceModel;
using System;
using System.Configuration;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class UsersService : Service
    {
        public IUsersRepository _repository { get; set; }  //Injected by IOC

        #region GET

        public HttpResult Get(GetUsersDataRequest request)
        {
            var response = _repository.GetUsersData(request.InitObj, request.site_guids);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            if (response.Length == 0)
            {
                return new HttpResult("", HttpStatusCode.NotFound);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserPermitedSubscriptionsRequest request)
        {
            var response = _repository.GetUserPermitedSubscriptions(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            if (response.Length == 0)
            {
                return new HttpResult("", HttpStatusCode.NotFound);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserExpiredSubscriptionsRequest request)
        {
            var response = _repository.GetUserExpiredSubscriptions(request.InitObj, request.site_guid, request.page_size);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserPermittedItemsRequest request)
        {
            var response = _repository.GetUserPermittedItems(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserExpiredItemsRequest request)
        {
            var response = _repository.GetUserExpiredItems(request.InitObj, request.site_guid, request.page_size);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserFavoritesRequest request)
        {
            var response = _repository.GetUserFavorites(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserGroupRulesRequest request)
        {
            var response = _repository.GetUserGroupRules(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(CheckGroupRuleRequest request)
        {
            var response = _repository.CheckGroupRule(request.InitObj, request.site_guid, request.rule_id, request.pin);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(RenewUserPINRequest request)
        {
            var response = _repository.RenewUserPIN(request.InitObj, request.site_guid, request.rule_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetItemFromListRequest request)
        {
            var response = _repository.GetItemFromList(request.InitObj, request.site_guid, request.item_objects, request.item_type, request.list_type);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(IsItemExistsInListRequest request)
        {
            var response = _repository.IsItemExistsInList(request.InitObj, request.site_guid, request.item_objects, request.item_type, request.list_type);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetPrepaidBalanceRequest request)
        {
            var response = _repository.GetPrepaidBalance(request.InitObj, request.site_guid,  request.currency_code);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(ResendActivationMailRequest request)
        {
            var response = _repository.ResendActivationMail(request.InitObj, request.user_name, request.password);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetLastWatchedMediasByPeriodRequest request)
        {
            var response = _repository.GetLastWatchedMediasByPeriod(request.InitObj, request.site_guid, request.pic_size, request.period_before, request.by_period);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserSocialMediasRequest request)
        {
            var response = _repository.GetUserSocialMedias(request.InitObj, request.site_guid, request.social_platform, request.social_action, request.pic_size, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserTransactionHistoryRequest request)
        {
            var response = _repository.GetUserTransactionHistory(request.InitObj, request.site_guid, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUsersBillingHistoryRequest request)
        {
            var response = _repository.GetUsersBillingHistory(request.InitObj, request.site_guids, request.start_date, request.end_date);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserItemsRequest request)
        {
            var response = _repository.GetUserItems(request.InitObj, request.site_guid, request.item_type, request.pic_size, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetLastBillingUserInfoRequest request)
        {
            var response = _repository.GetLastBillingUserInfo(request.InitObj, request.site_guid, request.billing_method);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetClientMerchantSigRequest request)
        {
            var response = _repository.GetClientMerchantSig(request.InitObj, request.paramaters);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(AreMediasFavoriteRequest request)
        {
            var response = _repository.AreMediasFavorite(request.InitObj, request.site_guid, request.media_ids);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetRecommendedMediasByTypesRequest request)
        {
            var response = _repository.GetRecommendedMediasByTypes(request.InitObj, request.site_guid, request.pic_size, request.page_size, request.page_number, request.media_types);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetDeviceNotificationsRequest request)
        {
            //Ofir
            int? message_count = request.page_size > 0 ? new Nullable<int>(request.page_size) : null;

            var response = _repository.GetDeviceNotifications(request.InitObj, request.site_guid, request.notification_type, request.view_status, message_count);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserStatusSubscriptionsRequest request)
        {
            var response = _repository.GetUserStatusSubscriptions(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserStartedWatchingMediasRequest request)
        {
            var response = _repository.GetUserStartedWatchingMedias(request.InitObj, request.site_guid, request.page_size);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(IsUserSignedInRequest request)
        {
            var response = _repository.IsUserSignedIn(request.InitObj, request.site_guid);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetAllFriendsWatchedRequest request)
        {
            var response = _repository.GetAllFriendsWatched(request.InitObj, request.site_guid, request.page_size);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetFriendsActionsRequest request)
        {
            var response = _repository.GetFriendsActions(request.InitObj, request.site_guid, request.user_actions, request.asset_type, request.asset_id, request.page_number, request.page_size, request.social_platform);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);

        }

        public HttpResult Get(GetUserAllowedSocialPrivacyListRequest request)
        {
            var response = _repository.GetUserAllowedSocialPrivacyList(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);

        }

        public HttpResult Get(GetUserExternalActionShareRequest request)
        {
            var response = _repository.GetUserExternalActionShare(request.InitObj, request.site_guid, request.user_action, request.social_platform);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserInternalActionPrivacyRequest request)
        {
            var response = _repository.GetUserInternalActionPrivacy(request.InitObj, request.site_guid, request.user_action, request.social_platform);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetUserFriendsRequest request)
        {
            var response = _repository.GetUserFriends(request.InitObj, request.site_guid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);

        }

        public HttpResult Get(AD_GetCustomDataIDRequest request)
        {
            var response = _repository.AD_GetCustomDataID(request.InitObj, request.site_guid, request.price, request.currency_code, request.asset_id, request.ppv_module_code, request.campaign_code, request.coupon_code, request.payment_method, request.country_code, request.language_code, request.device_name, request.asset_type);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetCustomDataIDRequest request)
        {
            var response = _repository.GetCustomDataID(request.InitObj, request.site_guid, request.price, request.currency_code, request.asset_id, request.ppv_module_code, request.campaign_code, request.coupon_code, request.payment_method, request.country_code, request.language_code, request.device_name, request.asset_type, request.override_end_date);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        #endregion

        #region PUT

        public HttpResult Put(SetUserDataRequest request)
        {
            var response = _repository.SetUserData(request.InitObj, request.site_guid, request.user_basic_data, request.user_dynamic_data);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Put(SetUserDynamicDataRequest request)
        {
            var response = _repository.SetUserDynamicData(request.InitObj, request.site_guid, request.key, request.value);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Put(SetUserGroupRuleRequest request)
        {
            var response = _repository.SetUserGroupRule(request.InitObj, request.site_guid, request.rule_id, request.pin, request.is_active);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Put(ChangeUserPasswordRequest request)
        {
            var response = _repository.ChangeUserPassword(request.InitObj, request.user_name, request.old_password, request.new_password);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Put(RenewUserPasswordRequest request)
        {
            var response = _repository.RenewUserPassword(request.InitObj, request.user_name, request.password);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Put(ActivateAccountRequest request)
        {
            var response = _repository.ActivateAccount(request.InitObj, request.user_name, request.token);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Put(ActivateAccountByDomainMasterRequest request)
        {
            var response = _repository.ActivateAccountByDomainMaster(request.InitObj, request.master_user_name, request.user_name, request.token);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Put(UpdateItemInListRequest request)
        {
            var response = _repository.UpdateItemInList(request.InitObj, request.site_guid, request.item_objects, request.item_type, request.list_type);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Put(SetNotificationMessageViewStatusRequest request)
        {
            var response = _repository.SetNotificationMessageViewStatus(request.InitObj, request.site_guid, request.notification_request_id, request.notification_message_id, request.view_status);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Put(CC_ChargeUserForPrePaidRequest request)
        {
            var response = _repository.CC_ChargeUserForPrePaid(request.InitObj, request.site_guid, request.price, request.currency, request.product_code, request.ppv_module_code);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Put(SetUserExternalActionShareRequest request)
        {
            var response = _repository.SetUserExternalActionShare(request.InitObj, request.site_guid, request.user_action, request.social_platform, request.social_action_privacy);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Put(SetUserInternalActionPrivacyRequest request)
        {
            var response = _repository.SetUserInternalActionPrivacy(request.InitObj, request.site_guid, request.user_action, request.social_platform, request.social_action_privacy);

            return new HttpResult(response, HttpStatusCode.OK);
        }
        
        #endregion

        #region POST

        public HttpResult Post(SignUpRequest request)
        {
            var response = _repository.SignUp(request.InitObj, request.user_basic_data, request.user_dynamic_data, request.password, request.affiliate_code);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Post(AddItemToListRequest request)
        {
            var response = _repository.AddItemToList(request.InitObj, request.site_guid, request.item_objects, request.item_type, request.list_type);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(SendNewPasswordRequest request)
        {
            var response = _repository.SendNewPassword(request.InitObj, request.user_name);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(SignInRequest request)
        {
            var response = _repository.SignIn(request.InitObj, request.user_name, request.password);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(DoUserActionRequest request)
        {
            var response = _repository.DoUserAction(request.InitObj, request.site_guid, request.user_action, request.extra_params, request.social_platform, request.asset_type, request.asset_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        #endregion

        #region DELETE

        public HttpResult Delete(RemoveItemFromListRequest request)
        {
            var response = _repository.RemoveItemFromList(request.InitObj, request.site_guid, request.item_objects, request.item_type, request.list_type);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Delete(ClearUserHistoryRequest request)
        {
            var response = _repository.CleanUserHistory(request.InitObj, request.site_guid, request.media_ids);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Delete(CancelSubscriptionRequest request)
        {
            var response = _repository.CancelSubscription(request.InitObj, request.site_guid, request.subscription_id, request.subscription_purchase_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Delete(SignOutRequest request)
        {
            _repository.SignOut(request.InitObj, request.user_name);

            return new HttpResult(HttpStatusCode.OK);
        }

        #endregion

    }
}
