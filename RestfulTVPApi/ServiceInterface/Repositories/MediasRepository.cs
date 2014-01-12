using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPPro.SiteManager.Helper;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApiModule.Interfaces;
using TVPApiModule.CatalogLoaders;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace RestfulTVPApi.ServiceInterface
{
    public class MediasRepository : IMediasRepository
    {
        //Ofir - Should udid, LocaleLanguage be a param?
        public List<Media> GetMediasInfo(InitializationObject initObj, List<int> MediaIDs, string picSize)
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

        public List<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex)
        {
            List<Comment> lstComment = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaComments", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstComment = CommentHelper.GetMediaComments(mediaID, groupID, pageSize, pageIndex);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstComment;
        }

        //Ofir - Should udid, LocaleLanguage, LocaleCountry be a param?
        public bool AddComment(InitializationObject initObj, int mediaID, int mediaType, string writer, string header, string subheader, string content, bool autoActive)
        {
            bool retVal = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddComment", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                retVal = CommentHelper.SaveMediaComments(groupID, initObj.Platform, initObj.SiteGuid, initObj.UDID, initObj.Locale.LocaleLanguage, initObj.Locale.LocaleCountry, mediaID, writer, header, subheader, content, autoActive);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retVal;
        }

        public TVPPro.SiteManager.TvinciPlatform.api.MediaMarkObject GetMediaMark(InitializationObject initObj, int iMediaID)
        {
            TVPPro.SiteManager.TvinciPlatform.api.MediaMarkObject mediaMark = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                mediaMark = new ApiApiService(groupID, initObj.Platform).GetMediaMark(initObj.SiteGuid, iMediaID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return mediaMark;
        }

        public string MediaMark(InitializationObject initObj, action Action, int mediaType, int iMediaID, int iFileID, int iLocation)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                sRet = ActionHelper.MediaMark(initObj, groupID, initObj.Platform, Action, mediaType, iMediaID, iFileID, iLocation);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return sRet;
        }

        public string MediaHit(InitializationObject initObj, int mediaType, int iMediaID, int iFileID, int iLocation)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                sRet = ActionHelper.MediaHit(initObj, groupID, initObj.Platform, mediaType, iMediaID, iFileID, iLocation);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return sRet;
        }

        //Ofir - Should udid, LocaleLanguage be a param?
        public List<Media> GetRelatedMediasByTypes(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex, List<int> reqMediaTypes)
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

        //Ofir - Should udid, LocaleLanguage be a param?
        public List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex)
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

        //Ofir - Should SiteGuid, DomainID be a param?
        public List<KeyValuePair<int, bool>> AreMediasFavorite(InitializationObject initObj, List<int> mediaIds)
        {
            List<KeyValuePair<int, bool>> result = new List<KeyValuePair<int, bool>>();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AreMediasFavorite", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                FavoritObject[] favoriteObjects = new ApiUsersService(groupID, initObj.Platform).GetUserFavorites(initObj.SiteGuid, string.Empty, initObj.DomainID, string.Empty);

                if (favoriteObjects != null)
                    result = mediaIds.Select(y => new KeyValuePair<int, bool>(y, favoriteObjects.Where(x => x.m_sItemCode == y.ToString()).Count() > 0)).ToList();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return result;
        }

        //Ofir - Moved from Player.
        //Ofir - Should udid be a param?
        public string GetMediaLicenseLink(InitializationObject initObj, int mediaFileID, string baseLink)
        {
            string sResponse = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetMediaLicenseLink", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                sResponse = new ApiConditionalAccessService(groupId, initObj.Platform).GetMediaLicenseLink(initObj.SiteGuid, mediaFileID, baseLink, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return sResponse;
        }

        //Ofir - Should sSiteGUID be a param?
        public bool IsItemPurchased(InitializationObject initObj, int iFileID, string sSiteGUID)
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

        //Ofir - Should SiteGuid, udid be a param?
        public PrePaidResponseStatus ChargeMediaWithPrepaid(InitializationObject initObj, double price, string currency, int mediaFileID, string ppvModuleCode, string couponCode)
        {
            PrePaidResponseStatus oResponse = PrePaidResponseStatus.UnKnown;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeMediaWithPrepaid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                oResponse = new ApiConditionalAccessService(groupId, initObj.Platform).PP_ChargeUserForMediaFile(initObj.SiteGuid, price, currency, mediaFileID, ppvModuleCode, couponCode, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return oResponse;
        }

        //Moved from CRM
        //Ofir - Should SiteGuid, udid be a param?
        public string DummyChargeUserForMediaFile(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon)
        {
            string response = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DummyChargeUserForMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                response = new ApiConditionalAccessService(groupId, initObj.Platform).DummyChargeUserForMediaFile(iPrice, sCurrency, iFileID, sPPVModuleCode, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        //Ofir - Should SiteGuid, udid, LocaleLanguage be a param?
        public List<Media> GetRecommendedMediasByTypes(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int[] reqMediaTypes)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRecommendedMediasByTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = new TVPApiModule.CatalogLoaders.APIPersonalRecommendedLoader(initObj.SiteGuid, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize)
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

        //Ofir - Should SiteGuid, udid, domianID be a param?
        public bool ActionDone(InitializationObject initObj, TVPApi.ActionType action, int mediaID, int mediaType, int extraVal)
        {
            bool retVal = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ActionDone", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                retVal = ActionHelper.PerformAction(action, mediaID, mediaType, groupID, initObj.Platform, initObj.SiteGuid, initObj.DomainID, initObj.UDID, extraVal);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retVal;
        }

        //Ofir - Should udid, LocaleLanguage be a param?
        public List<Media> SearchMediaByAndOrList(InitializationObject initObj, List<KeyValue> orList, List<KeyValue> andList, int mediaType, int pageSize, int pageIndex, string picSize, bool exact, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir orderDir, string orderMetaName)
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
            bool retVal = false;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SendToFriend", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                retVal = ActionHelper.SendToFriend(initObj, groupID, mediaID, senderName, senderEmail, toEmail);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retVal;
        }

        //Ofir - Should LocaleLanguage be a param?
        public string[] GetAutoCompleteSearchList(InitializationObject initObj, string prefixText, int[] iMediaTypes)
        {
            string[] retVal = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetAutoCompleteSearchList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                List<string> lstRet = new List<String>();

                int maxItems = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.MovieFinder.MaxItems;
                string[] arrMetaNames = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Metadata.ToString().Split(new Char[] { ';' });
                string[] arrTagNames = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Tags.ToString().Split(new Char[] { ';' });

                List<string> lstResponse = new ApiApiService(groupID, initObj.Platform).GetAutoCompleteList(iMediaTypes != null ? iMediaTypes : new int[0], arrMetaNames, arrTagNames, prefixText, initObj.Locale.LocaleLanguage, 0, maxItems).ToList();

                foreach (String sTitle in lstResponse)
                {
                    if (sTitle.ToLower().StartsWith(prefixText.ToLower())) lstRet.Add(sTitle);
                }
                retVal = lstRet.ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retVal;
        }

        //Moved from Pricing
        public int[] GetSubscriptionIDsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID)
        {
            int[] subIds = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionIDsContainingMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                subIds = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionIDsContainingMediaFile(iMediaID, iFileID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return subIds;
        }
    }
}