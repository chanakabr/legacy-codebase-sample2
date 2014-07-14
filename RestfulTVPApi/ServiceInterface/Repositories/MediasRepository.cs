using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Services;
using TVPApiModule.Interfaces;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Objects.Responses;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApiModule.Objects;
using TVPApiModule.Manager;
using TVPApiModule.Helper;
using TVPApiModule.Context;
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi.ServiceInterface
{
    public class MediasRepository : IMediasRepository
    {
        public List<Media> GetMediasInfo(GetMediasInfoRequest request)
        {
            List<Media> retMedia = null;

            retMedia = new TVPApiModule.CatalogLoaders.APIMediaLoader(request.media_ids, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.pic_size, request.InitObj.Locale.LocaleLanguage)
                {
                    UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
                }.Execute() as List<Media>;
            
            return retMedia;
        }

        public List<Comment> GetMediaComments(GetMediaCommentsRequest request)
        {
            return CommentHelper.GetMediaComments(request.media_id, request.GroupID, request.page_size, request.page_number);            
        }

        public bool AddComment(AddCommentRequest request)
        {
            return CommentHelper.SaveMediaComments(request.GroupID, request.InitObj.Platform, request.InitObj.SiteGuid, request.InitObj.UDID, request.InitObj.Locale.LocaleLanguage, request.InitObj.Locale.LocaleCountry, request.media_id, request.writer, request.header, request.sub_header, request.content, request.auto_active);            
        }

        public MediaMarkObject GetMediaMark(GetMediaMarkRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).GetMediaMark(request.InitObj.SiteGuid, request.media_id);
        }

        public string MediaMark(RestfulTVPApi.ServiceModel.MediaMarkRequest request)
        {
            return ActionHelper.MediaMark(request.InitObj, request.GroupID, request.InitObj.Platform, request.action, request.media_type, request.media_id, request.media_file_id, request.location);            
        }

        public string MediaHit(RestfulTVPApi.ServiceModel.MediaHitRequest request)
        {
            return ActionHelper.MediaHit(request.InitObj, request.GroupID, request.InitObj.Platform, request.media_type, request.media_id, request.media_file_id, request.location);            
        }

        public List<Media> GetRelatedMediasByTypes(GetRelatedMediasByTypesRequest request)
        {
            List<Media> lstMedia = null;

            lstMedia = new TVPApiModule.CatalogLoaders.APIRelatedMediaLoader(request.media_id, request.media_types, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.page_size, request.page_number, request.pic_size, request.InitObj.Locale.LocaleLanguage)
                {
                    UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
                }.Execute() as List<Media>;            

            return lstMedia;
        }

        public List<Media> GetPeopleWhoWatched(GetPeopleWhoWatchedRequest request)
        {
            List<Media> lstMedia = null;

            lstMedia = new TVPApiModule.CatalogLoaders.APIPeopleWhoWatchedLoader(request.media_id, 0, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.page_size, request.page_number, request.pic_size, request.InitObj.Locale.LocaleLanguage)
                {
                    UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
                }.Execute() as List<Media>;
            
            return lstMedia;
        }

        public List<Media> SearchMediaByAndOrList(SearchMediaByAndOrListRequest request)
        {
            List<Media> lstMedia = null;

            lstMedia = new APISearchMediaLoader(request.GroupID, request.InitObj.Platform, request.InitObj.UDID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP(), request.InitObj.Locale.LocaleLanguage, request.page_size, request.page_number, request.pic_size, request.exact, request.or_list, request.and_list, new List<int>() { request.media_type })
                {
                    OrderBy = request.order_by,
                    OrderDir = request.order_dir,
                    OrderMetaMame = request.order_meta_name,
                    UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
                }.Execute() as List<Media>;            

            return lstMedia;
        }

        public bool SendToFriend(SendToFriendRequest request)
        {
            return ActionHelper.SendToFriend(request.InitObj, request.GroupID, request.media_id, request.sender_name, request.sender_email, request.to_email);            
        }

        public List<string> GetAutoCompleteSearchList(GetAutoCompleteSearchListRequest request)
        {
            List<string> lstRet = null;

            int maxItems = ConfigManager.GetInstance().GetConfig(request.GroupID, request.InitObj.Platform).SiteConfiguration.Data.Features.MovieFinder.MaxItems;
            string[] arrMetaNames = ConfigManager.GetInstance().GetConfig(request.GroupID, request.InitObj.Platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Metadata.ToString().Split(new Char[] { ';' });
            string[] arrTagNames = ConfigManager.GetInstance().GetConfig(request.GroupID, request.InitObj.Platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Tags.ToString().Split(new Char[] { ';' });

            List<string> lstResponse = ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).GetAutoCompleteList(request.media_types != null ? request.media_types : new int[0], arrMetaNames, arrTagNames, request.prefix_text, request.InitObj.Locale.LocaleLanguage, 0, maxItems).ToList();

            if (lstResponse != null)
            {
                lstRet = new List<String>();

                foreach (String sTitle in lstResponse)
                {
                    if (sTitle.ToLower().StartsWith(request.prefix_text.ToLower()))
                        lstRet.Add(sTitle);
                }
            }

            return lstRet;
        }

        public List<int> GetSubscriptionIDsContainingMediaFile(GetSubscriptionIDsContainingMediaFileRequest request)
        {
            return ServicesManager.PricingService(request.GroupID, request.InitObj.Platform).GetSubscriptionIDsContainingMediaFile(request.media_id, request.media_file_id);            
        }

        public List<MediaFileItemPricesContainer> GetItemsPricesWithCoupons(GetItemsPricesWithCouponsRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetItemsPricesWithCoupons(request.site_guid, request.media_file_ids, request.site_guid, request.coupon_code, request.only_lowest, request.country_code, request.language_code, request.device_name);
        }

        public bool IsItemPurchased(IsItemPurchasedRequest request)
        {
            bool bRet = false;

            IImplementation impl = WSUtils.GetImplementation(request.GroupID, request.InitObj);

            bRet = impl.IsItemPurchased(request.media_file_id, request.site_guid);
            
            return bRet;
        }

        public bool IsUserSocialActionPerformed(IsUserSocialActionPerformedRequest request)
        {
            bool bRet = false;

            List<Media> lstMedia = new APIUserSocialMediaLoader(request.site_guid, request.social_action, request.social_platform, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.InitObj.Locale.LocaleLanguage, 20, 0, "full")
                {
                    UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
                }.Execute() as List<Media>;


                string sMediaID = request.media_id.ToString();

                bRet = (from r in lstMedia where r.media_id.Equals(sMediaID) select true).FirstOrDefault();
            
            return bRet;
        }

        public string GetMediaLicenseLink(GetMediaLicenseLinkRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetMediaLicenseLink(request.site_guid, request.media_file_id, request.base_link, request.InitObj.UDID);            
        }

        public PrePaidResponseStatus ChargeMediaWithPrepaid(ChargeMediaWithPrepaidRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).PP_ChargeUserForMediaFile(request.InitObj.SiteGuid, request.price, request.currency, request.media_file_id, request.ppv_module_code, request.coupon_code, request.InitObj.UDID);                            
        }

        //Ofir - Should DomainID be a param?
        public bool ActionDone(ActionDoneRequest request)
        {
            return ActionHelper.PerformAction(request.action_type, request.media_id, request.media_type, request.GroupID, request.InitObj.Platform, request.InitObj.SiteGuid, request.InitObj.DomainID, request.InitObj.UDID, request.extra_val);            
        }

        public List<string> GetUsersLikedMedia(GetUsersLikedMediaRequest request)
        {
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetUsersLikedMedia(request.site_guid, request.media_id, (int)TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform.FACEBOOK, request.only_friends, request.page_number, request.page_size);                            
        }
    }
}