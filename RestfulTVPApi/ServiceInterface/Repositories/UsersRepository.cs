using System;
using System.Collections.Generic;
using System.Linq;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Interfaces;
using TVPApiModule.Context;
using TVPApiModule.Helper;
using RestfulTVPApi.ServiceModel;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Objects.Responses.Enums;
using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.Clients;
using RestfulTVPApi.Notification;
using RestfulTVPApi.Objects.Response;

namespace RestfulTVPApi.ServiceInterface
{
    public class UsersRepository : IUsersRepository
    {
        public List<UserResponseObject> GetUsersData(GetUsersDataRequest request)
        {
            return ClientsManager.UsersClient().GetUsersData(request.site_guids);
        }

        public UserResponseObject SetUserData(SetUserDataRequest request)
        {
            return ClientsManager.UsersClient().SetUserData(request.site_guid, request.user_basic_data, request.user_dynamic_data);
        }

        public List<PermittedSubscriptionContainer> GetUserPermitedSubscriptions(GetUserPermitedSubscriptionsRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetUserPermitedSubscriptions(request.site_guid); 
        }

        public List<PermittedSubscriptionContainer> GetUserExpiredSubscriptions(GetUserExpiredSubscriptionsRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetUserExpiredSubscriptions(request.site_guid, request.page_size);
        }

        public List<PermittedMediaContainer> GetUserPermittedItems(GetUserPermittedItemsRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetUserPermittedItems(request.site_guid);   
        }

        public List<PermittedMediaContainer> GetUserExpiredItems(GetUserExpiredItemsRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetUserExpiredItems(request.site_guid, request.page_size);  
        }

        public UserResponseObject SignUp(SignUpRequest request)
        {
            return ClientsManager.UsersClient().SignUp(request.user_basic_data, request.user_dynamic_data, request.password, request.affiliate_code);
        }

        //Ofir - Should DomainID passed as param?
        public List<FavoriteObject> GetUserFavorites(GetUserFavoritesRequest request)
        {
            return ClientsManager.UsersClient().GetUserFavorites(request.site_guid, string.Empty, request.InitObj.DomainID, string.Empty);            
        }

        public List<GroupRule> GetUserGroupRules(GetUserGroupRulesRequest request)
        {
            return ClientsManager.ApiClient().GetUserGroupRules(request.site_guid);
        }

        public bool SetUserGroupRule(SetUserGroupRuleRequest request)
        {
            return ClientsManager.ApiClient().SetUserGroupRule(request.site_guid, request.rule_id, request.pin, request.is_active);            
        }

        public bool CheckGroupRule(CheckGroupRuleRequest request)
        {
            return ClientsManager.ApiClient().CheckParentalPIN(request.site_guid, request.rule_id, request.pin);      
        }

        public UserResponseObject ChangeUserPassword(ChangeUserPasswordRequest request)
        {
            return ClientsManager.UsersClient().ChangeUserPassword(request.user_name, request.old_password, request.new_password);            
        }

        public UserResponseObject RenewUserPassword(RenewUserPasswordRequest request)
        {
            return ClientsManager.UsersClient().RenewUserPassword(request.user_name, request.password);            
        }

        public UserResponseObject ActivateAccount(ActivateAccountRequest request)
        {
            return ClientsManager.UsersClient().ActivateAccount(request.user_name, request.token);            
        }

        public bool ResendActivationMail(ResendActivationMailRequest request)
        {
            return ClientsManager.UsersClient().ResendActivationMail(request.user_name, request.password);            
        }

        public eResponseStatus RenewUserPIN(RenewUserPINRequest request)
        {
            return ClientsManager.UsersClient().RenewUserPIN(request.site_guid, request.rule_id);            
        }

        // TODO: Create a SetUserTypeByUserIDRequest object, modify the function/interface/UserService
        public eResponseStatus SetUserTypeByUserID(TVPApiModule.Objects.InitializationObject initObj, string siteGuid, int nUserTypeID)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserTypeByUserID", initObj.ApiUser, initObj.ApiPass, Utils.GetClientIP());

            if (groupID > 0)
            {
                return ClientsManager.UsersClient().SetUserTypeByUserID(siteGuid, nUserTypeID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public UserResponseObject ActivateAccountByDomainMaster(ActivateAccountByDomainMasterRequest request)
        {
            return ClientsManager.UsersClient().ActivateAccountByDomainMaster(request.master_user_name, request.user_name, request.token);            
        }

        public bool AddItemToList(AddItemToListRequest request)
        {
            return ClientsManager.UsersClient().AddItemToList(request.site_guid, request.item_objects, request.item_type, request.list_type);            
        }

        public List<UserItemList> GetItemFromList(GetItemFromListRequest request)
        {
            return ClientsManager.UsersClient().GetItemFromList(request.site_guid, request.item_objects, request.item_type, request.list_type);            
        }

        public List<KeyValuePair> IsItemExistsInList(IsItemExistsInListRequest request)
        {
            return ClientsManager.UsersClient().IsItemExistsInList(request.site_guid, request.item_objects, request.item_type, request.list_type);            
        }

        public bool RemoveItemFromList(RemoveItemFromListRequest request)
        {
            return ClientsManager.UsersClient().RemoveItemFromList(request.site_guid, request.item_objects, request.item_type, request.list_type);            
        }

        public bool UpdateItemInList(UpdateItemInListRequest request)
        {
            return ClientsManager.UsersClient().UpdateItemInList(request.site_guid, request.item_objects, request.item_type, request.list_type);            
        }

        public List<string> GetPrepaidBalance(GetPrepaidBalanceRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetPrepaidBalance(request.site_guid, request.currency_code);            
        }

        public List<Media> GetLastWatchedMediasByPeriod(GetLastWatchedMediasByPeriodRequest request)
        {
            List<Media> lstMedia = null;

            //List<Media> lstAllMedias = new TVPApiModule.CatalogLoaders.APIPersonalLastWatchedLoader(request.site_guid, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, Utils.GetClientIP(), request.InitObj.Locale.LocaleLanguage, 100, 0, request.pic_size).Execute() as List<Media>;

            //    if (lstAllMedias != null)
            //    {
            //        lstMedia = (from media in lstAllMedias
            //                    where
            //                        (DateTime.Now.AddDays((double)request.by_period * request.period_before * -1) - (DateTime)media.last_watch_date).TotalDays >= 0 &&
            //                        (DateTime.Now.AddDays((double)request.by_period * request.period_before * -1) - (DateTime)media.last_watch_date).TotalDays <= (request.period_before + 1) * (int)request.by_period
            //                    select media).ToList<Media>();
            //    }
            
            return lstMedia;
        }

        public List<Media> GetUserSocialMedias(GetUserSocialMediasRequest request)
        {
            List<Media> lstMedia = null;

            //lstMedia = new APIUserSocialMediaLoader(request.site_guid, request.social_action, request.social_platform, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, Utils.GetClientIP(), request.InitObj.Locale.LocaleLanguage, request.page_size, request.page_number, request.pic_size)
            //    {
            //        //UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
            //    }.Execute() as List<Media>;
            
            return lstMedia;
        }

        public BillingTransactionsResponse GetUserTransactionHistory(GetUserTransactionHistoryRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetUserTransactionHistory(request.site_guid, request.page_number, request.page_size);
        }

        public BillingResponse CC_ChargeUserForPrePaid(CC_ChargeUserForPrePaidRequest request)
        {
            return ClientsManager.ConditionalAccessClient().CC_ChargeUserForPrePaid(request.site_guid, request.price, request.currency, request.product_code, request.ppv_module_code, request.InitObj.UDID);
        }

        public List<UserBillingTransactionsResponse> GetUsersBillingHistory(GetUsersBillingHistoryRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetUsersBillingHistory(request.site_guids, request.start_date, request.end_date);   
        }

        public List<Media> GetUserItems(GetUserItemsRequest request)
        {
            //return MediaHelper.GetUserItems(request.InitObj.Platform, request.site_guid, request.InitObj.DomainID, request.InitObj.UDID, request.InitObj.Locale.LocaleLanguage, request.item_type, request.pic_size, request.page_size, request.page_number, request.GroupID);            
            return null;
        }

        public AdyenBillingDetail GetLastBillingUserInfo(GetLastBillingUserInfoRequest request)
        {
            return ClientsManager.BillingClient().GetLastBillingUserInfo(request.site_guid, request.billing_method);        
        }

        public string GetClientMerchantSig(GetClientMerchantSigRequest request)
        {
            return ClientsManager.BillingClient().GetClientMerchantSig(request.paramaters);            
        }

        public List<KeyValuePair<int, bool>> AreMediasFavorite(AreMediasFavoriteRequest request)
        {
            List<KeyValuePair<int, bool>> result = null;

            List<FavoriteObject> favoriteObjects = ClientsManager.UsersClient().GetUserFavorites(request.site_guid, string.Empty, request.InitObj.DomainID, string.Empty);

                if (favoriteObjects != null)
                    result = request.media_ids.Select(y => new KeyValuePair<int, bool>(y, favoriteObjects.Where(x => x.item_code == y.ToString()).Count() > 0)).ToList();
            
            return result;
        }

        public List<Media> GetRecommendedMediasByTypes(GetRecommendedMediasByTypesRequest request)
        {
            List<Media> lstMedia = null;

            //lstMedia = new TVPApiModule.CatalogLoaders.APIPersonalRecommendedLoader(request.site_guid, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, Utils.GetClientIP(), request.InitObj.Locale.LocaleLanguage, request.page_size, request.page_number, request.pic_size)
            //    {
            //        //UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
            //    }.Execute() as List<Media>;
            
            return lstMedia;
        }

        public Status CancelSubscription(CancelSubscriptionRequest request)
        {
            return ClientsManager.ConditionalAccessClient().CancelSubscription(request.site_guid, request.subscription_id, request.subscription_purchase_id);             
        }

        public List<RestfulTVPApi.Objects.Responses.Notification> GetDeviceNotifications(GetDeviceNotificationsRequest request)
        {
            int? message_count = request.page_size > 0 ? new Nullable<int>(request.page_size) : null;

            return ClientsManager.NotificationClient().GetDeviceNotifications(request.site_guid, request.InitObj.UDID, request.notification_type == NotificationMessageType.All ? NotificationMessageType.Pull : request.notification_type, request.view_status, message_count);            
        }

        public bool SetNotificationMessageViewStatus(SetNotificationMessageViewStatusRequest request)
        {
            return ClientsManager.NotificationClient().SetNotificationMessageViewStatus(request.site_guid, request.notification_request_id, request.notification_message_id, request.view_status);            
        }

        public List<TagMetaPairArray> GetUserStatusSubscriptions(GetUserStatusSubscriptionsRequest request)
        {
            return ClientsManager.NotificationClient().GetUserStatusSubscriptions(request.site_guid);            
        }

        public bool CleanUserHistory(ClearUserHistoryRequest request)
        {
            return ClientsManager.ApiClient().CleanUserHistory(request.site_guid, request.media_ids);            
        }

        public List<string> GetUserStartedWatchingMedias(GetUserStartedWatchingMediasRequest request)
        {
            return ClientsManager.ApiClient().GetUserStartedWatchingMedias(request.site_guid, request.page_size);            
        }

        public Status SendNewPassword(SendNewPasswordRequest request)
        {
            return ClientsManager.UsersClient().SentNewPasswordToUser(request.user_name);
        }

        public bool IsUserSignedIn(IsUserSignedInRequest request)
        {
            bool isSingleLogin = Utils.GetIsSingleLoginValue(request.GroupID, request.InitObj.Platform);

            return ClientsManager.UsersClient().IsUserLoggedIn(request.site_guid, request.InitObj.UDID, string.Empty, Utils.GetClientIP(), isSingleLogin);            
        }

        public bool SetUserDynamicData(SetUserDynamicDataRequest request)
        {
            return ClientsManager.UsersClient().SetUserDynamicData(request.site_guid, request.key, request.value);            
        }

        public UsersClient.LogInResponseData SignIn(SignInRequest request)
        {
            IImplementation impl = WSUtils.GetImplementation(request.GroupID, request.InitObj);            
            //return impl.SignIn(request.user_name, request.password);
            return null;
        }

        public void SignOut(SignOutRequest request)
        {
            bool isSingleLogin = Utils.GetIsSingleLoginValue(request.GroupID, request.InitObj.Platform);

            ClientsManager.UsersClient().SignOut(request.user_name, request.InitObj.UDID, string.Empty, isSingleLogin);            
        }

        public List<FriendWatchedObject> GetAllFriendsWatched(GetAllFriendsWatchedRequest request)
        {
            return ClientsManager.SocialClient().GetAllFriendsWatched(request.site_guid, request.page_size);                            
        }

        public DoSocialActionResponse DoUserAction(DoUserActionRequest request)
        {
            return ClientsManager.SocialClient().DoUserAction(request.site_guid, request.InitObj.UDID, request.user_action, request.extra_params, request.social_platform, request.asset_type, request.asset_id);
        }

        public List<UserSocialActionObject> GetFriendsActions(GetFriendsActionsRequest request)
        {
            return ClientsManager.SocialClient().GetFriendsActions(request.site_guid, request.user_actions, request.asset_type, request.asset_id, request.page_number, request.page_size, request.social_platform);            
        }

        public List<UserSocialActionObject> GetUserActions(GetUserActionsRequest request)
        {
            throw new NotImplementedException();
            //return ClientsManager.SocialService().GetUserActions(request.site_guid, request.user_action, request.asset_type, request.asset_id, request.page_number, request.page_size, request.social_platform);            
        }

        public List<eSocialPrivacy> GetUserAllowedSocialPrivacyList(GetUserAllowedSocialPrivacyListRequest request)
        {
            return ClientsManager.SocialClient().GetUserAllowedSocialPrivacyList(request.site_guid);                            
        }

        public eSocialActionPrivacy GetUserExternalActionShare(GetUserExternalActionShareRequest request)
        {
            return ClientsManager.SocialClient().GetUserExternalActionShare(request.site_guid, request.user_action, request.social_platform);            
        }

        public List<string> GetUserFriends(GetUserFriendsRequest request)
        {
            return ClientsManager.SocialClient().GetUserFriends(request.site_guid);            
        }

        public eSocialActionPrivacy GetUserInternalActionPrivacy(GetUserInternalActionPrivacyRequest request)
        {
            return ClientsManager.SocialClient().GetUserInternalActionPrivacy(request.site_guid, request.user_action, request.social_platform);            
        }

        public eSocialPrivacy GetUserSocialPrivacy(GetUserSocialPrivacyRequest request)
        {
            return ClientsManager.SocialClient().GetUserSocialPrivacy(request.site_guid, request.social_platform, request.user_action);            
        }

        public bool SetUserExternalActionShare(SetUserExternalActionShareRequest request)
        {
            return ClientsManager.SocialClient().SetUserExternalActionShare(request.site_guid, request.user_action, request.social_platform, request.social_action_privacy);            
        }

        public bool SetUserInternalActionPrivacy(SetUserInternalActionPrivacyRequest request)
        {
            return ClientsManager.SocialClient().SetUserInternalActionPrivacy(request.site_guid, request.user_action, request.social_platform, request.social_action_privacy);            
        }

        public int AD_GetCustomDataID(AD_GetCustomDataIDRequest request)
        {
            return ClientsManager.ConditionalAccessClient().AD_GetCustomDataID(request.site_guid, request.price, request.currency_code, request.asset_id, request.ppv_module_code, request.campaign_code, request.coupon_code, request.payment_method, Utils.GetClientIP(), request.country_code, request.language_code, request.device_name, request.asset_type);
        }

        public int GetCustomDataID(GetCustomDataIDRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetCustomDataID(request.site_guid, request.price, request.currency_code, request.asset_id, request.ppv_module_code, request.campaign_code, request.coupon_code, request.payment_method, Utils.GetClientIP(), request.country_code, request.language_code, request.device_name, request.asset_type, request.override_end_date);            
        }

        public BillingResponse InApp_ChargeUserForMediaFile(InApp_ChargeUserForMediaFileRequest request)
        {
            return ClientsManager.ConditionalAccessClient().InApp_ChargeUserForMediaFile(request.site_guid, request.price, request.currency, request.product_code, request.ppv_module_code, request.InitObj.UDID, request.receipt);                            
        }

        public AdyenBillingDetail GetLastBillingTypeUserInfo(GetLastBillingTypeUserInfoRequest request)
        {
            return ClientsManager.BillingClient().GetLastBillingTypeUserInfo(request.site_guid);                            
        }

        public List<PermittedCollectionContainer> GetUserPermittedCollections(GetUserPermittedCollectionsRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetUserPermittedCollections(request.site_guid);
        }

        public ChangeSubscriptionStatus ChangeSubscription(ChangeSubscriptionRequest request)
        {
            return ClientsManager.ConditionalAccessClient().ChangeSubscription(request.site_guid, request.old_subscription, request.new_subscription);
        }

        public int CreatePurchaseToken(CreatePurchaseTokenRequest request)
        {
            return ClientsManager.ConditionalAccessClient().CreatePurchaseToken(request.site_guid, request.price, request.currency_code, request.asset_id, request.ppv_module_code, request.campaign_code, request.coupon_code, request.payment_method, Utils.GetClientIP(), request.country_code, request.language_code, request.device_name, request.asset_type, request.override_end_date, request.preview_module_id);
        }

        public string DummyChargeUserForCollection(DummyChargeUserForCollectionRequest request)
        {
            return ClientsManager.ConditionalAccessClient().DummyChargeUserForCollection(request.site_guid, request.collection_id, request.price, request.currency, request.coupon_code, Utils.GetClientIP(), request.extra_parameters, request.country_code, request.language_code, request.udid);
        }

        public BillingResponse ChargeUserForCollection(ChargeUserForCollectionRequest request)
        {
            return ClientsManager.ConditionalAccessClient().ChargeUserForCollection(request.site_guid, request.collection_code, request.price, request.currency, request.encrypted_cvv, request.coupon_code, Utils.GetClientIP(), request.extra_parameters, request.country_code, request.language_code, request.udid, request.payment_method_id);
        }
        
        public BillingResponse CellularChargeUserForSubscription(CellularChargeUserForSubscriptionRequest request)
        {
            return ClientsManager.ConditionalAccessClient().CellularChargeUserForSubscription(request.site_guid, request.price, request.currency, request.subscription_code, request.coupon_code, Utils.GetClientIP(), request.extra_parameters, request.country_code, request.language_code, request.udid);
        }

        public string ChargeUserForSubscriptionByPaymentMethod(ChargeUserForSubscriptionByPaymentMethodRequest request)
        {
            return ClientsManager.ConditionalAccessClient().ChargeUserForSubscriptionByPaymentMethod(request.site_guid, request.price, request.currency, request.subscription_code, request.coupon_code, Utils.GetClientIP(), request.extra_parameters, request.country_code, request.language_code, request.udid, request.payment_method_id, request.encrypted_cvv);
        }

        public string ChargeUserForMediaFileByPaymentMethod(ChargeUserForMediaFileByPaymentMethodRequest request)
        {
            return ClientsManager.ConditionalAccessClient().ChargeUserForMediaFileByPaymentMethod(request.price, request.currency, request.media_file_id, request.ppv_module_code, Utils.GetClientIP(), request.site_guid, request.udid, request.extra_parameters, request.payment_method_id, request.encrypted_cvv);
        }

        public string CellularChargeUserForMediaFile(CellularChargeUserForMediaFileRequest request)
        {
            return ClientsManager.ConditionalAccessClient().CellularChargeUserForMediaFileRequest(request.price, request.currency, request.media_file_id, request.ppv_module_code, Utils.GetClientIP(), request.site_guid, request.udid, request.extra_parameters, request.coupon_code, request.language_code, request.country_code);
        }

        public string ChargeUserForMediaFileUsingCC(ChargeUserForMediaFileUsingCCRequest request)
        {
            return ClientsManager.ConditionalAccessClient().ChargeUserForMediaFileUsingCC(request.price, request.currency, request.media_file_id, request.ppv_module_code, request.coupon_code, Utils.GetClientIP(), request.site_guid, request.udid, request.payment_method_id, request.encrypted_cvv);
        }

        public string ChargeUserForMediaSubscriptionUsingCC(ChargeUserForMediaSubscriptionUsingCCRequest request)
        {
            return ClientsManager.ConditionalAccessClient().ChargeUserForMediaSubscriptionUsingCC(request.price, request.currency, request.subscription_id, request.coupon_code, Utils.GetClientIP(), request.site_guid, request.udid, request.payment_method_id, request.encrypted_cvv, request.extra_parameters, request.country_code, request.language_code);
        }

        public UsersClient.LogInResponseData SignInWithToken(SignInWithTokenRequest request)
        {
            throw new NotImplementedException();
        }
        
        public List<PermittedCollectionContainer> GetUserExpiredCollections(GetUserExpiredCollectionsRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetUserExpiredCollections(request.site_guid, request.num_of_items);
        }

        /*public bool CancelTransaction(CancelTransactionRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).CancelTransaction(request.site_guid, request.asset_id, request.transaction_type, request.is_force);
        }*/

        public bool CancelTransaction(CancelTransactionRequest request)
        {
            return ClientsManager.ConditionalAccessClient().CancelTransaction(request.site_guid, request.asset_id, request.transaction_type, request.is_force);
        }

        public bool WaiverTransaction(WaiverTransactionRequest request)
        {
            return ClientsManager.ConditionalAccessClient().WaiverTransaction(request.site_guid, request.asset_id, request.transaction_type);
        }


        public UserResponseObject CheckTemporaryToken(CheckTemporaryTokenRequest request)
        {
            return ClientsManager.UsersClient().CheckTemporaryToken(request.token);
        }


        /*public Status CancelSubscriptionRenewal(CancelSubscriptionRenewalRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).CancelSubscriptionRenewal(request.domain_id, request.subscription_id);
        }*/


        public FBSignIn FBUserSignin(FBUserSigninRequest request)
        {
            return ClientsManager.SocialClient().FBUserSignin(request.token, request.ip, request.device_id, request.prevent_double_logins);
        }


        public BillingResponse DummyChargeUserForSubscription(DummyChargeUserForSubscriptionRequest request)
        {
            return ClientsManager.ConditionalAccessClient().DummyChargeUserForSubscription(request.price, request.currency, request.subscription_id, request.coupon_code, Utils.GetClientIP(), request.site_guid, request.extra_parameters, request.udid);
        }
    }
}