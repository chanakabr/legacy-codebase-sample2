using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.ServiceModel;
using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.Catalog;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Objects.Responses.Enums;
using RestfulTVPApi.Objects.Models;

namespace RestfulTVPApi.ServiceInterface
{
    public class MediasRepository : IMediasRepository
    {
        public List<Media> GetMediasInfo(GetMediasInfoRequest request)
        {
            List<Media> retMedia = null;

            //retMedia = new TVPApiModule.CatalogLoaders.APIMediaLoader(request.media_ids, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.pic_size, request.InitObj.Locale.LocaleLanguage)
            //    {
            //        UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
            //    }.Execute() as List<TVPApiModule.Objects.Responses.Media>;
            
            return retMedia;
        }

        public List<Comment> GetMediaComments(GetMediaCommentsRequest request)
        {
            List<Comment> retVal = null;
            //MediaComments mediaComments = (new MediaCommentsListLoader(mediaID, groupID, SiteHelper.GetClientIP(), pageSize, pageIndex)).Execute() as MediaComments;

            //if (mediaComments != null && mediaComments.commentsList.Count > 0)
            //{
            //    retVal = new List<Comment>();
            //    foreach (CommentContext context in mediaComments.commentsList)
            //    {
            //        retVal.Add(parseCommentContextToComment(context));
            //    }
            //}
            return retVal;            
        }

        public bool AddComment(AddCommentRequest request)
        {
            bool retVal = false;
            //int ilanguage = TextLocalizationManager.Instance.GetTextLocalization(groupID, platform).GetLanguageDBID(language);
            //CommentResponse response = new MediaCommentLoader(groupID, SiteHelper.GetClientIP(), ilanguage, siteGuid, udid, mediaId, content, country, header, subHeader, writer, autoActive).Execute() as CommentResponse;
            //retVal = response != null ? response.eStatusComment == StatusComment.SUCCESS : false;
            return retVal; 
        }

        public MediaMarkObject GetMediaMark(GetMediaMarkRequest request)
        {
            return ClientsManager.ApiClient().GetMediaMark(request.InitObj.SiteGuid, request.media_id);
        }

        public string MediaMark(RestfulTVPApi.ServiceModel.MediaMarkRequest request)
        {
            //return ActionHelper.MediaMark(request.InitObj, request.GroupID, request.InitObj.Platform, request.action, request.media_type, request.media_id, request.media_file_id, request.location);            
            return ClientsManager.CatalogClient().MediaMark(request.GroupID, (RestfulTVPApi.Objects.Enums.PlatformType)request.InitObj.Platform, request.InitObj.SiteGuid, request.InitObj.UDID, 0, request.media_id, 0, request.location, string.Empty,
                string.Empty, string.Empty, string.Empty, request.action.ToString(), 0, 0, 0);
        }

        public string MediaHit(RestfulTVPApi.ServiceModel.MediaHitRequest request)
        {
            //return new TVPPro.SiteManager.CatalogLoaders.MediaHitLoader(groupID, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID, iMediaID, iFileID, 0, 0, iLocation, 0, string.Empty, string.Empty)
            //{
            //    Platform = platform.ToString()
            //}.Execute() as string;
            
            return null;
        }

        public List<Media> GetRelatedMediasByTypes(GetRelatedMediasByTypesRequest request)
        {
            List<Media> lstMedia = null;

            //lstMedia = new TVPApiModule.CatalogLoaders.APIRelatedMediaLoader(request.media_id, request.media_types, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.page_size, request.page_number, request.pic_size, request.InitObj.Locale.LocaleLanguage)
            //    {
            //        UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
            //    }.Execute() as List<TVPApiModule.Objects.Responses.Media>;            

            return lstMedia;
        }

        public List<Media> GetPeopleWhoWatched(GetPeopleWhoWatchedRequest request)
        {
            List<Media> lstMedia = null;

            //lstMedia = new TVPApiModule.CatalogLoaders.APIPeopleWhoWatchedLoader(request.media_id, 0, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.page_size, request.page_number, request.pic_size, request.InitObj.Locale.LocaleLanguage)
            //    {
            //        UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
            //    }.Execute() as List<TVPApiModule.Objects.Responses.Media>;
            
            return lstMedia;
        }

        public List<Media> SearchMediaByAndOrList(SearchMediaByAndOrListRequest request)
        {
            List<Media> lstMedia = null;

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
            return ClientsManager.ApiClient().SendToFriend(request.sender_name, request.sender_email, request.to_email, request.media_id);             
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
            return ClientsManager.PricingClient().GetSubscriptionIDsContainingMediaFile(request.media_id, request.media_file_id);            
        }

        public List<RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer> GetItemsPricesWithCoupons(GetItemsPricesWithCouponsRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetItemsPricesWithCoupons(request.site_guid, request.media_file_ids, request.site_guid, request.coupon_code, request.only_lowest, request.country_code, request.language_code, request.device_name);
        }

        public bool IsItemPurchased(IsItemPurchasedRequest request)
        {
            bool bRet = false;

            IEnumerable<MediaFileItemPricesContainer> prices = ClientsManager.ConditionalAccessClient().GetItemsPrice(new int[] { request.media_file_id }, request.site_guid, true);

            MediaFileItemPricesContainer mediaPrice = null;
            foreach (MediaFileItemPricesContainer mp in prices)
            {
                if (mp.media_file_id == request.media_file_id)
                {
                    mediaPrice = mp;
                    break;
                }
            }

            if (mediaPrice != null && mediaPrice.item_prices != null && mediaPrice.item_prices.Length > 0)
            {
                PriceReason priceReason = (PriceReason)mediaPrice.item_prices[0].price_reason;

                bRet = mediaPrice.item_prices[0].price.price == 0 &&
                       (priceReason == PriceReason.PPVPurchased ||
                        priceReason == PriceReason.SubscriptionPurchased ||
                        priceReason == PriceReason.PrePaidPurchased ||
                        priceReason == PriceReason.Free);
            }
            else
            {
                bRet = true;
            }

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
            return ClientsManager.ConditionalAccessClient().GetMediaLicenseLink(request.site_guid, request.media_file_id, request.base_link, request.InitObj.UDID);            
        }

        public PrePaidResponseStatus ChargeMediaWithPrepaid(ChargeMediaWithPrepaidRequest request)
        {
            return ClientsManager.ConditionalAccessClient().PP_ChargeUserForMediaFile(request.InitObj.SiteGuid, request.price, request.currency, request.media_file_id, request.ppv_module_code, request.coupon_code, request.InitObj.UDID);                            
        }

        //Ofir - Should DomainID be a param?
        public bool ActionDone(ActionDoneRequest request)
        {
            bool retVal = false;
            string isOfflineSync="";// = ConfigurationManager.AppSettings[string.Concat(groupID, "_OfflineFavoriteSync")];

            switch (request.action_type)
            {
                case RestfulTVPApi.Objects.Enums.ActionType.AddFavorite:
                    {
                        if (!string.IsNullOrEmpty(isOfflineSync))
                            ClientsManager.UsersClient().AddUserOfflineMedia(request.InitObj.SiteGuid, request.media_id);

                        retVal = ClientsManager.UsersClient().AddUserFavorite(request.InitObj.SiteGuid, request.InitObj.DomainID, request.InitObj.UDID, request.media_type.ToString(), request.media_id.ToString(), request.extra_val.ToString());
                        break;
                    }
                case RestfulTVPApi.Objects.Enums.ActionType.RemoveFavorite:
                    {
                        ClientsManager.UsersClient().RemoveUserFavorite(request.InitObj.SiteGuid, new int[] { request.media_id });
                        retVal = true;

                        if (!string.IsNullOrEmpty(isOfflineSync))
                            ClientsManager.UsersClient().RemoveUserOfflineMedia(request.InitObj.SiteGuid, request.media_id);
                        break;
                    }
                case RestfulTVPApi.Objects.Enums.ActionType.Rate:
                    {
                        //var res = ClientsManager.UsersClient().RateMedia(request.InitObj.SiteGuid, request.media_id, request.extra_val);
                        //retVal = res.oStatus != null && res.oStatus.m_nStatusCode == 0;
                        break;
                    }
                case RestfulTVPApi.Objects.Enums.ActionType.Vote:
                    {
                        //string sRet = TVPApiModule.Helper.VotesHelper.UserVote(mediaID.ToString(), sUserID, platform, groupID);
                        //retVal = sRet.Equals("Success");
                        break;
                    }
                default:
                    break;
            }
            return retVal;
        }

        public List<string> GetUsersLikedMedia(GetUsersLikedMediaRequest request)
        {
            return ClientsManager.SocialClient().GetUsersLikedMedia(request.site_guid, request.media_id, (int)SocialPlatform.FACEBOOK, request.only_friends, request.page_number, request.page_size);                            
        }


        public BuzzScore GetBuzzMeterData(GetBuzzMeterDataRequest request)
        {
            //return new BuzzMeterLoader(request.GroupID, request.media_id).Execute() as TVPApiModule.Objects.Responses.BuzzWeightedAverScore;
            return null;
        }

        public List<Media> GetChannelMultiFilter(GetChannelMultiFilterRequest request)
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

        public List<AssetStats> GetAssetsStats(GetAssetsStatsRequest request)
        {
            List<AssetStats> retVal = null;

            try
            {
                retVal = ClientsManager.CatalogClient().GetAssetsStats(request.GroupID, request.InitObj.Platform, request.InitObj.SiteGuid, request.InitObj.UDID,
                    request.asset_ids, Utils.ConvertToUnixTimestamp(DateTime.MinValue), Utils.ConvertToUnixTimestamp(DateTime.MaxValue), request.asset_type); 
            }
            catch (Exception ex)
            {
                HttpContext.Current.Items.Add("Error", ex);
            }

            return retVal;
        }


        public List<AssetStats> GetAssetsStatsForTimePeriod(GetAssetsStatsForTimePeriodRequest request)
        {
            List<AssetStats> retVal = null;

            try
            {
                retVal = ClientsManager.CatalogClient().GetAssetsStats(request.GroupID, request.InitObj.Platform, request.InitObj.SiteGuid, request.InitObj.UDID,
                    request.asset_ids, Utils.ConvertToUnixTimestamp(request.start_time), Utils.ConvertToUnixTimestamp(request.end_time), request.asset_type);
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

        public List<Media> GetBundleMedia(GetBundleMediaRequest request)
        {
            List<Media> lstMedia = null;

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