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
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.GetSiteMap(initObj);
    }
    #endregion

    #region Page

    //Get specific page from site map
    [WebMethod(EnableSession=true, Description="Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public TVPApi.PageContext GetPage(InitializationObject initObj, string ws_User, string ws_Pass, long ID, bool withMenu, bool withFooter)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.GetPage(initObj, ID, withMenu, withFooter);
    }

    //Get specific page from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public TVPApi.PageContext GetPageByToken(InitializationObject initObj, string ws_User, string ws_Pass, Pages token, bool withMenu, bool withFooter)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.GetPageByToken(initObj, token, withMenu, withFooter);
    }


    //Get all page galleries from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public List<TVPApi.PageGallery> GetPageGalleries(InitializationObject initObj, string ws_User, string ws_Pass, long PageID, int pageSize, int start_index)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.GetPageGalleries(initObj, PageID, pageSize, start_index);
    }

    //Get all page galleries from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public PageGallery GetGallery(InitializationObject initObj, string ws_User, string ws_Pass, long galleryID, long PageID)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.GetGallery(initObj, galleryID, PageID);
    }

    #endregion


    #region Gallery


    //Get all gallery items for a specific gallery
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public List<GalleryItem> GetGalleryContent(InitializationObject initObj, string ws_User, string ws_Pass, long ID, long PageID, string picSize, int pageSize, int start_index)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.GetGalleryContent(initObj, ID, PageID, picSize, pageSize, start_index);
    }


    //Get content from specific gallery items
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public List<Media> GetGalleryItemContent(InitializationObject initObj, string ws_User, string ws_Pass, long ItemID, long GalleryID, long PageID, string picSize, int pageSize, int pageIndex)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.GetGalleryItemContent(initObj, ItemID, GalleryID, PageID, picSize, pageSize, pageIndex, TVPApi.OrderBy.None);
    }

    #endregion

    #region Footer Menu


    //Get specific menu from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public Menu GetMenu(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.GetMenu(initObj, ID);
    }


    //Get specific footer from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public Menu GetFooter(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.GetFooter(initObj, ID);
    }

    #endregion

    #region Profiles

    //Get full side profile from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public Profile GetSideProfile(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.GetSideProfile(initObj, ID);
    }


    //Get full bottom profile from site map
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public Profile GetBottomProfile(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.GetBottomProfile(initObj, ID);
    }

    #endregion


    #region Media


    //Get specific media info
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    [System.Xml.Serialization.XmlInclude(typeof(DynamicData))]
    public Media GetMediaInfo(InitializationObject initObj, string ws_User, string ws_Pass, long MediaID, int mediaType, string picSize, bool withDynamic)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.GetMediaInfo(initObj, MediaID, mediaType, picSize, withDynamic);
    }

    //Get Channel media
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetChannelMediaList(InitializationObject initObj, string ws_User, string ws_Pass, long ChannelID, string picSize, int pageSize, int pageIndex)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.GetChannelMediaList(initObj, ChannelID, picSize, pageSize, pageIndex, TVPApi.OrderBy.None);
    }

    //Get Channel media
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetChannelMediaListWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, long ChannelID, string picSize, int pageSize, int pageIndex, ref long mediaCount)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.GetChannelMediaListWithMediaCount(initObj, ChannelID, picSize, pageSize, pageIndex, ref mediaCount);
    }

    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public bool IsMediaFavorite(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.IsMediaFavorite(initObj, mediaID);
    }

    //Get Related media info
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetRelatedMedias(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.GetRelatedMedias(initObj, mediaID, mediaType, picSize, pageSize, pageIndex);
    }

    //Get Related media info
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetRelatedMediaWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, ref long mediaCount)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.GetRelatedMediaWithMediaCount(initObj, mediaID, mediaType, picSize, pageSize, pageIndex, ref mediaCount);
    }

    //Get Related media info
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetPeopleWhoWatched(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.GetPeopleWhoWatched(initObj, mediaID, mediaType, picSize, pageSize, pageIndex);
    }

    //Get Related media info
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Comment> GetMediaComments(InitializationObject initObj, string ws_User, string ws_Pass, int mediaID, int pageSize, int pageIndex)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.GetMediaComments(initObj, mediaID, pageSize, pageIndex);
    }

    //Serach media by tag
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> SearchMediaByTag(InitializationObject initObj, string ws_User, string ws_Pass, string tagName, string value, 
        int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.SearchMediaByTag(initObj, tagName, value, mediaType, picSize, pageSize, pageIndex, orderBy);
    }

    //Serach media by meta
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> SearchMediaByMeta(InitializationObject initObj, string ws_User, string ws_Pass, string metaName, string value, 
        int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.SearchMediaByMeta(initObj, metaName, value, mediaType, picSize, pageSize, pageIndex, orderBy);
    }

    //Serach media by meta
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> SearchMediaByMetaWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, string metaName, 
        string value, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy, ref long mediaCount)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.SearchMediaByMetaWithMediaCount(initObj, metaName, value, mediaType, picSize, pageSize, pageIndex, orderBy, ref mediaCount);
    }

    //Serach media by tag
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public Category GetCategory(InitializationObject initObj, string ws_User, string ws_Pass, int categoryID)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.GetCategory(initObj, categoryID);
    }

    //Serach media by free text
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> SearchMedia(InitializationObject initObj, string ws_User, string ws_Pass, string text, int mediaType, string picSize, 
        int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.SearchMedia(initObj, text, mediaType, picSize, pageSize, pageIndex, orderBy);
    }

    //Serach media by free text
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> SearchMediaWithMediaCount(InitializationObject initObj, string ws_User, string ws_Pass, string text, int mediaType, string picSize, 
        int pageSize, int pageIndex, TVPApi.OrderBy orderBy, ref long mediaCount)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.SearchMediaWithMediaCount(initObj, text, mediaType, picSize, pageSize, pageIndex, orderBy, ref mediaCount);
    }

    //Get User Items (Favorites, Rentals etc..)
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetUserItems(InitializationObject initObj, string ws_User, string ws_Pass, UserItemType itemType, int mediaType, string picSize,  int pageSize, int start_index)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.GetUserItems(initObj, itemType, mediaType, picSize, pageSize, start_index);
    }

    //Need to implement
    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<string> GetNMostSearchedTexts(InitializationObject initObj, string ws_User, string ws_Pass, int N, int pageSize, int start_index)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.GetNMostSearchedTexts(initObj, N, pageSize, start_index);
    }

    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public string[] GetAutoCompleteSearchList(InitializationObject initObj, string ws_User, string ws_Pass, string prefixText)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        //return m_mediaService.GetAutoCompleteSearchList(initObj, prefixText, null);
        return null;
    }

    #endregion

    #region Actions

    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public bool ActionDone(InitializationObject initObj, string ws_User, string ws_Pass, TVPApi.ActionType action, int mediaID, int mediaType, int extraVal)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.ActionDone(initObj, action, mediaID, mediaType, extraVal);
    }

    [WebMethod]
    public List<Media> GetMediasByMostAction(InitializationObject initObj, string ws_User, string ws_Pass, TVPApi.ActionType action, int mediaType)
    {
        return null;
    }

    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Media/Service.asmx' instead.")]
    public List<Media> GetMediasByRating(InitializationObject initObj, string ws_User, string ws_Pass, int rating)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_mediaService.GetMediasByRating(initObj, rating);
    }

    [WebMethod(EnableSession = true, Description = "Deprecated! Use 'ws/Site/Service.asmx' instead.")]
    public string SignIn(InitializationObject initObj, string ws_User, string ws_Pass, string userName, string password)
    {
        initObj.ApiUser = ws_User;
        initObj.ApiPass = ws_Pass;
        return m_siteService.SignIn(initObj, userName, password).SiteGuid;
    }

    #endregion

    #endregion


}
