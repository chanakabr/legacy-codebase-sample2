using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApi;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace TVPWebApi.Models
{
    public class MediasService : IMediasService
    {

        public Media GetMediaInfo(InitializationObject initObj, long MediaID, int mediaType, string picSize, bool withDynamic)
        {
            Media retMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                retMedia = MediaHelper.GetMediaInfo(initObj, MediaID, mediaType, picSize, groupID, withDynamic);
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
                TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);

                lstComment = CommentHelper.GetMediaComments(account.TVMUser, account.TVMPass, mediaID, pageSize, pageIndex);
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
                TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
                
                retVal = CommentHelper.SaveMediaComments(account.TVMUser, account.TVMPass, initObj.SiteGuid, initObj.UDID, mediaID, writer, header, subheader, content, autoActive);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retVal;
        }

        public TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject RateMedia(InitializationObject initObj, int mediaID, int mediaType, int extraVal)
        {
            TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject retVal = new TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RateMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                retVal = new ApiApiService(groupID, initObj.Platform).RateMedia(initObj.SiteGuid, mediaID, extraVal);
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

        public string MediaMark(InitializationObject initObj, action Action, int mediaType, long iMediaID, long iFileID, int iLocation)
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

        public string MediaHit(InitializationObject initObj, int mediaType, long iMediaID, long iFileID, int iLocation)
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

        public List<Media> GetRelatedMedias(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRelatedMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = MediaHelper.GetRelatedMediaList(initObj, mediaID, mediaType, picSize, pageSize, pageIndex, groupID, null);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPeopleWhoWatched", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = MediaHelper.GetPeopleWhoWatchedList(initObj, mediaID, mediaType, picSize, pageSize, pageIndex, groupID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public string CheckGeoBlockForMedia(InitializationObject initObj, int iMediaID)
        {
            string sRet = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CheckGeoBlockForMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                sRet = new ApiApiService(groupId, initObj.Platform).CheckGeoBlockMedia(iMediaID, SiteHelper.GetClientIP());
            }
            else
            {
                throw new UnknownGroupException();
            }

            return sRet;
        }

        //Files

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
                MediaFileItemPricesContainer[] prices = new ApiConditionalAccessService(groupId, initObj.Platform).GetItemsPrice(new int[] { iFileID }, sUserGuid, true);

                MediaFileItemPricesContainer mediaPrice = null;
                foreach (MediaFileItemPricesContainer mp in prices)
                {
                    if (mp.m_nMediaFileID == iFileID)
                    {
                        mediaPrice = mp;
                        break;
                    }
                }

                if (mediaPrice != null && mediaPrice.m_oItemPrices != null && mediaPrice.m_oItemPrices.Length > 0)
                {
                    TVPApi.PriceReason priceReason = (TVPApi.PriceReason)mediaPrice.m_oItemPrices[0].m_PriceReason;

                    bRet = mediaPrice.m_oItemPrices[0].m_oPrice.m_dPrice == 0 &&
                           (priceReason == TVPApi.PriceReason.PPVPurchased ||
                            priceReason == TVPApi.PriceReason.SubscriptionPurchased ||
                            priceReason == TVPApi.PriceReason.PrePaidPurchased ||
                            priceReason == TVPApi.PriceReason.Free);
                }
                else
                {
                    bRet = true;
                }
            }
            else
            {
                throw new UnknownGroupException();
            }

            return bRet;
        }
    }
}