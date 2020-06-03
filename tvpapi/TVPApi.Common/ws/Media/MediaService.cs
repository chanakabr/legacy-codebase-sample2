using System;
using System.Collections.Generic;
using System.Linq;
using TVPApi;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Services;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApiModule.Objects;
using TVPApiModule.Helper;
using System.Web;
using TVPApiModule.Manager;
using TVPApiModule.Interfaces;
using TVPPro.SiteManager.DataEntities;
using TVPApiModule.CatalogLoaders;
using TVPPro.Configuration.OrcaRecommendations;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Objects;
using RecordedEPGOrderObj = ApiObjects.RecordedEPGOrderObj;
using System.Data;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Objects.Authorization;
using KLogMonitor;
using System.Reflection;
using TVPApiModule.Objects.Requests;
using System.Threading;
using Core.Users;
using Core.ConditionalAccess;
using ApiObjects.Billing;
using ApiObjects.ConditionalAccess;
using ApiObjects;
using InitializationObject = TVPApi.InitializationObject;
using Core.ConditionalAccess.Response;
using ePersonalFilter = ApiObjects.ePersonalFilter;
using eAssetTypes = ApiObjects.eAssetTypes;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.Catalog;
using ApiObjects.CrowdsourceItems.Base;
using ApiObjects.Statistics;
using AssetsBookmarksResponse = Core.Catalog.Response.AssetsBookmarksResponse;
using ApiObjects.Catalog;
using MediaMarkObject = ApiObjects.MediaMarkObject;
using Media = TVPApi.Media;
using EPGUnit = ApiObjects.EPGUnit;
using TVinciShared;
using Language = ApiObjects.Language;
using ConfigurationManager;
using TVPApiModule.Objects.CRM;

namespace TVPApiServices
{
    [System.ComponentModel.ToolboxItem(false)]
    public class MediaService : IMediaService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        #region Get media

        //Get specific media info
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retMedia;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retMedia;
        }

        //Get Channel medias
        public List<Media> GetChannelMediaList(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        //Get Channel medias
        public List<Media> GetChannelMultiFilter(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy, eOrderDirection orderDir, List<TagMetaPair> tagsMetas, TVPApiModule.Objects.Enums.eCutWith cutWith)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetChannelMultiFilter", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetChannelMultiFilter(initObj, ChannelID, picSize, pageSize, pageIndex, groupID, orderBy, orderDir, tagsMetas, cutWith);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        //Get Channel medias
        public List<Media> GetOrderedChannelMultiFilter(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, OrderObj orderObj, List<TagMetaPair> tagsMetas, TVPApiModule.Objects.Enums.eCutWith cutWith)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetChannelMultiFilter", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    var catalogOrderObj = orderObj?.ToCore();
                    lstMedia = MediaHelper.GetOrderedChannelMultiFilter(initObj, ChannelID, picSize, pageSize, pageIndex, groupID, catalogOrderObj, tagsMetas, cutWith);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        //Get channel media with total number of medias
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        // Check if media has been added to favorites
        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return bRet;
        }

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return result;
        }

        //Get users comments for media
        public List<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex)
        {
            List<Comment> lstComment = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaComments", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstComment = CommentHelper.GetMediaComments(mediaID, groupID, pageSize, pageIndex);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstComment;
        }

        //Get User Items (Favorites, Rentals etc..)
        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return favoritesObj;
        }

        //Get User Items (Favorites, Rentals etc..)
        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        #endregion

        #region Media related

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        public TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId GetExternalRelatedMedias(InitializationObject initObj, int assetID, int pageSize, int pageIndex, int[] filter_types, string freeParam, List<string> with)
        {
            TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId ret = null;
            HashSet<string> validWithValues = new HashSet<string>() { "images", "stats", "files" };

            // validate with - make sure it contains only "stats" and/or "files"
            if (with != null)
            {
                foreach (var currentValue in with)
                {
                    if (!validWithValues.Contains(currentValue))
                    {
                        ret = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId();
                        ret.Status = ResponseUtils.ReturnBadRequestStatus(string.Format("Invalid with value: {0}", currentValue));
                        return ret;
                    }
                }
            }

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetExternalRelatedMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    ret = MediaHelper.GetExternalRelatedMediaList(initObj, assetID, pageSize, pageIndex, groupID, filter_types, freeParam, with);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    ret.Status = new TVPApiModule.Objects.Responses.Status();
                    ret.Status.Code = (int)eStatus.Error;
                    ret.Status.Message = "Error";
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return ret;
        }

        public TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId GetExternalSearchMedias(InitializationObject initObj, string query, int pageSize, int pageIndex, List<string> with)
        {
            TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId ret = null;
            HashSet<string> validWithValues = new HashSet<string>() { "images", "stats", "files" };

            // validate with - make sure it contains only "stats" and/or "files"
            if (with != null)
            {
                foreach (var currentValue in with)
                {
                    if (!validWithValues.Contains(currentValue))
                    {
                        ret = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId();
                        ret.Status = ResponseUtils.ReturnBadRequestStatus(string.Format("Invalid with value: {0}", currentValue));
                        return ret;
                    }
                }
            }

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetExternalSearchMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    ret = MediaHelper.GetExternalSearchMediaList(initObj, query, pageSize, pageIndex, groupID, null, with);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    ret.Status = new TVPApiModule.Objects.Responses.Status();
                    ret.Status.Code = (int)eStatus.Error;
                    ret.Status.Message = "Error";
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return ret;
        }

        //Get related media info
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        //Get related media info with total number of medias
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        //Get Related media info
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        [PrivateMethod]
        public List<Media> GetUserSocialMedias(InitializationObject initObj, SocialPlatform socialPlatform, SocialAction socialAction, string picSize, int pageSize, int pageIndex)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        [PrivateMethod]
        public bool IsUserSocialActionPerformed(InitializationObject initObj, string sMediaID, SocialPlatform socialPlatform, SocialAction socialAction)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsUserSocialActionPerformed", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    List<Media> lstMedia = GetUserSocialMedias(initObj, socialPlatform, socialAction, "full", 20, 0);

                    bRet = (from r in lstMedia
                            where r.MediaID.Equals(sMediaID)
                            select true).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return bRet;
        }

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        public List<Media> GetMediasByRating(InitializationObject initObj, int rating)
        {
            return null;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        public OrcaResponse GetRecommendationsByGallery(InitializationObject initObj, int mediaID, string picSize, int parentalLevel, eGalleryType galleryType, string coGuid)
        {
            logger.DebugFormat("MediaService::GetRecommendedMediasByGallery -> gallery type : {0}", true, galleryType);

            OrcaResponse retVal = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRecommendationsByGallery", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    IImplementation impl = WSUtils.GetImplementation(groupID, initObj);
                    retVal = impl.GetRecommendedMediasByGallery(initObj, groupID, mediaID, picSize, parentalLevel, galleryType, coGuid);


                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retVal;
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        #endregion

        #region Search media

        public List<Media> SearchMediaByMultiTag(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        public List<Media> SearchMediaByMetasTags(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, List<TVPApi.TagMetaPair> metaPairs, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        public List<Media> SearchMediaByMetasTagsExact(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, List<TVPApi.TagMetaPair> metaPairs, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        public List<Media> SearchMediaByAndOrList(InitializationObject initObj, List<KeyValue> orList, List<KeyValue> andList, 
            int mediaType, int pageSize, int pageIndex, bool exact, 
            ApiObjects.SearchObjects.OrderBy orderBy,
            ApiObjects.SearchObjects.OrderDir orderDir, string orderMeta)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchMediaByAndOrList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //picSize is injected with an empty string
                    lstMedia = MediaHelper.SearchMediaByAndOrList(initObj, mediaType, orList, andList, string.Empty, pageSize, pageIndex, groupID, exact, orderBy, orderDir, orderMeta);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        //Search medias by tag
        public List<Media> SearchMediaByTag(InitializationObject initObj, string tagName, string value, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        //Search medias by meta
        public List<Media> SearchMediaByMeta(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        //Search media by meta with total items number
        public List<Media> SearchMediaByMetaWithMediaCount(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy, ref long mediaCount)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return null;
        }

        //Search category info
        public Category GetCategory(InitializationObject initObj, int categoryID)
        {
            Category retCategory = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetCategory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retCategory = CategoryTreeHelper.GetCategoryTree(categoryID, groupID, initObj.Platform, initObj.Locale.LocaleLanguage);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retCategory;
        }

        public Category GetFullCategory(InitializationObject initObj, int categoryID, string picSize)
        {
            Category retCategory = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetFullCategory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retCategory = CategoryTreeHelper.GetFullCategoryTree(categoryID, picSize, groupID, initObj.Platform, initObj.Locale.LocaleLanguage);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retCategory;
        }

        //Search media by free text
        public List<Media> SearchMedia(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        //Search media by free text
        public List<EPGChannelProgrammeObject> SearchEPG(InitializationObject initObj, string text, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
        {
            List<EPGChannelProgrammeObject> programs = null;
            int searchOffsetDays = ApplicationConfiguration.Current.TVPApiConfiguration.EPGSearchOffsetDays.Value;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchEPG", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    var programsList = new APIEPGSearchLoader(groupID, initObj.Platform.ToString(), SiteHelper.GetClientIP(), pageSize, pageIndex, text, DateTime.UtcNow.AddDays(-searchOffsetDays), DateTime.UtcNow.AddDays(searchOffsetDays))
                    {
                        SiteGuid = initObj.SiteGuid,
                        Culture = initObj.Locale.LocaleLanguage,
                        DomainId = initObj.DomainID,

                    }.Execute() as List<BaseObject>;
                    if (programsList != null)
                        programs = programsList.Select(p => ((ProgramObj)p).m_oProgram).ToList();
                }
                catch (Exception ex)
                {

                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return programs;
        }

        //Search media by free text
        public List<Media> SearchMediaByTypes(InitializationObject initObj, string text, int[] mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        //Search media by free text with response total media count
        public List<Media> SearchMediaWithMediaCount(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy, ref long mediaCount)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        //Get most searched text
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
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retVal;
        }

        public string[] GetAutoCompleteSearch(InitializationObject initObj, string prefixText, int[] iMediaTypes, int pageSize, int pageIdx)
        {
            string[] retVal = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetAutoCompleteSearch", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                List<string> lstRet = new List<String>();

                List<string> lstResponse = MediaHelper.GetAutoCompleteList(groupID, initObj, iMediaTypes != null ? iMediaTypes.Cast<int>().ToArray() : new int[0],
                    prefixText, initObj.Locale.LocaleLanguage, pageIdx, pageSize);

                foreach (String sTitle in lstResponse)
                {
                    if (sTitle.ToLower().StartsWith(prefixText.ToLower()))
                        lstRet.Add(sTitle);
                }
                retVal = lstRet.ToArray();
            }

            return retVal;
        }

        // Get auto-complete media titles
        public string[] GetAutoCompleteSearchList(InitializationObject initObj, string prefixText, int?[] iMediaTypes)
        {
            string[] retVal = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetAutoCompleteSearchList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                List<string> lstRet = new List<String>();

                int maxItems = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.MovieFinder.MaxItems;
                List<string> lstResponse = MediaHelper.GetAutoCompleteList(groupID, initObj, iMediaTypes != null ? iMediaTypes.Cast<int>().ToArray() : new int[0],
                    prefixText, initObj.Locale.LocaleLanguage, 0, maxItems);

                foreach (String sTitle in lstResponse)
                {
                    if (sTitle.ToLower().StartsWith(prefixText.ToLower()))
                        lstRet.Add(sTitle);
                }
                retVal = lstRet.ToArray();
            }

            return retVal;
        }

        #endregion

        #region Actions
        [PrivateMethod]
        public string SendToFriend(InitializationObject initObj, int mediaID, string senderName, string senderEmail, string toEmail, string msg)
        {
            string retVal = null;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SendToFriend", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retVal = ActionHelper.SendToFriend(initObj, groupID, mediaID, senderName, senderEmail, toEmail, msg).ToString();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retVal;
        }

        [PrivateMethod]
        public bool AddComment(InitializationObject initObj, int mediaID, int mediaType, string writer, string header, string subheader, string content, bool autoActive)
        {
            bool retVal = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddComment", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retVal = CommentHelper.SaveMediaComments(groupID, initObj.Platform, initObj.SiteGuid, initObj.UDID, initObj.Locale.LocaleLanguage, initObj.Locale.LocaleCountry, mediaID, writer, header, subheader, content, autoActive);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return retVal;
        }

        // Perform action on media (AddFavorite, Comment, Like, Rate, Recommend, Record, Reminder, RemoveFavorite, Share, Watch)
        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return retVal;
        }

        [PrivateMethod]
        public RateMediaObject RateMedia(InitializationObject initObj, int mediaID, int mediaType, int extraVal)
        {
            RateMediaObject retVal = new RateMediaObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RateMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retVal = new ApiApiService(groupID, initObj.Platform).RateMedia(initObj.SiteGuid, mediaID, extraVal);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return retVal;
        }

        public List<Media> GetMediasByMostAction(InitializationObject initObj, TVPApi.ActionType action, int mediaType)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediasByMostAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            return null;
        }

        [PrivateMethod]
        public string MediaMark(InitializationObject initObj, action Action, int mediaType, long iMediaID, long iFileID, int iLocation, string NPVRID, long programId,
                                bool isReportingMode = false)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //ConnectionHelper.InitServiceConfigs(groupID, initObj.Platform);
                    eAssetTypes assetType = string.IsNullOrEmpty(NPVRID) ? eAssetTypes.MEDIA : eAssetTypes.NPVR;
                    sRet = ActionHelper.MediaMark(initObj, groupID, initObj.Platform, Action, iLocation, NPVRID, programId, iMediaID, iFileID, isReportingMode, assetType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sRet;
        }

        [PrivateMethod]
        public string AssetBookmark(InitializationObject initObj, string assetID, string assetType, long fileID, PlayerAssetData PlayerAssetData, long programId,
                                    bool isReportingMode = false)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            action action;
            eAssetTypes eAssetType;

            if (groupID > 0)
            {
                try
                {
                    try
                    {
                        action = (action)Enum.Parse(typeof(action), PlayerAssetData.action, true);
                    }
                    catch
                    {
                        return "Action not recognized";
                    }

                    try
                    {
                        eAssetType = (eAssetTypes)Enum.Parse(typeof(eAssetTypes), assetType, true);
                    }
                    catch
                    {
                        return "invalid asset type";
                    }

                    long mediaId = 0;
                    string npvrId = "";

                    if (eAssetType == eAssetTypes.NPVR)
                        npvrId = assetID;
                    else
                    {
                        if (!long.TryParse(assetID, out mediaId))
                        {
                            return "Invalid Asset id";
                        }
                    }
                    
                    sRet = ActionHelper.MediaMark(initObj, groupID, initObj.Platform, action, PlayerAssetData.location, npvrId, programId, mediaId, fileID, isReportingMode, 
                                                  eAssetType, PlayerAssetData.averageBitRate, PlayerAssetData.currentBitRate, PlayerAssetData.totalBitRate);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sRet;
        }

        [PrivateMethod]
        public string MediaHit(InitializationObject initObj, int mediaType, long iMediaID, long iFileID, int iLocation, string NPVRID, long programId, bool isReportingMode = false)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaHit", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //ConnectionHelper.InitServiceConfigs(groupID, initObj.Platform);
                    sRet = ActionHelper.MediaHit(initObj, groupID, initObj.Platform, iMediaID, iFileID, iLocation, NPVRID, programId, isReportingMode);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sRet;
        }

        [PrivateMethod]
        public MediaMarkObject GetMediaMark(InitializationObject initObj, int iMediaID, string npvrID)
        {
            MediaMarkObject mediaMark = null;
            eAssetTypes requestType;
            string assetRequestID;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    AssetBookmarkRequest AssetsToGet;
                    // npvrID is empty then we get media by iMediaID
                    if (string.IsNullOrEmpty(npvrID))
                    {
                        requestType = eAssetTypes.MEDIA;
                        assetRequestID = iMediaID.ToString();
                        AssetsToGet = new AssetBookmarkRequest() { AssetID = assetRequestID, AssetType = requestType };
                    }
                    // npvrId is not empty then we get NPVR by npvrID
                    else
                    {
                        requestType = eAssetTypes.NPVR;
                        assetRequestID = npvrID;
                        AssetsToGet = new AssetBookmarkRequest() { AssetID = assetRequestID, AssetType = eAssetTypes.NPVR };
                    }
                    var res = new AssetsBookmarksLoader(groupID, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID, new List<AssetBookmarkRequest>() { AssetsToGet })
                    {
                        DomainId = initObj.DomainID,
                        Platform = initObj.Platform.ToString()
                    }.Execute() as AssetsBookmarksResponse;

                    Bookmark usersBookmark = null;
                    bool isBookmarkFound = false;
                    if (res != null)
                    {
                        foreach (AssetBookmarks assetBookmark in res.AssetsBookmarks)
                        {
                            if (requestType == assetBookmark.AssetType && assetRequestID == assetBookmark.AssetID)
                            {
                                usersBookmark = assetBookmark.Bookmarks.FirstOrDefault(user => user.User.m_sSiteGUID == initObj.SiteGuid);
                                if (usersBookmark != null)
                                {
                                    isBookmarkFound = true;
                                    break;
                                }
                            }
                        }
                    }
                    mediaMark = new MediaMarkObject()
                    {
                        nGroupID = groupID,
                        nMediaID = int.Parse(assetRequestID),
                        sDeviceID = initObj.UDID,
                        sSiteGUID = initObj.SiteGuid,
                        sDeviceName = string.Empty
                    };
                    if (isBookmarkFound)
                    {
                        mediaMark.nLocationSec = usersBookmark.Location;
                        mediaMark.eStatus = MediaMarkObject.MediaMarkObjectStatus.OK;
                    }
                    else
                    {
                        mediaMark.nLocationSec = 0;
                        mediaMark.eStatus = MediaMarkObject.MediaMarkObjectStatus.FAILED;
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return mediaMark;
        }
        #endregion

        #region Purchase

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return priceReason;
        }

        [PrivateMethod]
        public bool IsItemPurchased(InitializationObject initObj, int iFileID, string sUserGuid)
        {
            bool bRet = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsItemPurchased", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, sUserGuid, 0, null, groupId, initObj.Platform))
                {
                    return false;
                }
                try
                {
                    IImplementation impl = WSUtils.GetImplementation(groupId, initObj);
                    bRet = impl.IsItemPurchased(iFileID, sUserGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return bRet;
        }

        [PrivateMethod]
        public PermittedMediaContainer[] GetUserPermittedItems(InitializationObject initObj)
        {
            PermittedMediaContainer[] permittedMediaContainer = { };

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermittedItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    var permitted = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermittedItems(initObj.SiteGuid);

                    if (permitted != null)
                        permittedMediaContainer = permitted.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return permittedMediaContainer;
        }

        //Search medias by meta
        public List<Media> GetSubscriptionMedia(InitializationObject initObj, string sSubID, string picSize, TVPApi.OrderBy orderBy)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        public List<Media> GetSubscriptionMedias(InitializationObject initObj, string[] sSubID, string picSize, TVPApi.OrderBy orderBy)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        [PrivateMethod]
        public PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj)
        {
            PermittedSubscriptionContainer[] permitedSubscriptions = new PermittedSubscriptionContainer[] { };
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermitedSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    var permitted = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermitedSubscriptions(initObj.SiteGuid);

                    if (permitted != null)
                        permitedSubscriptions = permitted.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return permitedSubscriptions;
        }

        [PrivateMethod]
        public List<PermittedPackages> GetUserPermittedPackages(InitializationObject initObj, string picSize)
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
                        APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass)
                        {
                            IgnoreFilter = true,
                            SearchTokenSignature = string.Concat("Base ID=", psc.m_sSubscriptionCode),
                            GroupID = groupId,
                            Platform = initObj.Platform,
                            dictMetas = dict,
                            WithInfo = false,
                            PageSize = 1,
                            PictureSize = picSize,
                            PageIndex = 0,
                            OrderBy = TVPApi.OrderBy.ABC,
                            MetaValues = psc.m_sSubscriptionCode,
                            Country = new TVPApiModule.Services.ApiUsersService(groupId, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()),
                            UseFinalEndDate = "true",
                            SiteGuid = initObj.SiteGuid
                        };

                        TVPPro.SiteManager.DataEntities.dsItemInfo ds = searchLoader.Execute();
                        if (ds.Item.Rows.Count > 0)
                            pp.Package = new Media(ds.Item[0], initObj, groupId, false);

                        permittedPackages.Add(pp);
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return permittedPackages;
        }

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";

            return response;
        }

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [PrivateMethod]
        public string ChargeUserForMediaFile(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon)
        {
            string response = string.Empty;

            // get the client IP from header/method parameters
            string clientIp = string.IsNullOrEmpty(sUserIP) ? SiteHelper.GetClientIP() : sUserIP;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserForMediaFile", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChargeUserForMediaFile(iPrice, sCurrency, iFileID, sPPVModuleCode, sCoupon, clientIp, initObj.SiteGuid, initObj.UDID, string.Empty, string.Empty, string.Empty);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }



        [PrivateMethod]
        public string ChargeUserForMediaSubscription(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID)
        {
            string response = string.Empty;

            // get the client IP from header/method parameters
            string clientIp = string.IsNullOrEmpty(sUserIP) ? SiteHelper.GetClientIP() : sUserIP;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserForMediaSubscription", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                // Tokenization: validate udid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, 0, sUDID, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChargeUserForSubscription(iPrice, sCurrency, sSubscriptionID, sCouponCode, clientIp, initObj.SiteGuid, sExtraParameters, sUDID, string.Empty, string.Empty);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }


        [PrivateMethod]
        public string DummyChargeUserForSubscription(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID)
        {
            string response = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DummyChargeUserForSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                // Tokenization: validate udid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, 0, sUDID, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).DummyChargeUserForSubscription(iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, initObj.SiteGuid, sExtraParameters, sUDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

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
                        MediaFileItemPricesContainer[] tmpRes = new ApiConditionalAccessService(groupId, initObj.Platform).GetItemsPrice(new int[] { fileID }, initObj.SiteGuid, initObj.UDID, bOnlyLowest);
                        if (tmpRes != null)
                            al.AddRange(tmpRes);
                    }

                    itemPrices = (MediaFileItemPricesContainer[])al.ToArray(typeof(MediaFileItemPricesContainer));
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return itemPrices;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return res;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return res;
        }

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return transactions;
        }

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            if (items == null)
            {
                items = new PermittedMediaContainer[0];
            }

            return items;
        }

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            if (items == null)
            {
                items = new PermittedSubscriptionContainer[0];
            }

            return items;
        }

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return items;
        }
        #endregion

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return fResponse;
        }

        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return oResponse.ToString();
        }

        [PrivateMethod]
        public bool AddUserSocialAction(InitializationObject initObj, int iMediaID, SocialAction action, SocialPlatform socialPlatform)
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return bResponse;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return bResponse;
        }

        public int GetVoteRatio(InitializationObject initObj, int iMediaID)
        {
            int nResponse = 0;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetVoteRatio", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    //XXX: Fix this to be unified Enum
                    // nResponse = TVPPro.SiteManager.Helper.VotesHelper.GetVotingRatio(initObj.SiteGuid);
                    nResponse = TVPApiModule.Helper.VotesHelper.GetVotingRatio(initObj.SiteGuid, groupId, initObj.Platform);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return nResponse;
        }

        [PrivateMethod]
        public string GetMediaLicenseLink(InitializationObject initObj, int mediaFileID, string baseLink)
        {
            string sResponse = string.Empty;

            string ip = SiteHelper.GetClientIP();
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetMediaLicenseLink", initObj.ApiUser, initObj.ApiPass, ip);

            if (groupId > 0)
            {
                try
                {

                    IImplementation impl = WSUtils.GetImplementation(groupId, initObj);
                    sResponse = impl.GetMediaLicenseLink(initObj, groupId, mediaFileID, baseLink, ip);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sResponse;
        }

        [PrivateMethod]
        public string GetMediaLicenseLinkWithIP(InitializationObject initObj, int mediaFileID, string baseLink, string clientIP)
        {
            string sResponse = string.Empty;


            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetMediaLicenseLinkWithIP", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {

                    IImplementation impl = WSUtils.GetImplementation(groupId, initObj);
                    sResponse = impl.GetMediaLicenseLink(initObj, groupId, mediaFileID, baseLink, clientIP);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sResponse;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sResponse;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lResponse;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return bResponse;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return bResponse;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return bResponse;
        }

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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sRet;
        }

        public EPGChannelObject[] GetEPGChannels(InitializationObject initObj, string sPicSize, TVPApi.OrderBy orderBy)
        {
            EPGChannelObject[] sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGChannels", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    sRet = new ApiApiService(groupId, initObj.Platform).GetEPGChannel(sPicSize);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sRet;
        }

        public List<Program> GetEPGProgramsByIds(InitializationObject initObj, TVPApiModule.Objects.Enums.ProgramIdType programIdType, List<string> programIds, int pageSize, int pageIndex)
        {
            List<Program> ret = new List<Program>();
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGProgramsByIds", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    ProgramObj programObj;
                    Program program;
                    switch (programIdType)
                    {
                        case TVPApiModule.Objects.Enums.ProgramIdType.EXTERNAL:

                            var collection = new EPGProgramsByProgramsIdentefierLoader(groupId, SiteHelper.GetClientIP(), pageSize, pageIndex, programIds, 0, default(Language)).Execute() as List<EPGChannelProgrammeObject>;
                            foreach (var obj in collection)
                            {
                                programObj = new ProgramObj();
                                programObj.m_oProgram = obj as EPGChannelProgrammeObject;

                                // convert to local object
                                program = new Program()
                                {
                                    m_oProgram = obj,
                                    AssetType = eAssetTypes.EPG
                                };
                                ret.Add(program);
                            }
                            break;

                        case TVPApiModule.Objects.Enums.ProgramIdType.INTERNAL:
                            List<int> pidsToInt = programIds.Select(id => int.Parse(id)).ToList<int>();
                            foreach (ProgramObj obj in (new EpgProgramDetailsLoader(groupId, SiteHelper.GetClientIP(), pageSize, pageIndex, pidsToInt).Execute() as List<BaseObject>))
                            {
                                // convert to local object
                                program = new Program()
                                {
                                    m_oProgram = obj.m_oProgram,
                                    AssetType = eAssetTypes.EPG,
                                    AssetId = obj.AssetId,
                                    m_dUpdateDate = obj.m_dUpdateDate
                                };
                                ret.Add(program);
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";


            return ret;
        }

        public EPGChannelProgrammeObject[] GetEPGChannelProgrammeByDates(InitializationObject initObj, string channelID, string picSize, DateTime fromDate, DateTime toDate, int utcOffset)
        {
            EPGChannelProgrammeObject[] sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGChannelProgrammeByDates", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {

                    List<int> channelIDs = new List<int>() { int.Parse(channelID) };

                    APIEPGLoader loader = new APIEPGLoader(groupId, initObj.Platform.ToString(), SiteHelper.GetClientIP(), 0, 0, channelIDs, EpgSearchType.ByDate, fromDate, toDate, 0, 0, initObj.Locale.LocaleLanguage);

                    loader.DeviceId = initObj.UDID;
                    loader.SiteGuid = initObj.SiteGuid;

                    var loaderRes = loader.Execute() as List<BaseObject>;
                    if (loaderRes != null && loaderRes.Count() > 0)
                        sRet = (loaderRes[0] as EPGMultiChannelProgrammeObject).EPGChannelProgrammeObject.ToArray();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";


            return sRet;
        }

        public EPGChannelProgrammeObject[] GetEPGChannelsPrograms(InitializationObject initObj, string sEPGChannelID, string sPicSize, EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        {
            EPGChannelProgrammeObject[] sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGChannelsPrograms", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    APIEPGLoader loader;
                    List<int> channelIDs = new List<int>() { int.Parse(sEPGChannelID) };
                    DateTime _offsetNow = DateTime.UtcNow.AddHours(iUTCOffSet);
                    switch (oUnit)
                    {
                        case EPGUnit.Days:
                            DateTime from = new DateTime(_offsetNow.Year, _offsetNow.Month, _offsetNow.Day, 0, 0, 0).AddDays(iFromOffset).AddHours(-iUTCOffSet);
                            DateTime to = new DateTime(_offsetNow.Year, _offsetNow.Month, _offsetNow.Day, 0, 0, 0).AddDays(iToOffset).AddHours(-iUTCOffSet);
                            loader = new APIEPGLoader(groupId, initObj.Platform.ToString(), SiteHelper.GetClientIP(), 0, 0, channelIDs, EpgSearchType.ByDate, from, to, 0, 0, initObj.Locale.LocaleLanguage);
                            break;
                        case EPGUnit.Hours:
                            loader = new APIEPGLoader(groupId, initObj.Platform.ToString(), SiteHelper.GetClientIP(), 0, 0, channelIDs, EpgSearchType.ByDate, _offsetNow.AddHours(iFromOffset), _offsetNow.AddHours(iToOffset), 0, 0, initObj.Locale.LocaleLanguage);
                            break;
                        case EPGUnit.Current:
                            loader = new APIEPGLoader(groupId, initObj.Platform.ToString(), SiteHelper.GetClientIP(), 0, 0, channelIDs, EpgSearchType.Current, _offsetNow, _offsetNow, iFromOffset, iToOffset, initObj.Locale.LocaleLanguage);
                            break;
                        default:
                            loader = new APIEPGLoader(groupId, initObj.Platform.ToString(), SiteHelper.GetClientIP(), 0, 0, channelIDs, EpgSearchType.Current, _offsetNow, _offsetNow, iFromOffset, iToOffset, initObj.Locale.LocaleLanguage);
                            break;
                    }

                    loader.DeviceId = initObj.UDID;
                    loader.SiteGuid = initObj.SiteGuid;

                    var loaderRes = loader.Execute() as List<BaseObject>;
                    if (loaderRes != null && loaderRes.Count() > 0)
                        sRet = (loaderRes[0] as EPGMultiChannelProgrammeObject).EPGChannelProgrammeObject.ToArray();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sRet;
        }

        public List<EPGMultiChannelProgrammeObject> GetEPGMultiChannelProgram(InitializationObject initObj, string[] sEPGChannelID, string sPicSize, EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        {
            List<EPGMultiChannelProgrammeObject> sRet = new List<EPGMultiChannelProgrammeObject>();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGMultiChannelProgram", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    APIEPGLoader loader;
                    List<int> channelIDs = sEPGChannelID.Distinct().Select(c => int.Parse(c)).ToList();

                    DateTime _offsetNow = DateTime.UtcNow.AddHours(iUTCOffSet);
                    switch (oUnit)
                    {
                        case EPGUnit.Days:
                            DateTime from = new DateTime(_offsetNow.Year, _offsetNow.Month, _offsetNow.Day, 0, 0, 0).AddDays(iFromOffset).AddHours(-iUTCOffSet);
                            DateTime to = new DateTime(_offsetNow.Year, _offsetNow.Month, _offsetNow.Day, 0, 0, 0).AddDays(iToOffset).AddHours(-iUTCOffSet);
                            loader = new APIEPGLoader(groupId, initObj.Platform.ToString(), SiteHelper.GetClientIP(), 0, 0, channelIDs, 
                                EpgSearchType.ByDate, from, to, 0, 0, initObj.Locale.LocaleLanguage);
                            break;
                        case EPGUnit.Hours:
                            loader = new APIEPGLoader(groupId, initObj.Platform.ToString(), SiteHelper.GetClientIP(), 0, 0, channelIDs, EpgSearchType.ByDate, _offsetNow.AddHours(iFromOffset), _offsetNow.AddHours(iToOffset), 0, 0, initObj.Locale.LocaleLanguage);
                            break;
                        case EPGUnit.Current:
                            loader = new APIEPGLoader(groupId, initObj.Platform.ToString(), SiteHelper.GetClientIP(), 0, 0, channelIDs, EpgSearchType.Current, _offsetNow, _offsetNow, iFromOffset, iToOffset, initObj.Locale.LocaleLanguage);
                            break;
                        default:
                            loader = new APIEPGLoader(groupId, initObj.Platform.ToString(), SiteHelper.GetClientIP(), 0, 0, channelIDs, EpgSearchType.Current, _offsetNow, _offsetNow, iFromOffset, iToOffset, initObj.Locale.LocaleLanguage);
                            break;
                    }

                    loader.DeviceId = initObj.UDID;
                    loader.SiteGuid = initObj.SiteGuid;
                    foreach (EPGMultiChannelProgrammeObject epg in (loader.Execute() as List<BaseObject>))
                    {
                        sRet.Add(epg);
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sRet;
        }

        public GroupRule[] GetGroupMediaRules(InitializationObject initObj, int mediaID)
        {
            GroupRule[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupMediaRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    // get Site GUID (put 0 by default)
                    int siteGuid = 0;
                    int.TryParse(initObj.SiteGuid, out siteGuid);

                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetGroupMediaRules(mediaID, siteGuid, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        public List<TVPApi.Channel> GetChannelsList(InitializationObject initObj, string sPicSize)
        {
            List<TVPApi.Channel> sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetChannelsList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    sRet = ChannelHelper.GetChannelsList(initObj, sPicSize, groupId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return sRet;
        }

        public List<UserVote> GetUsersVotes(InitializationObject initObj, long unixStartDate, long unixEndDate)
        {
            List<UserVote> sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUsersVotes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId == 134)
            {
                try
                {
                    sRet = TVPApiModule.Helper.VotesHelper.GetAllVotesByDates(unixStartDate, unixEndDate, groupId, initObj.Platform);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return sRet;
        }


        public int GetMediaVotes(InitializationObject initObj, int iMediaID)
        {
            int nResponse = 0;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetVoteRatio", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    //XXX: Fix this to be unified Enum
                    // nResponse = TVPPro.SiteManager.Helper.VotesHelper.GetVotingRatio(initObj.SiteGuid);
                    nResponse = TVPApiModule.Helper.VotesHelper.GetVotesByMediaID(iMediaID, groupId, initObj.Platform);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return nResponse;
        }

        [PrivateMethod]
        public string GetMediaLicenseData(InitializationObject initObj, int iMediaFileID, int iMediaID)
        {
            string sResponse = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetMediaLicenseData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    IImplementation impl = WSUtils.GetImplementation(groupId, initObj);
                    sResponse = impl.GetMediaLicenseData(iMediaFileID, iMediaID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sResponse;
        }

        #region MessageBox
        [PrivateMethod]
        public void SendMessage(InitializationObject initObj, string sSiteGuid, string sRecieverUDID, int iMediaID, int iMediaTypeID, int iLocation, string sAction, string sUsername, string sPassword)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SendMessage", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                        !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, sSiteGuid, 0, null, groupId, initObj.Platform))
                {
                    return;
                }

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
        }

        [PrivateMethod]
        public MBMessage GetMessage(InitializationObject initObj)
        {
            MBMessage msg = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetMessage", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                // Tokenization: validate device
                if (AuthorizationManager.IsTokenizationEnabled() &&
                        !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, initObj.UDID, groupId, initObj.Platform))
                {
                    return null;
                }

                msg = MessageBox.Instance.GetNewMessage(initObj.UDID);

            }

            return msg;
        }
        #endregion

        #region EPG related

        #region EPGComments

        public List<TVPPro.SiteManager.Objects.EPGComment> GetEPGCommentsList(InitializationObject initObj, int epgProgramID, int pageSize, int pageIndex)
        {
            List<TVPPro.SiteManager.Objects.EPGComment> retVal = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGCommentsList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    int language = TextLocalizationManager.Instance.GetTextLocalization(groupId, initObj.Platform).GetLanguageDBID(initObj.Locale.LocaleLanguage);
                    retVal = CommentHelper.GetEPGCommentsList(groupId, initObj.Platform, initObj.Locale.LocaleLanguage, epgProgramID, pageSize, pageIndex);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retVal;
        }

        [PrivateMethod]
        public string AddEPGComment(InitializationObject initObj, int epgProgramID, string contentText, string header, string subHeader, string writer, bool autoActive)
        {
            string retVal = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "AddEPGComment", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    retVal = CommentHelper.AddEPGComment(groupId, initObj.Platform, initObj.Locale.LocaleLanguage, initObj.SiteGuid, initObj.UDID, epgProgramID, contentText, initObj.Locale.LocaleCountry, header, subHeader, writer, autoActive).ToString();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retVal;
        }

        #endregion

        public List<EPGChannelProgrammeObject> SearchEPGPrograms(InitializationObject initObj, string searchText, int pageSize, int pageIndex)
        {
            List<EPGChannelProgrammeObject> retVal = null;
            List<BaseObject> loaderResult = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SearchEPGPrograms", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    int language = TextLocalizationManager.Instance.GetTextLocalization(groupId, initObj.Platform).GetLanguageDBID(initObj.Locale.LocaleLanguage);
                    DateTime _startTime, _endTime;

                    _startTime = DateTime.UtcNow.AddDays(-ApplicationConfiguration.Current.TVPApiConfiguration.EPGSearchOffsetDays.Value);
                    _endTime = DateTime.UtcNow.AddDays(ApplicationConfiguration.Current.TVPApiConfiguration.EPGSearchOffsetDays.Value);

                    loaderResult = new APIEPGSearchLoader(groupId, initObj.Platform.ToString(), SiteHelper.GetClientIP(), pageSize, pageIndex, searchText, _startTime, _endTime)
                    {
                        Culture = initObj.Locale.LocaleLanguage,
                        SiteGuid = initObj.SiteGuid,
                        DomainId = initObj.DomainID
                    }.Execute() as List<BaseObject>;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            retVal = new List<EPGChannelProgrammeObject>();
            foreach (ProgramObj p in loaderResult)
            {
                if (p != null)
                    retVal.Add(p.m_oProgram);
                else
                    retVal.Add(null);
            }

            return retVal;
        }

        public List<string> GetEPGAutoComplete(InitializationObject initObj, string searchText, int pageSize, int pageIndex)
        {
            List<string> retVal = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "EPGAutoComplete", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    int language = TextLocalizationManager.Instance.GetTextLocalization(groupId, initObj.Platform).GetLanguageDBID(initObj.Locale.LocaleLanguage);

                    DateTime _startTime, _endTime;

                    _startTime = DateTime.UtcNow.AddDays(-ApplicationConfiguration.Current.TVPApiConfiguration.EPGSearchOffsetDays.Value);
                    _endTime = DateTime.UtcNow.AddDays(ApplicationConfiguration.Current.TVPApiConfiguration.EPGSearchOffsetDays.Value);
                    ;

                    retVal = new APIEPGAutoCompleteLoader(groupId, initObj.Platform.ToString(), SiteHelper.GetClientIP(), pageSize, pageIndex, searchText, _startTime, _endTime)
                    {
                        Culture = initObj.Locale.LocaleLanguage,
                        SiteGuid = initObj.SiteGuid,
                        DomainId = initObj.DomainID
                    }.Execute() as List<string>;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retVal;
        }

        #endregion

        public List<AssetStatsResultDTO> GetAssetsStatsForTimePeriod(InitializationObject initObj, int pageSize, int pageIndex, List<int> assetsIDs, StatsType assetType, DateTime startTime, DateTime endTime)
        {
            
            List<AssetStatsResultDTO> response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetAssetsStats", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    List<AssetStatsResult> retVal  = new AssetStatsLoader(groupId, SiteHelper.GetClientIP(), pageSize, pageIndex, assetsIDs, assetType, startTime, endTime)
                    {
                        Platform = initObj.Platform.ToString(),
                        DeviceId = initObj.UDID
                    }.Execute() as List<AssetStatsResult>;

                    response = retVal.ConvertAll(AssetStatsResultDTO.ConvertToDTO);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        public List<AssetStatsResultDTO> GetAssetsStats(InitializationObject initObj, int pageSize, int pageIndex, List<int> assetsIDs, StatsType assetType)
        {
            List<AssetStatsResultDTO> response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetAssetsStats", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    List<AssetStatsResult> retVal = new AssetStatsLoader(groupId, SiteHelper.GetClientIP(), pageSize, pageIndex, assetsIDs, assetType, DateTime.MinValue, DateTime.MaxValue)
                    {
                        Platform = initObj.Platform.ToString(),
                        DeviceId = initObj.UDID,
                        SiteGuid = initObj.SiteGuid
                    }.Execute() as List<AssetStatsResult>;
                    response = retVal.ConvertAll(AssetStatsResultDTO.ConvertToDTO);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        public List<EPGChannelProgrammeObject> SearchEPGByAndOrList(InitializationObject initObj, List<KeyValue> orList, List<KeyValue> andList, int pageSize, int pageIndex)
        {
            List<EPGChannelProgrammeObject> retVal = null;
            List<BaseObject> loaderResult = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SearchEPGByAndOrList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    int language = TextLocalizationManager.Instance.GetTextLocalization(groupID, initObj.Platform).GetLanguageDBID(initObj.Locale.LocaleLanguage);
                    DateTime _startTime, _endTime;

                    _startTime = DateTime.UtcNow.AddDays(-ApplicationConfiguration.Current.TVPApiConfiguration.EPGSearchOffsetDays.Value);
                    _endTime = DateTime.UtcNow.AddDays(ApplicationConfiguration.Current.TVPApiConfiguration.EPGSearchOffsetDays.Value);

                    loaderResult = new APIEPGSearchLoader(groupID, initObj.Platform.ToString(), SiteHelper.GetClientIP(), pageSize, pageIndex, andList, orList, true, _startTime, _endTime)
                    {
                        Culture = initObj.Locale.LocaleLanguage,
                        SiteGuid = initObj.SiteGuid
                    }.Execute() as List<BaseObject>;

                    retVal = new List<EPGChannelProgrammeObject>();
                    foreach (ProgramObj p in loaderResult)
                    {
                        if (p != null)
                            retVal.Add(p.m_oProgram);
                        else
                            retVal.Add(null);
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retVal;
        }

        public List<BaseCrowdsourceItem> GetCrowdsourceFeed(InitializationObject initObj, int pageSize, long epochLastTime)
        {
            List<BaseCrowdsourceItem> retVal = null;
            string ip = SiteHelper.GetClientIP();
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCrowdsourceFeed", initObj.ApiUser, initObj.ApiPass, ip);

            if (groupId > 0)
            {
                try
                {
                    retVal = new APICrowdsourceLoader(groupId, initObj.Locale.LocaleLanguage, pageSize, epochLastTime, ip, initObj.Platform)
                    {
                        DeviceId = initObj.UDID,
                        SiteGuid = initObj.SiteGuid
                    }.Execute() as List<BaseCrowdsourceItem>;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retVal;
        }

        #region Collections

        public List<Media> GetBundleMedia(InitializationObject initObj, eBundleType bundleType, int bundleId,
            ApiObjects.SearchObjects.OrderBy orderBy,
            ApiObjects.SearchObjects.OrderDir orderDir, string mediaType, int pageIndex, int pageSize)
        {
            List<Media> lstMedia = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetBundleMedia", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupID > 0)
            {
                try
                {
                    var orderObj = new ApiObjects.SearchObjects.OrderObj()
                    {
                        m_eOrderDir = orderDir,
                        m_eOrderBy = orderBy
                    };

                    APIBundleMediaLoader loader = new APIBundleMediaLoader(bundleId, mediaType, orderObj, groupID, groupID, initObj.Platform.ToString(), clientIp, string.Empty, pageIndex, pageSize, bundleType)
                    {
                        Culture = initObj.Locale.LocaleLanguage,
                        SiteGuid = initObj.SiteGuid,
                        DomainId = initObj.DomainID
                    };
                    dsItemInfo returnedRows = loader.Execute() as dsItemInfo;
                    if (returnedRows != null && returnedRows.Tables != null && returnedRows.Tables[0].Rows != null && returnedRows.Tables[0].Rows.Count > 0)
                    {
                        lstMedia = new List<Media>();
                        foreach (dsItemInfo.ItemRow row in returnedRows.Item.Rows)
                        {
                            lstMedia.Add(new Media(row, initObj, groupID, false));
                        }
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return lstMedia;
        }

        public bool DoesBundleContainMedia(InitializationObject initObj, eBundleType bundleType, int bundleId, int mediaId, string mediaType)
        {
            bool isMediaInBundle = false;

            string clientIp = SiteHelper.GetClientIP();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetBundleMedia", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupID > 0)
            {
                try
                {
                    //OrderObj orderObj = new OrderObj() { m_eOrderDir = orderDir, m_eOrderBy = orderBy };
                    BundleContainingMediaLoader loader = new BundleContainingMediaLoader()
                    {
                        BundleID = bundleId,
                        BundleType = bundleType,
                        MediaID = mediaId,
                        GroupID = groupID,
                        MediaType = mediaType,
                    };

                    isMediaInBundle = (bool)loader.Execute();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return isMediaInBundle;
        }

        public BuzzWeightedAverScore GetBuzzMeterData(InitializationObject initObj, string sKey)
        {
            BuzzWeightedAverScore buzzMeterData = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetBuzzMeterData", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupID > 0)
            {
                try
                {
                    BuzzMeterLoader loader = new BuzzMeterLoader(groupID, sKey);
                    buzzMeterData = loader.Execute() as BuzzWeightedAverScore;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return buzzMeterData;
        }



        public TVPApiModule.Objects.Responses.UnifiedSearchResponse GetBundleAssets(InitializationObject initObj, eBundleType bundleType, int bundleId,
            ApiObjects.SearchObjects.OrderBy orderBy, ApiObjects.SearchObjects.OrderDir orderDir, string mediaType, int pageIndex, int pageSize)
        {
            TVPApiModule.Objects.Responses.UnifiedSearchResponse response = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetBundleMedia", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupID > 0)
            {
                try
                {
                    ApiObjects.SearchObjects.OrderObj orderObj = new ApiObjects.SearchObjects.OrderObj()
                    {
                        m_eOrderDir = orderDir,
                        m_eOrderBy = orderBy
                    };
                    APIUnifiedBundleMediaLoader loader =
                        new APIUnifiedBundleMediaLoader(bundleId, mediaType, orderObj,
                            groupID, groupID, initObj.Platform, clientIp, string.Empty, pageIndex, pageSize, bundleType, initObj.DomainID, initObj.Locale.LocaleLanguage)
                        {
                            Culture = initObj.Locale.LocaleLanguage,
                            SiteGuid = initObj.SiteGuid,
                            DomainId = initObj.DomainID
                        };
                    response = loader.Execute() as TVPApiModule.Objects.Responses.UnifiedSearchResponse;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        #endregion

        [PrivateMethod]
        public TVPApiModule.Objects.Responses.AssetsBookmarksResponse GetAssetsBookmarks(InitializationObject initObj, List<SlimAssetRequest> assets)
        {
            TVPApiModule.Objects.Responses.AssetsBookmarksResponse sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetAssetsBookmarks", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    List<AssetBookmarkRequest> assetsToSend = new List<AssetBookmarkRequest>();
                    bool isInvalidAsset = false;
                    foreach (SlimAssetRequest asset in assets)
                    {
                        AssetBookmarkRequest assetToAdd = new AssetBookmarkRequest();
                        assetToAdd.AssetID = asset.AssetID;
                        switch (asset.AssetType.ToUpper())
                        {
                            case "EPG":
                                assetToAdd.AssetType = eAssetTypes.EPG;
                                break;
                            case "MEDIA":
                                assetToAdd.AssetType = eAssetTypes.MEDIA;
                                break;
                            case "NPVR":
                                assetToAdd.AssetType = eAssetTypes.NPVR;
                                break;
                            default:
                                isInvalidAsset = true;
                                break;
                        }

                        if (isInvalidAsset)
                        {
                            sRet = new TVPApiModule.Objects.Responses.AssetsBookmarksResponse(null, (int)eStatus.BadRequest, "BadRequest", 0);
                            return sRet;
                        }

                        assetsToSend.Add(assetToAdd);
                    }
                    var res = new AssetsBookmarksLoader(groupId, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID, assetsToSend)
                    {
                        DomainId = initObj.DomainID,
                        Platform = initObj.Platform.ToString()
                    }.Execute() as AssetsBookmarksResponse;

                    sRet = new TVPApiModule.Objects.Responses.AssetsBookmarksResponse(res.AssetsBookmarks, res.Status.Code, res.Status.Message, res.m_nTotalItems);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";

            return sRet;
        }

        [PrivateMethod]
        public DomainLastPositionResponse GetDomainLastPosition(InitializationObject initObj, int mediaID)
        {
            DomainLastPositionResponse sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainLastPosition", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    if (mediaID == 0 || initObj.DomainID == 0)
                    {
                        sRet = new DomainLastPositionResponse()
                        {
                            m_sStatus = "INVALID_PARAMS"
                        };
                        return sRet;
                    }
                    AssetBookmarkRequest mediaAssets = new AssetBookmarkRequest() { AssetID = mediaID.ToString(), AssetType = eAssetTypes.MEDIA };
                    var res = new AssetsBookmarksLoader(groupId, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID, new List<AssetBookmarkRequest>() { mediaAssets })
                    {
                        DomainId = initObj.DomainID,
                        Platform = initObj.Platform.ToString()
                    }.Execute() as AssetsBookmarksResponse;

                    List<LastPosition> mediaBookmarks = new List<LastPosition>();
                    foreach (AssetBookmarks assetBookmark in res.AssetsBookmarks)
                    {
                        foreach (Bookmark bookmark in assetBookmark.Bookmarks)
                        {
                            mediaBookmarks.Add(new LastPosition(bookmark));
                        }
                    }

                    sRet = new DomainLastPositionResponse()
                    {
                        m_lPositions = mediaBookmarks,
                        m_sStatus = res.Status.Message
                    };
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";

            return sRet;
        }

        [PrivateMethod]
        public List<RecordedEPGChannelProgrammeObject> GetRecordings(InitializationObject initObj, int pageSize, int pageIndex,
            NPVRSearchBy searchBy, int epgChannelID, RecordingStatus recordingStatus, List<string> recordingIDs, List<int> programIDs, 
            List<string> seriesIDs, DateTime startDate, RecordedEPGOrderObj recordedEPGOrderObj, int? version, string timeFormat)
        {
            List<RecordedEPGChannelProgrammeObject> res = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetRecordings", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    RecordedEPGOrderObj catalogOrderObj = new RecordedEPGOrderObj()
                    {
                        m_eOrderBy = recordedEPGOrderObj.m_eOrderBy,
                        m_eOrderDir = recordedEPGOrderObj.m_eOrderDir,
                    };
                    res = new NPVRRetrieveLoader(groupId, SiteHelper.GetClientIP(), initObj.SiteGuid, pageSize, pageIndex, searchBy, 
                        epgChannelID, recordingStatus, recordingIDs, programIDs, seriesIDs, startDate, catalogOrderObj, version, timeFormat)
                    {
                        Platform = initObj.Platform.ToString()
                    }.Execute() as List<RecordedEPGChannelProgrammeObject>;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";

            return res;
        }
               

        [PrivateMethod]
        public List<RecordedSeriesObject> GetSeriesRecordings(InitializationObject initObj, int pageSize, int pageIndex, RecordedEPGOrderObj recordedEPGOrderObj
            ,int? version, string seriesId = "", int? seasonNumber = -1)
        {
            List<RecordedSeriesObject> res = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSeriesRecordings", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    RecordedEPGOrderObj catalogOrderObj = new RecordedEPGOrderObj()
                    {
                        m_eOrderBy = recordedEPGOrderObj.m_eOrderBy,
                        m_eOrderDir = recordedEPGOrderObj.m_eOrderDir,
                    };

                    if (!seasonNumber.HasValue)
                        seasonNumber = -1;

                    res = new NPVRSeriesLoader(groupId, SiteHelper.GetClientIP(), initObj.SiteGuid, pageSize, pageIndex, catalogOrderObj, seriesId, seasonNumber.Value, version)
                    {
                        Platform = initObj.Platform.ToString()
                    }.Execute() as List<RecordedSeriesObject>;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";

            return res;
        }

        
        public TVPApiModule.Objects.Responses.UnifiedSearchResponse SearchAssets(InitializationObject initObj, List<int> filter_types, string filter, string order_by,
            List<string> with, int page_index, int? page_size, string request_id)
        {
            TVPApiModule.Objects.Responses.UnifiedSearchResponse response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "UnifiedSearch", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    if (filter.Length > 4096)
                    {
                        response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                        response.Status = ResponseUtils.ReturnBadRequestStatus("too long filter");
                        return response;
                    }

                    if (page_size == null)
                    {
                        page_size = 25;
                    }
                    else if (page_size > 50)
                    {
                        page_size = 50;
                    }
                    else if (page_size < 5)
                    {
                        response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                        response.Status = ResponseUtils.ReturnBadRequestStatus("page_size range can be between 5 and 50");
                        return response;
                    }

                    ApiObjects.SearchObjects.OrderObj order = null;

                    if (string.IsNullOrEmpty(order_by))
                    {
                        order = new ApiObjects.SearchObjects.OrderObj()
                        {
                            m_eOrderBy = ApiObjects.SearchObjects.OrderBy.RELATED,
                            m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC
                        };
                    }
                    else
                    {
                        order = CreateOrderObject(order_by);

                        if (order == null)
                        {
                            response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                            response.Status = ResponseUtils.ReturnBadRequestStatus("invalid order_by value");
                            return response;
                        }
                    }

                    HashSet<string> validWithValues = new HashSet<string>() { "stats", "files" };

                    // validate with - make sure it contains only "stats" and/or "files"
                    if (with != null)
                    {
                        foreach (var currentValue in with)
                        {
                            if (!validWithValues.Contains(currentValue))
                            {
                                response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                                response.Status = ResponseUtils.ReturnBadRequestStatus(string.Format("Invalid with value: {0}", currentValue));
                                return response;
                            }
                        }
                    }

                    List<ePersonalFilter> personalFilters = null;

                    //if (personal_filters != null)
                    //{
                    //    personalFilters = new List<ePersonalFilter>();

                    //    // Converts strings to enum
                    //    foreach (var currentFilter in personal_filters)
                    //    {
                    //        if (currentFilter.ToLower() == "entitled_assets")
                    //        {
                    //            personalFilters.Add(ePersonalFilter.EntitledAssets);
                    //        }
                    //        else if (currentFilter.ToLower() == "geo_block")
                    //        {
                    //            personalFilters.Add(ePersonalFilter.GeoBlockRules);
                    //        }
                    //        else if (currentFilter.ToLower() == "parental")
                    //        {
                    //            personalFilters.Add(ePersonalFilter.ParentalRules);
                    //        }
                    //        // If it is not one of these three, return a bad request status
                    //        else
                    //        {
                    //            response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                    //            response.Status = ResponseUtils.ReturnBadRequestStatus(string.Format("Invalid personal filter value: {0}", currentFilter));
                    //            return response;
                    //        }
                    //    }
                    //}

                    response = new APIUnifiedSearchLoader(groupId, initObj.Platform, initObj.DomainID, SiteHelper.GetClientIP(), (int)page_size, page_index,
                        filter_types, filter, with, personalFilters, initObj.Locale.LocaleLanguage)
                    {
                        Order = order,
                        SiteGuid = initObj.SiteGuid,
                        DomainId = initObj.DomainID,
                        DeviceId = initObj.UDID
                    }.Execute() as TVPApiModule.Objects.Responses.UnifiedSearchResponse;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        /// <summary>
        /// Translates the order by string to the corresponding enum
        /// </summary>
        /// <param name="order_by"></param>
        /// <returns></returns>
        private static ApiObjects.SearchObjects.OrderObj CreateOrderObject(string order_by)
        {
            string orderBy = order_by.ToLower();

            ApiObjects.SearchObjects.OrderObj order = new ApiObjects.SearchObjects.OrderObj();

            switch (orderBy)
            {
                case "a_to_z":
                    order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.NAME;
                    order.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case "z_to_a":
                    order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.NAME;
                    order.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case "views":
                    order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.VIEWS;
                    order.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case "ratings":
                    order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.RATING;
                    order.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case "votes":
                    order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.VOTES_COUNT;
                    order.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case "newest":
                    order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE;
                    order.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case "relevancy":
                    order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.RELATED;
                    order.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case "likes":
                    order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER;
                    order.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case "oldest_first":
                    order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE;
                    order.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                default:
                    order = null;
                    break;
            }

            return order;
        }

        public TVPApiModule.Objects.Responses.AutocompleteResponse Autocomplete(InitializationObject initObj, List<int> filter_types,
            string query, string order_by, List<string> with, int? page_size)
        {
            TVPApiModule.Objects.Responses.AutocompleteResponse response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "Autocomplete", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    // Page size rules - according to spec.  10>=size>=1 is valid. default is 5.
                    if (page_size == null || page_size > 10 || page_size < 1)
                    {
                        page_size = 5;
                    }

                    // Translate order by string to order object
                    ApiObjects.SearchObjects.OrderObj order = null;

                    if (string.IsNullOrEmpty(order_by))
                    {
                        order = new ApiObjects.SearchObjects.OrderObj()
                        {
                            m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE,
                            m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC
                        };
                    }
                    else
                    {
                        order = CreateOrderObject(order_by);

                        if (order == null)
                        {
                            response = new TVPApiModule.Objects.Responses.AutocompleteResponse();
                            response.Status = ResponseUtils.ReturnBadRequestStatus("invalid order_by value");
                            return response;
                        }
                    }

                    HashSet<string> validWithValues = new HashSet<string>() { "images" };

                    // validate with - make sure it contains only "stats" and/or "files"
                    if (with != null)
                    {
                        foreach (var currentValue in with)
                        {
                            if (!validWithValues.Contains(currentValue))
                            {
                                response = new TVPApiModule.Objects.Responses.AutocompleteResponse();
                                response.Status = ResponseUtils.ReturnBadRequestStatus(string.Format("Invalid with value: {0}", currentValue));
                                return response;
                            }
                        }
                    }


                    List<ePersonalFilter> personalFilters = null;

                    //if (personal_filters != null)
                    //{
                    //    personalFilters = new List<ePersonalFilter>();

                    //    // Converts strings to enum
                    //    foreach (var currentFilter in personal_filters)
                    //    {
                    //        if (currentFilter.ToLower() == "entitled_assets")
                    //        {
                    //            personalFilters.Add(ePersonalFilter.EntitledAssets);
                    //        }
                    //        else if (currentFilter.ToLower() == "geo_block")
                    //        {
                    //            personalFilters.Add(ePersonalFilter.GeoBlockRules);
                    //        }
                    //        else if (currentFilter.ToLower() == "parental")
                    //        {
                    //            personalFilters.Add(ePersonalFilter.ParentalRules);
                    //        }
                    //        // If it is not one of these three, return a bad request status
                    //        else
                    //        {
                    //            response = new TVPApiModule.Objects.Responses.AutocompleteResponse();
                    //            response.Status = ResponseUtils.ReturnBadRequestStatus(string.Format("Invalid personal filter value: {0}", currentFilter));
                    //            return response;
                    //        }
                    //    }
                    //}

                    //// Create our own filter - only search in title
                    string filter = string.Format("(and name^'{0}')", query.Replace("'", "%27"));

                    object executedRespone = new APIAutocompleteLoader(groupId, initObj.Platform, initObj.DomainID, SiteHelper.GetClientIP(), (int)page_size, 0,
                        filter_types, filter, with, personalFilters, initObj.Locale.LocaleLanguage)
                    {
                        Order = order,
                        SiteGuid = initObj.SiteGuid,
                        DomainId = initObj.DomainID,
                        DeviceId = initObj.UDID
                    }.Execute();

                    if (executedRespone is AutocompleteResponse)
                    {
                        response = executedRespone as AutocompleteResponse;
                    }
                    else if (executedRespone is TVPApiModule.Objects.Responses.UnifiedSearchResponse)
                    {
                        response = new AutocompleteResponse(executedRespone as TVPApiModule.Objects.Responses.UnifiedSearchResponse);
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new TVPApiModule.Objects.Responses.AutocompleteResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new TVPApiModule.Objects.Responses.AutocompleteResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        [PrivateMethod]
        public WatchHistory WatchHistory(InitializationObject initObj, List<int> filter_types, eWatchStatus filter_status,
                                                                                 int? days, List<string> with, int? page_index, int page_size)
        {
            WatchHistory response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "WatchHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    // give default values
                    if (filter_status == eWatchStatus.Undefined)
                        filter_status = eWatchStatus.All;

                    // days - default value 7
                    if (days == null || days == 0)
                        days = 7;

                    // page index - default value 0
                    if (page_index == null)
                        page_index = 0;

                    // page size - 5 <= size <= 50
                    if (page_size == 0)
                        page_size = 25;
                    else
                    {
                        if (page_size > 50)
                            page_size = 50;
                        else
                        {
                            if (page_size < 5)
                            {
                                response = new WatchHistory();
                                response.Status = ResponseUtils.ReturnBadRequestStatus("page_size range can be between 5 and 50");
                                return response;
                            }
                        }
                    }

                    // fire request
                    response = new APIWatchHistoryLoader(groupId, initObj.Platform, initObj.DomainID, SiteHelper.GetClientIP(), page_size, (int)page_index,
                        filter_types, filter_status, with, (int)days, ApiObjects.SearchObjects.OrderDir.DESC)
                    {
                        SiteGuid = initObj.SiteGuid,
                        DomainId = initObj.DomainID
                    }.Execute() as WatchHistory;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new WatchHistory();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new WatchHistory();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            if (response == null || response.Status == null)
            {
                response = new WatchHistory();
                response.Status = ResponseUtils.ReturnGeneralErrorStatus();
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UnifiedSearchResponse GetChannelAssets(InitializationObject initObj,
            int kaltura_identifier,
            string filter,
            string order_by,
            List<string> with, int page_index, int? page_size)
        {
            TVPApiModule.Objects.Responses.UnifiedSearchResponse response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetChannelAssets", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    #region Paging

                    if (page_size == null)
                    {
                        page_size = 10;
                    }
                    else if (page_size > 20)
                    {
                        page_size = 20;
                    }
                    else if (page_size < 5)
                    {
                        response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                        response.Status = ResponseUtils.ReturnBadRequestStatus("page_size range can be between 5 and 20");
                        return response;
                    }

                    #endregion

                    #region With

                    HashSet<string> validWithValues = new HashSet<string>() { "stats", "files" };

                    // validate with - make sure it contains only "stats" and/or "files"
                    if (with != null)
                    {
                        foreach (var currentValue in with)
                        {
                            if (!validWithValues.Contains(currentValue))
                            {
                                response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                                response.Status = ResponseUtils.ReturnBadRequestStatus(string.Format("Invalid with value: {0}", currentValue));
                                return response;
                            }
                        }
                    }
                    #endregion

                    #region Order

                    ApiObjects.SearchObjects.OrderObj order = null;

                    if (string.IsNullOrEmpty(order_by))
                    {
                        order = new ApiObjects.SearchObjects.OrderObj()
                        {
                            m_eOrderBy = ApiObjects.SearchObjects.OrderBy.NONE,
                            m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC
                        };
                    }
                    else
                    {
                        order = CreateOrderObject(order_by);

                        if (order == null)
                        {
                            response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                            response.Status = ResponseUtils.ReturnBadRequestStatus("invalid order_by value");
                            return response;
                        }
                    }

                    #endregion

                    response = new APIInternalChannelLoader(groupId, initObj.Platform, SiteHelper.GetClientIP(), (int)page_size, page_index,
                        initObj.DomainID, initObj.SiteGuid, initObj.Locale.LocaleLanguage, with, string.Empty, filter, kaltura_identifier.ToString(), initObj.UDID)
                    {
                        Order = order
                    }.Execute() as TVPApiModule.Objects.Responses.UnifiedSearchResponse;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new TVPApiModule.Objects.Responses.UnifiedSearchResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId GetExternalAssets(InitializationObject initObj,
            string alias,
            string utc_offset,
            string free_param,
            List<string> with, int page_index, int? page_size)
        {
            TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetExternalAssets", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    #region Paging

                    if (page_size == null)
                    {
                        page_size = 10;
                    }
                    else if (page_size > 20)
                    {
                        page_size = 20;
                    }
                    else if (page_size < 5)
                    {
                        response = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId();
                        response.Status = ResponseUtils.ReturnBadRequestStatus("page_size range can be between 5 and 20");
                        return response;
                    }

                    #endregion

                    #region With

                    HashSet<string> validWithValues = new HashSet<string>() { "stats", "files" };

                    // validate with - make sure it contains only "stats" and/or "files"
                    if (with != null)
                    {
                        foreach (var currentValue in with)
                        {
                            if (!validWithValues.Contains(currentValue))
                            {
                                response = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId();
                                response.Status = ResponseUtils.ReturnBadRequestStatus(string.Format("Invalid with value: {0}", currentValue));
                                return response;
                            }
                        }
                    }
                    #endregion

                    #region UTC Offset

                    if (!string.IsNullOrEmpty(utc_offset))
                    {
                        double utcOffsetDouble;

                        if (!double.TryParse(utc_offset, out utcOffsetDouble))
                        {
                            response = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId();
                            response.Status = ResponseUtils.ReturnBadRequestStatus("UTC Offset must be a valid number between -12 and 12");
                            return response;
                        }
                        else if (utcOffsetDouble > 12 || utcOffsetDouble < -12)
                        {
                            response = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId();
                            response.Status = ResponseUtils.ReturnBadRequestStatus("UTC Offset must be a valid number between -12 and 12");
                            return response;
                        }
                    }

                    #endregion

                    string deviceType = System.Web.HttpContext.Current.Request.GetUserAgentString();

                    response = new APIRecommendationsLoader(groupId, initObj.Platform, SiteHelper.GetClientIP(), (int)page_size, page_index,
                        initObj.DomainID, initObj.SiteGuid, initObj.Locale.LocaleLanguage, with, initObj.UDID, deviceType, alias, utc_offset, string.Empty, string.Empty, free_param)
                    {
                    }.Execute() as TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        public PersonalAssetListResponse GetEnrichedPersonalData(InitializationObject initObj,
            PersonalAssetRequest[] assets,
            List<string> with)
        {
            PersonalAssetListResponse response = new PersonalAssetListResponse()
            {
                TotalItems = 0,
                Objects = new List<PersonalAssetInfo>(),
                Status = new TVPApiModule.Objects.Responses.Status()
            };

            if (assets == null)
            {
                assets = new List<PersonalAssetRequest>().ToArray();
            }

            if (with == null)
            {
                with = new List<string>();
            }

            // Validate with values
            foreach (var currentWith in with)
            {
                if (currentWith != "pricing" && currentWith != "bookmark")
                {
                    response.Status = ResponseUtils.ReturnBadRequestStatus(string.Format("Invalid with value: {0}", currentWith));
                    return response;
                }
            }

            Dictionary<string, PersonalAssetInfo> assetIdToPersonalAsset = new Dictionary<string, PersonalAssetInfo>();
            Dictionary<long, PersonalAssetInfo> fileToPersonalAsset = new Dictionary<long, PersonalAssetInfo>();

            // Create response list to be identical as request
            // +, map ids to objects in response

            response.TotalItems = assets.Length;
            response.Objects = new List<PersonalAssetInfo>();

            List<AssetFiles> assetFiles = new List<AssetFiles>();

            foreach (var asset in assets)
            {
                var type = eAssetTypes.UNKNOWN;

                if (asset.type != null)
                {
                    switch (asset.type.ToLower())
                    {
                        case "unknown":
                            {
                                type = eAssetTypes.UNKNOWN;
                                break;
                            }
                        case "npvr":
                            {
                                type = eAssetTypes.NPVR;
                                break;
                            }
                        case "media":
                            {
                                type = eAssetTypes.MEDIA;
                                break;
                            }
                        default:
                            break;
                    }
                }

                var responseAsset = new PersonalAssetInfo()
                {
                    Id = asset.Id,
                    Type = type,
                    Files = new List<MediaFileItemPricesContainer>()
                };

                string dictionaryKey = string.Format("{0}.{1}", asset.type.ToString().ToLower(), asset.Id);

                if (assetIdToPersonalAsset.ContainsKey(dictionaryKey))
                {
                    response.Status = ResponseUtils.ReturnBadRequestStatus(string.Format("Duplicate asset: Id = {0} type = {1}", asset.Id, asset.type));
                    return response;
                }
                else
                {
                    // pair example: Key = Media.875638 Value = new and empty personal asset data
                    assetIdToPersonalAsset.Add(dictionaryKey, responseAsset);
                }

                if (asset.FileIds != null)
                {
                    // Run on all file IDs and map them to respone asset
                    foreach (var file in asset.FileIds)
                    {
                        if (fileToPersonalAsset.ContainsKey(file))
                        {
                            response.Status = ResponseUtils.ReturnBadRequestStatus(string.Format("Duplicate file Id: {0}", file));
                            return response;
                        }
                        else
                        {
                            fileToPersonalAsset.Add(file, responseAsset);
                        }
                    }
                }

                response.Objects.Add(responseAsset);

                assetFiles.Add(new AssetFiles()
                {
                    AssetId = asset.Id.ToString(),
                    AssetType = type,
                    FileIds = asset.FileIds
                });
            }

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetItemPrices", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            AssetItemPriceResponse pricingsResponse = null;
            TVPApiModule.Objects.Responses.AssetsBookmarksResponse bookmarksResponse = null;

            if (groupId > 0)
            {
                if (assets.Length == 0)
                {
                    return response;
                }

                var ctx = HttpContext.Current;

                System.Threading.Thread threadPricing = new Thread(() =>
                {
                    try
                    {
                        HttpContext.Current.Items.Clear();
                        foreach (var item in ctx.Items.Keys)
                        {
                            HttpContext.Current.Items.Add(item, ctx.Items[item]);
                        }

                        pricingsResponse = new ApiConditionalAccessService(groupId, initObj.Platform).GetAssetsPrices(initObj.SiteGuid,
                            string.Empty, initObj.UDID, assetFiles);
                    }
                    catch (Exception ex)
                    {
                        ctx.Items["Error"] = ex;
                    }
                });

                System.Threading.Thread threadBookmarks = new Thread(() =>
                {
                    try
                    {
                        HttpContext.Current.Items.Clear();
                        foreach (var item in ctx.Items.Keys)
                        {
                            HttpContext.Current.Items.Add(item, ctx.Items[item]);
                        }

                        List<AssetBookmarkRequest> assetsToSend = new List<AssetBookmarkRequest>();
                        foreach (PersonalAssetRequest asset in assets)
                        {
                            AssetBookmarkRequest assetToAdd = new AssetBookmarkRequest();
                            assetToAdd.AssetID = asset.Id.ToString();

                            var type = eAssetTypes.UNKNOWN;

                            if (asset.type != null)
                            {
                                switch (asset.type.ToLower())
                                {
                                    case "unknown":
                                        {
                                            type = eAssetTypes.UNKNOWN;
                                            break;
                                        }
                                    case "npvr":
                                        {
                                            type = eAssetTypes.NPVR;
                                            break;
                                        }
                                    case "media":
                                        {
                                            type = eAssetTypes.MEDIA;
                                            break;
                                        }
                                    case "epg":
                                        {
                                            type = eAssetTypes.EPG;
                                            break;
                                        }
                                    default:
                                        break;
                                }
                            }
                            assetToAdd.AssetType = type;

                            assetsToSend.Add(assetToAdd);
                        }
                        var res = new AssetsBookmarksLoader(groupId, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID, assetsToSend)
                        {
                            DomainId = initObj.DomainID,
                            Platform = initObj.Platform.ToString()
                        }.Execute() as AssetsBookmarksResponse;
                        bookmarksResponse = new TVPApiModule.Objects.Responses.AssetsBookmarksResponse(res.AssetsBookmarks, res.Status.Code, res.Status.Message, res.m_nTotalItems);
                    }
                    catch (Exception ex)
                    {
                        ctx.Items["Error"] = ex;
                    }

                });

                if (with.Contains("pricing"))
                {
                    threadPricing.Start();
                }

                if (with.Contains("bookmark"))
                {
                    // Tokenization: validate domain
                    if (AuthorizationManager.IsTokenizationEnabled() &&
                        !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupId, initObj.Platform))
                    {
                        return null;
                    }

                    threadBookmarks.Start();
                }

                if (with.Contains("pricing"))
                {
                    threadPricing.Join();
                }

                if (with.Contains("bookmark"))
                {
                    threadBookmarks.Join();
                }

                // According to catalog response, update final response's objects
                if (bookmarksResponse != null)
                {
                    // If response from catalog web service is not OK, return error message
                    if (bookmarksResponse.Status != null && bookmarksResponse.Status.Code != 0)
                    {
                        response.Objects.Clear();
                        response.TotalItems = 0;
                        response.Status = bookmarksResponse.Status;
                        return response;
                    }

                    if (bookmarksResponse.AssetsBookmarks != null)
                    {
                        foreach (var bookmark in bookmarksResponse.AssetsBookmarks)
                        {
                            string key = string.Format("{0}.{1}", bookmark.AssetType.ToString().ToLower(), bookmark.AssetID);

                            PersonalAssetInfo personalAsset;

                            if (assetIdToPersonalAsset.TryGetValue(key, out personalAsset))
                            {
                                personalAsset.Bookmarks = bookmark.Bookmarks;
                            }
                        }
                    }
                }
                // According to CAS response, update final response's objects
                if (pricingsResponse != null)
                {
                    // If response from CAS web service is not OK, return error message
                    if (pricingsResponse.Status != null && pricingsResponse.Status.Code != 0)
                    {
                        response.Objects.Clear();
                        response.TotalItems = 0;
                        response.Status = new TVPApiModule.Objects.Responses.Status(pricingsResponse.Status.Code, pricingsResponse.Status.Message);
                        return response;
                    }

                    if (pricingsResponse.Prices != null)
                    {
                        foreach (var pricing in pricingsResponse.Prices)
                        {
                            string key = string.Format("{0}.{1}", pricing.AssetType.ToString().ToLower(), pricing.AssetId);

                            PersonalAssetInfo personalAsset;

                            if (assetIdToPersonalAsset.TryGetValue(key, out personalAsset))
                            {
                                personalAsset.Files = pricing.PriceContainers.ToList();
                            }
                        }
                    }
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

    }
}
