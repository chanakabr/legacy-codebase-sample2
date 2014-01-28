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

namespace RestfulTVPApi.ServiceInterface
{
    public class MediasRepository : IMediasRepository
    {
        public IEnumerable<Media> GetMediasInfo(InitializationObject initObj, List<int> MediaIDs, string picSize)
        {
            List<Media> retMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediasInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                retMedia = new TVPApiModule.CatalogLoaders.APIMediaLoader(MediaIDs, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), picSize, initObj.Locale.LocaleLanguage)
                {
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retMedia;
        }

        public IEnumerable<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaComments", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return CommentHelper.GetMediaComments(mediaID, groupID, pageSize, pageIndex);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool AddComment(InitializationObject initObj, int mediaID, int mediaType, string writer, string header, string subheader, string content, bool autoActive)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddComment", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return CommentHelper.SaveMediaComments(groupID, initObj.Platform, initObj.SiteGuid, initObj.UDID, initObj.Locale.LocaleLanguage, initObj.Locale.LocaleCountry, mediaID, writer, header, subheader, content, autoActive);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public MediaMarkObject GetMediaMark(InitializationObject initObj, int iMediaID)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiApiService _service = new ApiApiService(groupID, initObj.Platform);

                return _service.GetMediaMark(initObj.SiteGuid, iMediaID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public string MediaMark(InitializationObject initObj, action Action, int mediaType, int iMediaID, int iFileID, int iLocation)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ActionHelper.MediaMark(initObj, groupID, initObj.Platform, Action, mediaType, iMediaID, iFileID, iLocation);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public string MediaHit(InitializationObject initObj, int mediaType, int iMediaID, int iFileID, int iLocation)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ActionHelper.MediaHit(initObj, groupID, initObj.Platform, mediaType, iMediaID, iFileID, iLocation);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public IEnumerable<Media> GetRelatedMediasByTypes(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex, List<int> reqMediaTypes)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRelatedMediasByTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = new TVPApiModule.CatalogLoaders.APIRelatedMediaLoader(mediaID, reqMediaTypes, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), pageSize, pageIndex, picSize, initObj.Locale.LocaleLanguage)
                {
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public IEnumerable<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPeopleWhoWatched", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = new TVPApiModule.CatalogLoaders.APIPeopleWhoWatchedLoader(mediaID, 0, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), pageSize, pageIndex, picSize, initObj.Locale.LocaleLanguage)
                {
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public IEnumerable<Media> SearchMediaByAndOrList(InitializationObject initObj, List<KeyValue> orList, List<KeyValue> andList, int mediaType, int pageSize, int pageIndex, string picSize, bool exact, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir orderDir, string orderMetaName)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchMediaByAndOrList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = new APISearchMediaLoader(groupID, initObj.Platform, initObj.UDID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize, exact, orList, andList, new List<int>() { mediaType })
                {
                    OrderBy = orderBy,
                    OrderDir = orderDir,
                    OrderMetaMame = orderMetaName,
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public bool SendToFriend(InitializationObject initObj, int mediaID, string senderName, string senderEmail, string toEmail)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SendToFriend", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ActionHelper.SendToFriend(initObj, groupID, mediaID, senderName, senderEmail, toEmail);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public IEnumerable<string> GetAutoCompleteSearchList(InitializationObject initObj, string prefixText, int[] iMediaTypes)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetAutoCompleteSearchList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                List<string> lstRet = null;

                int maxItems = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.MovieFinder.MaxItems;
                string[] arrMetaNames = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Metadata.ToString().Split(new Char[] { ';' });
                string[] arrTagNames = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Tags.ToString().Split(new Char[] { ';' });

                List<string> lstResponse = new ApiApiService(groupID, initObj.Platform).GetAutoCompleteList(iMediaTypes != null ? iMediaTypes : new int[0], arrMetaNames, arrTagNames, prefixText, initObj.Locale.LocaleLanguage, 0, maxItems).ToList();

                if (lstResponse != null)
                {
                    lstRet = new List<String>();

                    foreach (String sTitle in lstResponse)
                    {
                        if (sTitle.ToLower().StartsWith(prefixText.ToLower())) lstRet.Add(sTitle);
                    }
                }

                return lstRet;
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public IEnumerable<int> GetSubscriptionIDsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionIDsContainingMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiPricingService _service = new ApiPricingService(groupId, initObj.Platform);

                return _service.GetSubscriptionIDsContainingMediaFile(iMediaID, iFileID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public IEnumerable<MediaFileItemPricesContainer> GetItemsPricesWithCoupons(InitializationObject initObj, string sSiteGUID, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetItemsPricesWithCoupons", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupId, initObj.Platform);

                return _service.GetItemsPricesWithCoupons(sSiteGUID, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public bool IsItemPurchased(InitializationObject initObj, string sSiteGUID, int iFileID)
        {
            bool bRet = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsItemPurchased", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                IImplementation impl = WSUtils.GetImplementation(groupId, initObj);

                bRet = impl.IsItemPurchased(iFileID, sSiteGUID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return bRet;
        }

        public bool IsUserSocialActionPerformed(InitializationObject initObj, string sSiteGUID, int nMediaID, int socialPlatform, int socialAction)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsUserSocialActionPerformed", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                List<Media> lstMedia = new APIUserSocialMediaLoader(sSiteGUID, socialAction, socialPlatform, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, 20, 0, "full")
                {
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;


                string sMediaID = nMediaID.ToString();

                bRet = (from r in lstMedia where r.media_id.Equals(sMediaID) select true).FirstOrDefault();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return bRet;
        }

        public string GetMediaLicenseLink(InitializationObject initObj, string sSiteGUID, int mediaFileID, string baseLink)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetMediaLicenseLink", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupId, initObj.Platform);

                return _service.GetMediaLicenseLink(sSiteGUID, mediaFileID, baseLink, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public PrePaidResponseStatus ChargeMediaWithPrepaid(InitializationObject initObj, string sSiteGUID, double price, string currency, int mediaFileID, string ppvModuleCode, string couponCode)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeMediaWithPrepaid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiConditionalAccessService _service = new ApiConditionalAccessService(groupId, initObj.Platform);

                return _service.PP_ChargeUserForMediaFile(sSiteGUID, price, currency, mediaFileID, ppvModuleCode, couponCode, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        //Ofir - Should DomainID be a param?
        public bool ActionDone(InitializationObject initObj, string sSiteGUID, TVPApi.ActionType action, int mediaID, int mediaType, int extraVal)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ActionDone", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ActionHelper.PerformAction(action, mediaID, mediaType, groupID, initObj.Platform, sSiteGUID, initObj.DomainID, initObj.UDID, extraVal);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public IEnumerable<string> GetUsersLikedMedia(InitializationObject initObj, string siteGuid, int mediaID, bool onlyFriends, int startIndex, int pageSize)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUsersLikedMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.GetUsersLikedMedia(siteGuid, mediaID, (int)TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform.FACEBOOK, onlyFriends, startIndex, pageSize);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }
    }
}