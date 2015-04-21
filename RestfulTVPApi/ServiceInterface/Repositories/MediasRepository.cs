using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Services;
using TVPApiModule.Interfaces;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Objects;
using TVPApiModule.Manager;
using TVPApiModule.Helper;
using TVPApiModule.Context;
using RestfulTVPApi.ServiceModel;
using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.Catalog;
using RestfulTVPApi.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public class MediasRepository : IMediasRepository
    {
        public List<TVPApiModule.Objects.Responses.Media> GetMediasInfo(GetMediasInfoRequest request)
        {
            List<TVPApiModule.Objects.Responses.Media> retMedia = null;

            //retMedia = new TVPApiModule.CatalogLoaders.APIMediaLoader(request.media_ids, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.pic_size, request.InitObj.Locale.LocaleLanguage)
            //    {
            //        UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
            //    }.Execute() as List<TVPApiModule.Objects.Responses.Media>;
            
            return retMedia;
        }

        public List<TVPApiModule.Objects.Responses.Comment> GetMediaComments(GetMediaCommentsRequest request)
        {
            return CommentHelper.GetMediaComments(request.media_id, request.GroupID, request.page_size, request.page_number);            
        }

        public bool AddComment(AddCommentRequest request)
        {
            return CommentHelper.SaveMediaComments(request.GroupID, request.InitObj.Platform, request.InitObj.SiteGuid, request.InitObj.UDID, request.InitObj.Locale.LocaleLanguage, request.InitObj.Locale.LocaleCountry, request.media_id, request.writer, request.header, request.sub_header, request.content, request.auto_active);            
        }

        public TVPApiModule.Objects.Responses.MediaMarkObject GetMediaMark(GetMediaMarkRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).GetMediaMark(request.InitObj.SiteGuid, request.media_id);
        }

        public string MediaMark(RestfulTVPApi.ServiceModel.MediaMarkRequest request)
        {
            //return ActionHelper.MediaMark(request.InitObj, request.GroupID, request.InitObj.Platform, request.action, request.media_type, request.media_id, request.media_file_id, request.location);            
            return ClientsManager.CatalogClient().MediaMark(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform, request.InitObj.SiteGuid, request.InitObj.UDID, 0, request.media_id, 0, request.location, string.Empty,
                string.Empty, string.Empty, string.Empty, request.action.ToString(), 0, 0, 0);
        }

        public string MediaHit(RestfulTVPApi.ServiceModel.MediaHitRequest request)
        {
            return ActionHelper.MediaHit(request.InitObj, request.GroupID, request.InitObj.Platform, request.media_type, request.media_id, request.media_file_id, request.location);            
        }

        public List<TVPApiModule.Objects.Responses.Media> GetRelatedMediasByTypes(GetRelatedMediasByTypesRequest request)
        {
            List<TVPApiModule.Objects.Responses.Media> lstMedia = null;

            //lstMedia = new TVPApiModule.CatalogLoaders.APIRelatedMediaLoader(request.media_id, request.media_types, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.page_size, request.page_number, request.pic_size, request.InitObj.Locale.LocaleLanguage)
            //    {
            //        UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
            //    }.Execute() as List<TVPApiModule.Objects.Responses.Media>;            

            return lstMedia;
        }

        public List<TVPApiModule.Objects.Responses.Media> GetPeopleWhoWatched(GetPeopleWhoWatchedRequest request)
        {
            List<TVPApiModule.Objects.Responses.Media> lstMedia = null;

            //lstMedia = new TVPApiModule.CatalogLoaders.APIPeopleWhoWatchedLoader(request.media_id, 0, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.page_size, request.page_number, request.pic_size, request.InitObj.Locale.LocaleLanguage)
            //    {
            //        UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
            //    }.Execute() as List<TVPApiModule.Objects.Responses.Media>;
            
            return lstMedia;
        }

        public List<TVPApiModule.Objects.Responses.Media> SearchMediaByAndOrList(SearchMediaByAndOrListRequest request)
        {
            List<TVPApiModule.Objects.Responses.Media> lstMedia = null;

            //lstMedia = new APISearchMediaLoader(request.GroupID, request.InitObj.Platform, request.InitObj.UDID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP(), request.InitObj.Locale.LocaleLanguage, request.page_size, request.page_number, request.pic_size, request.exact, request.or_list, request.and_list, new List<int>() { request.media_type })
            //    {
            //        OrderBy = request.order_by,
            //        OrderDir = request.order_dir,
            //        OrderMetaMame = request.order_meta_name,
            //        UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
            //    }.Execute() as List<TVPApiModule.Objects.Responses.Media>;            

            return lstMedia;
        }

        public bool SendToFriend(SendToFriendRequest request)
        {
            return ActionHelper.SendToFriend(request.InitObj, request.GroupID, request.media_id, request.sender_name, request.sender_email, request.to_email);            
        }

        public List<string> GetAutoCompleteSearchList(GetAutoCompleteSearchListRequest request)
        {
            List<string> lstRet = null;
            //Deprecated! 
            //int maxItems = ConfigManager.GetInstance().GetConfig(request.GroupID, request.InitObj.Platform).SiteConfiguration.Data.Features.MovieFinder.MaxItems;
            //string[] arrMetaNames = ConfigManager.GetInstance().GetConfig(request.GroupID, request.InitObj.Platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Metadata.ToString().Split(new Char[] { ';' });
            //string[] arrTagNames = ConfigManager.GetInstance().GetConfig(request.GroupID, request.InitObj.Platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Tags.ToString().Split(new Char[] { ';' });

            //List<string> lstResponse = ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).GetAutoCompleteList(request.media_types != null ? request.media_types : new int[0], arrMetaNames, arrTagNames, request.prefix_text, request.InitObj.Locale.LocaleLanguage, 0, maxItems).ToList();

            //if (lstResponse != null)
            //{
            //    lstRet = new List<String>();

            //    foreach (String sTitle in lstResponse)
            //    {
            //        if (sTitle.ToLower().StartsWith(request.prefix_text.ToLower()))
            //            lstRet.Add(sTitle);
            //    }
            //}

            return lstRet;
        }

        public List<int> GetSubscriptionIDsContainingMediaFile(GetSubscriptionIDsContainingMediaFileRequest request)
        {
            return ServicesManager.PricingService(request.GroupID, request.InitObj.Platform).GetSubscriptionIDsContainingMediaFile(request.media_id, request.media_file_id);            
        }

        public List<RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer> GetItemsPricesWithCoupons(GetItemsPricesWithCouponsRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetItemsPricesWithCoupons(request.site_guid, request.media_file_ids, request.site_guid, request.coupon_code, request.only_lowest, request.country_code, request.language_code, request.device_name);
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

            //List<TVPApiModule.Objects.Responses.Media> lstMedia = new APIUserSocialMediaLoader(request.site_guid, request.social_action, request.social_platform, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.InitObj.Locale.LocaleLanguage, 20, 0, "full")
            //    {
            //        UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
            //    }.Execute() as List<TVPApiModule.Objects.Responses.Media>;


            //string sMediaID = request.media_id.ToString();

            //bRet = (from r in lstMedia where r.media_id.Equals(sMediaID) select true).FirstOrDefault();
            
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
            return ServicesManager.SocialService(request.GroupID, request.InitObj.Platform).GetUsersLikedMedia(request.site_guid, request.media_id, (int)SocialPlatform.FACEBOOK, request.only_friends, request.page_number, request.page_size);                            
        }


        public TVPApiModule.Objects.Responses.BuzzWeightedAverScore GetBuzzMeterData(GetBuzzMeterDataRequest request)
        {
            //return new BuzzMeterLoader(request.GroupID, request.media_id).Execute() as TVPApiModule.Objects.Responses.BuzzWeightedAverScore;
            return null;
        }

        public List<TVPApiModule.Objects.Responses.Media> GetChannelMultiFilter(GetChannelMultiFilterRequest request)
        {
            try
            {
                //return new APIChannelMediaLoader(request.channel_id, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.page_size, request.page_number, request.pic_size, request.InitObj.Locale.LocaleLanguage, null, request.tags_metas, request.cut_with)
                //{
                //    UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
                //}.Execute() as List<TVPApiModule.Objects.Responses.Media>;
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public List<TVPApiModule.Objects.Responses.AssetStatsResult> GetAssetsStats(GetAssetsStatsRequest request)
        {
            List<TVPApiModule.Objects.Responses.AssetStatsResult> retVal = null;

            try
            {
                //retVal = new AssetStatsLoader(request.GroupID, SiteHelper.GetClientIP(), request.page_size, request.page_number, request.asset_ids, request.asset_type, DateTime.MinValue, DateTime.MaxValue)
                //{
                //    Platform = request.InitObj.Platform.ToString(),
                //    DeviceId = request.InitObj.UDID,
                //    SiteGuid = request.InitObj.SiteGuid
                //}.Execute() as List<TVPApiModule.Objects.Responses.AssetStatsResult>;
            }
            catch (Exception ex)
            {
                HttpContext.Current.Items.Add("Error", ex);
            }

            return retVal;
        }


        public List<TVPApiModule.Objects.Responses.AssetStatsResult> GetAssetsStatsForTimePeriod(GetAssetsStatsForTimePeriodRequest request)
        {
            List<TVPApiModule.Objects.Responses.AssetStatsResult> retVal = null;

            try
            {
                //retVal = new AssetStatsLoader(request.GroupID, SiteHelper.GetClientIP(), request.page_size, request.page_number, request.asset_ids, request.asset_type, request.start_time, request.end_time)
                //{
                //    Platform = request.InitObj.Platform.ToString(),
                //    DeviceId = request.InitObj.UDID
                //}.Execute() as List<TVPApiModule.Objects.Responses.AssetStatsResult>;
            }
            catch (Exception ex)
            {
                HttpContext.Current.Items.Add("Error", ex);
            }

            return retVal;
        }

        public bool DoesBundleContainMedia(DoesBundleContainMediaRequest request)
        {
            bool isMediaInBundle = false;

            try
            {
                ////Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderObj orderObj = new Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderObj() { m_eOrderDir = orderDir, m_eOrderBy = orderBy };
                //BundleContainingMediaLoader loader = new BundleContainingMediaLoader()
                //{
                //    BundleID = request.bundle_id,
                //    BundleType = request.bundle_type,
                //    MediaID = request.media_id,
                //    GroupID = request.GroupID,
                //    MediaType = request.media_type,
                //};

                //isMediaInBundle = (bool)loader.Execute();
            }
            catch (Exception ex)
            {
                HttpContext.Current.Items.Add("Error", ex);
            }

            return isMediaInBundle;
        }

        public List<TVPApiModule.Objects.Responses.Media> GetBundleMedia(GetBundleMediaRequest request)
        {
            List<TVPApiModule.Objects.Responses.Media> lstMedia = null;

            try
            {
                OrderObj orderObj = new OrderObj() { m_eOrderDir = request.order_dir, m_eOrderBy = (RestfulTVPApi.Catalog.OrderBy)request.order_by };
                //APIBundleMediaLoader loader = new APIBundleMediaLoader(request.bundle_id, request.media_type, orderObj, request.GroupID, request.GroupID, request.InitObj.Platform, SiteHelper.GetClientIP(), request.InitObj.Locale.LocaleLanguage, string.Empty, request.page_number, request.page_size, request.bundle_type);
                
                //lstMedia = loader.Execute() as List<TVPApiModule.Objects.Responses.Media>;                
            }
            catch (Exception ex)
            {
                HttpContext.Current.Items.Add("Error", ex);
            }

            return lstMedia;
        }

        public SearchAssetsResponse SearchAssets(SearchAssetsRequest request)
        {
            SearchAssetsResponse response = null;

            try
            {
                response = ClientsManager.CatalogClient().SearchAssets(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform, 
                    request.InitObj.SiteGuid, request.InitObj.UDID,
                    RestfulTVPApi.Clients.Utils.Utils.ConvertLocaleLanguageToInt(request.GroupID, request.InitObj.Locale.LocaleLanguage),
                   request.page_number, request.page_size, request.filter, request.order_by, request.filter_types, request.with);
            }
            catch (Exception ex)
            {
                throw new Exception("Error While getting response from client");
            }

            return response;
        }

    }
}