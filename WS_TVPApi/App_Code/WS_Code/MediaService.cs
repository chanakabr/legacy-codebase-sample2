using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using log4net;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPApiModule.Services;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPApiModule.Objects;
using TVPApiModule.Helper;
using System.Web;

namespace TVPApiServices
{
    /// <summary>
    /// Summary description for Service
    /// </summary>
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class MediaService : System.Web.Services.WebService, IMediaService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(MediaService));

        #region Get media

        //Get specific media info
        [WebMethod(EnableSession = true, Description = "Get specific media info")]
        [System.Xml.Serialization.XmlInclude(typeof(DynamicData))]
        public Media GetMediaInfo(InitializationObject initObj, long MediaID, int mediaType, string picSize, bool withDynamic)
        {
            Media retMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {

                    retMedia = MediaHelper.GetMediaInfo(initObj, MediaID, mediaType, picSize, groupID, withDynamic);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return retMedia;
        }

        [WebMethod(EnableSession = true, Description = "Get list media info")]
        [System.Xml.Serialization.XmlInclude(typeof(DynamicData))]
        public List<Media> GetMediasInfo(InitializationObject initObj, long[] MediaID, int mediaType, string picSize, bool withDynamic)
        {
            List<Media> retMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediasInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retMedia = MediaHelper.GetMediasInfo(initObj, MediaID, mediaType, picSize, groupID, withDynamic);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return retMedia;
        }

        //Get Channel medias
        [WebMethod(EnableSession = true, Description = "Get Channel medias")]
        public List<Media> GetChannelMediaList(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetChannelMediaList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetChannelMediaList(initObj, ChannelID, picSize, pageSize, pageIndex, groupID, orderBy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        //Get Channel medias
        [WebMethod(EnableSession = true, Description = "Get Channel medias with multiple filters")]
        public List<Media> GetChannelMultiFilter(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, OrderBy orderBy, string metaName, eOrderDirection orderDir, List<TVPApi.TagMetaPair> metas, List<TVPApi.TagMetaPair> tags)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetChannelMultiFilter", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetChannelMultiFilter(initObj, ChannelID, picSize, pageSize, pageIndex, groupID, orderBy, metas, tags);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        //Get channel media with total number of medias
        [WebMethod(EnableSession = true, Description = "Get channel media with total number of medias")]
        public List<Media> GetChannelMediaListWithMediaCount(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, ref long mediaCount)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetChannelMediaListWithMediaCount", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetChannelMediaList(initObj, ChannelID, picSize, pageSize, pageIndex, groupID, ref mediaCount);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        // Check if media has been added to favorites
        [WebMethod(EnableSession = true, Description = "Check if media has been added to favorites")]
        public bool IsMediaFavorite(InitializationObject initObj, int mediaID)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsMediaFavorite", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    bRet = MediaHelper.IsFavoriteMedia(initObj, groupID, mediaID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Check if media array has been added to favorites")]
        public List<KeyValuePair<long, bool>> AreMediasFavorite(InitializationObject initObj, List<long> mediaIds)
        {
            List<KeyValuePair<long, bool>> result = new List<KeyValuePair<long, bool>>();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AreMediasFavorite", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    result = MediaHelper.AreMediasFavorite(initObj, groupID, mediaIds);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return result;
        }

        //Get users comments for media
        [WebMethod(EnableSession = true, Description = "Get users comments for media")]
        public List<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex)
        {
            List<Comment> lstComment = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaComments", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
                    lstComment = CommentHelper.GetMediaComments(account.TVMUser, account.TVMPass, mediaID, pageSize, pageIndex);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstComment;
        }

        //Get User Items (Favorites, Rentals etc..)
        [WebMethod(EnableSession = true, Description = "Get User Items (Favorites, Rentals etc..)")]
        public FavoritObject[] GetUserFavorites(InitializationObject initObj)
        {
            FavoritObject[] favoritesObj = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserFavorites", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    favoritesObj = new ApiUsersService(groupID, initObj.Platform).GetUserFavorites(initObj.SiteGuid, string.Empty, initObj.DomainID, string.Empty);
                    favoritesObj = favoritesObj.OrderByDescending(r => r.m_dUpdateDate.Date).ThenByDescending(r => r.m_dUpdateDate.TimeOfDay).ToArray();
                    //lstMedia = new Api MediaHelper.GetUserItems(initObj, itemType, mediaType, picSize, pageSize, start_index, groupID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return favoritesObj;
        }

        //Get User Items (Favorites, Rentals etc..)
        [WebMethod(EnableSession = true, Description = "Get User Items (Favorites, Rentals etc..)")]
        public List<Media> GetUserItems(InitializationObject initObj, UserItemType itemType, int mediaType, string picSize, int pageSize, int start_index)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetUserItems(initObj, itemType, mediaType, picSize, pageSize, start_index, groupID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        #endregion

        #region Media related

        [WebMethod(EnableSession = true, Description = "Get related media info")]
        public List<Media> GetRelatedMediasByTypes(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int[] reqMediaTypes)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRelatedMediasByTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetRelatedMediaList(initObj, mediaID, mediaType, picSize, pageSize, pageIndex, groupID, reqMediaTypes);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        //Get related media info
        [WebMethod(EnableSession = true, Description = "Get related media info")]
        public List<Media> GetRelatedMedias(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRelatedMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetRelatedMediaList(initObj, mediaID, mediaType, picSize, pageSize, pageIndex, groupID, null);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        //Get related media info with total number of medias
        [WebMethod(EnableSession = true, Description = "Get related media info with total number of medias")]
        public List<Media> GetRelatedMediaWithMediaCount(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, ref long mediaCount)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRelatedMediaWithMediaCount", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetRelatedMediaList(initObj, mediaID, mediaType, picSize, pageSize, pageIndex, groupID, ref mediaCount);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        //Get Related media info
        [WebMethod(EnableSession = true, Description = "Get Related media info")]
        public List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPeopleWhoWatched", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetPeopleWhoWatchedList(initObj, mediaID, mediaType, picSize, pageSize, pageIndex, groupID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        [WebMethod(EnableSession = true, Description = "Get liked media info")]
        public List<Media> GetUserSocialMedias(InitializationObject initObj, string socialPlatform, string socialAction, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = new List<Media>();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserSocialMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetUserSocialMedias(initObj, picSize, pageSize, pageIndex, groupID, socialAction, socialPlatform);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        [WebMethod(EnableSession = true, Description = "check if social action performed on media by user")]
        public bool IsUserSocialActionPerformed(InitializationObject initObj, string sMediaID, string socialPlatform, string socialAction)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsUserSocialActionPerformed", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    List<Media> lstMedia = GetUserSocialMedias(initObj, socialPlatform, socialAction, "full", 20, 0);

                    bRet = (from r in lstMedia where r.MediaID.Equals(sMediaID) select true).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Get last watched medias")]
        public List<Media> GetLastWatchedMedias(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetLastWatchedMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetLastWatchedMedias(initObj, picSize, pageSize, pageIndex, groupID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        [WebMethod(EnableSession = true, Description = "Get last watched medias")]
        public List<Media> GetLastWatchedMediasByPeriod(InitializationObject initObj, int mediaID, int mediaType, string picSize, int periodBefore, MediaHelper.ePeriod byPeriod)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetLastWatchedMediasByPeriod", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetLastWatchedMediasByPeriod(initObj, picSize, periodBefore, groupID, byPeriod);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        [WebMethod(EnableSession = true, Description = "Get media list for package")]
        public List<Media> GetMediasInPackage(InitializationObject initObj, long iBaseID, int mediaType, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediasInPackage", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetMediasInPackage(initObj, iBaseID, mediaType, groupID, picSize, pageSize, pageIndex);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        [WebMethod(EnableSession = true, Description = "")]
        public List<Media> GetMediasByRating(InitializationObject initObj, int rating)
        {
            return null;
        }

        [WebMethod(EnableSession = true, Description = "Get media list for package")]
        public List<Media> GetRecommendedMedias(InitializationObject initObj, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRecommendedMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetRecommendedMediasList(initObj, picSize, pageSize, pageIndex, groupID, null);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        public List<Media> GetRecommendedMediasByTypes(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int[] reqMediaTypes)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRecommendedMediasByTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetRecommendedMediasList(initObj, picSize, pageSize, pageIndex, groupID, reqMediaTypes);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        #endregion

        #region Search media

        [WebMethod(EnableSession = true, Description = "Search medias by multi tags")]
        public List<Media> SearchMediaByMultiTag(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchMediaByMultiTag", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMediaByTag(initObj, mediaType, tagPairs, picSize, pageSize, pageIndex, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        [WebMethod(EnableSession = true, Description = "Search medias by multi tags")]
        public List<Media> SearchMediaByMetasTags(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, List<TVPApi.TagMetaPair> metaPairs, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchMediaByMetasTags", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMediaByMetasTags(initObj, mediaType, tagPairs, metaPairs, picSize, pageSize, pageIndex, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        [WebMethod(EnableSession = true, Description = "Search medias by multi tags")]
        public List<Media> SearchMediaByMetasTagsExact(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, List<TVPApi.TagMetaPair> metaPairs, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchMediaByMetasTagsExact", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMediaByMetasTagsExact(initObj, mediaType, tagPairs, metaPairs, picSize, pageSize, pageIndex, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        //Search medias by tag
        [WebMethod(EnableSession = true, Description = "Search medias by tag")]
        public List<Media> SearchMediaByTag(InitializationObject initObj, string tagName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchMediaByTag", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMediaByTag(initObj, mediaType, tagName, value, picSize, pageSize, pageIndex, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        //Search medias by meta
        [WebMethod(EnableSession = true, Description = "Search medias by meta")]
        public List<Media> SearchMediaByMeta(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchMediaByMeta", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMediaByMeta(initObj, mediaType, metaName, value, picSize, pageSize, pageIndex, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        //Search media by meta with total items number
        [WebMethod(EnableSession = true, Description = "Search media by meta with total items number")]
        public List<Media> SearchMediaByMetaWithMediaCount(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchMediaByMetaWithMediaCount", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    return MediaHelper.SearchMediaByMeta(initObj, mediaType, metaName, value, picSize, pageSize, pageIndex, groupID, (int)orderBy, ref mediaCount);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return null;
        }

        //Search category info
        [WebMethod(EnableSession = true, Description = "Search category info")]
        public Category GetCategory(InitializationObject initObj, int categoryID)
        {
            Category retCategory = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetCategory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retCategory = CategoryTreeHelper.GetCategoryTree(categoryID, groupID, initObj.Platform);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return retCategory;
        }

        [WebMethod(EnableSession = true, Description = "Search category info")]
        public Category GetFullCategory(InitializationObject initObj, int categoryID, string picSize)
        {
            Category retCategory = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetFullCategory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retCategory = CategoryTreeHelper.GetFullCategoryTree(categoryID, picSize, groupID, initObj.Platform);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return retCategory;
        }

        //Search media by free text
        [WebMethod(EnableSession = true, Description = "Search media by free text")]
        public List<Media> SearchMedia(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMedia(initObj, mediaType, text, picSize, pageSize, pageIndex, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        //Search media by free text
        [WebMethod(EnableSession = true, Description = "Search EPG by free text")]
        public TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] SearchEPG(InitializationObject initObj, string text, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] programs = { };

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchEPG", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //lstMedia = MediaHelper.SearchMedia(initObj, text, picSize, pageSize, pageIndex, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return programs;
        }

        //Search media by free text
        [WebMethod(EnableSession = true, Description = "Search media by free text")]
        public List<Media> SearchMediaByTypes(InitializationObject initObj, string text, int[] mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchMediaByTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMedia(initObj, mediaType, text, picSize, pageSize, pageIndex, groupID, orderBy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        //Search media by free text with response total media count
        [WebMethod(EnableSession = true, Description = "Search media by free text with response total media count")]
        public List<Media> SearchMediaWithMediaCount(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchMediaWithMediaCount", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMedia(initObj, mediaType, text, picSize, pageSize, pageIndex, groupID, (int)orderBy, ref mediaCount);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        //Get most searched text
        [WebMethod(EnableSession = true, Description = "Get most searched text")]
        public List<string> GetNMostSearchedTexts(InitializationObject initObj, int N, int pageSize, int start_index)
        {
            List<string> retVal = null;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetNMostSearchedTexts", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // TODO:
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return retVal;
        }

        [WebMethod(EnableSession = true, Description = "Get auto-complete media titles")]
        public string[] GetAutoCompleteSearch(InitializationObject initObj, string prefixText, int?[] iMediaTypes, int pageSize, int pageIdx)
        {
            string[] retVal = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetAutoCompleteSearch", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                List<string> lstRet = new List<String>();

                List<string> lstResponse = MediaHelper.GetAutoCompleteList(groupID, initObj.Platform, iMediaTypes != null ? iMediaTypes.Cast<int>().ToArray() : new int[0],
                    prefixText, initObj.Locale.LocaleLanguage, pageIdx, pageSize);

                foreach (String sTitle in lstResponse)
                {
                    if (sTitle.ToLower().StartsWith(prefixText.ToLower())) lstRet.Add(sTitle);
                }
                retVal = lstRet.ToArray();
            }

            return retVal;
        }

        // Get auto-complete media titles
        [WebMethod(EnableSession = true, Description = "Get auto-complete media titles")]
        public string[] GetAutoCompleteSearchList(InitializationObject initObj, string prefixText, int?[] iMediaTypes)
        {
            string[] retVal = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetAutoCompleteSearchList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                List<string> lstRet = new List<String>();

                int maxItems = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.MovieFinder.MaxItems;
                List<string> lstResponse = MediaHelper.GetAutoCompleteList(groupID, initObj.Platform, iMediaTypes != null ? iMediaTypes.Cast<int>().ToArray() : new int[0],
                    prefixText, initObj.Locale.LocaleLanguage, 0, maxItems);

                foreach (String sTitle in lstResponse)
                {
                    if (sTitle.ToLower().StartsWith(prefixText.ToLower())) lstRet.Add(sTitle);
                }
                retVal = lstRet.ToArray();
            }

            return retVal;
        }

        #endregion

        #region Actions
        [WebMethod(EnableSession = true, Description = "Send to a friend")]
        public string SendToFriend(InitializationObject initObj, int mediaID, string senderName, string senderEmail, string toEmail, string msg)
        {
            string retVal = null;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SendToFriend", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retVal = ActionHelper.SendToFriend(initObj, groupID, mediaID, senderName, senderEmail, toEmail, msg);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return retVal;
        }

        [WebMethod(EnableSession = true, Description = "Add comment")]
        public bool AddComment(InitializationObject initObj, int mediaID, int mediaType, string writer, string header, string subheader, string content, bool autoActive)
        {
            bool retVal = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddComment", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
                    retVal = CommentHelper.SaveMediaComments(account.TVMUser, account.TVMPass, initObj.SiteGuid, initObj.UDID, mediaID, writer, header, subheader, content, autoActive);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return retVal;
        }

        // Perform action on media (AddFavorite, Comment, Like, Rate, Recommend, Record, Reminder, RemoveFavorite, Share, Watch)
        [WebMethod(EnableSession = true, Description = "Perform action on media (AddFavorite, Comment, Like, Rate, Recommend, Record, Reminder, RemoveFavorite, Share, Watch)")]
        public bool ActionDone(InitializationObject initObj, TVPApi.ActionType action, int mediaID, int mediaType, int extraVal)
        {
            bool retVal = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ActionDone", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retVal = ActionHelper.PerformAction(action, mediaID, mediaType, groupID, initObj.Platform, initObj.SiteGuid, initObj.DomainID, initObj.UDID, extraVal);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return retVal;
        }

        [WebMethod(EnableSession = true, Description = "Rate a Media")]
        public TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject RateMedia(InitializationObject initObj, int mediaID, int mediaType, int extraVal)
        {
            TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject retVal = new TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RateMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retVal = new ApiApiService(groupID, initObj.Platform).RateMedia(initObj.SiteGuid, mediaID, extraVal);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return retVal;
        }

        [WebMethod(EnableSession = true, Description = "")]
        public List<Media> GetMediasByMostAction(InitializationObject initObj, TVPApi.ActionType action, int mediaType)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediasByMostAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Mark player status")]
        public string MediaMark(InitializationObject initObj, action Action, int mediaType, long iMediaID, long iFileID, int iLocation)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //ConnectionHelper.InitServiceConfigs(groupID, initObj.Platform);

                    sRet = ActionHelper.MediaMark(initObj, groupID, initObj.Platform, Action, mediaType, iMediaID, iFileID, iLocation);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Mark player position")]
        public string MediaHit(InitializationObject initObj, int mediaType, long iMediaID, long iFileID, int iLocation)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //ConnectionHelper.InitServiceConfigs(groupID, initObj.Platform);

                    sRet = ActionHelper.MediaHit(initObj, groupID, initObj.Platform, mediaType, iMediaID, iFileID, iLocation);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Get last player position")]
        public TVPPro.SiteManager.TvinciPlatform.api.MediaMarkObject GetMediaMark(InitializationObject initObj, int iMediaID)
        {
            TVPPro.SiteManager.TvinciPlatform.api.MediaMarkObject mediaMark = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //ConnectionHelper.InitServiceConfigs(groupID, initObj.Platform);

                    mediaMark = new ApiApiService(groupID, initObj.Platform).GetMediaMark(initObj.SiteGuid, iMediaID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return mediaMark;
        }
        #endregion

        #region Purchase

        [WebMethod(EnableSession = true, Description = "Get media price reason")]
        public TVPApi.PriceReason GetItemPriceReason(InitializationObject initObj, int iFileID)
        {
            TVPApi.PriceReason priceReason = TVPApi.PriceReason.UnKnown;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetItemPriceReason", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {

                    MediaFileItemPricesContainer[] prices = new ApiConditionalAccessService(groupId, initObj.Platform).GetItemsPrice(new int[] { iFileID }, initObj.SiteGuid, true);

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
                        priceReason = (TVPApi.PriceReason)mediaPrice.m_oItemPrices[0].m_PriceReason;
                    }
                    else
                    {
                        priceReason = TVPApi.PriceReason.Free;
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return priceReason;
        }

        [WebMethod(EnableSession = true, Description = "Check if item is purchased")]
        public bool IsItemPurchased(InitializationObject initObj, int iFileID, string sUserGuid)
        {

            bool bRet = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsItemPurchased", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
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
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Get list of purchased items for a user")]
        public PermittedMediaContainer[] GetUserPermittedItems(InitializationObject initObj)
        {
            PermittedMediaContainer[] permittedMediaContainer = { };

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermittedItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    permittedMediaContainer = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermittedItems(initObj.SiteGuid);
                    permittedMediaContainer = permittedMediaContainer.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return permittedMediaContainer;
        }

        //Search medias by meta
        [WebMethod(EnableSession = true, Description = "Search medias by meta")]
        public List<Media> GetSubscriptionMedia(InitializationObject initObj, string sSubID, string picSize, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMediaBySubID(initObj, sSubID, picSize, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        [WebMethod(EnableSession = true, Description = "Search medias by meta")]
        public List<Media> GetSubscriptionMedias(InitializationObject initObj, string[] sSubID, string picSize, OrderBy orderBy)
        {
            List<Media> lstMedia = new List<Media>();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    foreach (string subID in sSubID)
                        lstMedia.AddRange(GetSubscriptionMedia(initObj, subID, picSize, orderBy));
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lstMedia;
        }

        [WebMethod(EnableSession = true, Description = "Get list of purchased subscriptions for a user")]
        public PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj)
        {
            PermittedSubscriptionContainer[] permitedSubscriptions = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermitedSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    permitedSubscriptions = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermitedSubscriptions(initObj.SiteGuid);
                    permitedSubscriptions = permitedSubscriptions.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return permitedSubscriptions;
        }

        [WebMethod(EnableSession = true, Description = "Get list of purchased subscriptions and packages info for a user")]
        public List<PermittedPackages> GetUserPermittedPackages(InitializationObject initObj)
        {
            List<PermittedPackages> permittedPackages = new List<PermittedPackages>();
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermitedSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    PermittedSubscriptionContainer[] permitedSubscriptions = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermitedSubscriptions(initObj.SiteGuid);

                    if (permitedSubscriptions == null || permitedSubscriptions.Count() == 0)
                        return permittedPackages;

                    permitedSubscriptions = permitedSubscriptions.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();

                    foreach (PermittedSubscriptionContainer psc in permitedSubscriptions)
                    {
                        PermittedPackages pp = new PermittedPackages();
                        pp.PermittedSubscriptions = psc;

                        Dictionary<string, string> dict = new Dictionary<string, string>();
                        dict.Add("Base ID", psc.m_sSubscriptionCode);
                        TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupId, initObj.Platform).GetTVMAccountByAccountType(AccountType.Fictivic);
                        APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) { IgnoreFilter = true, SearchTokenSignature = string.Concat("Base ID=", psc.m_sSubscriptionCode), GroupID = groupId, Platform = initObj.Platform, dictMetas = dict, WithInfo = false, PageSize = 1, PictureSize = "full", PageIndex = 0, OrderBy = OrderBy.ABC, MetaValues = psc.m_sSubscriptionCode, Country = new TVPApiModule.Services.ApiUsersService(groupId, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), UseFinalEndDate = "true" };

                        TVPPro.SiteManager.DataEntities.dsItemInfo ds = searchLoader.Execute();
                        if (ds.Item.Rows.Count > 0)
                            pp.Package = new Media(ds.Item[0], initObj, groupId, false);

                        permittedPackages.Add(pp);
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return permittedPackages;
        }

        [WebMethod(EnableSession = true, Description = "Perform validation and purchase with Inapp")]
        public BillingResponse ChargeUserWithInApp(InitializationObject initObj, double price, string currency, string receipt, string productCode)
        {
            BillingResponse response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserWithInApp", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).InAppChargeUserForSubscription(price, currency, SiteHelper.GetClientIP(), initObj.SiteGuid, string.Empty, initObj.UDID, productCode, receipt);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
                HttpContext.Current.Items.Add("Error", "Unknown group");

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user dummy purchase for file")]
        public string DummyChargeUserForMediaFile(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon)
        {
            string response = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DummyChargeUserForMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).DummyChargeUserForMediaFile(iPrice, sCurrency, iFileID, sPPVModuleCode, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for file")]
        public string ChargeUserForMediaFile(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon)
        {
            string response = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserForMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChargeUserForMediaFile(iPrice, sCurrency, iFileID, sPPVModuleCode, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Cancel Subscription")]
        public bool CancelSubscription(InitializationObject initObj, string sSubscriptionID, int sSubscriptionPurchaseID)
        {
            bool response = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CancelSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).CancelSubscription(initObj.SiteGuid, sSubscriptionID, sSubscriptionPurchaseID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for subscription")]
        public string ChargeUserForMediaSubscription(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID)
        {
            string response = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserForMediaSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChargeUserForSubscription(iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, initObj.SiteGuid, sExtraParameters, sUDID);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user dummy purchase for subscription")]
        public string DummyChargeUserForSubscription(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID)
        {
            string response = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DummyChargeUserForSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).DummyChargeUserForSubscription(iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, initObj.SiteGuid, sExtraParameters, sUDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for subscription")]
        public MediaFileItemPricesContainer[] GetItemPrices(InitializationObject initObj, int[] fileIds, bool bOnlyLowest)
        {
            MediaFileItemPricesContainer[] itemPrices = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetItemPrices", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    //TODO: delete after tvm fix
                    /*itemPrices = new ApiConditionalAccessService(groupId, initObj.Platform).GetItemsPrice(fileIds, initObj.SiteGuid, bOnlyLowest);*/

                    System.Collections.ArrayList al = new System.Collections.ArrayList();
                    foreach (int fileID in fileIds)
                    {
                        MediaFileItemPricesContainer[] tmpRes = new ApiConditionalAccessService(groupId, initObj.Platform).GetItemsPrice(new int[] { fileID }, initObj.SiteGuid, bOnlyLowest);
                        if (tmpRes != null)
                            al.AddRange(tmpRes);
                    }

                    itemPrices = (MediaFileItemPricesContainer[])al.ToArray(typeof(MediaFileItemPricesContainer));
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return itemPrices;
        }

        [WebMethod(EnableSession = true, Description = "Get product code for subscription")]
        public string GetSubscriptionProductCode(InitializationObject initObj, int subID)
        {
            string res = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionProductCode", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    res = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionData(subID.ToString(), false).m_ProductCode;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get subscription")]
        public List<SubscriptionPrice> GetSubscriptionDataPrices(InitializationObject initObj, int[] subIDs)
        {
            List<SubscriptionPrice> res = new List<SubscriptionPrice>();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionDataPrices", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    foreach (int subID in subIDs)
                    {
                        var priceObj = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionData(subID.ToString(), false);

                        res.Add(new SubscriptionPrice
                        {
                            SubscriptionCode = priceObj.m_sObjectCode,
                            Price = priceObj.m_oSubscriptionPriceCode.m_oPrise.m_dPrice,
                            Currency = priceObj.m_oSubscriptionPriceCode.m_oPrise.m_oCurrency.m_sCurrencySign
                        });
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get user transaction history")]
        public BillingTransactionsResponse GetUserTransactionHistory(InitializationObject initObj, int start_index, int pageSize)
        {
            BillingTransactionsResponse transactions = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserTransactionHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    transactions = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserTransactionHistory(initObj.SiteGuid, start_index, pageSize);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return transactions;
        }

        [WebMethod(EnableSession = true, Description = "Get user expired items")]
        public PermittedMediaContainer[] GetUserExpiredItems(InitializationObject initObj, int iTotalItems)
        {
            PermittedMediaContainer[] items = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    items = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserExpiredItems(initObj.SiteGuid, iTotalItems);
                    if (items != null)
                    {
                        items = items.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            if (items == null)
            {
                items = new PermittedMediaContainer[0];
            }

            return items;
        }

        [WebMethod(EnableSession = true, Description = "Get user expired subscription")]
        public PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(InitializationObject initObj, int iTotalItems)
        {
            PermittedSubscriptionContainer[] items = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    items = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserExpiredSubscriptions(initObj.SiteGuid, iTotalItems);
                    if (items != null)
                        items = items.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            if (items == null)
            {
                items = new PermittedSubscriptionContainer[0];
            }

            return items;
        }

        [WebMethod(EnableSession = true, Description = "Get subscription price")]
        public SubscriptionsPricesContainer[] GetSubscriptionsPrices(InitializationObject initObj, string[] SubscriptionIDs)
        {
            SubscriptionsPricesContainer[] items = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsPrices", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    items = new ApiConditionalAccessService(groupId, initObj.Platform).GetSubscriptionsPrices(initObj.SiteGuid, SubscriptionIDs, true);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return items;
        }
        #endregion

        [WebMethod(EnableSession = true, Description = "Get Prepaid balance")]
        public string[] GetPrepaidBalance(InitializationObject initObj, string currencyCode)
        {
            string[] fResponse = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetPrepaidBalance", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    fResponse = new ApiConditionalAccessService(groupId, initObj.Platform).GetPrepaidBalance(initObj.SiteGuid, currencyCode);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return fResponse;
        }

        [WebMethod(EnableSession = true, Description = "Buy PPV With PP")]
        public string ChargeMediaWithPrepaid(InitializationObject initObj, double price, string currency, int mediaFileID, string ppvModuleCode, string couponCode)
        {
            PrePaidResponseStatus oResponse = PrePaidResponseStatus.UnKnown;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeMediaWithPrepaid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    oResponse = new ApiConditionalAccessService(groupId, initObj.Platform).PP_ChargeUserForMediaFile(initObj.SiteGuid, price, currency, mediaFileID, ppvModuleCode, couponCode, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return oResponse.ToString();
        }

        [WebMethod(EnableSession = true, Description = "Add user social sites action")]
        public bool AddUserSocialAction(InitializationObject initObj, int iMediaID, TVPPro.SiteManager.TvinciPlatform.api.SocialAction action, TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform socialPlatform)
        {
            bool bResponse = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "AddUserSocialAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    bResponse = new ApiApiService(groupId, initObj.Platform).AddUserSocialAction(iMediaID, initObj.SiteGuid, action, socialPlatform);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return bResponse;
        }

        [WebMethod(EnableSession = true, Description = "Has the user voted already")]
        public bool IsUserVoted(InitializationObject initObj, int iMediaID)
        {
            bool bResponse = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsUserVoted", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    //XXX: Fix this to be unified Enum
                    bResponse = TVPApiModule.Helper.VotesHelper.IsAlreadyVoted(iMediaID.ToString(), initObj.SiteGuid, groupId, initObj.Platform);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return bResponse;
        }

        [WebMethod(EnableSession = true, Description = "Has the user voted already")]
        public int GetVoteRatio(InitializationObject initObj, int iMediaID)
        {
            int nResponse = 0;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetVoteRatio", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    //XXX: Fix this to be unified Enum
                    nResponse = TVPPro.SiteManager.Helper.VotesHelper.GetVotingRatio(initObj.SiteGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return nResponse;
        }

        [WebMethod(EnableSession = true, Description = "Get Media License")]
        public string GetMediaLicenseLink(InitializationObject initObj, int mediaFileID, string baseLink)
        {
            string sResponse = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetMediaLicenseLink", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    sResponse = new ApiConditionalAccessService(groupId, initObj.Platform).GetMediaLicenseLink(initObj.SiteGuid, mediaFileID, baseLink, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return sResponse;
        }

        [WebMethod(EnableSession = true, Description = "Get user offline list")]
        public UserOfflineObject[] GetUserOfflineList(InitializationObject initObj)
        {
            UserOfflineObject[] sResponse = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserOfflineList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    if (new ApiUsersService(groupId, initObj.Platform).IsOfflineModeEnabled(initObj.SiteGuid))
                        sResponse = new ApiUsersService(groupId, initObj.Platform).GetUserOfflineList(initObj.SiteGuid).OrderBy(x => x.m_CreateDate).ToArray();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return sResponse;
        }

        [WebMethod(EnableSession = true, Description = "Toggle the user offline mode")]
        public void ToggleOfflineMode(InitializationObject initObj, bool isTurnOn)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ToggleOfflineMode", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    new ApiUsersService(groupId, initObj.Platform).ToggleOfflineMode(initObj.SiteGuid, isTurnOn);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
        }

        [WebMethod(EnableSession = true, Description = "Get full user offline list")]
        public List<Media> GetUserOfflineListFull(InitializationObject initObj, string picSize, bool withDynamic)
        {
            List<Media> lResponse = new List<Media>();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserOfflineListFull", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    if (!new ApiUsersService(groupId, initObj.Platform).IsOfflineModeEnabled(initObj.SiteGuid))
                        return lResponse;

                    UserOfflineObject[] offArr = new ApiUsersService(groupId, initObj.Platform).GetUserOfflineList(initObj.SiteGuid).OrderBy(x => x.m_CreateDate).ToArray();
                    long[] mediaIDs = offArr.Select(x => long.Parse(x.m_MediaID)).ToArray();
                    lResponse.AddRange(GetMediasInfo(initObj, mediaIDs, 0, picSize, withDynamic));
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return lResponse;
        }

        [WebMethod(EnableSession = true, Description = "Add user offline media")]
        public bool AddUserOfflineMedia(InitializationObject initObj, int mediaID)
        {
            bool bResponse = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "AddUserOfflineMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    bResponse = new ApiUsersService(groupId, initObj.Platform).AddUserOfflineMedia(initObj.SiteGuid, mediaID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return bResponse;
        }

        [WebMethod(EnableSession = true, Description = "Remove user offline media")]
        public bool RemoveUserOfflineMedia(InitializationObject initObj, int mediaID)
        {
            bool bResponse = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "RemoveUserOfflineMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    bResponse = new ApiUsersService(groupId, initObj.Platform).RemoveUserOfflineMedia(initObj.SiteGuid, mediaID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return bResponse;
        }

        [WebMethod(EnableSession = true, Description = "Clear user offline list")]
        public bool ClearUserOfflineList(InitializationObject initObj)
        {
            bool bResponse = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ClearUserOfflineList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    bResponse = new ApiUsersService(groupId, initObj.Platform).ClearUserOfflineList(initObj.SiteGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return bResponse;
        }

        [WebMethod(EnableSession = true, Description = "Check if media is blocked by Geo")]
        public string CheckGeoBlockForMedia(InitializationObject initObj, int iMediaID)
        {
            string sRet = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CheckGeoBlockForMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    sRet = new ApiApiService(groupId, initObj.Platform).CheckGeoBlockMedia(iMediaID, SiteHelper.GetClientIP());
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Get EPG Channels")]
        public TVPPro.SiteManager.TvinciPlatform.api.EPGChannelObject[] GetEPGChannels(InitializationObject initObj, string sPicSize, OrderBy orderBy)
        {
            TVPPro.SiteManager.TvinciPlatform.api.EPGChannelObject[] sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGChannels", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    sRet = new ApiApiService(groupId, initObj.Platform).GetEPGChannel(sPicSize);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Get EPG Channel Program by Dates")]
        public TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] GetEPGChannelProgrammeByDates(InitializationObject initObj, string channelID, string picSize, DateTime fromDate, DateTime toDate, int utcOffset)
        {
            TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGChannelProgrammeByDates", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    sRet = new ApiApiService(groupId, initObj.Platform).GetEPGChannelProgrammeByDates(channelID, picSize, fromDate, toDate, utcOffset);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
                HttpContext.Current.Items.Add("Error", "Unknown group");


            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Get EPG Channels")]
        public TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] GetEPGChannelsPrograms(InitializationObject initObj, string sEPGChannelID, string sPicSize, TVPPro.SiteManager.TvinciPlatform.api.EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        {
            TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGChannelsPrograms", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    sRet = new ApiApiService(groupId, initObj.Platform).GetEPGChannel(sEPGChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Get Multi EPG Channels")]
        public TVPPro.SiteManager.TvinciPlatform.api.EPGMultiChannelProgrammeObject[] GetEPGMultiChannelProgram(InitializationObject initObj, string[] sEPGChannelID, string sPicSize, TVPPro.SiteManager.TvinciPlatform.api.EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        {
            TVPPro.SiteManager.TvinciPlatform.api.EPGMultiChannelProgrammeObject[] sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGMultiChannelProgram", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    sRet = new ApiApiService(groupId, initObj.Platform).GetEPGMultiChannelProgram(sEPGChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Get Group Media Rules")]
        public TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetGroupMediaRules(InitializationObject initObj, int mediaID)
        {
            TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupMediaRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetGroupMediaRules(mediaID, int.Parse(initObj.SiteGuid), initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get All Channels")]
        public List<Channel> GetChannelsList(InitializationObject initObj, string sPicSize)
        {
            List<Channel> sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetChannelsList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    sRet = ChannelHelper.GetChannelsList(initObj, sPicSize, groupId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return sRet;
        }

        #region MessageBox
        [WebMethod(EnableSession = true, Description = "SendMessage")]
        public void SendMessage(string sSiteGuid, string sRecieverUDID, int iMediaID, int iMediaTypeID, int iLocation, string sAction, string sUsername, string sPassword)
        {
            MBMessage msg = new MBMessage()
            {
                MediaID = iMediaID,
                MediaTypeID = iMediaTypeID,
                Location = iLocation,
                SendToUDID = sRecieverUDID,
                SiteGuid = sSiteGuid,
                Action = sAction,
                Username = sUsername,
                Password = sPassword
            };

            MessageBox.Instance.Send(msg);
        }

        [WebMethod(EnableSession = true, Description = "Get new message")]
        public MBMessage GetMessage(string sUDID)
        {
            MBMessage msg = null;

            msg = MessageBox.Instance.GetNewMessage(sUDID);

            return msg;
        }
        #endregion
    }
}
