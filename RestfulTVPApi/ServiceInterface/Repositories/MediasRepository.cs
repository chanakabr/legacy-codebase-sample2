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

namespace RestfulTVPApi.ServiceInterface
{
    public class MediasRepository : IMediasRepository
    {
        public List<Media> GetMediasInfo(InitializationObject initObj, List<int> MediaID, string picSize)
        {
            List<Media> retMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediasInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                retMedia = MediaHelper.GetMediasInfo(initObj, MediaID, picSize, groupID);
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

        public List<Media> GetRelatedMediasByTypes(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex, List<int> reqMediaTypes)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRelatedMediasByTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = MediaHelper.GetRelatedMediaList(initObj, mediaID, picSize, pageSize, pageIndex, groupID, reqMediaTypes);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPeopleWhoWatched", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = MediaHelper.GetPeopleWhoWatchedList(initObj, mediaID, picSize, pageSize, pageIndex, groupID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public List<KeyValuePair<int, bool>> AreMediasFavorite(InitializationObject initObj, List<int> mediaIds)
        {
            List<KeyValuePair<int, bool>> result = new List<KeyValuePair<int, bool>>();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AreMediasFavorite", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                result = MediaHelper.AreMediasFavorite(initObj, groupID, mediaIds);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return result;
        }

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

        public bool IsItemPurchased(InitializationObject initObj, int iFileID, string sUserGuid)
        {

            bool bRet = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsItemPurchased", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                IImplementation impl = WSUtils.GetImplementation(groupId, initObj);

                bRet = impl.IsItemPurchased(iFileID, sUserGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return bRet;
        }

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

        public List<Media> GetRecommendedMediasByTypes(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int[] reqMediaTypes)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRecommendedMediasByTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = MediaHelper.GetRecommendedMediasList(initObj, picSize, pageSize, pageIndex, groupID, reqMediaTypes);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

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
    }
}