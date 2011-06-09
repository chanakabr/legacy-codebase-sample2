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

            logger.InfoFormat("GetMediaInfo-> [{0}, {1}], Params: [MediaID: {2}, MediaType: {3}, picSize: {4}, withDynamic: {5}]", groupID, initObj.Platform, MediaID, mediaType, picSize, withDynamic);

            if (groupID > 0)
            {
                try
                {
                    retMedia = MediaHelper.GetMediaInfo(initObj, MediaID, mediaType, picSize, groupID, withDynamic);
                }
                catch (Exception ex)
                {
                    logger.Error("GetMediaInfo->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetMediaInfo-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return retMedia;
        }

        //Get Channel medias
        [WebMethod(EnableSession = true, Description = "Get Channel medias")]
        public List<Media> GetChannelMediaList(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetChannelMediaList-> [{0}, {1}], Params:[ChannelID: {2}, picSize: {3}, pageSize: {4}, pageIndex: {5}]", groupID, initObj.Platform, ChannelID, picSize, pageSize, pageIndex);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetChannelMediaList(initObj, ChannelID, picSize, pageSize, pageIndex, groupID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetChannelMediaList->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetChannelMediaList-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstMedia;
        }

        //Get channel media with total number of medias
        [WebMethod(EnableSession = true, Description = "Get channel media with total number of medias")]
        public List<Media> GetChannelMediaListWithMediaCount(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, ref long mediaCount)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetChannelMediaListWithMediaCount-> [{0}, {1}], Params:[ChannelID: {2}, picSize: {3}, pageSize: {4}, pageIndex: {5}, mediaCount: {6}]", groupID, initObj.Platform, ChannelID, picSize, pageSize, pageIndex, mediaCount);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetChannelMediaList(initObj, ChannelID, picSize, pageSize, pageIndex, groupID, ref mediaCount);
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("GetChannelMediaListWithMediaCount-> ChannelID: {0}", ChannelID), ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetChannelMediaListWithMediaCount-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstMedia;
        }

        // Check if media has been added to favorites
        [WebMethod(EnableSession = true, Description = "Check if media has been added to favorites")]
        public bool IsMediaFavorite(InitializationObject initObj, int mediaID)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("IsMediaFavorite-> [{0}, {1}], Params:[mediaID: {2}]", groupID, initObj.Platform, mediaID);

            if (groupID > 0)
            {
                try
                {
                    bRet = MediaHelper.IsFavoriteMedia(initObj.Locale.SiteGuid, mediaID, initObj.Platform.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error("IsMediaFavorite->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("IsMediaFavorite-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return bRet;
        }

        //Get users comments for media
        [WebMethod(EnableSession = true, Description = "Get users comments for media")]
        public List<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex)
        {
            List<Comment> lstComment = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetMediaComments-> [{0}, {1}], Params:[mediaID: {2}, pageSize: {3}, pageIndex: {4}]", groupID, initObj.Platform, mediaID, pageSize, pageIndex);

            if (groupID > 0)
            {
                try
                {
                    lstComment = CommentHelper.GetMediaComments(mediaID, pageSize, pageIndex);
                }
                catch (Exception ex)
                {
                    logger.Error("GetMediaComments->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("IsMediaFavorite-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstComment;
        }

        //Get User Items (Favorites, Rentals etc..)
        [WebMethod(EnableSession = true, Description = "Get User Items (Favorites, Rentals etc..)")]
        public List<Media> GetUserItems(InitializationObject initObj, UserItemType itemType, int mediaType, string picSize, int pageSize, int start_index)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetUserItems-> [{0}, {1}], Params:[ItemType: {2}]", groupID, initObj.Platform, itemType);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetUserItems(initObj, itemType, mediaType, picSize, pageSize, start_index, groupID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserItems->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetUserItems-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstMedia;
        }

        #endregion

        #region Media related

        //Get related media info
        [WebMethod(EnableSession = true, Description = "Get related media info")]
        public List<Media> GetRelatedMedias(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetRelatedMedias-> [{0}, {1}], Params:[mediaID: {2}, mediaType: {3}, picSize: {4}, pageSize: {5}, pageIndex: {6}]", groupID, initObj.Platform, mediaID, mediaType, picSize, pageSize, pageIndex);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetRelatedMediaList(initObj, mediaID, mediaType, picSize, pageSize, pageIndex, groupID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetRelatedMedias->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetRelatedMedias-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstMedia;
        }

        //Get related media info with total number of medias
        [WebMethod(EnableSession = true, Description = "Get related media info with total number of medias")]
        public List<Media> GetRelatedMediaWithMediaCount(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, ref long mediaCount)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetRelatedMediaWithMediaCount-> [{0}, {1}], Params:[mediaID: {2}, mediaType: {3}, picSize: {4}, pageSize: {5}, pageIndex: {6}, mediaCount: {7}]", groupID, initObj.Platform, mediaID, mediaType, picSize, pageSize, pageIndex, mediaCount);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetRelatedMediaList(initObj, mediaID, mediaType, picSize, pageSize, pageIndex, groupID, ref mediaCount);
                }
                catch (Exception ex)
                {
                    logger.Error("GetRelatedMediaWithMediaCount->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetRelatedMediaWithMediaCount-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstMedia;
        }

        //Get Related media info
        [WebMethod(EnableSession = true, Description = "Get Related media info")]
        public List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetPeopleWhoWatched-> [{0}, {1}], Params:[mediaID: {2}, mediaType: {3}, picSize: {4}, pageSize: {5}, pageIndex: {6}]", groupID, initObj.Platform, mediaID, mediaType, picSize, pageSize, pageIndex);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.GetPeopleWhoWatchedList(initObj, mediaID, mediaType, picSize, pageSize, pageIndex, groupID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetPeopleWhoWatched->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetPeopleWhoWatched-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstMedia;
        }

        [WebMethod(EnableSession = true, Description = "")]
        public List<Media> GetMediasByRating(InitializationObject initObj, int rating)
        {
            return null;
        }
        #endregion

        #region Search media

        //Search medias by tag
        [WebMethod(EnableSession = true, Description = "Search medias by tag")]
        public List<Media> SearchMediaByTag(InitializationObject initObj, string tagName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SearchMediaByTag-> [{0}, {1}], Params:[tagName: {2}, value: {3}, mediaType: {4}, picSize: {5}, pageSize: {6}, pageIndex: {7}, orderBy: {8}]", groupID, initObj.Platform, tagName, value, mediaType, picSize, pageSize, pageIndex, orderBy);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMediaByTag(initObj, mediaType, tagName, value, picSize, pageSize, pageIndex, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    logger.Error("SearchMediaByTag->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SearchMediaByTag-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstMedia;
        }

        //Search medias by meta
        [WebMethod(EnableSession = true, Description = "Search medias by meta")]
        public List<Media> SearchMediaByMeta(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SearchMediaByMeta-> [{0}, {1}], Params:[tagName: {2}, value: {3}, mediaType: {4}, picSize: {5}, pageSize: {6}, pageIndex: {7}, orderBy: {8}]", groupID, initObj.Platform, metaName, value, mediaType, picSize, pageSize, pageIndex, orderBy);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMediaByMeta(initObj, mediaType, metaName, value, picSize, pageSize, pageIndex, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    logger.Error("SearchMediaByMeta->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SearchMediaByMeta-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstMedia;
        }

        //Saerch media by meta with total items number
        [WebMethod(EnableSession = true, Description = "Saerch media by meta with total items number")]
        public List<Media> SearchMediaByMetaWithMediaCount(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SearchMediaByMetaWithMediaCount-> [{0}, {1}], Params:[tagName: {2}, value: {3}, mediaType: {4}, picSize: {5}, pageSize: {6}, pageIndex: {7}, orderBy: {8}, mediaCount: {9}]", groupID, initObj.Platform, metaName, value, mediaType, picSize, pageSize, pageIndex, orderBy, mediaCount);

            if (groupID > 0)
            {
                try
                {
                    return MediaHelper.SearchMediaByMeta(initObj, mediaType, metaName, value, picSize, pageSize, pageIndex, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    logger.Error("SearchMediaByMetaWithMediaCount->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SearchMediaByMetaWithMediaCount-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return null;
        }

        //Search category info
        [WebMethod(EnableSession = true, Description = "Search category info")]
        public Category GetCategory(InitializationObject initObj, int categoryID)
        {
            Category retCategory = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetCategory-> [{0}, {1}], Params:[categoryID: {2}]", groupID, initObj.Platform, categoryID);

            if (groupID > 0)
            {
                try
                {
                    retCategory = CategoryTreeHelper.GetCategoryTree(categoryID, groupID, initObj.Platform);
                }
                catch (Exception ex)
                {
                    logger.Error("GetCategory->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetCategory-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return retCategory;
        }

        //Search media by free text
        [WebMethod(EnableSession = true, Description = "Search media by free text")]
        public List<Media> SearchMedia(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SearchMedia-> [{0}, {1}], Params:[text: {2}, mediaType: {3}, picSize: {4}, pageSize: {5}, pageIndex: {6}, orderBy: {7}]", groupID, initObj.Platform, text, mediaType, picSize, pageSize, pageIndex, orderBy);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMedia(initObj, mediaType, text, picSize, pageSize, pageIndex, groupID, (int)orderBy);
                }
                catch (Exception ex)
                {
                    logger.Error("SearchMedia->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SearchMedia-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstMedia;
        }

        //Search media by free text with response total media count
        [WebMethod(EnableSession = true, Description = "Search media by free text with response total media count")]
        public List<Media> SearchMediaWithMediaCount(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SearchMediaWithMediaCount-> [{0}, {1}], Params:[text: {2}, mediaType: {3}, picSize: {4}, pageSize: {5}, pageIndex: {6}, orderBy: {7}, mediaCount: {8}]", groupID, initObj.Platform, text, mediaType, picSize, pageSize, pageIndex, orderBy, mediaCount);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = MediaHelper.SearchMedia(initObj, mediaType, text, picSize, pageSize, pageIndex, groupID, (int)orderBy, ref mediaCount);
                }
                catch (Exception ex)
                {
                    logger.Error("SearchMediaWithMediaCount->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SearchMediaWithMediaCount-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstMedia;
        }

        //Get most searched text
        [WebMethod(EnableSession = true, Description = "Get most searched text")]
        public List<string> GetNMostSearchedTexts(InitializationObject initObj, int N, int pageSize, int start_index)
        {
            List<string> retVal = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetNMostSearchedTexts-> [{0}, {1}], Params:[N: {2}, pageSize: {3}, start_index: {4}]", groupID, initObj.Platform, N, pageSize, start_index);

            if (groupID > 0)
            {
                // TODO:
            }
            else
            {
                logger.ErrorFormat("GetNMostSearchedTexts-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return retVal;
        }

        // Get auto-complete media titles
        [WebMethod(EnableSession = true, Description = "Get auto-complete media titles")]
        public string[] GetAutoCompleteSearchList(InitializationObject initObj, string prefixText)
        {
            string[] retVal = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetAutoCompleteSearchList-> [{0}, {1}], Params:[prefixText: {2}]", groupID, initObj.Platform, prefixText);

            if (groupID > 0)
            {
                List<string> lstRet = new List<String>();

                List<string> lstResponse = MediaHelper.GetAutoCompleteList(groupID, initObj.Platform, groupID);

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

        // Perform action on media (AddFavorite, Comment, Like, Rate, Recommend, Record, Reminder, RemoveFavorite, Share, Watch)
        [WebMethod(EnableSession = true, Description = "Perform action on media (AddFavorite, Comment, Like, Rate, Recommend, Record, Reminder, RemoveFavorite, Share, Watch)")]
        public bool ActionDone(InitializationObject initObj, TVPApi.ActionType action, int mediaID, int mediaType, int extraVal)
        {
            bool retVal = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ActionDone", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("ActionDone-> [{0}, {1}], Params:[mediaID: {2}, mediaType: {3}, extraVal: {4}]", groupID, initObj.Platform, mediaID, mediaType, extraVal);

            if (groupID > 0)
            {
                try
                {
                    retVal = ActionHelper.PerformAction(action, mediaID, mediaType, groupID, initObj.Platform, initObj.Locale.SiteGuid, extraVal);
                }
                catch (Exception ex)
                {
                    logger.Error("ActionDone->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("ActionDone-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            return retVal;
        }

        [WebMethod(EnableSession = true, Description = "")]
        public List<Media> GetMediasByMostAction(InitializationObject initObj, TVPApi.ActionType action, int mediaType)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediasByMostAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            return null;
        }

        #endregion

        #region Purchase

        [WebMethod(EnableSession = true, Description = "Check if item is purchased")]
        public bool IsItemPurchased(InitializationObject initObj, int iFileID, string sUserGuid)
        {
            bool bRet = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsItemPurchased", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("IsItemPurchased-> [{0}, {1}], Params:[fileID: {2}, user: {3}]", groupId, initObj.Platform, iFileID, sUserGuid);

            if (groupId > 0)
            {
                try
                {
                    MediaFileItemPricesContainer[] prices = new ApiConditionalAccessService(groupId, initObj.Platform).GetItemsPrice(new int[] { iFileID }, sUserGuid, true);

                    if (prices == null || prices.Length == 0)
                        return bRet;
                    
                    MediaFileItemPricesContainer mediaPrice = null;
                    foreach (MediaFileItemPricesContainer mp in prices)
                    {
                        if (mp.m_nMediaFileID == iFileID)
                            mediaPrice = mp;
                    }

                    bRet = mediaPrice.m_oItemPrices[0].m_oPrice.m_dPrice == 0;
                }
                catch (Exception ex)
                {
                    logger.Error("IsItemPurchased->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("IsItemPurchased-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Get list of purchased items for a user")]
        public PermittedMediaContainer[] GetUserPermittedItems(InitializationObject initObj, string sSiteGuid)
        {
            PermittedMediaContainer[] permittedMediaContainer = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermittedItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetUserPermittedItems-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, sSiteGuid);

            if (groupId > 0)
            {
                try
                {
                    permittedMediaContainer = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermittedItems(sSiteGuid);
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserPermittedItems->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetUserPermittedItems-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return permittedMediaContainer;
        }

        [WebMethod(EnableSession = true, Description = "Get list of purchased subscriptions for a user")]
        public PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj, string sSiteGuid)
        {
            PermittedSubscriptionContainer[] permitedSubscriptions = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermitedSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetUserPermitedSubscriptions-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, sSiteGuid);

            if (groupId > 0)
            {
                try
                {
                    permitedSubscriptions = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermitedSubscriptions(sSiteGuid);
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserPermitedSubscriptions->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetUserPermitedSubscriptions-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return permitedSubscriptions;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for file")]
        public BillingResponse ChargeUserForMediaFile(InitializationObject initObj, string guid, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon)
        {
            BillingResponse response = null;

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for subscription")]
        public BillingResponse ChargeUserForSubscription(InitializationObject initObj, string guid, double iPrice, string sCurrency, int iFileID, string sSubscriptionID, string sUserIP, string sCoupon)
        {
            BillingResponse response = null;

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for subscription")]
        public MediaFileItemPricesContainer[] GetItemPrices(InitializationObject initObj, string sSiteGuid, int[] fileIds, bool bOnlyLowest)
        {
            MediaFileItemPricesContainer[] itemPrices = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetItemPrices", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetItemPrices-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, sSiteGuid);

            if (groupId > 0)
            {
                try
                {
                    itemPrices = new ApiConditionalAccessService(groupId, initObj.Platform).GetItemsPrice(fileIds, sSiteGuid, bOnlyLowest);
                }
                catch (Exception ex)
                {
                    logger.Error("GetItemPrices->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetItemPrices-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            
            return itemPrices;
        }
        #endregion
    }
}
