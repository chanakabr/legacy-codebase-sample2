using System;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.DataEntities;
using TVPApi;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Xml;
using System.IO;
using Tvinci.Data.DataLoader;
using System.Text;
using System.Web.Script.Serialization;
using TVPPro.SiteManager.Services;
using TVPApiServices;

[WebService(Namespace = "http://tvpapi.tvinci.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class Service : System.Web.Services.WebService
{
    private SiteService m_siteService = new SiteService();
    private MediaService m_mediaService = new MediaService();

    public Service()
    {
    }

    #region Web Methods

    #region SiteMap

    //Get complete user site map - retrieve on first time from DB for each new groupID. Next calls will get ready site map
    [WebMethod(EnableSession=true, Description="Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public TVPApi.SiteMap GetSiteMap(InitializationObject initObj, string ws_User, string ws_Pass)
    {
        return m_siteService.GetSiteMap(initObj, ws_User, ws_Pass);
    }
    #endregion

    #region Page

    //Get specific page from site map
    [WebMethod(EnableSession=true, Description="Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public TVPApi.PageContext GetPage(InitializationObject initObj, string ws_User, string ws_Pass, long ID, bool withMenu, bool withFooter)
    {
        return m_siteService.GetPage(initObj, ws_User, ws_Pass, ID, withMenu, withFooter);
    }

    //Get specific page from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public TVPApi.PageContext GetPageByToken(InitializationObject initObj, string ws_User, string ws_Pass, Pages token, bool withMenu, bool withFooter)
    {
        return m_siteService.GetPageByToken(initObj, ws_User, ws_Pass, token, withMenu, withFooter);
    }


    //Get all page galleries from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public List<TVPApi.PageGallery> GetPageGalleries(InitializationObject initObj, string ws_User, string ws_Pass, long PageID, int pageSize, int start_index)
    {
        return m_siteService.GetPageGalleries(initObj, ws_User, ws_Pass, PageID, pageSize, start_index);
    }

    //Get all page galleries from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public PageGallery GetGallery(InitializationObject initObj, string ws_User, string ws_Pass, long galleryID, long PageID)
    {
        return m_siteService.GetGallery(initObj, ws_User, ws_Pass, galleryID, PageID);
    }

    #endregion


    #region Gallery


    //Get all gallery items for a specific gallery
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public List<GalleryItem> GetGalleryContent(InitializationObject initObj, string ws_User, string ws_Pass, long ID, long PageID, string picSize, int pageSize, int start_index)
    {
        return m_siteService.GetGalleryContent(initObj, ws_User, ws_Pass, ID, PageID, picSize, pageSize, start_index);
    }


    //Get content from specific gallery items
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public List<Media> GetGalleryItemContent(InitializationObject initObj, string ws_User, string ws_Pass, long ItemID, long GalleryID, long PageID, string picSize, int pageSize, int pageIndex)
    {
        return m_siteService.GetGalleryItemContent(initObj, ws_User, ws_Pass, ItemID, GalleryID, PageID, picSize, pageSize, pageIndex);
    }

    #endregion

    #region Footer Menu


    //Get specific menu from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public Menu GetMenu(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
    {
        return m_siteService.GetMenu(initObj, ws_User, ws_Pass, ID);
    }


    //Get specific footer from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public Menu GetFooter(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
    {
        return m_siteService.GetFooter(initObj, ws_User, ws_Pass, ID);
    }

    #endregion

    #region Profiles

    //Get full side profile from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public Profile GetSideProfile(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
    {
        return m_siteService.GetSideProfile(initObj, ws_User, ws_Pass, ID);
    }


    //Get full bottom profile from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public Profile GetBottomProfile(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
    {
        return m_siteService.GetBottomProfile(initObj, ws_User, ws_Pass, ID);
    }

    #endregion


    #region Media


    //Get specific media info
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    [System.Xml.Serialization.XmlInclude(typeof(DynamicData))]
    public Media GetMediaInfo(InitializationObject initObj, string ws_User, string ws_Pass, long MediaID, int mediaType, string picSize, bool withDynamic)
    {
        return m_mediaService.GetMediaInfo(initObj, ws_User, ws_Pass, MediaID, mediaType, picSize, withDynamic);
    }

    //Get Channel media
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetChannelMediaList(InitializationObject initObj, string ws_User, string ws_Pass, long ChannelID, string picSize, int pageSize, int pageIndex)
    {
        return m_mediaService.GetChannelMediaList(initObj, ws_User, ws_Pass, ChannelID, picSize, pageSize, pageIndex);
    }

    //Get Channel media
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetChannelMediaListWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, long ChannelID, string picSize, int pageSize, int pageIndex, ref long mediaCount)
    {
        return m_mediaService.GetChannelMediaListWithMediaCount(initObj, ws_User, ws_Pass, ChannelID, picSize, pageSize, pageIndex, ref mediaCount);
    }

    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public bool IsMediaFavorite(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID)
    {
        return m_mediaService.IsMediaFavorite(initObj, ws_User, ws_Pass, mediaID);
    }

    //Get Related media info
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetRelatedMedias(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex)
    {
        return m_mediaService.GetRelatedMedias(initObj, ws_User, ws_Pass, mediaID, mediaType, picSize, pageSize, pageIndex);
    }

    //Get Related media info
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetRelatedMediaWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, ref long mediaCount)
    {
        return m_mediaService.GetRelatedMediaWithMediaCount(initObj, ws_User, ws_Pass, mediaID, mediaType, picSize, pageSize, pageIndex, ref mediaCount);
    }

    //Get Related media info
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetPeopleWhoWatched(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex)
    {
        return m_mediaService.GetPeopleWhoWatched(initObj, ws_User, ws_Pass, mediaID, mediaType, picSize, pageSize, pageIndex);
    }

    //Get Related media info
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Comment> GetMediaComments(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int pageSize, int pageIndex)
    {
        return m_mediaService.GetMediaComments(initObj, ws_User, ws_Pass, mediaID, pageSize, pageIndex);
    }

    //Serach media by tag
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> SearchMediaByTag(InitializationObject initObj, string ws_User, string ws_Pass, string tagName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
    {
        return m_mediaService.SearchMediaByTag(initObj, ws_User, ws_Pass, tagName, value, mediaType, picSize, pageSize, pageIndex, orderBy);
    }

    //Serach media by meta
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> SearchMediaByMeta(InitializationObject initObj, string ws_User, string ws_Pass, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
    {
        return m_mediaService.SearchMediaByMeta(initObj, ws_User, ws_Pass, metaName, value, mediaType, picSize, pageSize, pageIndex, orderBy);
    }

    //Serach media by meta
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> SearchMediaByMetaWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount)
    {
        return m_mediaService.SearchMediaByMetaWithMediaCount(initObj, ws_User, ws_Pass, metaName, value, mediaType, picSize, pageSize, pageIndex, orderBy, ref mediaCount);
    }

    //Serach media by tag
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public Category GetCategory(InitializationObject initObj, string ws_User, string ws_Pass, int categoryID)
    {
        return m_mediaService.GetCategory(initObj, ws_User, ws_Pass, categoryID);
    }

    //Serach media by free text
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> SearchMedia(InitializationObject initObj, string ws_User, string ws_Pass, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
    {
        return m_mediaService.SearchMedia(initObj, ws_User, ws_Pass, text, mediaType, picSize, pageSize, pageIndex, orderBy);
    }

    //Serach media by free text
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> SearchMediaWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount)
    {
        return m_mediaService.SearchMediaWithMediaCount(initObj, ws_User, ws_Pass, text, mediaType, picSize, pageSize, pageIndex, orderBy, ref mediaCount);
    }

    //Get User Items (Favorites, Rentals etc..)
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetUserItems(InitializationObject initObj, string ws_User, string ws_Pass, UserItemType itemType, int mediaType, string picSize,  int pageSize, int start_index)
    {
        return m_mediaService.GetUserItems(initObj, ws_User, ws_Pass, itemType, mediaType, picSize, pageSize, start_index);
    }

    //Need to implement
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<string> GetNMostSearchedTexts(InitializationObject initObj, string ws_User, string ws_Pass, int N, int pageSize, int start_index)
    {
        return m_mediaService.GetNMostSearchedTexts(initObj, ws_User, ws_Pass, N, pageSize, start_index);
    }

    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public string[] GetAutoCompleteSearchList(InitializationObject initObj, string ws_User, string ws_Pass, string prefixText)
    {
        return m_mediaService.GetAutoCompleteSearchList(initObj, ws_User, ws_Pass, prefixText);
    }

    #endregion

    #region Actions

    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public bool ActionDone(InitializationObject initObj, string ws_User, string ws_Pass, TVPApi.ActionType action, int mediaID, int mediaType, int extraVal)
    {
        return m_mediaService.ActionDone(initObj, ws_User, ws_Pass, action, mediaID, mediaType, extraVal);
    }

    [WebMethod]
    public List<Media> GetMediasByMostAction(InitializationObject initObj, string ws_User, string ws_Pass, TVPApi.ActionType action, int mediaType)
    {
        return null;
    }

    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetMediasByRating(InitializationObject initObj, string ws_User, string ws_Pass, int rating)
    {
        return m_mediaService.GetMediasByRating(initObj, ws_User, ws_Pass, rating);
    }

    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public string SignIn(InitializationObject initObj, string ws_User, string ws_Pass, string userName, string password)
    {
        return m_siteService.SignIn(initObj, ws_User, ws_Pass, userName, password);
    }

    #endregion

    #endregion


}
