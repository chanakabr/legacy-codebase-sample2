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
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi.ServiceInterface
{
    public class UsersRepository : IUsersRepository
    {
        public List<UserResponseObject> GetUsersData(GetUsersDataRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).GetUsersData(request.site_guids);
        }

        public UserResponseObject SetUserData(SetUserDataRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).SetUserData(request.site_guid, request.user_basic_data, request.user_dynamic_data);
        }

        public List<PermittedSubscriptionContainer> GetUserPermitedSubscriptions(GetUserPermitedSubscriptionsRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetUserPermitedSubscriptions(request.site_guid);       
        }

        public List<PermittedSubscriptionContainer> GetUserExpiredSubscriptions(GetUserExpiredSubscriptionsRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetUserExpiredSubscriptions(request.site_guid, request.page_size);
        }

        public List<PermittedMediaContainer> GetUserPermittedItems(GetUserPermittedItemsRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetUserPermittedItems(request.site_guid);   
        }

        public List<PermittedMediaContainer> GetUserExpiredItems(GetUserExpiredItemsRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetUserExpiredItems(request.site_guid, request.page_size);               
        }

        public UserResponseObject SignUp(SignUpRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).SignUp(request.user_basic_data, request.user_dynamic_data, request.password, request.affiliate_code);
        }

        //Ofir - Should DomainID passed as param?
        public List<FavoriteObject> GetUserFavorites(GetUserFavoritesRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).GetUserFavorites(request.site_guid, string.Empty, request.InitObj.DomainID, string.Empty);            
        }

        public List<GroupRule> GetUserGroupRules(GetUserGroupRulesRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).GetUserGroupRules(request.site_guid);
        }

        public bool SetUserGroupRule(SetUserGroupRuleRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).SetUserGroupRule(request.site_guid, request.rule_id, request.pin, request.is_active);            
        }

        public bool CheckGroupRule(CheckGroupRuleRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).CheckParentalPIN(request.site_guid, request.rule_id, request.pin);            
        }

        public UserResponseObject ChangeUserPassword(ChangeUserPasswordRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).ChangeUserPassword(request.user_name, request.old_password, request.new_password);            
        }

        public UserResponseObject RenewUserPassword(RenewUserPasswordRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).RenewUserPassword(request.user_name, request.password);            
        }

        public UserResponseObject ActivateAccount(ActivateAccountRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).ActivateAccount(request.user_name, request.token);            
        }

        public bool ResendActivationMail(ResendActivationMailRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).ResendActivationMail(request.user_name, request.password);            
        }

        public eResponseStatus RenewUserPIN(RenewUserPINRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).RenewUserPIN(request.site_guid, request.rule_id);            
        }

        // TODO: Create a SetUserTypeByUserIDRequest object, modify the function/interface/UserService
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

        public UserResponseObject ActivateAccountByDomainMaster(ActivateAccountByDomainMasterRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).ActivateAccountByDomainMaster(request.master_user_name, request.user_name, request.token);            
        }

        public bool AddItemToList(AddItemToListRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).AddItemToList(request.site_guid, request.item_objects, request.item_type, request.list_type);            
        }

        public List<UserItemList> GetItemFromList(GetItemFromListRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).GetItemFromList(request.site_guid, request.item_objects, request.item_type, request.list_type);            
        }

        public List<KeyValuePair> IsItemExistsInList(IsItemExistsInListRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).IsItemExistsInList(request.site_guid, request.item_objects, request.item_type, request.list_type);            
        }

        public bool RemoveItemFromList(RemoveItemFromListRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).RemoveItemFromList(request.site_guid, request.item_objects, request.item_type, request.list_type);            
        }

        public bool UpdateItemInList(UpdateItemInListRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).UpdateItemInList(request.site_guid, request.item_objects, request.item_type, request.list_type);            
        }

        public List<string> GetPrepaidBalance(GetPrepaidBalanceRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetPrepaidBalance(request.site_guid, request.currency_code);            
        }

        public List<Media> GetLastWatchedMediasByPeriod(GetLastWatchedMediasByPeriodRequest request)
        {
            List<Media> lstMedia = null;

            List<Media> lstAllMedias = new TVPApiModule.CatalogLoaders.APIPersonalLastWatchedLoader(request.site_guid, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.InitObj.Locale.LocaleLanguage, 100, 0, request.pic_size).Execute() as List<Media>;

                if (lstAllMedias != null)
                {
                    lstMedia = (from media in lstAllMedias
                                where
                                    (DateTime.Now.AddDays((double)request.by_period * request.period_before * -1) - (DateTime)media.last_watch_date).TotalDays >= 0 &&
                                    (DateTime.Now.AddDays((double)request.by_period * request.period_before * -1) - (DateTime)media.last_watch_date).TotalDays <= (request.period_before + 1) * (int)request.by_period
                                select media).ToList<Media>();
                }
            
            return lstMedia;
        }

        public List<Media> GetUserSocialMedias(GetUserSocialMediasRequest request)
        {
            List<Media> lstMedia = null;

            lstMedia = new APIUserSocialMediaLoader(request.site_guid, request.social_action, request.social_platform, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.InitObj.Locale.LocaleLanguage, request.page_size, request.page_number, request.pic_size)
                {
                    //UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            
            return lstMedia;
        }

        public BillingTransactionsResponse GetUserTransactionHistory(GetUserTransactionHistoryRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetUserTransactionHistory(request.site_guid, request.page_number, request.page_size);
        }

        public BillingResponse CC_ChargeUserForPrePaid(CC_ChargeUserForPrePaidRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).CC_ChargeUserForPrePaid(request.site_guid, request.price, request.currency, request.product_code, request.ppv_module_code, request.InitObj.UDID);
        }

        public List<UserBillingTransactionsResponse> GetUsersBillingHistory(GetUsersBillingHistoryRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetUsersBillingHistory(request.site_guids, request.start_date, request.end_date);            
        }

        public List<Media> GetUserItems(GetUserItemsRequest request)
        {
            return MediaHelper.GetUserItems(request.InitObj.Platform, request.site_guid, request.InitObj.DomainID, request.InitObj.UDID, request.InitObj.Locale.LocaleLanguage, request.item_type, request.pic_size, request.page_size, request.page_number, request.GroupID);            
        }

        public AdyenBillingDetail GetLastBillingUserInfo(GetLastBillingUserInfoRequest request)
        {
            return ServicesManager.BillingService(request.GroupID, request.InitObj.Platform).GetLastBillingUserInfo(request.site_guid, request.billing_method);            
        }

        public string GetClientMerchantSig(GetClientMerchantSigRequest request)
        {
            return ServicesManager.BillingService(request.GroupID, request.InitObj.Platform).GetClientMerchantSig(request.paramaters);            
        }

        public List<KeyValuePair<int, bool>> AreMediasFavorite(AreMediasFavoriteRequest request)
        {
            List<KeyValuePair<int, bool>> result = null;

            List<FavoriteObject> favoriteObjects = ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).GetUserFavorites(request.site_guid, string.Empty, request.InitObj.DomainID, string.Empty);

                if (favoriteObjects != null)
                    result = request.media_ids.Select(y => new KeyValuePair<int, bool>(y, favoriteObjects.Where(x => x.item_code == y.ToString()).Count() > 0)).ToList();
            
            return result;
        }

        public List<Media> GetRecommendedMediasByTypes(GetRecommendedMediasByTypesRequest request)
        {
            List<Media> lstMedia = null;

            lstMedia = new TVPApiModule.CatalogLoaders.APIPersonalRecommendedLoader(request.site_guid, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.InitObj.Locale.LocaleLanguage, request.page_size, request.page_number, request.pic_size)
                {
                    //UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            
            return lstMedia;
        }

        public Status CancelSubscription(CancelSubscriptionRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).CancelSubscription(request.site_guid, request.subscription_id, request.subscription_purchase_id);             
        }

        public List<Notification> GetDeviceNotifications(GetDeviceNotificationsRequest request)
        {
            int? message_count = request.page_size > 0 ? new Nullable<int>(request.page_size) : null;

            return ServicesManager.NotificationService(request.GroupID, request.InitObj.Platform).GetDeviceNotifications(request.site_guid, request.InitObj.UDID, request.notification_type == NotificationMessageType.All ? NotificationMessageType.Pull : request.notification_type, request.view_status, message_count);            
        }

        public bool SetNotificationMessageViewStatus(SetNotificationMessageViewStatusRequest request)
        {
            return ServicesManager.NotificationService(request.GroupID, request.InitObj.Platform).SetNotificationMessageViewStatus(request.site_guid, request.notification_request_id, request.notification_message_id, request.view_status);            
        }

        public List<TagMetaPairArray> GetUserStatusSubscriptions(GetUserStatusSubscriptionsRequest request)
        {
            return ServicesManager.NotificationService(request.GroupID, request.InitObj.Platform).GetUserStatusSubscriptions(request.site_guid);            
        }

        public bool CleanUserHistory(ClearUserHistoryRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).CleanUserHistory(request.site_guid, request.media_ids);            
        }

        public List<string> GetUserStartedWatchingMedias(GetUserStartedWatchingMediasRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).GetUserStartedWatchingMedias(request.site_guid, request.page_size);            
        }

        public Status SendNewPassword(SendNewPasswordRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).SentNewPasswordToUser(request.user_name);
        }

        public bool IsUserSignedIn(IsUserSignedInRequest request)
        {
            bool isSingleLogin = Utils.GetIsSingleLoginValue(request.GroupID, request.InitObj.Platform);

            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).IsUserLoggedIn(request.site_guid, request.InitObj.UDID, string.Empty, SiteHelper.GetClientIP(), isSingleLogin);            
        }

        public bool SetUserDynamicData(SetUserDynamicDataRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).SetUserDynamicData(request.site_guid, request.key, request.value);            
        }

        public ApiUsersService.LogInResponseData SignIn(SignInRequest request)
        {
            IImplementation impl = WSUtils.GetImplementation(request.GroupID, request.InitObj);            
            return impl.SignIn(request.user_name, request.password);
        }

        public void SignOut(SignOutRequest request)
        {
            bool isSingleLogin = Utils.GetIsSingleLoginValue(request.GroupID, request.InitObj.Platform);

            ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).SignOut(request.user_name, request.InitObj.UDID, string.Empty, isSingleLogin);            
        }

        public List<FriendWatchedObject> GetAllFriendsWatched(GetAllFriendsWatchedRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetAllFriendsWatched(request.site_guid, request.page_size);                            
        }

        public TVPApiModule.Objects.Responses.DoSocialActionResponse DoUserAction(DoUserActionRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).DoUserAction(request.site_guid, request.InitObj.UDID, request.user_action, request.extra_params, request.social_platform, request.asset_type, request.asset_id);
        }

        public List<UserSocialActionObject> GetFriendsActions(GetFriendsActionsRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetFriendsActions(request.site_guid, request.user_actions, request.asset_type, request.asset_id, request.page_number, request.page_size, request.social_platform);            
        }

        public List<UserSocialActionObject> GetUserActions(GetUserActionsRequest request)
        {
            throw new NotImplementedException();//return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetUserActions(request.site_guid, request.user_action, request.asset_type, request.asset_id, request.page_number, request.page_size, request.social_platform);            
        }

        public List<eSocialPrivacy> GetUserAllowedSocialPrivacyList(GetUserAllowedSocialPrivacyListRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetUserAllowedSocialPrivacyList(request.site_guid);                            
        }

        public eSocialActionPrivacy GetUserExternalActionShare(GetUserExternalActionShareRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetUserExternalActionShare(request.site_guid, request.user_action, request.social_platform);            
        }

        public List<string> GetUserFriends(GetUserFriendsRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetUserFriends(request.site_guid);            
        }

        public eSocialActionPrivacy GetUserInternalActionPrivacy(GetUserInternalActionPrivacyRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetUserInternalActionPrivacy(request.site_guid, request.user_action, request.social_platform);            
        }

        public eSocialPrivacy GetUserSocialPrivacy(GetUserSocialPrivacyRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetUserSocialPrivacy(request.site_guid, request.social_platform, request.user_action);            
        }

        public bool SetUserExternalActionShare(SetUserExternalActionShareRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).SetUserExternalActionShare(request.site_guid, request.user_action, request.social_platform, request.social_action_privacy);            
        }

        public bool SetUserInternalActionPrivacy(SetUserInternalActionPrivacyRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).SetUserInternalActionPrivacy(request.site_guid, request.user_action, request.social_platform, request.social_action_privacy);            
        }

        public int AD_GetCustomDataID(AD_GetCustomDataIDRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).AD_GetCustomDataID(request.site_guid, request.price, request.currency_code, request.asset_id, request.ppv_module_code, request.campaign_code, request.coupon_code, request.payment_method, SiteHelper.GetClientIP(), request.country_code, request.language_code, request.device_name, request.asset_type);
        }

        public int GetCustomDataID(GetCustomDataIDRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetCustomDataID(request.site_guid, request.price, request.currency_code, request.asset_id, request.ppv_module_code, request.campaign_code, request.coupon_code, request.payment_method, SiteHelper.GetClientIP(), request.country_code, request.language_code, request.device_name, request.asset_type, request.override_end_date);            
        }

        public BillingResponse InApp_ChargeUserForMediaFile(InApp_ChargeUserForMediaFileRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).InApp_ChargeUserForMediaFile(request.site_guid, request.price, request.currency, request.product_code, request.ppv_module_code, request.InitObj.UDID, request.receipt);                            
        }

        public AdyenBillingDetail GetLastBillingTypeUserInfo(GetLastBillingTypeUserInfoRequest request)
        {
            return ServicesManager.BillingService(request.GroupID, request.InitObj.Platform).GetLastBillingTypeUserInfo(request.site_guid);                            
        }

        public List<PermittedCollectionContainer> GetUserPermittedCollections(GetUserPermittedCollectionsRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetUserPermittedCollections(request.site_guid);
        }

        public ChangeSubscriptionStatus ChangeSubscription(ChangeSubscriptionRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).ChangeSubscription(request.site_guid, request.old_subscription, request.new_subscription);
        }

        public int CreatePurchaseToken(CreatePurchaseTokenRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).CreatePurchaseToken(request.site_guid, request.price, request.currency_code, request.asset_id, request.ppv_module_code, request.campaign_code, request.coupon_code, request.payment_method, SiteHelper.GetClientIP(), request.country_code, request.language_code, request.device_name, request.asset_type, request.override_end_date, request.preview_module_id);
        }

        public string DummyChargeUserForCollection(DummyChargeUserForCollectionRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).DummyChargeUserForCollection(request.site_guid, request.collection_id, request.price, request.currency, request.coupon_code, SiteHelper.GetClientIP(), request.extra_parameters, request.country_code, request.language_code, request.udid);
        }

        public BillingResponse ChargeUserForCollection(ChargeUserForCollectionRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).ChargeUserForCollection(request.site_guid, request.collection_code, request.price, request.currency, request.encrypted_cvv, request.coupon_code, SiteHelper.GetClientIP(), request.extra_parameters, request.country_code, request.language_code, request.udid, request.payment_method_id);
        }
        
        public BillingResponse CellularChargeUserForSubscription(CellularChargeUserForSubscriptionRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).CellularChargeUserForSubscription(request.site_guid, request.price, request.currency, request.subscription_code, request.coupon_code, SiteHelper.GetClientIP(), request.extra_parameters, request.country_code, request.language_code, request.udid);
        }

        public string ChargeUserForSubscriptionByPaymentMethod(ChargeUserForSubscriptionByPaymentMethodRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).ChargeUserForSubscriptionByPaymentMethod(request.site_guid, request.price, request.currency, request.subscription_code, request.coupon_code, SiteHelper.GetClientIP(), request.extra_parameters, request.country_code, request.language_code, request.udid, request.payment_method_id, request.encrypted_cvv);
        }

        public string ChargeUserForMediaFileByPaymentMethod(ChargeUserForMediaFileByPaymentMethodRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).ChargeUserForMediaFileByPaymentMethod(request.price, request.currency, request.media_file_id, request.ppv_module_code, SiteHelper.GetClientIP(), request.site_guid, request.udid, request.extra_parameters, request.payment_method_id, request.encrypted_cvv);
        }

        public string CellularChargeUserForMediaFile(CellularChargeUserForMediaFileRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).CellularChargeUserForMediaFileRequest(request.price, request.currency, request.media_file_id, request.ppv_module_code, SiteHelper.GetClientIP(), request.site_guid, request.udid, request.extra_parameters, request.coupon_code, request.language_code, request.country_code);
        }

        public string ChargeUserForMediaFileUsingCC(ChargeUserForMediaFileUsingCCRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).ChargeUserForMediaFileUsingCC(request.price, request.currency, request.media_file_id, request.ppv_module_code, request.coupon_code, SiteHelper.GetClientIP(), request.site_guid, request.udid, request.payment_method_id, request.encrypted_cvv);
        }

        public string ChargeUserForMediaSubscriptionUsingCC(ChargeUserForMediaSubscriptionUsingCCRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).ChargeUserForMediaSubscriptionUsingCC(request.price, request.currency, request.subscription_id, request.coupon_code, SiteHelper.GetClientIP(), request.site_guid, request.udid, request.payment_method_id, request.encrypted_cvv, request.extra_parameters, request.country_code, request.language_code);
        }

        public ApiUsersService.LogInResponseData SignInWithToken(SignInWithTokenRequest request)
        {
            throw new NotImplementedException();
        }
        
        public List<PermittedCollectionContainer> GetUserExpiredCollections(GetUserExpiredCollectionsRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetUserExpiredCollections(request.site_guid, request.num_of_items);
        }

        /*public bool CancelTransaction(CancelTransactionRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).CancelTransaction(request.site_guid, request.asset_id, request.transaction_type, request.is_force);
        }*/

        public bool CancelTransaction(CancelTransactionRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).CancelTransaction(request.site_guid, request.asset_id, request.transaction_type);
        }

        public bool WaiverTransaction(WaiverTransactionRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).WaiverTransaction(request.site_guid, request.asset_id, request.transaction_type);
        }


        public UserResponseObject CheckTemporaryToken(CheckTemporaryTokenRequest request)
        {
            return ServicesManager.UsersService(request.GroupID, request.InitObj.Platform).CheckTemporaryToken(request.token);
        }


        /*public Status CancelSubscriptionRenewal(CancelSubscriptionRenewalRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).CancelSubscriptionRenewal(request.domain_id, request.subscription_id);
        }*/


        public FBSignIn FBUserSignin(FBUserSigninRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).FBUserSignin(request.token, request.ip, request.device_id, request.prevent_double_logins);
        }
    }
}